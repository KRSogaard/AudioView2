using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioView.Common.Data
{
    [Serializable]
    public class User
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
        public DateTime? Expires { get; set; }
    }
}
