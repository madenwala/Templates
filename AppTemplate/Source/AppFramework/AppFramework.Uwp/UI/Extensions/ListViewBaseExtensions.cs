using System.Linq;
using Windows.UI.Xaml.Controls;

/// <summary>
/// Extensions to ListViewBase object.
/// </summary>
public static class ListViewBaseExtensions
{
    /// <summary>
    /// Scrolls to the top item in the list.
    /// </summary>
    /// <param name="list"></param>
    public static void ScrollToTop(this ListViewBase list)
    {
        // TODO removed scroll up on PullToRefresh
        //if (list is Microsoft.Toolkit.Uwp.UI.Controls.PullToRefreshListView)
        //{
        //    var sv = list.GetDescendantByType(typeof(ScrollViewer)) as ScrollViewer;
        //    sv?.ScrollToTop();
        //}
        //else
        //{
            var item = list?.Items?.FirstOrDefault();
            if (item != null)
                list?.ScrollIntoView(item);
        //}
    }
}