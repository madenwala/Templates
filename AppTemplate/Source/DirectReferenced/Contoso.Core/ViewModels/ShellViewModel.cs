using AppFramework.Core;
using AppFramework.Core.Models;
using Contoso.Core.Strings;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;

namespace Contoso.Core.ViewModels
{
    public sealed partial class ShellViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public ShellViewModel ViewModel { get { return this; } }

        public string WelcomeMessage
        {
            get
            {
                if (this.Platform.AuthManager?.IsAuthenticated() == true)
                    return string.Format(Account.TextWelcomeAuthenticated, this.Platform.AuthManager.CurrentUser?.DisplayName);
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

        protected override async Task OnUserAuthenticatedChanged()
        {
            if (this.RequiresAuthorization && this.IsUserAuthenticated == false)
                await this.UserSignoutAsync(true);
        }

        #endregion Methods
    }
}