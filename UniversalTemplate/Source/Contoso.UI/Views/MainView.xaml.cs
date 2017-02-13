using Contoso.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml;

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
                this.SetViewModel(Platform.Current.ViewModel);

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            list.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}
