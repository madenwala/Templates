using Contoso.Core;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    public static class ScrollToTopHelper
    {
        public static void ScrollToTop(object content)
        {
            if (content is IViewScrollToTop)
            {
                var i = content as IViewScrollToTop;
                i.ScrollToTop();
            }
            else if (content is ListViewBase)
            {
                var list = content as ListViewBase;
                list.ScrollToTop();
            }
            else if (content is ScrollViewer)
            {
                var sv = content as ScrollViewer;
                sv.ScrollToTop();
            }
            else if (content is Microsoft.Toolkit.Uwp.UI.Controls.AdaptiveGridView)
            {
                var grid = content as Microsoft.Toolkit.Uwp.UI.Controls.AdaptiveGridView;
                grid.ScrollToTop();
            }
            else if (content is WebViewPanel)
            {
                var control = content as WebViewPanel;
                control.ScrollToTop();
            }
            else if (content is WebView)
            {
                var control = content as WebView;
                // TODO scroll web view control
            }
            else if (content is Pivot)
            {
                var pivot = content as Pivot;
                foreach (var pi in pivot.Items)
                    ScrollToTop(pi);
            }
            else if (content is PivotItem)
            {
                var pi = content as PivotItem;
                ScrollToTop(pi.Content ?? pi.ContentTemplateRoot);
            }
        }
    }
}