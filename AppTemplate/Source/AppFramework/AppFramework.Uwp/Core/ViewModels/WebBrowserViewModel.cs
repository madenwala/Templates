using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace AppFramework.Core.ViewModels
{
    public partial class WebBrowserViewModel : ViewModelBase
    {
        #region Events

        public event EventHandler RefreshRequested;
        public event EventHandler GoForwardRequested;
        public event EventHandler GoBackwardsRequested;
        public event EventHandler GoHomeRequested;
        public event EventHandler<string> NavigateToRequested;

        #endregion

        #region Properties

        public object BrowserInstance { get; set; }

        private bool _ShowNavigation;
        /// <summary>
        /// Gets whether or not the navigation command bar should be shown.
        /// </summary>
        public bool ShowNavigation
        {
            get { return _ShowNavigation; }
            protected set { this.SetProperty(ref _ShowNavigation, value); }
        }

        /// <summary>
        /// Gets or sets a flag indicating whether navigating back on the view should skip back override to allow browser back history to be navigated to on back presses.
        /// </summary>
        protected bool ForceBrowserGoBackOnNavigationBack { get; set; }

        private bool _ShowBrowser;
        /// <summary>
        /// Gets whether the browser should be visible or not.
        /// </summary>
        public bool ShowBrowser
        {
            get { return _ShowBrowser; }
            private set
            {
                if (this.SetProperty(ref _ShowBrowser, value))
                    this.BrowserRefreshCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _IsBrowserRefreshEnabled;
        public bool IsBrowserRefreshEnabled
        {
            get { return _IsBrowserRefreshEnabled; }
            private set
            {
                if (this.SetProperty(ref _IsBrowserRefreshEnabled, value))
                    this.BrowserRefreshCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _IsOpenInExternalBrowserVisible = true;
        public bool IsOpenInExternalBrowserVisible
        {
            get { return _IsOpenInExternalBrowserVisible; }
            protected set { this.SetProperty(ref _IsOpenInExternalBrowserVisible, value); }
        }

        private bool _IsSharingAllowed = true;
        public bool IsSharingAllowed
        {
            get { return _IsSharingAllowed; }
            protected set { this.SetProperty(ref _IsSharingAllowed, value); }
        }

        private string _CurrentUrl;
        public string CurrentUrl
        {
            get { return _CurrentUrl; }
            private set { this.SetProperty(ref _CurrentUrl, value); }
        }

        /// <summary>
        /// Gets a command used to refresh the current view.
        /// </summary>
        public CommandBase BrowserRefreshCommand { get; private set; }

        /// <summary>
        /// Gets a command used to take the user back to the home webpage.
        /// </summary>
        public CommandBase BrowserHomeCommand { get; private set; }

        /// <summary>
        /// Returns whether or not the browser can go back or not.
        /// </summary>
        public Func<bool> BrowserCanGoBack { get; private set; }

        /// <summary>
        /// Returns whether or not the browser can go forward or not.
        /// </summary>
        public Func<bool> BrowserCanGoForward { get; private set; }

        public bool HasNavigationFailed { get; set; }

        #endregion Properties

        #region Constructors

        public WebBrowserViewModel(bool showNavigation = true)
        {
            this.Title = Strings.WebBrowser.TextWebDefaultTitle;

            if (DesignMode.DesignModeEnabled)
                return;

            this.ShowNavigation = showNavigation;
            this.BrowserRefreshCommand = new GenericCommand<IModel>("WebViewViewModel-RefreshCommand", this.BrowserRefresh, () => this.IsBrowserRefreshEnabled);
            this.BrowserHomeCommand = new GenericCommand("WebBrowserViewModel-Home", this.BrowserGoHome, () => this.BrowserCanGoBack());

            this.SetBrowserFunctions(() => false, () => false);
        }

        #endregion Constructors

        #region Methods

        /// <summary>
        /// Initial page that should be navigated on launch of the application. 
        /// </summary>
        protected internal virtual void InitialNavigation()
        {
            if (this.ViewParameter is string)
                this.NavigateTo(this.ViewParameter.ToString());
        }

        protected override Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if(this.HasNavigationFailed)
                this.BrowserRefresh();

            return base.OnRefreshAsync(forceRefresh, ct);
        }

        protected internal override bool OnBackNavigationRequested()
        {
            // Allow the browser to tell the global navigation that it should override back navigation and instead nav back in the browser view.
            if (this.ForceBrowserGoBackOnNavigationBack == false && this.BrowserCanGoBack != null && this.BrowserCanGoBack())
            {
                this.BrowserGoBack();
                return true;
            }
            else
                return base.OnBackNavigationRequested();
        }

        protected internal override bool OnForwardNavigationRequested()
        {
            // Allow the browser to tell the global navigation that it should override forward navigation and instead nav forward in the browser view.
            if (this.BrowserCanGoForward != null && this.BrowserCanGoForward())
            {
                this.BrowserGoForward();
                return true;
            }
            else
                return base.OnForwardNavigationRequested();
        }

        /// <summary>
        /// Notify the VM that the browser is in the process of navigating to a particular page and offer the ability for it to cancel the navigation.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal bool Navigating(Uri uri)
        {
            this.HasNavigationFailed = false;
            this.ShowBusyStatus(Strings.Resources.TextLoading);
            this.ShowBrowser = false;
            this.IsBrowserRefreshEnabled = false;
            this.CurrentUrl = null;

            // True if navigation should be cancelled, False if it should continue
            return false;
        }

        /// <summary>
        /// Notify this VM that a page has been navigated to.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="title"></param>
        internal void Navigated(Uri uri, string title = null)
        {
            this.Title = title ?? Strings.Resources.TextNotApplicable;
            this.ClearStatus();
            this.ShowBrowser = true;
            this.PlatformBase.NavigationBase.GoBackCommand.RaiseCanExecuteChanged();
            this.PlatformBase.NavigationBase.GoForwardCommand.RaiseCanExecuteChanged();
            this.BrowserHomeCommand.RaiseCanExecuteChanged();
            this.IsBrowserRefreshEnabled = true;
            this.CurrentUrl = uri.ToString();
            this.OnNavigated(this.CurrentUrl);
        }

        protected virtual void OnNavigated(string url)
        {
        }

        /// <summary>
        /// Notify this VM that a navigation failure has occurred.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="exception"></param>
        /// <param name="title"></param>
        internal async void NavigationFailed(Uri uri, Exception exception, string title = null)
        {
            this.HasNavigationFailed = true;
            this.ClearStatus();
            this.Title = Strings.Resources.TextNotApplicable;
            this.ShowBrowser = false;
            this.PlatformBase.NavigationBase.GoBackCommand.RaiseCanExecuteChanged();
            this.PlatformBase.NavigationBase.GoForwardCommand.RaiseCanExecuteChanged();
            await this.HandleExceptionAsync(exception, "Error navigating to " + uri.ToString());
            this.IsBrowserRefreshEnabled = true;
            this.CurrentUrl = null;
        }

        /// <summary>
        /// Configure the VM to perform execute custom functions when the browser can go back/forward.
        /// </summary>
        /// <param name="canGoBack"></param>
        /// <param name="canGoForward"></param>
        internal void SetBrowserFunctions(Func<bool> canGoBack, Func<bool> canGoForward)
        {
            if (canGoBack != null) this.BrowserCanGoBack = canGoBack;
            if (canGoForward != null) this.BrowserCanGoForward = canGoForward;
        }

        /// <summary>
        /// Refreshes the web browser.
        /// </summary>
        protected void BrowserRefresh()
        {
            this.RefreshRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigates the web browser backwards.
        /// </summary>
        protected void BrowserGoBack()
        {
            this.GoBackwardsRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigates the web browser forward.
        /// </summary>
        protected internal void BrowserGoForward()
        {
            this.GoForwardRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigates the web browser to the home page.
        /// </summary>
        protected void BrowserGoHome()
        {
            this.GoHomeRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigate to a specific web page.
        /// </summary>
        /// <param name="url">URL to navigate to.</param>
        protected void NavigateTo(string url)
        {
            if (!string.IsNullOrEmpty(url))
                this.NavigateToRequested?.Invoke(this.BrowserInstance, url);
        }

        #endregion Methods
    }

    public partial class WebBrowserViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public WebBrowserViewModel ViewModel { get { return this; } }
    }
}

namespace AppFramework.Core.ViewModels.Designer
{
    public sealed class WebBrowserViewModel : AppFramework.Core.ViewModels.WebBrowserViewModel
    {
        public WebBrowserViewModel()
            : base()
        {
        }
    }
}