namespace AppFramework.UI.Core
{
    public interface IViewScrollToTop
    {
        void ScrollToTop();
    }

    public interface IView : IViewScrollToTop
    {
        object ViewParameter { get; }
        bool OnForwardNavigationRequested();
        bool OnBackNavigationRequested();
        void ShowInterstitialAd();
    }
}