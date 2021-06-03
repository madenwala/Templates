using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;

namespace AppFramework.UI.Extensions
{
    public static class FrameworkElementExtensions
    {
        public static void ShowContextMenu(this FrameworkElement element)
        {
            var flyout = FlyoutBase.GetAttachedFlyout(element);
            if (flyout != null)
                FlyoutBase.ShowAttachedFlyout(element);
        }
    }
}