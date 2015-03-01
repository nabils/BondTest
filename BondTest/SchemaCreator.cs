using System;
using Microsoft.Isam.Esent.Interop;
using Microsoft.Isam.Esent.Interop.Vista;

namespace BondTest
{
    public class SchemaCreator
    {
        private readonly Session _session;
        public const string SchemaVersion = "1.0";

        public SchemaCreator(Session session)
        {
            this._session = session;
        }

        public void Create(string database, string table)
        {
            JET_DBID dbid;
            Api.JetCreateDatabase(_session, database, null, out dbid, CreateDatabaseGrbit.None);
            try
            {
                using (var tx = new Transaction(_session))
                {
                    JET_TABLEID tableid;
                    Api.JetCreateTable(_session, dbid, table, 1, 100, out tableid);

                    // ID
                    JET_COLUMNID columnid;
                    Api.JetAddColumn(_session, tableid, "Id",
                        new JET_COLUMNDEF
                        {                            
                            coltyp = JET_coltyp.Currency,
                            grbit = ColumndefGrbit.ColumnNotNULL
                        }, defaultValue: null, defaultValueSize: 0, columnid: out columnid);

                    // Data
                    Api.JetAddColumn(_session, tableid, "Data",
                        new JET_COLUMNDEF
                        {
                            cbMax = 200,
                            coltyp = JET_coltyp.Binary,
                            grbit = ColumndefGrbit.None
                        }, defaultValue: null, defaultValueSize: 0, columnid: out columnid);

                    // Define table indices
                    const string indexDef = "+Id\0\0";
                    Api.JetCreateIndex(_session, tableid, "id_index",
                        CreateIndexGrbit.IndexPrimary, indexDef, indexDef.Length, 100);

                    tx.Commit(CommitTransactionGrbit.None);
                }
            }
            finally
            {
                Api.JetCloseDatabase(_session, dbid, CloseDatabaseGrbit.None);
            }
        }
    }
}