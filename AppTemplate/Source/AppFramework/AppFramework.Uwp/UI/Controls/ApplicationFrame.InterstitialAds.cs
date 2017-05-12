using AppFramework.Core;
using Microsoft.Advertising.WinRT.UI;
using System;

namespace AppFramework.UI.Controls
{
    public partial class ApplicationFrame
    {
        #region Interstitial Ads

        #region Variables

        //Initialize the ads:
        private InterstitialAd _interstitialAdVideo;
        private InterstitialAd _interstitialAdBanner;
        bool _showInterstitialAdWhenReady = false;

        #endregion

        #region Methods

        private void InitializeInterstitialAd()
        {
            if (Controls.AdControl.DevCenterInterstitialVideoAdUnitID != null)
            {
                _interstitialAdVideo = new InterstitialAd();
                _interstitialAdVideo.AdReady += Interstitial_Video_AdReady;
                _interstitialAdVideo.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdVideo.Completed += Interstitial_Completed;
                _interstitialAdVideo.Cancelled += Interstitial_Cancelled;

                this.RequestInterstitialAdVideo();
            }

            if (Controls.AdControl.DevCenterInterstitialBannerAdUnitID != null)
            {
                _interstitialAdBanner = new InterstitialAd();
                _interstitialAdBanner.AdReady += Interstitial_Banner_AdReady;
                _interstitialAdBanner.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdBanner.Completed += Interstitial_Completed;
                _interstitialAdBanner.Cancelled += Interstitial_Cancelled;

                this.RequestInterstitialAdBanner();
            }
        }

        /// <summary>
        /// Displays a banner or video interstitial ad (whichever loads first).
        /// </summary>
        public void ShowInterstitialAd()
        {
            _showInterstitialAdWhenReady = false;

            if (_interstitialAdVideo?.State == InterstitialAdState.Ready)
                this.ShowInterstitialAdVideo();
            else if (_interstitialAdBanner?.State == InterstitialAdState.Ready)
                this.ShowInterstitialAdBanner();
            else
                _showInterstitialAdWhenReady = true;
        }

        private void ShowInterstitialAdVideo()
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - ShowInterstitialAdVideo");
                this.OnInterstitialAdReady(_interstitialAdVideo, _interstitialAdVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdReady");
            }
            finally
            {
                _interstitialAdVideo.Show();
            }
        }

        private void ShowInterstitialAdBanner()
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - ShowInterstitialAdBanner");
                this.OnInterstitialAdReady(_interstitialAdBanner, _interstitialAdBanner.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdReady");
            }
            finally
            {
                _interstitialAdBanner.Show();
            }
        }

        private void RequestInterstititalAd()
        {
            this.RequestInterstitialAdVideo();
            this.RequestInterstitialAdBanner();
        }

        private void RequestInterstitialAdVideo()
        {
            // pre-fetch an ad 30-60 seconds before you need it
            if (_interstitialAdVideo != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Requesting interstitial video ad.");
                _interstitialAdVideo.RequestAd(AdType.Video, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialVideoAdUnitID);
            }
        }

        private void RequestInterstitialAdBanner()
        {
            // pre-fetch an ad 30-60 seconds before you need it
            if (_interstitialAdBanner != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Requesting interstitial banner ad.");
                _interstitialAdBanner.RequestAd(AdType.Display, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialBannerAdUnitID);
            }
        }

        //write the code for the events
        private void Interstitial_Video_AdReady(object sender, object e)
        {
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Interstitial video ad ready.");
            if (_showInterstitialAdWhenReady)
            {
                _showInterstitialAdWhenReady = false;
                this.ShowInterstitialAdVideo();
            }
        }

        //write the code for the events
        private void Interstitial_Banner_AdReady(object sender, object e)
        {
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Interstitial banner ad ready.");
            if (_showInterstitialAdWhenReady)
            {
                _showInterstitialAdWhenReady = false;
                this.ShowInterstitialAdBanner();
            }
        }

        #endregion

        #region Override Methods

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

        #region Events

        private void Interstitial_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_ErrorOccurred {e.ErrorCode} - {e.ErrorMessage}");
                this.OnInterstitialAdErrorOccurred(sender, _interstitialAdVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdErrorOccurred");
            }
            finally
            {
                this.RequestInterstititalAd();
            }
        }

        private void Interstitial_Completed(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_Completed");
                this.OnInterstitialAdCompleted(sender, _interstitialAdVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdCompleted");
            }
            finally
            {
                this.RequestInterstititalAd();
            }
        }

        private void Interstitial_Cancelled(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_Cancelled");
                this.OnInterstitialAdCancelled(sender, _interstitialAdVideo.State);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdCancelled");
            }
            finally
            {
                this.RequestInterstititalAd();
            }
        }

        #endregion

        #endregion
    }
}