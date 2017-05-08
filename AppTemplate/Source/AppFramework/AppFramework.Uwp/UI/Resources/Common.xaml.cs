using AppFramework.Core.Models;
using Windows.UI.Xaml.Controls;

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
            if(sender is Button btn && btn.DataContext is INotifyTaskCompletion task)
                task.Refresh();
        }
    }
}