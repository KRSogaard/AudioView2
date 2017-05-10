using System;
using System.Data.Entity.Core.Metadata.Edm;

namespace AudioView.Common
{
    public class MeasurementSettings
    {
        public TimeSpan MinorInterval { get; set; }
        public TimeSpan MajorInterval { get; set; }
        public int BarsDisplayed { get; set; }
        public int MinorDBLimit { get; set; }
        public int MajorDBLimit { get; set; }
        public int GraphUpperBound { get; set; }
        public int GraphLowerBound { get; set; }
        public Type MinorClockMainItem { get; set; }
        public Type MinorClockSecondaryItem { get; set; }
        public Type MajorClockMainItem { get; set; }
        public Type MajorClockSecondaryItem { get; set; }
        public string ProjectName { get; set; }
        public string ProjectNumber { get; set; }
        public int Port { get; set; }
        public bool IsLocal { get; set; }

        public void MeasurementViewModel()
        {
            BarsDisplayed = 15;
        }
    }
}