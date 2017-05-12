using AppFramework.Core;
using AppFramework.Core.ViewModels;
using Microsoft.Advertising.WinRT.UI;
using System;

namespace AppFramework.UI.Views
{
    public abstract partial class ViewBase<TViewModel> where TViewModel : ViewModelBase
    {
        #region Interstitial Ads

        #region Variables

        //Initialize the ads:
        private static InterstitialAd _interstitialAdVideo;
        private static InterstitialAd _interstitialAdBanner;
        private bool _showInterstitialAdWhenReady = false;

        #endregion

        #region Constructors

        static ViewBase()
        {
            if (Controls.AdControl.DevCenterInterstitialVideoAdUnitID != null)
            {
                _interstitialAdVideo = new InterstitialAd();
                RequestInterstitialAdVideo();
            }

            if (Controls.AdControl.DevCenterInterstitialBannerAdUnitID != null)
            {
                _interstitialAdBanner = new InterstitialAd();
                RequestInterstitialAdBanner();
            }
        }

        #endregion

        #region Methods
        
        private bool CheckIfAdsOpen()
        {
            if (_interstitialAdVideo.State == InterstitialAdState.Showing)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Closing interstitial video ad due to back navigation request.");
                _interstitialAdVideo.Close();
                return true;
            }
            else if (_interstitialAdBanner.State == InterstitialAdState.Showing)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Closing interstitial banner ad due to back navigation request.");
                _interstitialAdBanner.Close();
                return true;
            }
            else
                return false;
        }

        #region Initialize/Tear-Down

        private void InitializeInterstitialAdEvents()
        {
            if (_interstitialAdVideo != null)
            {
                _interstitialAdVideo.AdReady += Interstitial_Video_AdReady;
                _interstitialAdVideo.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdVideo.Completed += Interstitial_Completed;
                _interstitialAdVideo.Cancelled += Interstitial_Cancelled;
            }

            if (_interstitialAdBanner != null)
            {
                _interstitialAdBanner.AdReady += Interstitial_Banner_AdReady;
                _interstitialAdBanner.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdBanner.Completed += Interstitial_Completed;
                _interstitialAdBanner.Cancelled += Interstitial_Cancelled;
            }
        }

        private void TearDownInterstitialAdEvents()
        {
            if (_interstitialAdVideo != null)
            {
                _interstitialAdVideo.AdReady -= Interstitial_Video_AdReady;
                _interstitialAdVideo.ErrorOccurred -= Interstitial_ErrorOccurred;
                _interstitialAdVideo.Completed -= Interstitial_Completed;
                _interstitialAdVideo.Cancelled -= Interstitial_Cancelled;
            }

            if (_interstitialAdBanner != null)
            {
                _interstitialAdBanner.AdReady -= Interstitial_Banner_AdReady;
                _interstitialAdBanner.ErrorOccurred -= Interstitial_ErrorOccurred;
                _interstitialAdBanner.Completed -= Interstitial_Completed;
                _interstitialAdBanner.Cancelled -= Interstitial_Cancelled;
            }
        }

        #endregion

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
            {
                _showInterstitialAdWhenReady = true;
                RequestInterstititalAd();
            }
        }

        private void ShowInterstitialAdVideo()
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - ShowInterstitialAdVideo");
                this.OnInterstitialAdDisplayed();
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
                this.OnInterstitialAdDisplayed();
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

        private static void RequestInterstititalAd()
        {
            RequestInterstitialAdVideo();
            RequestInterstitialAdBanner();
        }

        private static void RequestInterstitialAdVideo()
        {
            // pre-fetch an ad 30-60 seconds before you need it
            if (_interstitialAdVideo != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Requesting interstitial video ad.");
                _interstitialAdVideo.RequestAd(AdType.Video, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialVideoAdUnitID);
            }
        }

        private static void RequestInterstitialAdBanner()
        {
            // pre-fetch an ad 30-60 seconds before you need it
            if (_interstitialAdBanner != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - Requesting interstitial banner ad.");
                _interstitialAdBanner.RequestAd(AdType.Display, Controls.AdControl.DevCenterAdAppID, Controls.AdControl.DevCenterInterstitialBannerAdUnitID);
            }
        }

        #endregion

        #region Override Methods

        protected virtual void OnInterstitialAdDisplayed()
        {
        }

        protected virtual void OnInterstitialAdCompleted()
        {
        }

        protected virtual void OnInterstitialAdErrorOccurred()
        {
        }

        protected virtual void OnInterstitialAdCancelled()
        {
        }

        #endregion

        #region Events

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

        private void Interstitial_ErrorOccurred(object sender, AdErrorEventArgs e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_ErrorOccurred {e.ErrorCode} - {e.ErrorMessage}");
                this.OnInterstitialAdErrorOccurred();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdErrorOccurred");
            }
            //finally
            //{
            //    RequestInterstititalAd();
            //}
        }

        private void Interstitial_Completed(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_Completed");
                this.OnInterstitialAdCompleted();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdCompleted");
            }
            finally
            {
                RequestInterstititalAd();
            }
        }

        private void Interstitial_Cancelled(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_Cancelled");
                this.OnInterstitialAdCancelled();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdCancelled");
            }
            finally
            {
                RequestInterstititalAd();
            }
        }

        #endregion

        #endregion
    }
}