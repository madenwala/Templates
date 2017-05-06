using AppFramework.Core.Models;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace AppFramework.UI.Resources
{
    public partial class Common
    {
        public Common()
        {
            this.InitializeComponent();
        }

        private void btnTryAgain_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if(sender is Button btn)
                if (btn.DataContext is INotifyTaskCompletion task)
                    task.Refresh();
        }

        private void List_RightTapped(object sender, Windows.UI.Xaml.Input.RightTappedRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }

        private void List_Holding(object sender, Windows.UI.Xaml.Input.HoldingRoutedEventArgs e)
        {
            FlyoutBase.ShowAttachedFlyout((FrameworkElement)sender);
        }
    }
}