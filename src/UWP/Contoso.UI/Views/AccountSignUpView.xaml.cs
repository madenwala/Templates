using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseAccountSignUpView : BaseView<AccountSignUpViewModel>
    {
    }

    public sealed partial class AccountSignUpView : BaseAccountSignUpView
    {
        public AccountSignUpView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new AccountSignUpViewModel());

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            txtFirstName.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}