namespace AppFramework.Core.Models
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
    }
}
