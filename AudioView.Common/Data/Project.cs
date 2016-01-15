using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.Common.Data
{
    public class Project
    {
        public Guid Id { get; set; }
        public TimeSpan MinorInterval { get; set; }
        public TimeSpan MajorInterval { get; set; }
        public int DBLimit { get; set; }
        public string Name { get; set; }
        public DateTime Created { get; set; }
        public int Readings { get; set; }
    }
}
