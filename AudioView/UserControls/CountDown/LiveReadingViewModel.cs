using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.CountDown.ClockItems;
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

        public LiveReadingViewModel(bool isMajor, TimeSpan interval, int limitDb, Type mainItem, Type secondItem, bool showArch) : 
            base(isMajor, interval, limitDb, mainItem, secondItem, showArch)
        {
            _readingType = ClockItemsFactory.AllClockItems.Where(x => x.GetType() == mainItem).Select(x => x.Name).First();
            StayOnTop = false;
            IsEnabled = true; // Always true for this control
        }
    }
}
