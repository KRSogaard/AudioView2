﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace AudioView.UserControls.Graphs
{
    public class OctaveBandGraph : UserControl
    {
        private static readonly int LeftMargin = 40; //px
        private static readonly int buttomMargin = 10; //px
        private static readonly int buttomTextMargin = 5; //px
        private static readonly double axisFontSize = 12;
        private static readonly Typeface FontTypeFace = new Typeface("Arial");
        private static readonly int LeftTextMargin = 5;
        private static readonly double AxisLinePenSize = 0.5;
        private static readonly DashStyle AxisLineDashStyle = new DashStyle(new[] { 5.0, 5.0 }, 0);

        #region Bindings

        public static readonly DependencyProperty ReadingBoundMaxProperty =
            DependencyProperty.Register(
                "ReadingBoundMax", typeof (int?), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ReadingBoundMinProperty =
            DependencyProperty.Register(
                "ReadingBoundMin", typeof (int?), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    null,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty ReadingLimitProperty =
            DependencyProperty.Register(
            "ReadingLimit", typeof(double), typeof(OctaveBandGraph),
            new FrameworkPropertyMetadata(
                90.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarValuesProperty =
            DependencyProperty.Register(
                "BarValues", typeof (ObservableCollection<double>), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    new ObservableCollection<double>(),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));


        public static readonly DependencyProperty BarPenProperty =
            DependencyProperty.Register(
                "BarBorderPen", typeof (Pen), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    new Pen(new SolidColorBrush(Colors.CadetBlue), 2),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarFillBrushProperty =
            DependencyProperty.Register(
                "BarFillBrush", typeof (SolidColorBrush), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.CornflowerBlue),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarOverPenProperty =
            DependencyProperty.Register(
                "BarOverBorderPen", typeof (Pen), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    new Pen(new SolidColorBrush(Colors.DarkRed), 2),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty BarOverFillBrushProperty =
            DependencyProperty.Register(
                "BarOverFillBrush", typeof (SolidColorBrush), typeof (OctaveBandGraph),
                new FrameworkPropertyMetadata(
                    new SolidColorBrush(Colors.Crimson),
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public static readonly DependencyProperty AxisPenProperty =
            DependencyProperty.Register(
            "AxisPen", typeof(Pen), typeof(OctaveBandGraph),
            new FrameworkPropertyMetadata(
                new Pen(new SolidColorBrush(Colors.WhiteSmoke), AxisLinePenSize)
                {
                    DashStyle = AxisLineDashStyle
                },
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty AxisBrushProperty =
            DependencyProperty.Register(
            "AxisBrush", typeof(SolidColorBrush), typeof(OctaveBandGraph),
            new FrameworkPropertyMetadata(
                new SolidColorBrush(Colors.WhiteSmoke),
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        public static readonly DependencyProperty DisplayAxisProperty =
            DependencyProperty.Register(
            "DisplayAxis", typeof(bool), typeof(OctaveBandGraph),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
        #endregion

        #region Peroperties
        public double ReadingLimit
        {
            get { return (double)GetValue(ReadingLimitProperty); }
            set { SetValue(ReadingLimitProperty, value); }
        }
        public ObservableCollection<double> BarValues
        {
            get { return (ObservableCollection<double>) GetValue(BarValuesProperty); }
            set { SetValue(BarValuesProperty, value); }
        }

        public int? ReadingBoundMax
        {
            get { return (int?) GetValue(ReadingBoundMaxProperty); }
            set { SetValue(ReadingBoundMaxProperty, value); }
        }

        public int? ReadingBoundMin
        {
            get { return (int?) GetValue(ReadingBoundMinProperty); }
            set { SetValue(ReadingBoundMinProperty, value); }
        }

        public Pen BarBorderPen
        {
            get { return (Pen) GetValue(BarPenProperty); }
            set { SetValue(BarPenProperty, value); }
        }

        public SolidColorBrush BarFillBrush
        {
            get { return (SolidColorBrush) GetValue(BarFillBrushProperty); }
            set { SetValue(BarFillBrushProperty, value); }
        }

        public Pen BarOverBorderPen
        {
            get { return (Pen) GetValue(BarOverPenProperty); }
            set { SetValue(BarOverPenProperty, value); }
        }

        public SolidColorBrush BarOverFillBrush
        {
            get { return (SolidColorBrush) GetValue(BarOverFillBrushProperty); }
            set { SetValue(BarOverFillBrushProperty, value); }
        }

        public Pen AxisPen
        {
            get { return (Pen)GetValue(AxisPenProperty); }
            set { SetValue(AxisPenProperty, value); }
        }
        public SolidColorBrush AxisBrush
        {
            get { return (SolidColorBrush)GetValue(AxisBrushProperty); }
            set { SetValue(AxisBrushProperty, value); }
        }
        public bool DisplayAxis
        {
            get { return (bool)GetValue(DisplayAxisProperty); }
            set { SetValue(DisplayAxisProperty, value); }
        }
        #endregion

        private bool shouldRender = false;

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == BarValuesProperty)
            {
                if (e.OldValue != null)
                {
                    ((ObservableCollection<double>) e.OldValue)
                        .CollectionChanged -= OnBarsCollectionChanged;
                }
                if (e.NewValue != null)
                {
                    ((ObservableCollection<double>) e.NewValue)
                        .CollectionChanged += OnBarsCollectionChanged;
                }
            }

            switch (e.Property.Name)
            {
                case nameof(BarValues):
                case nameof(ReadingBoundMax):
                case nameof(ReadingBoundMin):
                case nameof(BarBorderPen):
                case nameof(BarFillBrush):
                case nameof(BarOverBorderPen):
                case nameof(BarOverFillBrush):
                    shouldRender = true;
                    break;
            }
        }

        private void OnBarsCollectionChanged(object sender,
            NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
        {
            shouldRender = true;
        }

        public OctaveBandGraph()
        {
            // We user a time to keep updates collected to a max 25 fps
            DispatcherTimer timer = new DispatcherTimer(DispatcherPriority.Background);
            timer.Interval = TimeSpan.FromMilliseconds(100);
            timer.Tick += TimerOnTick;
            timer.Start();
        }

        private void TimerOnTick(object sender, EventArgs eventArgs)
        {
            // If there is not TimeEnd must we continusly update
            if (!shouldRender)
                return;
            shouldRender = false;
            InvalidateVisual();
        }

        private Tuple<double, double> graphBounds;
        private double borderWith = 2;
        private double yValuePr = 1;
        private double spaceProcentage = 0.05;
        private double workingWidth;
        private double workingHeight;

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (this.BarValues == null)
            {
                return;
            }

            graphBounds = getGraphBound();

            double yOffset = 0;

            if (DisplayAxis)
            {
                workingWidth = this.ActualWidth - LeftMargin;
                workingHeight = this.ActualHeight - buttomMargin;
                yOffset = buttomMargin;
            }
            else
            {
                workingWidth = this.ActualWidth;
                workingHeight = this.ActualHeight;
            }

            double spacePrBar = workingWidth / (double)this.BarValues.Count;
            spacePrBar = spacePrBar - (2*borderWith);
            yValuePr = workingHeight / (graphBounds.Item2 - graphBounds.Item1);

            if (DisplayAxis)
            {
                drawAxis(drawingContext);
            }

            double x1 = borderWith;

            if (DisplayAxis)
            {
                x1 += LeftMargin;
            }

            foreach (var barValue in BarValues)
            {
                x1 += borderWith;
                var y = getY(barValue);
                var barHeight = Math.Max(0, workingHeight - y);
                drawingContext.DrawRectangle(barValue >= ReadingLimit ? BarOverFillBrush : BarFillBrush,
                                            barValue >= ReadingLimit ? BarOverBorderPen : BarBorderPen,
                                            new Rect(new Point(x1, this.ActualHeight - barHeight - yOffset),
                                                     new Size(spacePrBar, barHeight)));
                x1 += spacePrBar + borderWith;
            }
        }
        private double getY(double value)
        {
            value = Math.Min(graphBounds.Item2, value);
            value = Math.Max(value, graphBounds.Item1);
            var v = value - graphBounds.Item1;
            return this.ActualHeight - v * yValuePr;
        }

        private Tuple<double, double> getGraphBound()
        {
            double min = int.MaxValue;
            double max = int.MinValue;
            
            if (ReadingBoundMin != null)
            {
                min = (double) ReadingBoundMin;
            }
            if (ReadingBoundMax != null)
            {
                max = (double)ReadingBoundMax;
            }

            if (ReadingBoundMin == null || ReadingBoundMax == null)
            {
                double newMin = double.MaxValue;
                double newMax = double.MinValue;

                foreach (var barValue in BarValues)
                {
                    if (barValue < newMin)
                    {
                        newMin = barValue;
                    }
                    if (barValue > newMax)
                    {
                        newMax = barValue;
                    }
                }

                if (ReadingBoundMin == null)
                {
                    min = newMin;
                }
                if (ReadingBoundMax == null)
                {
                    max = newMax;
                }
            }

            return new Tuple<double, double>(min, max);
        }

        private void drawAxis(DrawingContext drawingContext)
        {
            int axisInterval = Math.Max(1, (int)Math.Round(((graphBounds.Item2 - graphBounds.Item1) / 10) / 5.0) * 5);
            for (int i = (int)Math.Ceiling(graphBounds.Item1); i < graphBounds.Item2; i = i + axisInterval)
            {
                var y = getY(i) - buttomMargin;
                drawingContext.DrawLine(
                    AxisPen, 
                    new Point(LeftMargin, y),
                    new Point(this.ActualWidth, y));

                var text = new FormattedText(i.ToString(), CultureInfo.CurrentCulture, FlowDirection.LeftToRight,
                    FontTypeFace, axisFontSize, AxisBrush);
                drawingContext.DrawText(text, new Point
                    (LeftMargin - text.Width - LeftTextMargin,
                    y - (text.Height / 2)));
            }
        }
    }
}
