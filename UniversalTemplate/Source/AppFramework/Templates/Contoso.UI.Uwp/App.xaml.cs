using AppFramework.Core.Services.Analytics;
using AppFramework.UI.Controls;
using Contoso.Core;
using Contoso.UI.Services;

namespace Contoso.UI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    sealed partial class App : AppFramework.UI.App
    {
        #region Constructor

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();

            // Initalize the platform object which is the singleton instance to access various services
            Platform.Current.Navigation = new NavigationManager();
            //Platform.Current.Analytics.Register(new FlurryAnalyticsService("M76D4BWBDRTWTVJZZ27P"));
            Platform.Current.Analytics.Register(new HockeyAppService("f83e8cf6e95047d5ba8dfee810a94754", "adenwala@outlook.com"));
            Platform.Current.Analytics.Register(new GoogleAnalyticsService("UA-91538532-2"));

            AdControl.DevCenterAdAppID = "7f0c824b-5c94-4cc6-b4ea-db78b7641398";
            AdControl.DevCenterAdUnitID = "11641061";
            AdControl.AdDuplexAppKey = "45758d4d-6f55-4f90-b646-fcbfc7f8bfa3";
            AdControl.AdDuplexAdUnitID = "199455";

            //this.RequestedTheme = ApplicationTheme.Dark;
        }

        #endregion

        #region Methods

        protected override void CustomizeApplicationUI()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 200));

            base.CustomizeApplicationUI();
        }

        #endregion
    }
}