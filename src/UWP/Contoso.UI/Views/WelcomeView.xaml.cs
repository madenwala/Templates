using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseWelcomeView : BaseView<WelcomeViewModel>
    {
    }

    public sealed partial class WelcomeView : BaseWelcomeView
    {
        public WelcomeView()
        {
            this.InitializeComponent();
        }

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new WelcomeViewModel());

            await base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            btnSignIn.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}