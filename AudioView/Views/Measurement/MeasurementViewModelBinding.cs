using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common.Engine;
using AudioView.UserControls.CountDown;
using AudioView.UserControls.Graphs;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public partial class MeasurementViewModel : BindableBase, IMeterListener
    {
        public TimeSpan MinorSpan
        {
            get { return minorSpan; }
            set { SetProperty(ref minorSpan, value); }
        }
        public TimeSpan MajorSpan
        {
            get { return majorSpan; }
            set { SetProperty(ref majorSpan, value); }
        }
        public ObservableCollection<OctaveBandGraphValue> OctaveValues
        {
            get { return octaveValues; }
            set { SetProperty(ref octaveValues, value); }
        }
        public GraphViewModel MinorGraphViewModel
        {
            get { return minorGraphViewModel; }
            set { SetProperty(ref minorGraphViewModel, value); }
        }
        public GraphViewModel MajorGraphViewModel
        {
            get { return majorGraphViewModel; }
            set { SetProperty(ref majorGraphViewModel, value); }
        }

        public string Title
        {
            get { return title; }
            set { title = value; }
        }
        public AudioViewCountDownViewModel MinorClock
        {
            get { return minorClockViewModel; }
            set { SetProperty(ref minorClockViewModel, value); }
        }
        public AudioViewCountDownViewModel MajorClock
        {
            get { return majorClockViewModel; }
            set { SetProperty(ref majorClockViewModel, value); }
        }

        private ICommand _displayReadingsTabel;
        public ICommand DisplayReadingsTabel
        {
            get
            {
                if (_displayReadingsTabel == null)
                {
                    _displayReadingsTabel = new DelegateCommand(OnDisplayReadingsTabel);
                }
                return _displayReadingsTabel;
            }
        }

        private ICommand _displayReadingsGraph;
        public ICommand DisplayReadingsGraph
        {
            get
            {
                if (_displayReadingsGraph == null)
                {
                    _displayReadingsGraph = new DelegateCommand(OnDisplayReadingsGraph);
                }
                return _displayReadingsGraph;
            }
        }

        private ICommand _downloadAsCSV;
        public ICommand DownloadAsCSV
        {
            get
            {
                if (_downloadAsCSV == null)
                {
                    _downloadAsCSV = new DelegateCommand(OnDownloadAsCsv);
                }
                return _downloadAsCSV;
            }
        }
    }
}
