using AppFramework.Core;
using AppFramework.Core.Commands;
using AppFramework.Core.Services;
using Contoso.Core.Strings;
using Contoso.Core.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Security.Authentication.Web.Core;

namespace Contoso.Core.ViewModels
{
    public sealed class WelcomeViewModel : ViewModelBase
    {
        #region Variables

        private CancellationTokenSource _cts;

        #endregion

        #region Properties

        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public WelcomeViewModel ViewModel { get { return this; } }

        private CommandBase _LaunchWebAccountManagerCommand = null;
        /// <summary>
        /// Command to access Web Account Manager
        /// </summary>
        public CommandBase LaunchWebAccountManagerCommand
        {
            get { return _LaunchWebAccountManagerCommand ?? (_LaunchWebAccountManagerCommand = new GenericCommand("LaunchWebAccountManagerCommand", async () => await this.LaunchWebAccountManager())); }
        }

        #endregion Properties

        #region Constructors

        public WelcomeViewModel()
        {
            this.Title = Resources.ViewTitleWelcome;

            if (DesignMode.DesignModeEnabled)
                return;
        }

        #endregion Constructors

        #region Methods

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if(!this.IsInitialized)
            {
            }

            return base.OnLoadStateAsync(e);
        }
        
        /// <summary>
        /// Launches the WebAccountManager (WAM)
        /// </summary>
        /// <returns></returns>
        private async Task LaunchWebAccountManager()
        {
            await this.Platform.WebAccountManager.SignoutAsync();
            this.ShowTimedStatus(Resources.TextLoading, 3000, true);
            this.Platform.WebAccountManager.Show(this.WAM_Success, this.WAM_Failed);
        }

        /// <summary>
        /// Flow to perform on successful pick of an account from the WAM popup
        /// </summary>
        /// <param name="pi">Details of the WAM provider choosen</param>
        /// <param name="info">Details of the WAM authenticated account</param>
        /// <param name="result">WebTokenRequestResult instance containing token info.</param>
        private async void WAM_Success(WebAccountManager.WebAccountProviderInfo pi, WebAccountManager.WebAccountInfo info, WebTokenRequestResult result)
        {
            try
            {
                this.ShowBusyStatus(Account.TextAuthenticating, true);

                // Create an account with the API
                _cts = new CancellationTokenSource();                
                using (var api = new ClientApi())
                {
                    var response = await api.AuthenticateAsync(info, _cts.Token);

                    // Authenticate the user into the app
                    await this.Platform.AuthManager.SetUserAsync(response);
                }

                this.ClearStatus();
                this.Platform.Navigation.Home(this.ViewParameter);
            }
            catch(Exception ex)
            {
                await this.HandleExceptionAsync(ex, "Failed to perform work during WAM success");
            }
            finally
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Flow to perform on any failures from the WAM popup
        /// </summary>
        /// <param name="pi"></param>
        /// <param name="result"></param>
        private async void WAM_Failed(WebAccountManager.WebAccountProviderInfo pi, WebTokenRequestResult result)
        {
            try
            {
                // Failure with WAM
                this.Platform.Logger.LogError(result?.ResponseError.ToException(), "WAM failed to retrieve user account token.");
                await this.ShowMessageBoxAsync(CancellationToken.None, string.Format(Account.TextWebAccountManagerRegisterAccountFailure, pi.WebAccountType));
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(ex, "Failed to perform work during WAM failure");
            }
        }

        public override void Dispose()
        {
            // Terminate any open tasks
            if (_cts?.IsCancellationRequested == false)
                _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;

            base.Dispose();
        }

        #endregion Methods
    }
}