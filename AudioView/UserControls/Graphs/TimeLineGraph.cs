using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioView.UserControls.Graphs
{
    public partial class TimeLineGraph : UserControl
    {
        #region Settings
        private static readonly int LeftMargin = 40; //px
        private static readonly int buttomMargin = 40; //px
        private static readonly int buttomTextMargin = 5; //px
        private static readonly double axisFontSize = 12;
        private static readonly Typeface fontTypeFace = new Typeface("Arial");
        private static readonly int LeftTextMargin = 5;
        private static readonly double axisLinePenSize = 0.5;
        private static readonly DashStyle axisLineDashStyle = new DashStyle(new[] { 5.0, 5.0 }, 0);
        private static readonly double limitLinePenSize = 2.0;
        private static readonly DashStyle limitLineDashStyle = new DashStyle(new[] { 5.0, 5.0 }, 0);
        #endregion

        #region Binding Stuff
        public static readonly DependencyProperty ReadingLimitProperty =
            DependencyProperty.Register(
            "ReadingLimit", typeof(double), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                90.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ReadingBoundMaxProperty =
            DependencyProperty.Register(
            "ReadingBoundMax", typeof(int?), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty ReadingBoundMinProperty =
            DependencyProperty.Register(
            "ReadingBoundMin", typeof(int?), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TimeEndProperty =
            DependencyProperty.Register(
            "TimeEnd", typeof(DateTime?), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty TimeSpanProperty =
            DependencyProperty.Register(
            "TimeSpan", typeof(TimeSpan), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new TimeSpan(0, 2, 0),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarValuesProperty =
            DependencyProperty.Register(
            "BarValues", typeof(ObservableCollection<Tuple<DateTime, double>>), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new ObservableCollection<Tuple<DateTime, double>>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarSizeProperty =
            DependencyProperty.Register(
            "BarSize", typeof(TimeSpan), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new TimeSpan(0, 0, 10),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty LineValuesProperty =
            DependencyProperty.Register(
            "LineValues", typeof(ObservableCollection<Tuple<DateTime, double>>), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new ObservableCollection<Tuple<DateTime, double>>(),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        // Colors
        public static readonly DependencyProperty LineValuesPenColorProperty =
            DependencyProperty.Register(
            "LineValuesPenColor", typeof(Brush), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.Yellow),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty LineValuesPenSizeProperty =
            DependencyProperty.Register(
            "LineValuesPenSize", typeof(double), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                1.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarPenProperty =
            DependencyProperty.Register(
            "BarBorderPen", typeof(Pen), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new Pen(new SolidColorBrush(Colors.CadetBlue), 2),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarFillBrushProperty =
            DependencyProperty.Register(
            "BarFillBrush", typeof(SolidColorBrush), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.CornflowerBlue),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarOverPenProperty =
            DependencyProperty.Register(
            "BarOverBorderPen", typeof(Pen), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new Pen(new SolidColorBrush(Colors.DarkRed), 2),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty BarOverFillBrushProperty =
            DependencyProperty.Register(
            "BarOverFillBrush", typeof(SolidColorBrush), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.Crimson),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarLabelBrushProperty =
            DependencyProperty.Register(
            "BarLabelBrush", typeof(SolidColorBrush), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.WhiteSmoke),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty AxisPenProperty =
            DependencyProperty.Register(
            "AxisPen", typeof(Pen), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new Pen(new SolidColorBrush(Colors.WhiteSmoke), axisLinePenSize)
                {
                    DashStyle = axisLineDashStyle
                },
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty AxisBrushProperty =
            DependencyProperty.Register(
            "AxisBrush", typeof(SolidColorBrush), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.WhiteSmoke),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty LimitPenProperty =
            DependencyProperty.Register(
            "LimitPen", typeof(Pen), typeof(TimeLineGraph),
            new FrameworkPropertyMetadata(
                new Pen(new SolidColorBrush(Colors.Red), limitLinePenSize)
                {
                    DashStyle = limitLineDashStyle
                },
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        private bool shouldRerender = false;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == BarValuesProperty)
            {
                if (e.OldValue != null)
                {
                    ((ObservableCollection<Tuple<DateTime, double>>)e.OldValue)
                        .CollectionChanged -= OnBarsCollectionChanged;
                }
                if (e.NewValue != null)
                {
                    ((ObservableCollection<Tuple<DateTime, double>>) e.NewValue)
                        .CollectionChanged += OnBarsCollectionChanged;
                }
            }
            else if (e.Property == LineValuesProperty)
            {
                if (e.OldValue != null)
                {
                    ((ObservableCollection<Tuple<DateTime, double>>)e.NewValue)
                        .CollectionChanged -= OnLineCollectionChanged;
                }
                ((ObservableCollection<Tuple<DateTime, double>>)e.NewValue)
                    .CollectionChanged += OnLineCollectionChanged;
            }

            switch (e.Property.Name)
            {
                case nameof(TimeEnd):
                case nameof(TimeSpan):
                case nameof(BarSize):
                case nameof(LineValues):
                case nameof(BarValues):
                case nameof(ReadingBoundMax):
                case nameof(ReadingBoundMin):
                    shouldRerender = true;
                    break;
            }
        }

        private void OnBarsCollectionChanged(object sender, NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            shouldRerender = true;
        }

        private void OnLineCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            shouldRerender = true;
        }

        public double ReadingLimit
        {
            get { return (double)GetValue(ReadingLimitProperty); }
            set { SetValue(ReadingLimitProperty, value); }
        }
        public DateTime? TimeEnd
        {
            get { return (DateTime?)GetValue(TimeEndProperty); }
            set { SetValue(TimeEndProperty, value); }
        }
        public TimeSpan TimeSpan
        {
            get { return (TimeSpan)GetValue(TimeSpanProperty); }
            set { SetValue(TimeSpanProperty, value); }
        }
        public ObservableCollection<Tuple<DateTime, double>> BarValues
        {
            get { return (ObservableCollection<Tuple<DateTime, double>>)GetValue(BarValuesProperty); }
            set { SetValue(BarValuesProperty, value); }
        }
        public TimeSpan BarSize
        {
            get { return (TimeSpan)GetValue(BarSizeProperty); }
            set { SetValue(BarSizeProperty, value); }
        }
        public ObservableCollection<Tuple<DateTime, double>> LineValues
        {
            get { return (ObservableCollection<Tuple<DateTime, double>>)GetValue(LineValuesProperty); }
            set { SetValue(LineValuesProperty, value); }
        }
        public int? ReadingBoundMax
        {
            get { return (int?)GetValue(ReadingBoundMaxProperty); }
            set { SetValue(ReadingBoundMaxProperty, value); }
        }
        public int? ReadingBoundMin
        {
            get { return (int?)GetValue(ReadingBoundMinProperty); }
            set { SetValue(ReadingBoundMinProperty, value); }
        }

        // Colors
        public Brush LineValuesPenColor
        {
            get { return (Brush)GetValue(LineValuesPenColorProperty); }
            set { SetValue(LineValuesPenColorProperty, value); }
        }
        public double LineValuesPenSize
        {
            get { return (double)GetValue(LineValuesPenSizeProperty); }
            set { SetValue(LineValuesPenSizeProperty, value); }
        }
        public Pen BarBorderPen
        {
            get { return (Pen)GetValue(BarPenProperty); }
            set { SetValue(BarPenProperty, value); }
        }
        public SolidColorBrush BarFillBrush
        {
            get { return (SolidColorBrush)GetValue(BarFillBrushProperty); }
            set { SetValue(BarFillBrushProperty, value); }
        }
        public Pen BarOverBorderPen
        {
            get { return (Pen)GetValue(BarOverPenProperty); }
            set { SetValue(BarOverPenProperty, value); }
        }
        public SolidColorBrush BarOverFillBrush
        {
            get { return (SolidColorBrush)GetValue(BarOverFillBrushProperty); }
            set { SetValue(BarOverFillBrushProperty, value); }
        }
        public SolidColorBrush BarLabelBrush
        {
            get { return (SolidColorBrush)GetValue(BarLabelBrushProperty); }
            set { SetValue(BarLabelBrushProperty, value); }
        }
        public SolidColorBrush AxisBrush
        {
            get { return (SolidColorBrush)GetValue(AxisBrushProperty); }
            set { SetValue(AxisBrushProperty, value); }
        }
        public Pen AxisPen
        {
            get { return (Pen)GetValue(AxisPenProperty); }
            set { SetValue(AxisPenProperty, value); }
        }
        public Pen LimitPen
        {
            get { return (Pen)GetValue(LimitPenProperty); }
            set { SetValue(LimitPenProperty, value); }
        }


        public TimeLineGraph()
        {
           // InitializeComponent();

            // We user a time to keep updates collected to a max 25 fps
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            // If there is not TimeEnd must we continusly update
            if (!shouldRerender && TimeEnd != null)
                return;
            shouldRerender = false;
            InvalidateVisual();
        }


        private double lastLabelRightBound;
        private double xValuePrMs;
        private double yValuePr;
        private Tuple<double, double> graphBounds;
        private DateTime rightTime;
        private DateTime leftTime;
        private double workingWidth;
        private double workingHeight;
        private List<Tuple<Point, FormattedText>> barTexts; 
        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            barTexts = new List<Tuple<Point, FormattedText>>();
            lastLabelRightBound = 0;
            workingWidth = this.ActualWidth - LeftMargin;
            workingHeight = this.ActualHeight - buttomMargin;
            graphBounds = getGraphBound();

            rightTime = TimeEnd ?? DateTime.Now;
            leftTime = rightTime - TimeSpan;
            xValuePrMs = workingWidth / TimeSpan.TotalMilliseconds;
            yValuePr = workingHeight / (graphBounds.Item2 - graphBounds.Item1);

            drawAxis(drawingContext);
            drawBars(drawingContext);
            drawLines(drawingContext);
            drawLimit(drawingContext);
            drawBarNumbers(drawingContext);
        }

        private Tuple<double, double> getGraphBound()
        {
            double min = int.MaxValue;
            double max = int.MinValue;
            if (ReadingBoundMin != null)
            {
                min = (double)ReadingBoundMin;
            }
            if (ReadingBoundMax != null)
            {
                max = (double)ReadingBoundMax;
            }

            if (ReadingBoundMin == null || ReadingBoundMax == null)
            {
                if ((BarValues == null || BarValues.Count == 0) && 
                    (LineValues == null || LineValues.Count == 0))
                {
                    return new Tuple<double, double>(0, 150);
                }

                List<Tuple<DateTime, double>> values = new List<Tuple<DateTime, double>>();
                if (BarValues == null)
                {
                    values.AddRange(LineValues);
                } else if (LineValues == null)
                {
                    values.AddRange(BarValues);
                }
                else
                {
                    values.AddRange(BarValues.Union(LineValues));
                }

                foreach (var source in values)
                {
                    if (ReadingBoundMin == null && source.Item2 < min)
                    {
                        min = source.Item2;
                    }
                    if (ReadingBoundMax == null && source.Item2 > max)
                    {
                        max = source.Item2;
                    }
                }
            }
            if (Math.Abs(max - min) < 10)
            {
                return new Tuple<double, double>(Math.Max(60, min - 10), Math.Min(150, max + 10));
            }
            if (min > ReadingLimit)
            {
                min = ReadingLimit - 10;
            }
            if (max < ReadingLimit)
            {
                max = ReadingLimit;
            }
            return new Tuple<double, double>(min, max);
        }

        private void drawAxis(DrawingContext drawingContext)
        {
            int axisInterval = Math.Max(1, (int)Math.Round(((graphBounds.Item2 - graphBounds.Item1) / 10) / 5.0) * 5);
            for (int i = (int)Math.Ceiling(graphBounds.Item1); i < graphBounds.Item2; i = i + axisInterval)
            {
                var y = getY(i);
                drawingContext.DrawLine(
                    AxisPen,
                    new Point(LeftMargin, y),
                    new Point(this.ActualWidth, y));

                var text = new FormattedText(i.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    fontTypeFace, axisFontSize, AxisBrush);
                drawingContext.DrawText(text, new Point
                    (LeftMargin - text.Width - LeftTextMargin,
                    y - (text.Height / 2)));
            }
        }

        private void drawBars(DrawingContext drawingContext)
        {
            if (BarValues == null)
            {
                return;
            }

            var barWidth = BarSize.TotalMilliseconds * xValuePrMs / 2;
            var barWidthHalf = barWidth / 2;
            var barLeft = leftTime - (BarSize);
            foreach (var barValue in BarValues.Where(x => x.Item1 >= barLeft && x.Item1 <= rightTime + BarSize))
            {
                var x = getX(barValue.Item1);
                var y = getY(barValue.Item2);

                var leftEdge = x - barWidthHalf;
                var rightEdge = x + barWidthHalf;
                if (rightEdge <= LeftMargin)
                {
                    // Out of bound, skip
                    continue;
                }

                if (leftEdge <= LeftMargin)
                {
                    var tempBarWidth = Math.Max(1, barWidth - (LeftMargin - leftEdge));
                    drawBar(drawingContext,
                        barValue.Item1.ToString("HH:mm:ss"),
                        new Point(x, y),
                        tempBarWidth,
                        new Point(LeftMargin, y),
                        barValue.Item2);
                }
                else
                {
                    drawBar(drawingContext,
                        barValue.Item1.ToString("HH:mm:ss"),
                        new Point(x, y),
                        barWidth,
                        new Point(x - barWidth / 2, y),
                        barValue.Item2);
                }
            }
        }

        private void drawBarNumbers(DrawingContext drawingContext)
        {
            foreach (var barText in barTexts)
            {

                drawingContext.DrawText(barText.Item2, barText.Item1);
            }
        }

        private void drawBar(DrawingContext drawingContext, string text, Point location, double barWidth, Point barLocation, double reading)
        {
            var barHeight = Math.Max(0, workingHeight - barLocation.Y);
            drawingContext.DrawRectangle(reading >= ReadingLimit ? BarOverFillBrush : BarFillBrush,
                                        reading >= ReadingLimit ? BarOverBorderPen : BarBorderPen,
                                        new Rect(new Point(barLocation.X, barLocation.Y),
                                                    new Size(barWidth, barHeight)));

            var _text = new FormattedText(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                fontTypeFace,
                axisFontSize,
                BarLabelBrush);

            var textLeftBound = location.X - _text.Width / 2;
            if (textLeftBound > LeftMargin && textLeftBound > lastLabelRightBound)
            {
                drawingContext.DrawText(_text, new Point(textLeftBound, workingHeight + buttomTextMargin));
                lastLabelRightBound = location.X + _text.Width / 2;
            }

            // Draw the bar value
            _text = new FormattedText(Math.Round(reading, 2).ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                fontTypeFace,
                axisFontSize,
                BarLabelBrush);

            textLeftBound = location.X - _text.Width / 2;
            if (textLeftBound > LeftMargin && barWidth > _text.Width)
            {
                if (barHeight > _text.Height*2)
                {
                    barTexts.Add(new Tuple<Point, FormattedText>(new Point(textLeftBound, barLocation.Y + _text.Height / 2), _text));
                }
                else
                {
                    barTexts.Add(new Tuple<Point, FormattedText>(new Point(textLeftBound, barLocation.Y - _text.Height * 1.5), _text));
                }
            }
        }

        public void drawLines(DrawingContext drawingContext)
        {
            if (LineValues == null)
            {
                return;
            }

            for (int i = 1; i < LineValues.Count; i++)
            {
                if (LineValues[i].Item1 < leftTime - (BarSize - BarSize) ||
                    LineValues[i].Item1 > rightTime
                   )
                {
                    continue;
                }

                var prev = new Point(
                        getX(LineValues[i - 1].Item1),
                        getY(LineValues[i - 1].Item2)
                    );
                var current = new Point(
                    getX(LineValues[i].Item1),
                    getY(LineValues[i].Item2)
                    );

                if (current.X <= LeftMargin)
                {
                    continue;
                }

                if (prev.X < LeftMargin)
                {
                    // Caluclate the angel
                    //var d = Math.Sqrt(Math.Pow(current.Y - prev.Y, 2) + Math.Pow(current.X - prev.X, 2));
                    //var a = Math.Asin((current.Y - prev.Y)/d);

                    //var newY = (current.X - LeftMargin) / Math.Tan(a); 
                    prev.X = LeftMargin;
                    //prev.Y = newY;
                }

                drawingContext.DrawLine(new Pen(LineValuesPenColor, LineValuesPenSize), prev, current);
            }
        }

        public void drawLimit(DrawingContext drawingContext)
        {
            var y = getY(ReadingLimit);
            drawingContext.DrawLine(LimitPen,
                new Point(LeftMargin, y), new Point(this.ActualWidth, y));
        }

        private double getY(double value)
        {
            value = Math.Min(graphBounds.Item2, value);
            value = Math.Max(value, graphBounds.Item1);
            var v = value - graphBounds.Item1;
            return workingHeight - v * yValuePr;
        }
        private double getX(DateTime time)
        {
            var v = rightTime - time;
            return workingWidth - v.TotalMilliseconds * xValuePrMs;
        }
    }
}
