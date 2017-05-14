using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class LatestIntervalClockItem : ClockItem
    {
        public override string Name => "Previous LAeq";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastInterval == null)
            {
                viewModel.NoValue();
                return;
            }
            viewModel.Value = ((int)Math.Ceiling(data.LastInterval.LAeq)).ToString();
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
        }
    }
}
