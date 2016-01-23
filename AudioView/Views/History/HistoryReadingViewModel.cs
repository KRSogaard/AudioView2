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
            PropertyChanged += (sender, args) =>
            {
                logger.Trace("HistoryReadingViewModel {0} was change", args.PropertyName);
            };

            databaseService = new DatabaseService();
            this.parent = parent;
            Reading = reading;
        }

        public Reading Reading { get; set; }
    }
}