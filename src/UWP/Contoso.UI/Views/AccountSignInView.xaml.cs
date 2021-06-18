using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseAccountSignInView : BaseView<AccountSignInViewModel>
    {
    }

    public sealed partial class AccountSignInView : BaseAccountSignInView
    {
        public AccountSignInView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new AccountSignInViewModel());

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            txtUsername.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}