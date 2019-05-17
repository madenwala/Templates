using AppFramework.Core.Models;
using AppFramework.UI.Core;

namespace AppFramework.UI.Controls
{
    /// <summary>
    /// Helper class to scroll a control or object to its default position.
    /// </summary>
    public static class ScrollToTopHelper
    {
        /// <summary>
        /// Scrolls a control or object that can be scrolled to its default top position.
        /// </summary>
        /// <param name="content"></param>
        public static void ScrollToTop(object content)
        {
            if (content is IViewScrollToTop i)
            {
                i.ScrollToTop();
            }
            else if (content is Windows.UI.Xaml.Controls.ListViewBase list)
            {
                list.ScrollToTop();
            }
            else if (content is Windows.UI.Xaml.Controls.ScrollViewer sv)
            {
                sv.ScrollToTop();
            }
            else if (content is Microsoft.Toolkit.Uwp.UI.Controls.AdaptiveGridView grid)
            {
                grid.ScrollToTop();
            }
            else if (content is WebViewPanel wvp)
            {
                wvp.ScrollToTop();
            }
            else if (content is Windows.UI.Xaml.Controls.WebView wv)
            {
                var t = wv.InvokeScriptAsync("eval", new string[] { @"window.scrollTo(0, 0)" });
            }
            else if (content is Windows.UI.Xaml.Controls.Pivot pivot)
            {
                if(pivot.Items != null)
                    foreach (var pi in pivot.Items)
                        ScrollToTop(pi);
            }
            else if (content is Windows.UI.Xaml.Controls.PivotItem pi)
            {
                ScrollToTop(pi.Content ?? pi.ContentTemplateRoot);
            }
        }
    }
}