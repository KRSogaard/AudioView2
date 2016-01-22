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
        public int MinorClockMainItemId { get; set; }
        public int MinorClockSecondaryItemId { get; set; }
        public int MajorClockMainItemId { get; set; }
        public int MajorClockSecondaryItemId { get; set; }
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