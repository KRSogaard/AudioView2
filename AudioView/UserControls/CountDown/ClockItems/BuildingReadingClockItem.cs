using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class BuildingReadingClockItem : ClockItem
    {
        // When a new building starts will there be no measurments for a second, which will result in N/A, just display the last measurment in that case.
        private String lastValue;

        public override string Name => "Building LAeq";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            // Default to last recorded value
            String value = lastValue;

            if (data.LastBuilding == null && lastValue == null)
            {
                viewModel.NoValue();
                return;
            }
            // If we have a value populate it
            if (data.LastBuilding != null)
            {
                value = ((int)Math.Ceiling(data.LastReading.LAeq)).ToString();
            }


            viewModel.Value = value;
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
            lastValue = value;
        }
    }
}
