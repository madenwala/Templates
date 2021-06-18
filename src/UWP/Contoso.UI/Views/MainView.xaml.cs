using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseMainView : BaseView<MainViewModel>
    {
    }

    public sealed partial class MainView : BaseMainView
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