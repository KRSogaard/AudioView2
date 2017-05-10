using System;
using AudioView.Common.Data;

namespace AudioView.UserControls.CountDown
{
    public class ClockItemData
    {
        public ReadingData LastReading { get; set; }
        public ReadingData LastInterval { get; set; }
        public ReadingData LastBuilding { get; set; }
        public DateTime NextReadingTime { get; set; }
    }
}