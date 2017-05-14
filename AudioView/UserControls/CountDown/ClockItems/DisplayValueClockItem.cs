using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class DisplayValueClockItem : ClockItem
    {
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
            viewModel.Value = data.LastReading.GetValue(displayValue).ToString("0.0");
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
    }
}
