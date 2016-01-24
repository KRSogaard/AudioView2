using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace AudioView.Web.Models
{
    public class AreYouSureModel
    {
        public string Message;
        public string RedirectAction;
        public RouteValueDictionary Redirect;
    }
}