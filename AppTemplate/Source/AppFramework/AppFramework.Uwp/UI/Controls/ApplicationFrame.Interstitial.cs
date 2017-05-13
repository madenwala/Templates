using AppFramework.Core;
using Microsoft.Advertising.WinRT.UI;
using System;
using Windows.UI.Xaml;

namespace AppFramework.UI.Controls
{
    public partial class ApplicationFrame
    {
        #region Variables

        //Initialize the ads:
        private InterstitialAd _interstitialAdVideo;
        private InterstitialAd _interstitialAdBanner;
        private bool _showInterstitialAdWhenReady = false;

        #endregion

        #region Events

        public event EventHandler OnInterstitialAdDisplayed;
        public event EventHandler OnInterstitialAdCompleted;
        public event EventHandler OnInterstitialAdErrorOccurred;
        public event EventHandler OnInterstitialAdCancelled;

        #endregion

        #region Properties
        
        public bool DisableInterstitialAds
        {
            get { return (bool)GetValue(DisableInterstitialAdsProperty); }
            set { SetValue(DisableInterstitialAdsProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisableInterstitialAds.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DisableInterstitialAdsProperty =
            DependencyProperty.Register(nameof(DisableInterstitialAds), typeof(bool), typeof(ApplicationFrame), new PropertyMetadata(false));

        #endregion

        #region Methods

        private void InitializeInterstitialAds()
        {
            this.AdsDisplayedCount = 0;

            if (Controls.AdControl.DevCenterInterstitialVideoAdUnitID != null && !this.DisableInterstitialAds)
            {
                _interstitialAdVideo = new InterstitialAd();
                _interstitialAdVideo.AdReady += Interstitial_Video_AdReady;
                _interstitialAdVideo.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdVideo.Completed += Interstitial_Completed;
                _interstitialAdVideo.Cancelled += Interstitial_Cancelled;
                this.RequestInterstitialAdVideo();
            }

            if (Controls.AdControl.DevCenterInterstitialBannerAdUnitID != null && !this.DisableInterstitialAds)
            {
                _interstitialAdBanner = new InterstitialAd();
                _interstitialAdBanner.AdReady += Interstitial_Banner_AdReady;
                _interstitialAdBanner.ErrorOccurred += Interstitial_ErrorOccurred;
                _interstitialAdBanner.Completed += Interstitial_Completed;
                _interstitialAdBanner.Cancelled += Interstitial_Cancelled;
                this.RequestInterstitialAdBanner();
            }
        }

        internal bool CheckIfAdsOpen()
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
                this.RequestInterstititalAd();
            }
        }

        private void ShowInterstitialAdVideo()
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - ShowInterstitialAdVideo");
                this.OnInterstitialAdDisplayed?.Invoke(this, new EventArgs());
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
            if (this.DisableInterstitialAds)
                return;

            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "DevCenter - ShowInterstitialAdBanner");
                this.OnInterstitialAdDisplayed?.Invoke(this, new EventArgs());
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
                this.OnInterstitialAdErrorOccurred?.Invoke(this, new EventArgs());
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"DevCenter - Error occured while executing {this.GetType().Name}.OnInterstitialAdErrorOccurred");
            }
        }

        private void Interstitial_Completed(object sender, object e)
        {
            try
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"DevCenter - {this.GetType().Name}.Interstitial_Completed");
                this.OnInterstitialAdCompleted?.Invoke(this, new EventArgs());
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
                this.OnInterstitialAdCancelled?.Invoke(this, new EventArgs());
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
    }
}