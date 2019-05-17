using AppFramework.Core;
using AppFramework.Core.ViewModels;
using AppFramework.UI.Controls;
using System;

namespace AppFramework.UI.Views
{
    public abstract partial class ViewBase<TViewModel> where TViewModel : BaseViewModel
    {
        #region Interstitial Ads

        public void ShowInterstitialAd()
        {
            if (PlatformBase.CurrentCore.NavigationBase.ParentFrame is ApplicationFrame frame)
                frame.ShowInterstitialAd();
        }

        private void InitializeInterstitialAds()
        {
            if (PlatformBase.CurrentCore.NavigationBase.ParentFrame is ApplicationFrame frame)
            {
                frame.OnInterstitialAdCancelled += OnInterstitialAdCancelled;
                frame.OnInterstitialAdCompleted += OnInterstitialAdCompleted;
                frame.OnInterstitialAdDisplayed += OnInterstitialAdDisplayed;
                frame.OnInterstitialAdErrorOccurred += OnInterstitialAdErrorOccurred;
            }
        }

        private void TeardownInterstitialAds()
        {
            if (PlatformBase.CurrentCore.NavigationBase.ParentFrame is ApplicationFrame frame)
            {
                frame.OnInterstitialAdCancelled -= OnInterstitialAdCancelled;
                frame.OnInterstitialAdCompleted -= OnInterstitialAdCompleted;
                frame.OnInterstitialAdDisplayed -= OnInterstitialAdDisplayed;
                frame.OnInterstitialAdErrorOccurred -= OnInterstitialAdErrorOccurred;
            }
        }

        protected virtual void OnInterstitialAdErrorOccurred(object sender, EventArgs e)
        {
        }

        protected virtual void OnInterstitialAdDisplayed(object sender, EventArgs e)
        {
        }

        protected virtual void OnInterstitialAdCompleted(object sender, EventArgs e)
        {
        }

        protected virtual void OnInterstitialAdCancelled(object sender, EventArgs e)
        {
        }

        #endregion
    }
}