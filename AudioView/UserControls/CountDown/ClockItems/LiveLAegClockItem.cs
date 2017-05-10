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
        public override string Name => "Live LAeq";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastReading == null)
            {
                viewModel.NoValue();
                return;
            }
            viewModel.Value = ((int)Math.Ceiling(data.LastReading.LAeq)).ToString();
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
        }
    }
}
