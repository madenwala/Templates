namespace AppFramework.Core.Commands
{
    /// <summary>
    /// Command for navigating to an internal web browser to display a webpage within the app.
    /// </summary>
    public sealed class WebViewCommand : GenericCommand<string>
    {
        #region Constructors

        /// <summary>
        /// Create an instance of the command for internal webpage browsing.
        /// </summary>
        public WebViewCommand()
            : base("WebViewCommand", PlatformCore.Core.NavigationBase.NavigateToWebView, (address) => { return address is string && !string.IsNullOrWhiteSpace(address.ToString()); })
        {
        }

        #endregion
    }

    /// <summary>
    /// Command for navigating to an external web browser to display a webpage.
    /// </summary>
    public sealed class WebBrowserCommand : GenericCommand<string>
    {
        #region Constructors

        /// <summary>
        /// Create an instance of the command for external webpage browsing.
        /// </summary>
        public WebBrowserCommand()
            : base("WebBrowserCommand", PlatformCore.Core.NavigationBase.NavigateToWebBrowser, (address) => { return address is string && !string.IsNullOrWhiteSpace(address.ToString()); })
        {
        }

        #endregion
    }
}