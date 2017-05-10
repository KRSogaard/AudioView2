using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class LatestIntervalClockItem : ClockItem
    {
        public override string Name => "Latest interval";
        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            viewModel.Value = (data.NextReadingTime - DateTime.Now).ToString(@"mm\:ss", null);
            viewModel.Unit = "";
            viewModel.Measurement = "";
        }
    }
}
