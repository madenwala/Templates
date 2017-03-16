using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.Strings;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Contoso.Core.ViewModels
{
    public partial class ShellViewModel : ViewModelBase
    {
        #region Properties

        public string WelcomeMessage
        {
            get
            {
                if (this.Platform.AuthManager.IsAuthenticated())
                    return string.Format(Account.TextWelcomeAuthenticated, this.Platform.AuthManager.User?.FirstName);
                else
                    return Account.TextWelcomeUnauthenticated;
            }
        }

        private bool _IsMenuOpen = true;
        public bool IsMenuOpen
        {
            get { return _IsMenuOpen; }
            set { this.SetProperty(ref _IsMenuOpen, value); }
        }

        #endregion

        #region Constructors

        public ShellViewModel()
        {
            this.Title = Resources.ViewTitleWelcome;

            if (DesignMode.DesignModeEnabled)
                return;

            this.RequiresAuthorization = true;
        }

        #endregion Constructors

        #region Methods

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (!this.IsInitialized)
            {
            }

            // If the view parameter contains any navigation requests, forward on to the global navigation service
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New && e.Parameter is NavigationRequest)
                this.Platform.Navigation.NavigateTo(e.Parameter as NavigationRequest);

            return base.OnLoadStateAsync(e);
        }

        public override void OnHandleKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.GamepadMenu || e.Key == Windows.System.VirtualKey.Home)
            {
                this.IsMenuOpen = !this.IsMenuOpen;
                e.Handled = true;
            }

            base.OnHandleKeyUp(e);
        }

        protected override async Task OnUserAuthenticatedChanged()
        {
            if (this.RequiresAuthorization && this.IsUserAuthenticated == false)
                await this.UserSignoutAsync(true);
        }

        #endregion Methods
    }

    public partial class ShellViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public ShellViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class ShellViewModel : Contoso.Core.ViewModels.ShellViewModel
    {
        public ShellViewModel()
        {
        }
    }
}