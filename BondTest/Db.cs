using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Microsoft.Isam.Esent.Interop;

namespace BondTest
{
    public class Db : IDisposable
    {
        private readonly string _database;
        private readonly ISerializer<Data> _serializer;
        private readonly string _path = "Esent";
        private readonly Instance _instance;
        private const string Table = "data";

        public Db(string database, ISerializer<Data> serializer)
        {
            _database = database;
            _serializer = serializer;
            _path = database;
            if (Path.IsPathRooted(database) == false)
                _path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, database);
            _database = Path.Combine(_path, Path.GetFileName(database + ".edb"));

            _instance = new Instance(database + Guid.NewGuid());
        }

        public void Initialize()
        {
            ConfigureInstance(_instance);
            try
            {
                _instance.Init();

                EnsureDatabaseIsCreatedAndAttachToDatabase();
            }
            catch (Exception e)
            {
                Dispose();
                throw new InvalidOperationException("Could not open database: " + _database, e);
            }
        }

        private void EnsureDatabaseIsCreatedAndAttachToDatabase()
        {
            using (var session = new Session(_instance))
            {
                try
                {
                    Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
                    return;
                }
                catch (EsentErrorException e)
                {
                    if (e.Error == JET_err.DatabaseDirtyShutdown)
                    {
                        try
                        {
                            using (var recoverInstance = new Instance("Recovery instance for: " + _database))
                            {
                                recoverInstance.Init();
                                using (var recoverSession = new Session(recoverInstance))
                                {
                                    ConfigureInstance(recoverInstance);
                                    Api.JetAttachDatabase(recoverSession, _database, AttachDatabaseGrbit.DeleteCorruptIndexes);
                                    Api.JetDetachDatabase(recoverSession, _database);
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }

                        Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
                        return;
                    }
                    if (e.Error != JET_err.FileNotFound)
                        throw;
                }

                new SchemaCreator(session).Create(_database, Table);
                Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
            }
        }

        private void ConfigureInstance(Instance instance)
        {
            instance.Parameters.CircularLog = true;
            instance.Parameters.Recovery = true;
            instance.Parameters.CreatePathIfNotExist = true;
            instance.Parameters.TempDirectory = Path.Combine(_path, "temp");
            instance.Parameters.SystemDirectory = Path.Combine(_path, "system");
            instance.Parameters.LogFileDirectory = Path.Combine(_path, "logs");
        }

        private IList<Data> ExecuteInTransaction(Func<Session, Table, IList<Data>> dataFunc)
        {
            IList<Data> results;
            using (var session = new Session(_instance))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, _database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                {
                    using (var table = new Table(session, dbid, Table, OpenTableGrbit.None))
                    {
                        results = dataFunc(session, table);
                    }

                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }

            return results;
        }

        public IEnumerable<Data> GetAllData()
        {
            using (var session = new Session(_instance))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, _database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                {
                    using (var table = new Table(session, dbid, Table, OpenTableGrbit.None))
                    {
                        if (Api.TryMoveFirst(session, table))
                        {
                            do
                            {
                                 yield return GetData(session, table);
                            }
                            while (Api.TryMoveNext(session, table));
                        }
                    }

                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
        }

        public void AddData(Data data)
        {
            ExecuteInTransaction((session, table) =>
            {
                using (var updater = new Update(session, table, JET_prep.Insert))
                {
                    var columnId = Api.GetTableColumnid(session, table, "Id");
                    Api.SetColumn(session, table, columnId, data.TimeStamp);

                    var output = _serializer.Serialize(data);

                    var columnData = Api.GetTableColumnid(session, table, "Data");

                    Api.SetColumn(session, table, columnData, output);

                    updater.Save();
                }
                return null;
            });
        }

        private Data GetData(Session session, Table table)
        {
            var binaryColumnid = Api.GetTableColumnid(session, table, "Data");
            var bytes = new byte[200];
            int actualSize;
            Api.JetRetrieveColumn(session, table, binaryColumnid, bytes, bytes.Length, 0, out actualSize, RetrieveColumnGrbit.None, null);

            var dst = _serializer.Deserialize(bytes);
            return dst;
        }

        public IEnumerable<Data> GetDataForDateRange(long fromTicks, long toTicks)
        {
            using (var session = new Session(_instance))
            {
                JET_DBID dbid;
                Api.JetAttachDatabase(session, _database, AttachDatabaseGrbit.None);
                Api.JetOpenDatabase(session, _database, String.Empty, out dbid, OpenDatabaseGrbit.None);
                using (var transaction = new Transaction(session))
                {
                    using (var table = new Table(session, dbid, Table, OpenTableGrbit.None))
                    {
                        Api.JetSetCurrentIndex(session, table, "id_index");
                        Api.MakeKey(session, table, fromTicks, MakeKeyGrbit.NewKey);

                        if (Api.TrySeek(session, table, SeekGrbit.SeekGE))
                        {
                            Api.MakeKey(session, table, toTicks, MakeKeyGrbit.NewKey);
                            Api.JetSetIndexRange(session, table,
                                  SetIndexRangeGrbit.RangeUpperLimit | SetIndexRangeGrbit.RangeInclusive);

                            do
                            {
                                yield return GetData(session, table);
                            }
                            while (Api.TryMoveNext(session, table));
                        }
                    }

                    transaction.Commit(CommitTransactionGrbit.None);
                }
            }
        }

        private bool _disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    if (_instance != null)
                    {
                        _instance.Dispose();
                        GC.SuppressFinalize(this);
                    }
                }

                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }

		~Db()
		{
		    try
		    {
		        Trace.WriteLine(
		            "Disposing esent resources from finalizer! You should call PersistentHashTable.Dispose() instead!");
		        _instance.Dispose();
		    }
		    catch (Exception exception)
		    {
		        try
		        {
		            Trace.WriteLine("Failed to dispose esent instance from finalizer" +
		                            exception);
		        }
		        catch
		        {
		        }
		    }
		}
    }
}
