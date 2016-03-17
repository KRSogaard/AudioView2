using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.UserControls.CountDown;
using GalaSoft.MvvmLight.CommandWpf;
using NLog;
using Prism.Commands;

namespace AudioView.ViewModels
{
    public class LiveReadingViewModel : AudioViewCountDownViewModel
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private string _title;
        private string _readingType;

        public string Title
        {
            get
            {
                return "AudioView - "+ (this.isMajor ? "Major" : "Minor") +" - "+ _readingType + " - " + _title;
            }
            set { _title = value; OnPropertyChanged(); }
        }

        private bool _stayOnTop;
        public bool StayOnTop
        {
            get { return _stayOnTop; }
            set { _stayOnTop = value; OnPropertyChanged(); OnPropertyChanged("IsNotStayOnTop");}
        }
        public bool IsNotStayOnTop
        {
            get { return !_stayOnTop; }
        }

        public ICommand ToggleOnTop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    StayOnTop = !StayOnTop;
                });
            }
        }

        public LiveReadingViewModel(bool isMajor, TimeSpan interval, int limitDb, int mainItem, int secondItem) : 
            base(isMajor, interval, limitDb, mainItem, secondItem, true)
        {
            _readingType = ClockItems.Get.Where(x => x.Id == mainItem).Select(x => x.Name).First();
            StayOnTop = false;
            IsEnabled = true; // Always true for this control
        }
    }
}
