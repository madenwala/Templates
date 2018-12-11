using AppFramework.Core;
using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using Contoso.Core.Strings;
using Contoso.Core.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public sealed class AccountSignInViewModel : ViewModelBase
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
        public AccountSignInViewModel ViewModel { get { return this; } }

        /// <summary>
        /// Command used to submit the sign in form.
        /// </summary>
        public CommandBase SubmitCommand { get; private set; }

        private string _Username;

        /// <summary>
        /// Username entered by user.
        /// </summary>
        public string Username
        {
            get { return _Username; }
            set
            {
                if (this.SetProperty(ref _Username, value))
                    this.CheckIfValid();
            }
        }

        private string _Password;
        /// <summary>
        /// Password entered by user.
        /// </summary>
        public string Password
        {
            get { return _Password; }
            set
            {
                if (this.SetProperty(ref _Password, value))
                    this.CheckIfValid();
            }
        }

        private bool _IsSubmitEnabled = false;
        /// <summary>
        /// Gets a boolean indicating whether or not the form has valid data to enable the submit button.
        /// </summary>
        public bool IsSubmitEnabled
        {
            get { return _IsSubmitEnabled; }
            private set
            {
                if (this.SetProperty(ref _IsSubmitEnabled, value))
                    this.SubmitCommand.RaiseCanExecuteChanged();
            }
        }

        #endregion

        #region Constructors

        public AccountSignInViewModel()
        {
            this.Title = Account.ViewTitleSignIn;

            if (DesignMode.DesignModeEnabled)
                return;

            this.SubmitCommand = new GenericCommand<IModel>("AccountSignInViewModel-SubmitCommand", async () => await this.SubmitAsync(), () => this.IsSubmitEnabled);

            // Properties to preserve during tombstoning
            this.PreservePropertyState(() => this.Username);
            this.PreservePropertyState(() => this.Password);
        }

        #endregion

        #region Methods

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (!this.IsInitialized)
                this.Username = e.Parameter?.ToString();

            this.ClearStatus();
            this.CheckIfValid();

            return base.OnLoadStateAsync(e);
        }

        /// <summary>
        /// Submit the form to the authentication service.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        private async Task SubmitAsync()
        {
            try
            {
                this.IsSubmitEnabled = false;
                _cts = new CancellationTokenSource();
                this.ShowBusyStatus(Account.TextAuthenticating, true, true);

                using (var api = new ClientApi())
                {
                    string userMessage = null;
                    var response = await api.AuthenticateAsync(this, _cts.Token);

                    // Ensure that there is a valid token returned
                    if (!string.IsNullOrEmpty(response?.AccessToken))
                        await this.Platform.AuthManager.SetUserAsync(response);
                    else
                        userMessage = Account.TextAuthenticationFailed;

                    this.ClearStatus();

                    // Nav home if authenticated else display error message
                    if (this.IsUserAuthenticated)
                        this.Platform.Navigation.Home();
                    else
                        await this.ShowMessageBoxAsync(_cts.Token, userMessage, this.Title);
                }
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(ex, "Error during account sign-in.");
            }
            finally
            {
                _cts = null;
                this.CheckIfValid();
            }
        }

        /// <summary>
        /// Called by the cancel button on the blocking status view.  If user wants to cancel, cancel the call to the authentication process.
        /// </summary>
        protected override void CancelStatus()
        {
            if (_cts?.IsCancellationRequested == false)
                _cts?.Cancel();
            base.CancelStatus();
        }

        /// <summary>
        /// Checks to see if the form is valid.
        /// </summary>
        private void CheckIfValid()
        {
            this.IsSubmitEnabled = !string.IsNullOrWhiteSpace(this.Username)
                && !string.IsNullOrWhiteSpace(this.Password)
                && this.Username.Length > 0
                && this.Password.Length > 0;
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

        #endregion
    }
}