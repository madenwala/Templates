using AppFramework.Core;
using AppFramework.Core.ViewModels;
using Microsoft.Advertising.WinRT.UI;
using System;

namespace AppFramework.UI.Views
{
    public abstract partial class ViewBase<TViewModel> where TViewModel : ViewModelBase
    {
        #region Interstitial Ads

        //Initialize the ads:
        private InterstitialAd _adInterstitialVideo;
        private InterstitialAd _adInterstitialBanner;
        bool _adInterstitialBannerReady = false;
        bool _adInterstitialVideoReady = false;

        /// <summary>
        /// Displays a banner or video interstitial ad (whichever loads first).
        /// </summary>
        public void ShowInterstitialAd()
        {
            _adInterstitialBannerReady = false;
            _adInterstitialVideoReady = false;

            //Request the ads:
            // instantiate an InterstitialAd
            _adInterstitialVideo = new InterstitialAd();
            _adInterstitialBanner = new InterstitialAd();

            // wire up all 4 events, see below for function template
            _adInterstitialVideo.AdReady += Interstitial_Video_AdReady;
            _adInterstitialVideo.ErrorOccurred += Interstitial_ErrorOccurred;
            _adInterstitialVideo.Completed += Interstitial_Completed;
            _adInterstitialVideo.Cancelled += Interstitial_Cancelled;

            _adInterstitialBanner.AdReady += Interstitial_Banner_AdReady;
            _adInterstitialBanner.ErrorOccurred += Interstitial_ErrorOccurred;
            _adInterstitialBanner.Completed += Interstitial_Completed;
            _adInterstitialBanner.Cancelled += Interstitial_Cancelled;

            // pre-fetch an ad 30-60 seconds before you need it
            _adInterstitialVideo.RequestAd(AdType.Video, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialVideoAdUnitID);
            _adInterstitialBanner.RequestAd(AdType.Display, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialBannerAdUnitID);
        }

        //write the code for the events
        private void Interstitial_Video_AdReady(object sender, object e)
        {
            // code
            if (!_adInterstitialBannerReady)
            {
                _adInterstitialVideoReady = true;
                try
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"{this.GetType().Name}.Interstitial_AdReady (Video)");
                    this.OnInterstitialAdReady(sender, _adInterstitialVideo.State);
                }
                catch (Exception ex)
                {
                    PlatformBase.CurrentCore.Logger.LogError(ex, $"Error occured while executing {this.GetType().Name}.OnInterstitialAdReady");
                }
                _adInterstitialVideo.Show();
            }
        }

        //write the code for the events
        private void Interstitial_Banner_AdReady(object sender, object e)
        {
            // code
            if (!_adInterstitialVideoReady)
            {
                _adInterstitialBannerReady = true;
                try
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"{this.GetType().Name}.Interstitial_AdReady (Banner)");
                    this.OnInterstitialAdReady(sender, _adInterstitialVideo.State);
                }
                catch (Exception ex)
                {
                    PlatformBase.CurrentCore.Logger.LogError(ex, $"Error occured while executing {this.GetType().Name}.OnInterstitialAdReady");
                }
                _adInterstitialBanner.Show();
            }
        }

        private void Interstitial_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"{this.GetType().Name}.Interstitial_ErrorOccurred {e.ErrorCode} - {e.ErrorMessage}");
                this.OnInterstitialAdErrorOccurred(sender, _adInterstitialVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"Error occured while executing {this.GetType().Name}.OnInterstitialAdErrorOccurred");
            }
        }

        private void Interstitial_Completed(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"{this.GetType().Name}.Interstitial_Completed");
                this.OnInterstitialAdCompleted(sender, _adInterstitialVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"Error occured while executing {this.GetType().Name}.OnInterstitialAdCompleted");
            }
        }

        private void Interstitial_Cancelled(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"{this.GetType().Name}.Interstitial_Cancelled");
                this.OnInterstitialAdCancelled(sender, _adInterstitialVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"Error occured while executing {this.GetType().Name}.OnInterstitialAdCancelled");
            }
        }

        protected virtual void OnInterstitialAdReady(object sender, InterstitialAdState state)
        {
        }

        protected virtual void OnInterstitialAdCompleted(object sender, InterstitialAdState state)
        {
        }

        protected virtual void OnInterstitialAdErrorOccurred(object sender, InterstitialAdState state)
        {
        }

        protected virtual void OnInterstitialAdCancelled(object sender, InterstitialAdState state)
        {
        }

        #endregion
    }
}
