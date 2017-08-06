using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AudioView.Common.Data;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class InactiveClockItem : ClockItem
    {
        public override string Name => "Inactive";

        public override void SetValues(MeasurementItemViewModel viewModel, ClockItemData data)
        {
            viewModel.Clear();
        }

        public override bool IsReadingOverLimit(double limit)
        {
            return false;
        }
    }
}
