using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
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
        private DispatcherTimer timer { get; set; }
        private Canvas canvas;

        // Color settings
        private SolidColorBrush _limitColor;
        public SolidColorBrush LimitColor
        {
            get
            {
                if (_limitColor == null)
                {
                    _limitColor = new SolidColorBrush(ColorSettings.LimitColor);
                }
                return _limitColor; }
            set { _limitColor = value; }
        }
        private SolidColorBrush _barColor;
        public SolidColorBrush BarColor
        {
            get
            {
                if (_barColor == null)
                {
                    _barColor = new SolidColorBrush(ColorSettings.BarColorUnderLimit);
                }
                return _barColor;
            }
            set { _barColor = value; }
        }
        private SolidColorBrush _barWarrningColor;
        public SolidColorBrush BarWarrningColor
        {
            get
            {
                if (_barWarrningColor == null)
                {
                    _barWarrningColor = new SolidColorBrush(ColorSettings.BarColorOverLimit);
                }
                return _barWarrningColor;
            }
            set { _barWarrningColor = value; }
        }
        private SolidColorBrush _axisColor;
        public SolidColorBrush AxisColor
        {
            get
            {
                if (_axisColor == null)
                {
                    _axisColor = new SolidColorBrush(ColorSettings.AxisColor);
                }
                return _axisColor;
            }
            set { _axisColor = value; }
        }
        private SolidColorBrush _lineColor;
        public SolidColorBrush LineColor
        {
            get
            {
                if (_lineColor == null)
                {
                    _lineColor = new SolidColorBrush(ColorSettings.LineColor);
                }
                return _lineColor;
            }
            set { _lineColor = value; }
        }

        private Brush BarBorderColor
        {
            get
            {
                return new SolidColorBrush(GetColorDarker(BarColor.Color, 0.8));
            }
        }

        private Brush BarWarrningBorderColor
        {
            get
            {
                return new SolidColorBrush(GetColorDarker(BarWarrningColor.Color, 0.8));
            }
        }

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
            timer.Interval = TimeSpan.FromMilliseconds(1000); // 5 fps
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

        public void Draw()
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

            if (model.IsCustomSpan)
            {
                this.latestReading = model.RightDate;
                this.leftDateTime = model.LeftDate;

                // Close the ticker as we do not need it
                timer.IsEnabled = false;
            }
            else
            {
                this.latestReading = getLastestReading();
                if (this.latestReading + TimeSpan.FromTicks((long)(this.interval.Ticks * 1.5)) >= DateTime.Now)
                {
                    this.latestReading = DateTime.Now;
                }
                this.leftDateTime = calculateLeftDateTime();
            }

            // Copy over so we don't have thread problems
            this.secondsReading = model.SecondReadings.Where(x=>x.Item1 >= leftDateTime && x.Item1 <= latestReading).ToList();
            this.intervalReadings = model.Readings.Where(x => x.Item1 >= leftDateTime && x.Item1 <= latestReading).ToList();
            this.limit = model.LimitDb;
            this.intervalsShown = model.IntervalsShown;
            this.interval = model.Interval;

            this.lastMinHeight = this.minHeight;
            this.lastMaxHeight = this.maxHeight;
            this.maxHeight = model.MaxHeight;
            this.minHeight = model.MinHeight;

            this.leftMargin = 40;
            this.bottomMargin = 40;
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
                    RenderTransform = new TranslateTransform(0, 0),
                    Height = Math.Max(this.bottomMargin, this.ActualHeight),
                    Width = Math.Max(this.leftMargin, this.ActualWidth - leftMargin),
                    ClipToBounds = true
                };
                Canvas.SetTop(labelsCanvas, 0);
                Canvas.SetLeft(labelsCanvas, leftMargin);
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
            var totalMs = (end - start).TotalMilliseconds;
#if DEBUG
            var label = new Label()
            {
                Content = "x: " + xPixelValue + " p/ms. - y: " + yPixelValue + " p/ms. - Left: " + this.leftDateTime.ToString("hh:mm:ss.fff") + " Right: " + this.latestReading.ToString("hh:mm:ss.fff") + " - Diff: " + (latestReading - leftDateTime).TotalMilliseconds + " ms - Render time: " + totalMs + " ms. Interval: " + timer.Interval.TotalMilliseconds
            };
            Canvas.SetLeft(label, 10);
            Canvas.SetTop(label, 10);
            this.innerCanvas.Children.Add(label);
#endif
            this.lastActualWidth = this.ActualWidth;
            this.lastActualHeight = this.ActualHeight;
        }

        private void DrawBars()
        {
            var barWidth = CalculateBarWidth();

            int interval = 1;
            if (this.intervalReadings.Count >= 15)
                interval = (int)Math.Ceiling(this.intervalReadings.Count / 30.0);

            int i = 0;
            foreach (var reading in this.intervalReadings)
            {
                int x = ConvertTimeToGraph(reading.Item1);
                int y = ConvertValueToGraph(reading.Item2);
                bool over = reading.Item2 >= this.limit;
                var bar = new Rectangle()
                {
                    Width = Math.Max(0, barWidth),
                    Height = Math.Max(0, this.ActualHeight - this.bottomMargin - y),
                    Fill = over ? BarWarrningColor : BarColor,
                    Stroke = over ? BarWarrningBorderColor : BarBorderColor,
                    StrokeThickness = 2,
                    Opacity = 0.75
                };
                var offsetedX = x - (int) Math.Ceiling((double) barWidth/2.0);
                Canvas.SetTop(bar, y);
                Canvas.SetLeft(bar, offsetedX);
                this.innerCanvas.Children.Add(bar);
                
                if (this.intervalReadings.Count <= 25)
                {
                    var labelValue = new Label()
                    {
                        Content = (int)Math.Ceiling(reading.Item2),
                        FontWeight = FontWeights.Bold,
                        FontSize = 16,
                        Width = Math.Max(0, barWidth),
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };
                    Canvas.SetTop(labelValue, y);
                    Canvas.SetLeft(labelValue, offsetedX);
                    this.innerCanvas.Children.Add(labelValue);
                }

                if (i % interval == 0)
                {
                    var label = new Label()
                    {
                        Content = reading.Item1.ToString("HH:mm"),
                        Width = Math.Max(40, barWidth),
                        HorizontalContentAlignment = HorizontalAlignment.Center
                    };
                    Canvas.SetTop(label, this.ActualHeight - bottomMargin + (int)Math.Ceiling((bottomMargin / 2.0)));
                    Canvas.SetLeft(label, offsetedX);
                    this.labelsCanvas.Children.Add(label);
                }

                i++;
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
                Stroke = LineColor,
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
                Stroke = LimitColor,
                StrokeThickness = 4,
                StrokeDashArray = new DoubleCollection(new[] { 2.0, 2.0 })
            });
        }

        private void DrawAxis()
        {
            int labelRightMargin = 4;
            int axisInterval = Math.Max(1, (int)Math.Round(((maxHeight - minHeight) / 10) / 5.0) * 5);
            for (int i = (int)Math.Ceiling(minHeight); i < maxHeight; i = i + axisInterval)
            {
                var y = ConvertValueToGraph(i);
                this.canvas.Children.Add(new Line()
                {
                    X1 = leftMargin,
                    Y1 = y,
                    X2 = this.ActualWidth,
                    Y2 = y,
                    Stroke = AxisColor,
                    StrokeThickness = 0.5,
                    StrokeDashArray = new DoubleCollection(new[] { 5.0, 5.0 })
                });
                
                var label = new Label()
                {
                    Content = i,
                    Width = leftMargin - labelRightMargin,
                    HorizontalContentAlignment = HorizontalAlignment.Right,
                    FontSize = 16
                };
                this.canvas.Children.Add(label);
                var size = GetLabelSize(label);
                Canvas.SetLeft(label, leftMargin - labelRightMargin - size.Width);
                Canvas.SetTop(label, y - (int)Math.Ceiling(size.Height / 2));
            }
        }

        private double calculateYPixelValue()
        {
            return Math.Ceiling((((double)this.ActualHeight - (double)this.bottomMargin) / (maxHeight - minHeight)) * 100000) / 100000;
        }
        private double calculateXPixelValue()
        {
            return Math.Ceiling(((this.ActualWidth - leftMargin) / graphTimeSpan) * 100000) / 100000;
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
            return (int)Math.Ceiling(xPixelValue * this.interval.TotalMilliseconds * 0.5);
        }

        private DateTime calculateLeftDateTime()
        {
            return latestReading - TimeSpan.FromMilliseconds((int)this.interval.TotalMilliseconds * (this.intervalsShown + 1));
        }

        private DateTime getLastestReading()
        {
            if (this.secondsReading == null)
                return DateTime.Now;

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

        private Color GetColorDarker(Color color, double factor)
        {
            // The factor value value cannot be greater than 1 or smaller than 0.
            // Otherwise return the original colour
            if (factor < 0 || factor > 1)
                return color;

            byte r = (byte)(factor * color.R);
            byte g = (byte)(factor * color.G);
            byte b = (byte)(factor * color.B);
            return Color.FromArgb(color.A, r, g, b);
        }
    }
}
