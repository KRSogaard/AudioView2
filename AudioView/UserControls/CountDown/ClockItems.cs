using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.UserControls.CountDown
{
    public class ClockItems
    {
        public static IList<ClockItem> Get
        {
            get
            {
                return new List<ClockItem>()
                {
                    new ClockItem()
                    {
                        Id = -1,
                        Name = "Inactive"
                    },
                    new ClockItem()
                    {
                        Id = 0,
                        Name = "Live readings"
                    },
                    new ClockItem()
                    {
                        Id = 1,
                        Name = "Latest interval"
                    },
                    new ClockItem()
                    {
                        Id = 2,
                        Name = "Time to next interval"
                    },
                    new ClockItem()
                    {
                        Id = 3,
                        Name = "Latest building reading"
                    },
                };
            }
        } 
    }

    public class ClockItem
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
