using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using FizzWare.NBuilder;

namespace BondTest
{
    public class Program
    {
        public static IObservable<Data> GetItemsStream(Db db)
        {
            var ad = db.GetAllData().OrderBy(x => x.TimeStamp);

            return ad.ToObservable();
        }

        static void Main(string[] args)
        {
            using (var db = new Db("data", new BondSerializer()))
            {
                db.Initialize();



                var ad = db.GetAllData().ToList();
                //foreach (var data in ad)
                //{
                //    Console.WriteLine(data);
                //}

                //var symbols = new List<string>
                //{
                //    "AAPL",
                //    "MSFT",
                //    "JPM"
                //};

                //var dataStream = Observable.Interval(TimeSpan.FromMilliseconds(100))
                //    .Select(d =>
                //    {
                //        var data = new Data() { TimeStamp = DateTime.Now.Ticks };
                //        data.Points = (List<DataPoint>)Builder<DataPoint>.CreateListOfSize(10)
                //            .All().With(x => x.Symbol = Pick<string>.RandomItemFrom(symbols))
                //            .Build();

                //        return data;
                //    });

                //dataStream.Subscribe(d =>
                //{
                //    Console.WriteLine("Got data " + d.TimeStamp);

                //    db.AddData(d);
                //});

                Console.ReadLine();
            }
        }
    }
}
