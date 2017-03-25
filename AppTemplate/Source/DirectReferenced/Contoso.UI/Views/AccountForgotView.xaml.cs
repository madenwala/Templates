using AppFramework.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class AccountForgetViewBase : ViewBase<AccountForgotViewModel>
    {
    }

    public sealed partial class AccountForgotView : AccountForgetViewBase
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
