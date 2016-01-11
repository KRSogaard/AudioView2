using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using AudioView.Common.Data;
using AudioView.Common.Services;
using Prism.Mvvm;

namespace AudioView.ViewModels
{
    public class HistoryViewModel : BindableBase
    {
        private IDatabaseService databaseService;

        private ObservableCollection<HistorySearchResult> _searchResults;
        public ObservableCollection<HistorySearchResult> SearchResults
        {
            get { return _searchResults; }
            set { SetProperty(ref _searchResults, value); }
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

        public HistoryViewModel()
        {
            databaseService = new DatabaseService();
            SearchResults = new ObservableCollection<HistorySearchResult>();
            SearchName = null;

            PerformSearch();
        }
        
        public async void PerformSearch()
        {
            try
            {
                databaseService.SearchProjects(SearchName, SearchLeftDate, SearchRightDate).ContinueWith((Task<IList<Project>> task) =>
                {
                    var results = task.Result;
                    Execute.OnUIThread(() =>
                    {
                        SearchResults.Clear();
                        SearchResults.AddRange(results.OrderBy(x => x.Created).Select(x => new HistorySearchResult(x)));
                        IsSearching = false;
                    });
                });
            }
            catch (Exception exp)
            {
                
            }
        }
    }

    public class HistorySearchResult : BindableBase
    {
        private Project project;

        private string _projectName;
        public string ProjectName
        {
            get { return _projectName; }
            set { SetProperty(ref _projectName, value); }
        }

        public HistorySearchResult(Project project)
        {
            this.project = project;
            this.ProjectName = project.Name;
        }
    }
}