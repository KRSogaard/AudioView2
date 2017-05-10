using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class BuildingReadingClockItem : ClockItem
    {
        public override string Name => "Latest building reading";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            if (data.LastBuilding == null)
            {
                viewModel.NoValue();
                return;
            }
            viewModel.Value = ((int)Math.Ceiling(data.LastBuilding.LAeq)).ToString();
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
        }
    }
}
