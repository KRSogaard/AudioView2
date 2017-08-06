using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common;
using NLog;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class DisplayValueClockItem : ClockItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private double colorByValue = 0;
        private string displayValue;

        public DisplayValueClockItem(string displayValue)
        {
            this.displayValue = displayValue;
        }

        public override string Name => "Display Value Clock item";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastReading == null)
            {
                viewModel.NoValue();
                return;
            }

            // Use building LAeq to color by for Octave values
            colorByValue = data.LastReading.GetValue(displayValue);
            if (colorByValue <= 0)
                viewModel.Value = "-";
            else
                viewModel.Value = colorByValue.ToString("0.0");
            viewModel.Unit = "dB";
            viewModel.Measurement = GetMeasurement();
        }

        private String GetMeasurement()
        {
            string[] split = displayValue.Split(new[] {'-'});
            string key = split[split.Length - 1];
            bool isHz = key.Contains("Hz");
            key = key.Replace("Hz", "").Replace("_", ".");
            if (isHz)
            {
                key = key + " Hz";
            }

            if (displayValue.StartsWith("1-1"))
            {
                key = "1/1 " + key;
            } else if (displayValue.StartsWith("1-3"))
            {
                key = "1/3 " + key;
            }

            return key;
        }

        public override bool IsReadingOverLimit(double limit)
        {
            double limitOffset = DecibelHelper.GetLimitOffSet(displayValue);
            return colorByValue >= limitOffset + limit;
        }
    }
}
