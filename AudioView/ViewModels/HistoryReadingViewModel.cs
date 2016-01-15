using System.Windows.Input;
using AudioView.Common.Data;
using AudioView.Common.Services;
using GalaSoft.MvvmLight.Threading;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class HistoryReadingViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();
        private IDatabaseService databaseService;
        private HistorySearchResult parent;

        public HistoryReadingViewModel(Reading reading, HistorySearchResult parent)
        {
            databaseService = new DatabaseService();
            this.parent = parent;
            Reading = reading;
        }

        public Reading Reading { get; set; }

        private ICommand _deleteReading;
        public ICommand DeleteReading
        {
            get
            {
                if (_deleteReading == null)
                {
                    _deleteReading = new DelegateCommand(() =>
                    {
                        logger.Trace("Deleting reading {0}", Reading.Id);
                        databaseService.DeleteReading(Reading.Id).ContinueWith((task) =>
                        {
                            DispatcherHelper.CheckBeginInvokeOnUI(() => {
                                                                            parent.RemoveReading(this);
                            });
                        });
                    });
                }
                return _deleteReading;
            }
        }
    }
}