using Windows.UI.Xaml.Controls;

public static class ScrollViewerExtensions
{
    public static void ScrollToTop(this ScrollViewer sv)
    {
        sv.ChangeView(0, 0, null);
    }
}