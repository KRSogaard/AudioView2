using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.UserControls.CountDown;
using GalaSoft.MvvmLight.CommandWpf;

namespace AudioView.ViewModels
{
    public class LiveReadingViewModel : AudioViewCountDownViewModel
    {
        private string _title;

        public string Title
        {
            get { return "AudioView - Live Readings - " + _title; }
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
                return new RelayCommand(() =>
                {
                    StayOnTop = !StayOnTop;
                });
            }
        }

        public LiveReadingViewModel(bool isMajor, TimeSpan interval, int limitDb, int mainItem, int secondItem) : 
            base(isMajor, interval, limitDb, mainItem, secondItem)
        {
            StayOnTop = false;
            IsEnabled = true; // Always true for this control
        }
    }
}
