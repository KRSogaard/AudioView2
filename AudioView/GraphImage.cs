using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AudioView
{
    public class GraphImage
    {
        private List<Tuple<DateTime, double>> seconds;
        private List<Tuple<DateTime, double>> bars;
        private bool live;
        private Color backgroundColor;
        private Color axisColor;
        private int minHeight;
        private int maxHeight;
        private int width;
        private int height;
        private double bottomMargin;
        private int leftMargin;
        private double yValue;
        private double xValue;
        private double graphSpan;
        private DateTime first;

        public GraphImage()
        {
            live = false;
            minHeight = 0;
            maxHeight = 150;
            this.leftMargin = 40;
            this.bottomMargin = 40;
        }

        public void SetSeconds(List<Tuple<DateTime, double>> seconds)
        {
            this.seconds = seconds;
        }
        public void SetBars(List<Tuple<DateTime, double>> bars)
        {
            this.bars = bars;
        }
        public void SetLive()
        {
            this.live = true;
        }

        public void SetBackgroundColor(Color color)
        {
            this.backgroundColor = color;
        }
        public void SetAxisColor(Color color)
        {
            this.axisColor = color;
        }

        public BitmapSource CreateImage(int width, int height)
        {
            this.width = width;
            this.height = height;

            this.first = this.bars.OrderBy(x => x.Item1).Select(x=>x.Item1).First();
            DateTime last = DateTime.Now;
            if (!live)
            {
                last = this.bars.OrderBy(x => x.Item1).Select(x => x.Item1).Last();
            }
            this.graphSpan = (last - first).TotalMilliseconds;

            this.xValue = calculateXPixelValue();
            this.yValue = calculateYPixelValue();

            // Initialize the WriteableBitmap with size 512x512 and set it as source of an Image control
            WriteableBitmap writeableBmp = BitmapFactory.New(width, height);
            using (writeableBmp.GetBitmapContext())
            {
                writeableBmp.Clear(this.backgroundColor);
                DrawAxis(writeableBmp);
            }

            writeableBmp.WriteTga(new FileStream("image.png", FileMode.OpenOrCreate));
            writeableBmp.Freeze();
            return writeableBmp;
        }

        private void DrawAxis(WriteableBitmap writeableBmp)
        {
            int labelRightMargin = 4;
            int axisInterval = Math.Max(1, (int)Math.Round(((maxHeight - minHeight) / 10) / 5.0) * 5);
            for (int i = minHeight; i < maxHeight; i = i + axisInterval)
            {
                var y = ConvertValueToGraph(i);

                //float[] dashValues = { 5, 2, 15, 4 };
                //Pen axisPen = new Pen(new SolidColorBrush(axisColor), 0.2);
                //axisPen.DashStyle = new DashStyle(new[] { 2.0, 2.0 }, 0);

                writeableBmp.DrawLineAa(leftMargin, y,
                                      width, y,
                                      axisColor,
                                      2);

                //var label = new Label()
                //{
                //    Content = i,
                //    Width = leftMargin - labelRightMargin,
                //    HorizontalContentAlignment = HorizontalAlignment.Right
                //};
                //this.canvas.Children.Add(label);
                //var size = GetLabelSize(label);
                //Canvas.SetLeft(label, leftMargin - labelRightMargin - size.Width);
                //Canvas.SetTop(label, y - (int)Math.Ceiling(size.Height / 2));
            }
        }

        private double calculateYPixelValue()
        {
            return Math.Ceiling((((double)this.height - (double)this.bottomMargin) / (maxHeight - minHeight)) * 100000) / 100000;
        }
        private double calculateXPixelValue()
        {
            return Math.Ceiling(((this.width - leftMargin) / graphSpan) * 100000) / 100000;
        }
        private int ConvertValueToGraph(double value)
        {
            return (int)Math.Ceiling((double)this.height - bottomMargin - (yValue * Math.Max(0, value - minHeight)));
        }
        private int ConvertTimeToGraph(DateTime time)
        {
            return (int)Math.Ceiling((time - first).TotalMilliseconds * xValue);
        }
    }
}
