using AppFramework.Core;
using AppFramework.Core.Models;
using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Strings;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public sealed class SearchViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public SearchViewModel ViewModel { get { return this; } }

        private ModelList<ItemModel> _Results;
        /// <summary>
        /// List of search results.
        /// </summary>
        public ModelList<ItemModel> Results
        {
            get { return _Results; }
            private set { this.SetProperty(ref _Results, value); }
        }

        private string _SearchText;
        /// <summary>
        /// Search status text.
        /// </summary>
        public string SearchText
        {
            get { return _SearchText; }
            private set { this.SetProperty(ref _SearchText, value); }
        }

        private bool _parameterChanged = false;

        #endregion Properties

        #region Constructors

        public SearchViewModel()
        {
            this.Title = Search.ButtonTextSearch;

            if (DesignMode.DesignModeEnabled)
                return;

            this.RequiresAuthorization = true;
        }

        #endregion Constructors

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            // Use any page parameters as the initial search query
            if (e.NavigationEventArgs.Parameter is string)
            {
                var param = e.NavigationEventArgs.Parameter.ToString().Trim();
                _parameterChanged = this.SearchText != param;
                this.SearchText = param;
            }
            
            await base.OnLoadStateAsync(e);
        }

        /// <summary>
        /// Refreshes the search results
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>Awaitable task.</returns>
        protected override async Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (!string.IsNullOrWhiteSpace(this.SearchText))
            {
                if (forceRefresh || this.Results == null || _parameterChanged)
                {
                    _parameterChanged = false;

                    // Show the busy status
                    this.Title = string.Format(Search.TextSearching, this.SearchText);
                    this.ShowBusyStatus(this.Title, true);

                    // Call the API to perform the search
                    this.Platform.Analytics.Event("Search", this.SearchText);
                    using (var api = new ClientApi())
                    {
                        this.Results = new ModelList<ItemModel>(await api.SearchItems(this.SearchText, ct));
                    }

                    // Update the page title
                    this.Title = string.Format(Search.TextSearchResultsCount, this.Results.Count, this.SearchText);
                }
            }
            else
            {
                // No results, clear page
                this.Results = null;
                this.Title = Search.ButtonTextSearch;
            }
        }

        #endregion Methods
    }
}