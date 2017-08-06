using System.Collections.Generic;
using AudioView.UserControls.CountDown.ClockItems;

namespace AudioView.UserControls.CountDown
{
    public abstract class ClockItem
    {
        public abstract string Name { get; }
        public abstract void SetValues(MeasurementItemViewModel viewModel, ClockItemData data);

        public override string ToString()
        {
            return Name;
        }

        public abstract bool IsReadingOverLimit(double limit);
    }
}