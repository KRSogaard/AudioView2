using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using AudioView.Common.Data;
using AudioView.Common.Services;
using GalaSoft.MvvmLight.CommandWpf;
using GalaSoft.MvvmLight.Threading;
using NLog;
using Prism.Commands;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class HistoryViewModel : BindableBase
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        private IDatabaseService databaseService;

        private ObservableCollection<HistorySearchResult> _searchResults;
        public ObservableCollection<HistorySearchResult> SearchResults
        {
            get { return _searchResults; }
            set { SetProperty(ref _searchResults, value); }
        }

        private HistorySearchResult _selectedSearch;
        public HistorySearchResult SelectedSearch
        {
            get { return _selectedSearch; }
            set
            {
                SetProperty(ref _selectedSearch, value);
                CanSeeDetails = SelectedSearch != null;
                if (value != null)
                {
                    SelectedSearch.OnSelected();
                }
            }
        }
        
        private bool _isSearching;
        public bool IsSearching
        {
            get { return _isSearching; }
            set { SetProperty(ref _isSearching, value); }
        }

        private string _searchName;
        public string SearchName
        {
            get { return _searchName; }
            set { SetProperty(ref _searchName, value); }
        }

        private DateTime? _searchRightDate;
        public DateTime? SearchRightDate
        {
            get { return _searchRightDate; }
            set { SetProperty(ref _searchRightDate, value); }
        }

        private DateTime? _searchLeftDate;
        public DateTime? SearchLeftDate
        {
            get { return _searchLeftDate; }
            set { SetProperty(ref _searchLeftDate, value); }
        }

        private string _searchHeader;
        public string SearchHeader
        {
            get { return _searchHeader; }
            set { SetProperty(ref _searchHeader, value); }
        }

        private ICommand _executeSearch;
        public ICommand ExecuteSearch
        {
            get
            {
                if (_executeSearch == null)
                {
                    _executeSearch = new DelegateCommand(() =>
                    {
                        PerformSearch();
                    }, 
                    () =>
                    {
                        if (SearchLeftDate != null && SearchRightDate != null && SearchRightDate < SearchLeftDate)
                            return false;
                        return true;
                    })
                    .ObservesProperty(() => SearchLeftDate)
                    .ObservesProperty(() => SearchRightDate)
                    .ObservesProperty(() => SearchName);
                }
                return _executeSearch;
            }
        }
        
        private int _selectedTab;
        public int SelectedTab
        {
            get { return _selectedTab; }
            set
            {
                SetProperty(ref _selectedTab, value);
                if (SelectedTab != 0 && SelectedSearch != null)
                {
                    SelectedSearch.LoadReadings();
                }
            }
        }

        private bool _canSeeDetails;
        public bool CanSeeDetails
        {
            get { return _canSeeDetails; }
            set { SetProperty(ref _canSeeDetails, value); }
        }

        public HistoryViewModel()
        {
            databaseService = new DatabaseService();
            SearchResults = new ObservableCollection<HistorySearchResult>();
            SelectedSearch = null;
            SearchName = null;
            PerformSearch();
        }

        public void PerformSearch()
        {
            try
            {
                logger.Debug("Searching for projects with Name=\"{0}\" Left=\"{1}\" Right=\"{2}\"", SearchName, SearchLeftDate, SearchRightDate);
                SearchHeader = "Search Settings - Searching";
                logger.Info("Starting the search!");
                databaseService.SearchProjects(SearchName, SearchLeftDate, SearchRightDate).ContinueWith((Task<IList<Project>> task) =>
                {
                    logger.Info("Search finished!");
                    var results = task.Result;
                    DispatcherHelper.CheckBeginInvokeOnUI(() =>
                    {
                        try
                        {
                            _searchResults = new ObservableCollection<HistorySearchResult>();
                            _searchResults.AddRange(results.OrderBy(x => x.Created).Select(x => new HistorySearchResult(x)));
                            OnPropertyChanged(nameof(SearchResults));

                            IsSearching = false;
                            SearchHeader = string.Format("Search Settings - {0} results", SearchResults.Count);
                            logger.Debug("Finisehd search with {0} results.", SearchResults.Count);
                        }
                        catch (Exception exp)
                        {
                            logger.Error(exp, "Search failed.");
                        }
                    });
                });
                logger.Info("Search have been started!");
            }
            catch (Exception exp)
            {
                logger.Error(exp, "Search failed. (Outter)");
            }
        }

        public void SelectedProjectDelete()
        {
            SelectedTab = 0;
            SearchResults.Remove(SelectedSearch);
        }
    }
}