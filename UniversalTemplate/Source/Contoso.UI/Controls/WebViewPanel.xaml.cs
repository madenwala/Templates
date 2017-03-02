using Contoso.Core;
using Contoso.Core.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    public abstract class WebViewPanelBase : ViewControlBase<WebBrowserViewModel>
    {
    }

    public sealed partial class WebViewPanel : WebViewPanelBase, IViewScrollToTop
    {
        #region Constructors

        public WebViewPanel()
        {
            this.InitializeComponent();
            this.DataContextChanged += (sender, args) => this.SetCurrentViewModel(this.DataContext as WebBrowserViewModel);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Configures a WebBrowserViewModel instance to get notified of WebView control events.
        /// </summary>
        /// <param name="vm"></param>
        private void SetCurrentViewModel(WebBrowserViewModel vm)
        {
            if (this.ViewModel != null)
            {
                this.ViewModel.GoHomeRequested -= ViewModel_GoHomeRequested;
                this.ViewModel.RefreshRequested -= ViewModel_RefreshRequested;
                this.ViewModel.GoForwardRequested -= ViewModel_GoForwardRequested;
                this.ViewModel.GoBackwardsRequested -= ViewModel_GoBackwardsRequested;
                this.ViewModel.NavigateToRequested -= ViewModel_NavigateToRequested;
                this.ViewModel.BrowserInstance = null;

                webView.NavigationStarting -= webView_Navigating;
                webView.NavigationCompleted -= webView_Navigated;

                while (webView.CanGoBack)
                    webView.GoBack();
            }

            this.ViewModel = vm;

            if (this.ViewModel != null)
            {
                webView.NavigationStarting += webView_Navigating;
                webView.NavigationCompleted += webView_Navigated;

                this.ViewModel.BrowserInstance = webView;
                this.ViewModel.SetBrowserFunctions(() => webView.CanGoBack, () => webView.CanGoForward);
                this.ViewModel.GoHomeRequested += ViewModel_GoHomeRequested;
                this.ViewModel.RefreshRequested += ViewModel_RefreshRequested;
                this.ViewModel.GoForwardRequested += ViewModel_GoForwardRequested;
                this.ViewModel.GoBackwardsRequested += ViewModel_GoBackwardsRequested;
                this.ViewModel.NavigateToRequested += ViewModel_NavigateToRequested;

                this.ViewModel.InitialNavigation();
            }
        }

        public void ScrollToTop()
        {
            ScrollToTopHelper.ScrollToTop(webView);
        }

        #endregion

        #region Events

        private void webView_Navigating(WebView webView, WebViewNavigationStartingEventArgs e)
        {
            if (this.ViewModel != null)
                e.Cancel = this.ViewModel.Navigating(e.Uri);
        }

        private void webView_Navigated(WebView webView, WebViewNavigationCompletedEventArgs e)
        {
            if (this.ViewModel == null)
                return;

            if (e.IsSuccess)
                this.ViewModel.Navigated(e.Uri, webView.DocumentTitle);
            else
                this.ViewModel.NavigationFailed(e.Uri, new Exception(e.WebErrorStatus.ToString()), webView.DocumentTitle);
        }

        private void ViewModel_NavigateToRequested(object sender, string e)
        {
            var webView = sender as WebView;
            webView?.Navigate(new Uri(e, UriKind.Absolute));
        }

        private void ViewModel_RefreshRequested(object sender, EventArgs e)
        {
            var webView = sender as WebView;
            webView?.Refresh();
        }

        private void ViewModel_GoHomeRequested(object sender, EventArgs e)
        {
            var webView = sender as WebView;
            while (webView?.CanGoBack == true)
                webView.GoBack();
        }

        private void ViewModel_GoBackwardsRequested(object sender, EventArgs e)
        {
            var webView = sender as WebView;
            if (webView?.CanGoBack == true)
                webView.GoBack();
        }

        private void ViewModel_GoForwardRequested(object sender, EventArgs e)
        {
            var webView = sender as WebView;
            if (webView?.CanGoForward == true)
                webView.GoForward();
        }

        #endregion
    }
}