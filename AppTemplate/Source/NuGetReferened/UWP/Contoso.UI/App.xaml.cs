using AppFramework.Core.Services.Analytics;
using AppFramework.UI.Controls;
using Contoso.Core;
using Contoso.UI.Services;
using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;

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
            Platform.Current.Analytics.Register(new FlurryAnalyticsService("M76D4BWBDRTWTVJZZ27P"));
            //Platform.Current.Analytics.Register(new HockeyAppService("f83e8cf6e95047d5ba8dfee810a94754", "adenwala@outlook.com"));
            Platform.Current.Analytics.Register(new GoogleAnalyticsService("UA-91538532-2"));

            AdControl.DevCenterAdAppID = "9nblggh5k9hj";
            AdControl.DevCenterAdUnitID = "11641061";
            AdControl.DevCenterInterstitialVideoAdUnitID = "11683186";
            AdControl.DevCenterInterstitialBannerAdUnitID = "11683185";

            AdControl.AdDuplexAppKey = "45758d4d-6f55-4f90-b646-fcbfc7f8bfa3";
            AdControl.AdDuplexAdUnitID = "199455";

            //this.RequestedTheme = ApplicationTheme.Dark;
        }

        #endregion

        #region Methods

        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            base.OnLaunched(e);

            var t = Windows.System.Threading.ThreadPool.RunAsync(async (o) =>
            {
                try
                {
                    // Install the VCD. Since there's no simple way to test that the VCD has been imported, or that it's your most recent
                    // version, it's not unreasonable to do this upon app load.
                    var vcd = await Package.Current.InstalledLocation.GetFileAsync(@"Resources\VCD.xml");
                    await Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstallCommandDefinitionsFromStorageFileAsync(vcd);
                }
                catch (Exception ex)
                {
                    Platform.Current.Logger.LogError(ex, "Installing voice commands failed!");
                }
            });
        }

        protected override void OnCustomizeApplicationUI()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.UI.ViewManagement.ApplicationView"))
                Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SetPreferredMinSize(new Windows.Foundation.Size(320, 200));
        }

        #endregion
    }
}