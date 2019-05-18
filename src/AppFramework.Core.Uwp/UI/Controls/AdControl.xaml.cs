using AppFramework.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    /// <summary>
    /// Ad control:  https://msdn.microsoft.com/en-us/windows/uwp/monetize/supported-ad-sizes-for-banner-ads
    /// </summary>
    public sealed partial class AdControl : UserControl
    {
        public static string DevCenterAdAppID { get; set; }

        public static string DevCenterAdUnitID { get; set; }

        public static string DevCenterInterstitialBannerAdUnitID { get; set; }

        public static string DevCenterInterstitialVideoAdUnitID { get; set; }

        public static string AdDuplexAppKey { get; set; }

        public static string AdDuplexAdUnitID { get; set; }

        public AdControl()
        {
            this.InitializeComponent();

            this.IsTabStop = false;

            if (!string.IsNullOrEmpty(DevCenterAdAppID)) devCenterAd.ApplicationId = DevCenterAdAppID;
            if (!string.IsNullOrEmpty(DevCenterAdUnitID)) devCenterAd.AdUnitId = DevCenterAdUnitID;

            if (!string.IsNullOrEmpty(AdDuplexAppKey)) adDuplex.AppKey = AdDuplexAppKey;
            if (!string.IsNullOrEmpty(AdDuplexAdUnitID)) adDuplex.AdUnitId = AdDuplexAdUnitID;

            if (BasePlatform.IsDebugMode)
            {
                //adDuplex.IsTest = true;

                // Test mode values https://msdn.microsoft.com/en-us/windows/uwp/monetize/test-mode-values
                devCenterAd.ApplicationId = "3f83fe91-d6be-434d-a0ae-7351c5a997f1";
                devCenterAd.AdUnitId = "10865270";
            }
        }

        private void DevCenterAdControl_ErrorOccurred(object sender, Microsoft.Advertising.WinRT.UI.AdErrorEventArgs e)
        {
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Error, $"DevCenterAdControl_ErrorOccurred: {e.ErrorCode} - {e.ErrorMessage}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("ErrorCode", e.ErrorCode.ToString());
            dic.Add("ErrorMessage", e.ErrorMessage);
            BasePlatform.CurrentCore.Analytics.Event("DevCenterAdControl_ErrorOccurred", dic);
            devCenterAd.Visibility = Visibility.Collapsed;
            adDuplex.Visibility = Visibility.Visible;
        }

        private void DevCenterAdControl_AdRefreshed(object sender, RoutedEventArgs e)
        {
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, $"DevCenter Ad control refreshed");
            devCenterAd.Visibility = Visibility.Visible;
        }

        private void AdDuplex_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Error, $"AdDuplex_AdLoadingError: {e.Error.Message}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Error", e.Error.ToString());
            dic.Add("ErrorMessage", e.Error.Message);
            BasePlatform.CurrentCore.Analytics.Event("AdDuplex_AdLoadingError", dic);
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
            BasePlatform.CurrentCore.Analytics.Event("AdDuplex_AdClick");
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, $"AdDuplex_AdClick");
        }

        private void adDuplex_NoAd(object sender, AdDuplex.Common.Models.NoAdEventArgs args)
        {
            BasePlatform.CurrentCore.Analytics.Event("AdDuplex_NoAd");
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, $"AdDuplex_NoAd");
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
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, $"AdDuplex Ad control loaded");
        }

        private void adDuplex_AdCovered(object sender, AdDuplex.Banners.Core.AdCoveredEventArgs args)
        {
            BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, $"AdDuplex_AdCovered");
        }
    }
}