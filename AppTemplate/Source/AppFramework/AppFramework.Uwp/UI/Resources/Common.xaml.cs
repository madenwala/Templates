using AppFramework.Core.Models;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Resources
{
    internal partial class Common
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
    }
}