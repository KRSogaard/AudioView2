using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown.ClockItems
{
    public class ClockItemsFactory
    {
        private ClockItemsFactory()
        {
        }

        public static List<ClockItem> AllClockItems = new List<ClockItem>()
        {
            new InactiveClockItem(),
            new LiveLAegClockItem(),
            new TimeToIntervalClockItem(),
            new LatestIntervalClockItem(),
            new BuildingReadingClockItem()
        };
    }
}
