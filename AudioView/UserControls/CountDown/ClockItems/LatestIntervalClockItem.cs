using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class LatestIntervalClockItem : ClockItem
    {
        private double currentValue = 0;
        public override string Name => "Previous LAeq";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastInterval == null)
            {
                viewModel.NoValue();
                return;
            }
            currentValue = Math.Round(data.LastInterval.LAeq, 0);
            if (currentValue < 0)
                viewModel.Value = "-";
            else
                viewModel.Value = ((int)currentValue).ToString();
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
        }

        public override bool IsReadingOverLimit(double limit)
        {
            return currentValue >= limit;
        }
    }
}
