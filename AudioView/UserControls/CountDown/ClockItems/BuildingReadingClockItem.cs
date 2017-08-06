using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class BuildingReadingClockItem : ClockItem
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        // When a new building starts will there be no measurments for a second, which will result in N/A, just display the last measurment in that case.
        private String lastValue = "-";

        private double currentValue = 0;

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
                currentValue = Math.Round(data.LastBuilding.LAeq);
                if (Double.IsNaN(data.LastBuilding.LAeq) || currentValue < 0)
                    value = "-";
                else
                    value = ((int)currentValue).ToString();
            }


            viewModel.Value = value;
            viewModel.Unit = "dB";
            viewModel.Measurement = "LAeq";
            lastValue = value;
        }

        public override bool IsReadingOverLimit(double limit)
        {
            return currentValue >= limit;
        }
    }
}
