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
    /// Interaction logic for LiveReadingWindow.xaml
    /// </summary>
    public partial class LiveReadingWindow
    {
        public LiveReadingWindow()
        {
            InitializeComponent();
            this.Topmost = true;
        }
    }
}
