using AppFramework.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    /// <summary>
    /// Ad control:  https://msdn.microsoft.com/en-us/windows/uwp/monetize/supported-ad-sizes-for-banner-ads
    /// </summary>
    public sealed partial class AdControl : UserControl
    {
        public static string DevCenterAdAppID { get; set; }

        public static string DevCenterAdUnitID { get; set; }

        public static string AdDuplexAppKey { get; set; }

        public static string AdDuplexAdUnitID { get; set; }

        public AdControl()
        {
            this.InitializeComponent();

            devCenterAd.ApplicationId = DevCenterAdAppID;
            devCenterAd.AdUnitId = DevCenterAdUnitID;

            adDuplex.AppKey = AdDuplexAppKey;
            adDuplex.AdUnitId = AdDuplexAdUnitID;

#if DEBUG
            //adDuplex.IsTest = true;

            // Test mode values https://msdn.microsoft.com/en-us/windows/uwp/monetize/test-mode-values
            devCenterAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
            devCenterAd.AdUnitId = "10865270";
#endif
        }

        private void DevCenterAdControl_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            PlatformBase.Current.Logger.Log(LogLevels.Error, $"DevCenterAdControl_ErrorOccurred: {e.ErrorCode} - {e.ErrorMessage}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("ErrorCode", e.ErrorCode.ToString());
            dic.Add("ErrorMessage", e.ErrorMessage);
            PlatformBase.Current.Analytics.Event("DevCenterAdControl_ErrorOccurred", dic);
            devCenterAd.Visibility = Visibility.Collapsed;
            adDuplex.Visibility = Visibility.Visible;
        }

        private void DevCenterAdControl_AdRefreshed(object sender, RoutedEventArgs e)
        {
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"DevCenter Ad control refreshed");
            devCenterAd.Visibility = Visibility.Visible;
        }

        private void AdDuplex_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            PlatformBase.Current.Logger.Log(LogLevels.Error, $"AdDuplex_AdLoadingError: {e.Error.Message}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Error", e.Error.ToString());
            dic.Add("ErrorMessage", e.Error.Message);
            PlatformBase.Current.Analytics.Event("AdDuplex_AdLoadingError", dic);
            adDuplex.Visibility = Visibility.Collapsed;
            devCenterAd.Visibility = Visibility.Visible;
            try
            {
                if (!devCenterAd.IsAutoRefreshEnabled && this.Visibility == Visibility.Visible)
                    devCenterAd.Refresh();
            }
            catch { }
        }

        private void AdDuplex_AdClick(object sender, AdDuplex.Banners.Models.AdClickEventArgs e)
        {
            PlatformBase.Current.Analytics.Event("AdDuplex_AdClick");
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"AdDuplex_AdClick");
        }

        private void adDuplex_NoAd(object sender, AdDuplex.Common.Models.NoAdEventArgs args)
        {
            PlatformBase.Current.Analytics.Event("AdDuplex_NoAd");
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"AdDuplex_NoAd");
            adDuplex.Visibility = Visibility.Collapsed;
            devCenterAd.Visibility = Visibility.Visible;
            try
            {
                if (!devCenterAd.IsAutoRefreshEnabled && this.Visibility == Visibility.Visible)
                    devCenterAd.Refresh();
            }
            catch { }
        }

        private void adDuplex_AdLoaded(object sender, AdDuplex.Banners.Models.BannerAdLoadedEventArgs args)
        {
            devCenterAd.Visibility = Visibility.Visible;
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"AdDuplex Ad control loaded");
        }

        private void adDuplex_AdCovered(object sender, AdDuplex.Banners.Core.AdCoveredEventArgs args)
        {
            PlatformBase.Current.Logger.Log(LogLevels.Information, $"AdDuplex_AdCovered");
        }
    }
}