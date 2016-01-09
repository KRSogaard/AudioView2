using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using AudioView.UserControls.Graph;
using AudioView.ViewModels;

namespace AudioView.UserControls
{
    public class AudioViewGraph : UserControl
    {
        private bool isEventRegistered;
        private ConcurrentDictionary<string, Size> sizeMap;
        private DateTime sizeMapCreated;
        private double maxHeight;
        private double minHeight;
        private int leftMargin;
        private int bottomMargin;
        private double yPixelValue;
        private double xPixelValue;
        private List<Tuple<DateTime, double>> secondsReading;
        private List<Tuple<DateTime, double>> intervalReadings;
        private int limit;
        private int intervalsShown;
        private DateTime latestReading;
        private DateTime leftDateTime;
        private TimeSpan interval;
        private double graphTimeSpan;
        private Canvas innerCanvas;
        private Canvas labelsCanvas;
        private double lastMinHeight;
        private double lastMaxHeight;
        private double lastActualWidth;
        private double lastActualHeight;
        public DispatcherTimer timer { get; set; }
        private Canvas canvas;

        public AudioViewGraph()
        {
            sizeMap = new ConcurrentDictionary<string, Size>();
            sizeMapCreated = DateTime.Now;

            this.MinWidth = this.leftMargin*2;
            this.MinHeight = this.bottomMargin*2;
            var b = new Border { ClipToBounds = true };
            canvas = new Canvas { RenderTransform = new TranslateTransform(0, 0) };
            b.Child = canvas;
            Content = b;

            this.timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(100); // 5 fps
            timer.Tick += Tick;
            timer.Start();

            this.SizeChanged += OnSizeChanged;
        }

        private void Tick(object sender, EventArgs eventArgs)
        {
            Draw();
        }

        /// <summary>
        /// When in custom mode, do we need to listen for events to know if we should update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="propertyChangedEventArgs"></param>
        private void ModelOnPropertyChanged(object sender, PropertyChangedEventArgs propertyChangedEventArgs)
        {
            var model = (AudioViewGraphViewModel)this.DataContext;
            if (model == null || !model.IsEnabled)
                return;

            switch (propertyChangedEventArgs.PropertyName)
            {
                case "IsCustomSpan":
                    timer.IsEnabled = !model.IsCustomSpan;
                    break;
                case "LeftDate":
                case "RightDate":
                    if (model.IsCustomSpan)
                    {
                        Draw();
                    }
                    break;
            }
        }

        /// <summary>
        /// If we are in custom mode = not auto updating, update the graph when the control is resized.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sizeChangedEventArgs"></param>
        private void OnSizeChanged(object sender, SizeChangedEventArgs sizeChangedEventArgs)
        {
            var model = (AudioViewGraphViewModel)this.DataContext;
            if (model == null || !model.IsEnabled)
                return;

            if (model.IsCustomSpan)
            {
                Draw();
            }
        }

        private void Draw()
        {
            DateTime start = DateTime.Now;

            var model = (AudioViewGraphViewModel)this.DataContext;
            if(model == null || !model.IsEnabled)
                return;

            if (!isEventRegistered)
            {
                model.PropertyChanged += ModelOnPropertyChanged;
                isEventRegistered = true;
            }

            if ((DateTime.Now - sizeMapCreated).TotalMinutes > 1)
            {
                sizeMap.Clear();
                sizeMapCreated = DateTime.Now;
            }

            // Copy over so we don't have thread problems
            this.secondsReading = model.SecondReadings.ToList();
            this.intervalReadings = model.Readings.ToList();
            this.limit = model.LimitDb;
            this.intervalsShown = model.IntervalsShown;
            this.interval = model.Interval;

            this.lastMinHeight = this.minHeight;
            this.lastMaxHeight = this.maxHeight;
            this.maxHeight = model.MaxHeight;
            this.minHeight = model.MinHeight;

            this.leftMargin = 40;
            this.bottomMargin = 40;

            if (model.IsCustomSpan)
            {
                this.latestReading = model.LeftDate;
                this.leftDateTime = model.RightDate;

                // Close the ticker as we do not need it
                timer.IsEnabled = false;
            }
            else
            {
                this.latestReading = getLastestReading();
                this.leftDateTime = calculateLeftDateTime();
            }
            this.graphTimeSpan = (latestReading - leftDateTime).TotalMilliseconds; // We need this value many times, so no need to recalculate everytime
            this.yPixelValue = calculateYPixelValue();
            this.xPixelValue = calculateXPixelValue();

            // Only redraw axies if we have too
            if (this.lastMinHeight != this.minHeight || 
                this.lastMaxHeight != this.maxHeight ||
                this.lastActualHeight != this.ActualHeight ||
                this.lastActualWidth != this.ActualWidth)
            {
                sizeMap.Clear();
                canvas.Children.Clear();
                DrawAxis();
                DrawLimit();

                this.labelsCanvas = new Canvas()
                {
                    RenderTransform = new TranslateTransform(0, 0)
                };
                this.canvas.Children.Add(labelsCanvas);

                // Create the inner canvas
                this.innerCanvas = new Canvas
                {
                    RenderTransform = new TranslateTransform(0, 0),
                    Width = Math.Max(this.leftMargin, this.ActualWidth - leftMargin),
                    Height = Math.Max(this.bottomMargin, this.ActualHeight - bottomMargin),
                    ClipToBounds = true
                };
                Canvas.SetTop(innerCanvas, 0);
                Canvas.SetLeft(innerCanvas, leftMargin);
                this.canvas.Children.Add(innerCanvas);
            }
            else
            {
                this.innerCanvas.Children.Clear();
                this.labelsCanvas.Children.Clear();
            }

            DrawBars();
            DrawSeconds();

            DateTime end = DateTime.Now;
            var label = new Label()
            {
                Content = "Render time: " + (end - start).TotalMilliseconds + " ms."
            };
            Canvas.SetLeft(label, 10);
            Canvas.SetTop(label, 10);
            this.innerCanvas.Children.Add(label);
            this.lastActualWidth = this.ActualWidth;
            this.lastActualHeight = this.ActualHeight;
        }

        private void DrawBars()
        {
            var barWidth = CalculateBarWidth();
            // Calculate the intervals
            foreach (var reading in this.intervalReadings)
            {
                int x = ConvertTimeToGraph(reading.Item1);
                int y = ConvertValueToGraph(reading.Item2);
                bool over = reading.Item2 >= this.limit;
                var bar = new Rectangle()
                {
                    Width = Math.Max(0, barWidth),
                    Height = Math.Max(0, this.ActualHeight - this.bottomMargin - y),
                    Fill = new SolidColorBrush(over ? ColorSettings.BarColorOverLimit : ColorSettings.BarColorUnderLimit),
                    Stroke = new SolidColorBrush(over ? ColorSettings.BarColorOverLimitStroke : ColorSettings.BarColorUnderLimitStroke),
                    StrokeThickness = 2,
                    Opacity = 0.75
                };
                Canvas.SetTop(bar, y);
                Canvas.SetLeft(bar, x - (int)Math.Ceiling((double)barWidth / 2.0));
                this.innerCanvas.Children.Add(bar);

                var label = new Label()
                {
                    Content = reading.Item1.ToString("HH:mm")
                };
                this.labelsCanvas.Children.Add(label);
                var size = GetLabelSize(label);
                Canvas.SetTop(label, this.ActualHeight - bottomMargin + (int)Math.Ceiling((bottomMargin / 2.0) - (size.Height / 2)));
                Canvas.SetLeft(label, leftMargin + x - (int)Math.Ceiling((double)size.Width / 2));
            }
        }

        private void DrawSeconds()
        {
            // Calculate seconds
            LineSegment first = null;
            LinkedList<LineSegment> list = new LinkedList<LineSegment>();
            for (int _i = 0; _i < secondsReading.Count; _i++)
            {
                int x = ConvertTimeToGraph(secondsReading[_i].Item1);
                int y = ConvertValueToGraph(secondsReading[_i].Item2);
                var segment = new LineSegment(new Point(x, y), true);
                if (_i == 0)
                {
                    first = segment;
                }
                else
                {
                    list.AddLast(segment);
                }
            }

            PathGeometry patGeoh = new PathGeometry();

            if (first == null)
                return;

            var figure = new PathFigure(first.Point, list, false);
            patGeoh.Figures.Add(figure);
            var path = new Path()
            {
                Data = patGeoh,
                Stroke = new SolidColorBrush(ColorSettings.LineColor),
                StrokeThickness = 1
            };
            this.innerCanvas.Children.Add(path);
        }

        private void DrawLimit()
        {
            var y = ConvertValueToGraph(this.limit);
            this.canvas.Children.Add(new Line()
            {
                X1 = leftMargin,
                Y1 = y,
                X2 = this.ActualWidth,
                Y2 = y,
                Stroke = new SolidColorBrush(ColorSettings.LimitColor),
                StrokeThickness = 2,
                StrokeDashArray = new DoubleCollection(new[] { 2.0, 2.0 })
            });
        }

        private void DrawAxis()
        {
            int labelRightMargin = 4;
            int axisInterval = (int)Math.Round(((maxHeight - minHeight) / 10) / 5.0) * 5;
            List<Tuple<Label, Point>> labels = new List<Tuple<Label, Point>>();
            for (int i = (int)Math.Ceiling(minHeight); i < maxHeight; i = i + axisInterval)
            {
                var y = ConvertValueToGraph(i);
                this.canvas.Children.Add(new Line()
                {
                    X1 = leftMargin,
                    Y1 = y,
                    X2 = this.ActualWidth,
                    Y2 = y,
                    Stroke = new SolidColorBrush(ColorSettings.AxisColor),
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection(new[] { 5.0, 5.0 })
                });

                var label = new Label()
                {
                    Content = i
                };
                this.canvas.Children.Add(label);
                var size = GetLabelSize(label);
                Canvas.SetLeft(label, leftMargin - labelRightMargin - size.Width);
                Canvas.SetTop(label, y - (int)Math.Ceiling(size.Height / 2));
            }
        }

        private double calculateYPixelValue()
        {
            return ((double)this.ActualHeight - (double)this.bottomMargin) / (maxHeight - minHeight);
        }
        private double calculateXPixelValue()
        {
            double workingWidth = this.ActualWidth - leftMargin;
            var test = (workingWidth / graphTimeSpan);
            return test;
        }

        private int ConvertValueToGraph(double value)
        {
            return (int)Math.Ceiling((double)this.ActualHeight - bottomMargin - (yPixelValue * Math.Max(0, value - minHeight)));
        }

        private int ConvertTimeToGraph(DateTime time)
        {
            return (int)Math.Ceiling((time - leftDateTime).TotalMilliseconds * xPixelValue);
        }

        private int CalculateBarWidth()
        {
            return (int)Math.Ceiling(xPixelValue * new TimeSpan(0,1,0).TotalMilliseconds * 0.5);
        }

        private DateTime calculateLeftDateTime()
        {
            return latestReading - TimeSpan.FromMilliseconds((int)this.interval.TotalMilliseconds * (this.intervalsShown + 1));
        }

        private DateTime getLastestReading()
        {
            var last = this.secondsReading.LastOrDefault();
            DateTime lastReading;
            if (last == null)
                lastReading = DateTime.Now;
            else
                lastReading = last.Item1;
            return lastReading;
        }

        private Size GetLabelSize(Label l)
        {
            var c = l.Content.ToString();
            if (!sizeMap.ContainsKey(c))
            {
                l.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                sizeMap.TryAdd(c, l.DesiredSize);
            }
            return sizeMap[c];
        }
    }
}
