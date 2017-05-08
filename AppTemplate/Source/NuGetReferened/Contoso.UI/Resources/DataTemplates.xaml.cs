using Contoso.Core;
using Contoso.Core.Models;
using Contoso.UI.Views;
using Windows.UI.Xaml;

namespace Contoso.UI.Resources
{
    public partial class DataTemplates
    {
        public DataTemplates()
        {
            this.InitializeComponent();
        }

        private async void OpenInNewWindow_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is ItemModel model)
                await Platform.Current.Navigation.NewWindow(typeof(DetailsView), model.ID);
        }
    }
}