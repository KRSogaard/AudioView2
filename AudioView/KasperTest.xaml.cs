using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace AudioView
{
    /// <summary>
    /// Interaction logic for KasperTest.xaml
    /// </summary>
    public partial class KasperTest : Window
    {

        public KasperTest()
        {
            InitializeComponent();

            GraphImage image = new GraphImage();

            Random rnd = new Random();
            var list = new List<Tuple<DateTime, double>>();
            for (int i = -20; i <= 0; i++)
            {
                list.Add(new Tuple<DateTime, double>(DateTime.Now.AddMinutes(i), rnd.Next(60, 150)));
            }
            image.SetBars(list);
            image.SetAxisColor(Colors.White);
            image.SetBackgroundColor(Colors.Black);

            Background.Source = image.CreateImage((int) Background.Width, (int) Background.Height);
        }
    }
}
