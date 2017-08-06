using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class LiveLAegClockItem : ClockItem
    {
        private double currentValue = 0;
        public override string Name => "LAeq, 1s";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastReading == null)
            {
                viewModel.NoValue();
                return;
            }
            currentValue = Math.Round(data.LastReading.LAeq, 0);
            if (currentValue <= 0)
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
