using Contoso.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;
using Contoso.Core.Services;

namespace Contoso.UI.Views
{
    public abstract class MainViewBase : ViewBase<MainViewModel>
    {
    }

    public sealed partial class MainView : MainViewBase
    {
        public MainView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(Core.Services.PlatformBase.Current.ViewModel as MainViewModel); // TODO

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            list.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}
