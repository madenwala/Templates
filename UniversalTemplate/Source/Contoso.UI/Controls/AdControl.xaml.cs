using Contoso.Core;
using Contoso.Core.Services;
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
            Platform.Current.Logger.Log(LogLevels.Error, $"DevCenterAdControl_ErrorOccurred: {e.ErrorCode} - {e.ErrorMessage}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("ErrorCode", e.ErrorCode.ToString());
            dic.Add("ErrorMessage", e.ErrorMessage);
            Platform.Current.Analytics.Event("DevCenterAdControl_ErrorOccurred", dic);
            devCenterAd.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            adDuplex.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void DevCenterAdControl_AdRefreshed(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            Platform.Current.Logger.Log(LogLevels.Information, $"DevCenter Ad control refreshed");
            devCenterAd.Visibility = Windows.UI.Xaml.Visibility.Visible;
        }

        private void AdDuplex_AdLoadingError(object sender, AdDuplex.Common.Models.AdLoadingErrorEventArgs e)
        {
            Platform.Current.Logger.Log(LogLevels.Error, $"AdDuplex_AdLoadingError: {e.Error.Message}");
            var dic = new System.Collections.Generic.Dictionary<string, string>();
            dic.Add("Error", e.Error.ToString());
            dic.Add("ErrorMessage", e.Error.Message);
            Platform.Current.Analytics.Event("AdDuplex_AdLoadingError", dic);
            adDuplex.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            devCenterAd.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                if (!devCenterAd.IsAutoRefreshEnabled && this.Visibility == Windows.UI.Xaml.Visibility.Visible)
                    devCenterAd.Refresh();
            }
            catch { }
        }

        private void AdDuplex_AdClick(object sender, AdDuplex.Banners.Models.AdClickEventArgs e)
        {
            Platform.Current.Analytics.Event("AdDuplex_AdClick");
            Platform.Current.Logger.Log(LogLevels.Information, $"AdDuplex_AdClick");
        }

        private void adDuplex_NoAd(object sender, AdDuplex.Common.Models.NoAdEventArgs args)
        {
            Platform.Current.Analytics.Event("AdDuplex_NoAd");
            Platform.Current.Logger.Log(LogLevels.Information, $"AdDuplex_NoAd");
            adDuplex.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            devCenterAd.Visibility = Windows.UI.Xaml.Visibility.Visible;
            try
            {
                if (!devCenterAd.IsAutoRefreshEnabled && this.Visibility == Windows.UI.Xaml.Visibility.Visible)
                    devCenterAd.Refresh();
            }
            catch { }
        }

        private void adDuplex_AdLoaded(object sender, AdDuplex.Banners.Models.BannerAdLoadedEventArgs args)
        {
            devCenterAd.Visibility = Windows.UI.Xaml.Visibility.Visible;
            Platform.Current.Logger.Log(LogLevels.Information, $"AdDuplex Ad control loaded");
        }

        private void adDuplex_AdCovered(object sender, AdDuplex.Banners.Core.AdCoveredEventArgs args)
        {
            Platform.Current.Logger.Log(LogLevels.Information, $"AdDuplex_AdCovered");
        }
    }
}