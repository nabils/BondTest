using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using BondTest;
using System;
using FizzWare.NBuilder;
using Visifire.Charts;

namespace Visifire
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Db db = new Db("data", new BondSerializer());

        public MainWindow()
        {
            InitializeComponent();

            this.Loaded += MainWindow_Loaded;
            this.Unloaded += MainWindow_Unloaded;
        }

        private IDisposable deltaSub;

        private static Random random = new Random();


        void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            db.Initialize();


            //var symbols = new List<string>
            //    {
            //        "AAPL",
            //        "MSFT",
            //        "JPM"
            //    };

            //var r = new RandomGenerator();

            //var dataStream = Observable.Interval(TimeSpan.FromMilliseconds(100))
            //    .Select(d =>
            //    {
            //        var data = new Data()
            //        {
            //            TimeStamp = DateTime.Now.AddDays(4).Ticks,
            //            Delta = r.Next(0.0, 2.0),
            //            Symbol = Pick<string>.RandomItemFrom(symbols)
            //        };
            //        return data;
            //    });

            //dataStream.Subscribe(d =>
            //{
            //    Console.WriteLine("Got data " + d.TimeStamp);

            //    db.AddData(d);
            //});

            Axis axisX = new Axis();
            //axisX.AxisLabels = new AxisLabels();
            //axisX.AxisLabels.Enabled = false;

            Axis axisY = new Axis();
            axisY.AxisMinimum = 0;
            axisY.AxisMaximum = 1;
            //axisY.AxisLabels = new AxisLabels();
            //axisY.AxisLabels.Enabled = false;

            //// Set properties of Axis
            axisX.IntervalType = IntervalTypes.Number;
            axisX.Interval = 100;
            //axisX.ValueFormatString = "MMM/d/yyyy";

            // Add Axis to AxesX collection
            Chart.AxesX.Add(axisX);
            Chart.AxesY.Add(axisY);

            DataSeries lineSeries = new DataSeries() { RenderAs = RenderAs.QuickLine, LightWeight = true };
            Chart.Series.Add(lineSeries);

            lineSeries.DataPoints = new DataPointCollection();

            var delta = Observable.Interval(TimeSpan.FromMilliseconds(1000))
                .Select(x => new Data() {Delta = random.NextDouble(), TimeStamp = DateTime.Now.Ticks, Symbol = "AAPL"})
                .ObserveOnDispatcher();

            Program.GetItemsStream(db)
                .Where(i => i.Symbol == "AAPL")
                //.Buffer(10)
                //.Buffer(TimeSpan.FromSeconds(1))
                //.Take(120)
                .ObserveOnDispatcher()
                .Finally(() =>
                {
                    deltaSub = delta.Subscribe(x =>
                    {
                        //Debug.WriteLine(x.TimeStamp + ": " + x.Delta);

                        var dataPoint = new LightDataPoint();
                        dataPoint.YValue = x.Delta;
                        //dataPoint.XValue = x.TimeStamp;

                        lineSeries.DataPoints.Add(dataPoint);
                    });
                })
                .Subscribe(a =>
                {
                    //Debug.WriteLine(a.TimeStamp + ": " + a.Delta);
                    var dataPoint = new LightDataPoint();
                    dataPoint.YValue = a.Delta;
                    //dataPoint.XValue = a.TimeStamp;

                    lineSeries.DataPoints.Add(dataPoint);

                    //foreach (var data in a)
                    //{
                    //    Debug.WriteLine(data.TimeStamp + ": " + data.Delta);

                    //    var dataPoint = new LightDataPoint();
                    //    dataPoint.YValue = data.Delta;
                    //    dataPoint.XValue = data.TimeStamp;

                    //    lineSeries.DataPoints.Add(dataPoint);
                    //}
                });


        }


        void MainWindow_Unloaded(object sender, RoutedEventArgs e)
        {
            db.Dispose();

        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            deltaSub.Dispose();

            Chart.Series[0].DataPoints.Clear();

            var delta = Observable.Interval(TimeSpan.FromMilliseconds(1000))
    .Select(x => new Data() { Delta = random.NextDouble(), TimeStamp = DateTime.Now.Ticks, Symbol = "AAPL" })
    .ObserveOnDispatcher();


            Program.GetItemsStream(db)
               .Where(i => i.Symbol == "AAPL")
               .ObserveOnDispatcher()
               .Finally(() =>
               {
                   deltaSub = delta.Subscribe(x =>
                   {
                       //Debug.WriteLine(x.TimeStamp + ": " + x.Delta);

                       var dataPoint = new LightDataPoint();
                       dataPoint.YValue = x.Delta;
                       //dataPoint.XValue = x.TimeStamp;

                       Chart.Series[0].DataPoints.Add(dataPoint);
                   });
               })
               .Subscribe(a =>
               {

                   //Debug.WriteLine(a.TimeStamp + ": " + a.Delta);
                   var dataPoint = new LightDataPoint();
                   dataPoint.YValue = a.Delta;
                   //dataPoint.XValue = a.TimeStamp;

                   Chart.Series[0].DataPoints.Add(dataPoint);

                   //foreach (var data in a)
                   //{
                   //    Debug.WriteLine(data.TimeStamp + ": " + data.Delta);

                   //    var dataPoint = new LightDataPoint();
                   //    dataPoint.YValue = data.Delta;
                   //    dataPoint.XValue = data.TimeStamp;

                   //    lineSeries.DataPoints.Add(dataPoint);
                   //}
               });
        }
    }
}
