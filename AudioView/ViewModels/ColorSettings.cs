using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AudioView.ViewModels
{
    public class ColorSettings
    {
        public static Color AxisColor = Color.FromRgb(205, 205, 205);
        public static Color LineColor = Color.FromRgb(240, 195, 15);
        public static Color LimitColor = Color.FromRgb(170, 16, 10);
        public static Color BarColorUnderLimit = Color.FromRgb(41, 127, 184);
        public static Color BarColorOverLimit = Color.FromRgb(230, 76, 60);
        public static Color BarColorUnderLimitStroke = Color.FromRgb(21, 107, 164);
        public static Color BarColorOverLimitStroke = Color.FromRgb(210, 56, 40);
    }
}
