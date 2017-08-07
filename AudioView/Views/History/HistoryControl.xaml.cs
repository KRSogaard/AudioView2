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
using System.Windows.Navigation;
using System.Windows.Shapes;
using AudioView.Common.Services;

namespace AudioView.UserControls
{
    /// <summary>
    /// Interaction logic for HistoryControl.xaml
    /// </summary>
    public partial class HistoryControl : UserControl
    {
        public HistoryControl()
        {
            InitializeComponent();
        }

        private async void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var service = new UserService();
            var user = await service.Validate(username.Text, password.Password);
            if (user.Expires != null && user.Expires < DateTime.Now)
            {
                ExpiredLable.Visibility = Visibility.Visible;
                user = null;
            }
            if (user == null)
            {

                loginFailedLabel.Visibility = Visibility.Visible;
            }
            else
            {
                ExpiredLable.Visibility = Visibility.Hidden;
                GlobalContainer.CurrentUser = user;
                this.LogInGrid.Visibility = Visibility.Collapsed;
                this.contentTabControl.Visibility = Visibility.Visible;
            }
        }
    }
}
