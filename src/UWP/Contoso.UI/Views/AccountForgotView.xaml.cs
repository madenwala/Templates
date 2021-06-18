using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseAccountForgetView : BaseView<AccountForgotViewModel>
    {
    }

    public sealed partial class AccountForgotView : BaseAccountForgetView
    {
        public AccountForgotView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new AccountForgotViewModel());

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            txtUsername.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}