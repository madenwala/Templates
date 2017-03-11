using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Services;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public partial class SearchViewModel : ViewModelBase
    {
        #region Properties

        private ModelList<ItemModel> _Results;
        /// <summary>
        /// List of search results.
        /// </summary>
        public ModelList<ItemModel> Results
        {
            get { return _Results; }
            protected set { this.SetProperty(ref _Results, value); }
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
            this.Title = Strings.Search.ButtonTextSearch;

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
                    this.Title = string.Format(Strings.Search.TextSearching, this.SearchText);
                    this.ShowBusyStatus(this.Title, true);

                    // Call the API to perform the search
                    this.Platform.Analytics.Event("Search", this.SearchText);
                    using (var api = new ClientApi())
                    {
                        this.Results = new ModelList<ItemModel>(await api.SearchItems(this.SearchText, ct));
                    }

                    // Update the page title
                    this.Title = string.Format(Strings.Search.TextSearchResultsCount, this.Results.Count, this.SearchText);
                }
            }
            else
            {
                // No results, clear page
                this.Results = null;
                this.Title = Strings.Search.ButtonTextSearch;
            }
        }

        #endregion Methods
    }

    public partial class SearchViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public SearchViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class SearchViewModel : Contoso.Core.ViewModels.SearchViewModel
    {
        public SearchViewModel()
        {
            this.Results = new ModelList<ItemModel>();
            this.Results.Add(new ItemModel()
            {
                ID = "0",
                LineOne = "Mohammed",
                LineTwo = "Adenwala",
                LineThree = "hello world!"
            });

            this.Results.Add(new ItemModel() { ID = "1", LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu" });
            this.Results.Add(new ItemModel() { ID = "2", LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus" });
        }
    }
}