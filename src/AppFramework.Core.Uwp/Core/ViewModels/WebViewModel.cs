using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Controls;

namespace AppFramework.Core.ViewModels
{
    public abstract class WebViewModelBase : BaseViewModel
    {
        #region Events

        public event EventHandler RefreshRequested;
        public event EventHandler GoForwardRequested;
        public event EventHandler GoBackwardsRequested;
        public event EventHandler GoHomeRequested;
        public event EventHandler<string> NavigateToRequested;

        #endregion

        #region Properties

        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public WebViewModelBase ViewModel { get { return this; } }

        public WebViewArguments Args { get; private set; }

        public override string Title
        {
            get => !string.IsNullOrEmpty(this.Args.Title)? this.Args.Title : base.Title;
            protected set => base.Title = value;
        }

        public object BrowserInstance { get; set; }

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

        public WebViewModelBase(WebViewArguments args)
        {
            this.Title = Strings.WebBrowser.TextWebDefaultTitle;

            if (DesignMode.DesignModeEnabled)
                return;

            this.CurrentUrl = args.WebAddress;
            this.Args = args;
            this.BrowserRefreshCommand = new GenericCommand<IModel>("WebViewViewModel-RefreshCommand", this.BrowserRefresh, () => this.IsBrowserRefreshEnabled);
            this.BrowserHomeCommand = new GenericCommand("WebBrowserViewModel-Home", this.BrowserGoHome, () => this.BrowserCanGoBack());

            this.SetBrowserFunctions(() => false, () => false);
        }

        #endregion Constructors

        #region Methods

        #region Browser Management

        /// <summary>
        /// Initial page that should be navigated on launch of the application. 
        /// </summary>
        internal void InitialNavigation()
        {
            if (this.ViewParameter is string url)
                this.NavigateTo(url);
            else if (this.ViewParameter is WebViewArguments args)
                this.NavigateTo(args.WebAddress);
            else if (!string.IsNullOrEmpty(this.Args.WebAddress))
                this.NavigateTo(this.Args.WebAddress);
        }

        protected sealed override Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (this.HasNavigationFailed)
                this.BrowserRefresh();

            return base.OnRefreshAsync(forceRefresh, ct);
        }

        protected internal sealed override bool OnBackNavigationRequested()
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

        protected internal sealed override bool OnForwardNavigationRequested()
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
            this.ShowBusyStatus(Strings.Resources.TextLoading, true);
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
        private void BrowserGoBack()
        {
            this.GoBackwardsRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigates the web browser forward.
        /// </summary>
        internal void BrowserGoForward()
        {
            this.GoForwardRequested?.Invoke(this.BrowserInstance, new EventArgs());
        }

        /// <summary>
        /// Navigates the web browser to the home page.
        /// </summary>
        private void BrowserGoHome()
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

        #endregion 

        #region Browser Manipulation

        protected async Task HideCssClassesAsync(params string[] classNames)
        {
            var wv = this.BrowserInstance as WebView;
            if (wv != null && classNames != null)
            {
                foreach (var className in classNames)
                {
                    try
                    {
                        // https://www.w3schools.com/jsref/met_element_getelementsbytagname.asp
                        await wv.InvokeScriptAsync("eval", new string[] { "var x = document.getElementsByClassName('" + className + "'); if(x) {x[0].style.display = 'none';}" });
                    }
                    catch (Exception ex)
                    {
                        PlatformBase.CurrentCore.Logger.LogError(ex, $"{this.GetType().Name} -- could not hide class '{className}' in the WebView via InvokeScriptAsync.");
                    }
                }
            }
        }

        protected async Task HideElementIdsAsync(params string[] ids)
        {
            var wv = this.BrowserInstance as WebView;
            if (wv != null && ids != null)
            {
                foreach (var id in ids)
                {
                    try
                    {
                        // https://www.w3schools.com/jsref/met_element_getelementsbytagname.asp
                        await wv.InvokeScriptAsync("eval", new string[] { "var x = document.getElementById('" + id + "'); if(x) {x[0].style.display = 'none';}" });
                    }
                    catch (Exception ex)
                    {
                        PlatformBase.CurrentCore.Logger.LogError(ex, $"{this.GetType().Name} -- could not hide element ID '{id}' in the WebView via InvokeScriptAsync.");
                    }
                }
            }
        }

        #endregion

        #endregion Methods
    }
}