using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using FizzWare.NBuilder;

namespace BondTest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var db = new Db("data", new BondSerializer()))
            {
                db.Initialize();

                //var ad = db.GetAllData().ToList();
                //var ad = db.GetDataForDateRange(DateTime.Now.AddMinutes(-4).Ticks, DateTime.Now.Ticks).ToList();

                var symbols = new List<string>
                {
                    "AAPL",
                    "MSFT",
                    "JPM"
                };

                var dataStream = Observable.Interval(TimeSpan.FromMilliseconds(100))
                    .Select(d =>
                    {
                        var data = new Data() { TimeStamp = DateTime.Now.Ticks };
                        data.Points = (List<DataPoint>) Builder<DataPoint>.CreateListOfSize(10)
                            .All().With(x => x.Symbol = Pick<string>.RandomItemFrom(symbols))
                            .Build();

                        return data;
                    });

                dataStream.Subscribe(d =>
                {
                    Console.WriteLine("Got data " + d.TimeStamp);

                    db.AddData(d);
                });

                Console.ReadLine();
            }
        }
    }
}
