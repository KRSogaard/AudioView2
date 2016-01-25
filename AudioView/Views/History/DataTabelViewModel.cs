using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common.Data;
using AudioView.ViewModels;
using NLog;
using Prism.Commands;

namespace AudioView.Views.History
{
    public class DataTabelViewModel : HistorySearchResult
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public DataTabelViewModel(Project project) : base(project)
        {
        }

        private string _title;

        public string Title
        {
            get { return "AudioView - Data Tabel - " + _title; }
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
                return new DelegateCommand(() =>
                {
                    StayOnTop = !StayOnTop;
                });
            }
        }
    }
}
