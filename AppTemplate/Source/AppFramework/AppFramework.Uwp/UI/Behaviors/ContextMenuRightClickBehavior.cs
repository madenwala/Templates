using AppFramework.UI.Extensions;
using Microsoft.Xaml.Interactivity;
using Windows.UI.Xaml;

namespace AppFramework.UI.Behaviors
{
    public sealed class ContextMenuRightClickBehavior : Behavior<FrameworkElement>
    {
        protected override void OnAttached()
        {
            this.AssociatedObject.RightTapped += AssociatedObject_RightTapped;
            this.AssociatedObject.Holding += AssociatedObject_Holding;
            base.OnAttached();
        }

        private void AssociatedObject_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            this.AssociatedObject.ShowContextMenu();
        }

        private void AssociatedObject_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            this.AssociatedObject.ShowContextMenu();
        }

        protected override void OnDetaching()
        {
            this.AssociatedObject.RightTapped -= AssociatedObject_RightTapped;
            this.AssociatedObject.Holding -= AssociatedObject_Holding;
            base.OnDetaching();
        }
    }
}