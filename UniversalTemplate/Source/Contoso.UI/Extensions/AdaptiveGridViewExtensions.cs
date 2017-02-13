using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml.Controls;

public static class AdaptiveGridViewExtensions
{
    public static void ScrollToTop(this AdaptiveGridView grid)
    {
        var list = grid.GetDescendantByName("ListView") as ListViewBase;
        list?.ScrollToTop();
    }
}