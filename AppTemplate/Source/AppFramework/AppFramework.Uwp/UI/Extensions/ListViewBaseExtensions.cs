using System.Linq;
using Windows.UI.Xaml.Controls;

public static class ListViewBaseExtensions
{
    public static void ScrollToTop(this ListViewBase list)
    {
        //if (list is Microsoft.Toolkit.Uwp.UI.Controls.PullToRefreshListView)
        //{
        //    var sv = list.GetDescendantByType(typeof(ScrollViewer)) as ScrollViewer;
        //    sv?.ScrollToTop();
        //}
        //else
        {
            var item = list?.Items?.FirstOrDefault();
            if (item != null)
                list?.ScrollIntoView(item);
        }
    }
}