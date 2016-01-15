using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.Graph;
using GalaSoft.MvvmLight.CommandWpf;

namespace AudioView.ViewModels
{
    public class GraphReadingViewModel : AudioViewGraphViewModel
    {
        private string _title;

        public string Title
        {
            get { return "AudioView - Graph Readings - " + _title; }
            set { _title = value; OnPropertyChanged(); }
        }

        private bool _stayOnTop;
        public bool StayOnTop
        {
            get { return _stayOnTop; }
            set { _stayOnTop = value; OnPropertyChanged(); OnPropertyChanged("IsNotStayOnTop"); }
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

        public GraphReadingViewModel(bool isMajor, int intervalsShown, int limitDb, TimeSpan interval, int minHeight, int maxHeight) : 
            base(isMajor, intervalsShown, limitDb, interval, minHeight, maxHeight)
        {
            StayOnTop = false;
            IsEnabled = true; // Always true for this control
        }
    }
}
