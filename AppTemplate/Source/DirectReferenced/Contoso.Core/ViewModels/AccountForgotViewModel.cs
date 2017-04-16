﻿using AppFramework.Core;
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
    public partial class AccountForgotViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Command used to submit the form.
        /// </summary>
        public CommandBase SubmitCommand { get; private set; }

        private bool _IsSubmitEnabled;
        /// <summary>
        /// Gets whether or not the form is in a valid state and can be submitted.
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

        private string _Username;
        /// <summary>
        /// Gets or sets the username to check to see if the account exists and send credentials to.
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

        #endregion

        #region Constructors

        public AccountForgotViewModel()
        {
            this.Title = Account.ViewTitleForgotPassword;

            if (DesignMode.DesignModeEnabled)
                return;

            this.SubmitCommand = new GenericCommand<IModel>("AccountForgotViewModel-SubmitCommand", async () => await this.SubmitAsync(), () => this.IsSubmitEnabled);

            // Properties to preserve during tombstoning
            this.PreservePropertyState(() => this.Username);
        }

        #endregion

        #region Methods

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (!this.IsInitialized)
            {
                this.Username = e.Parameter?.ToString();
            }

            this.CheckIfValid();

            return base.OnLoadStateAsync(e);
        }

        /// <summary>
        /// Checks to see if the form data is valid.
        /// </summary>
        private void CheckIfValid()
        {
            this.IsSubmitEnabled = !string.IsNullOrWhiteSpace(this.Username);
        }

        /// <summary>
        /// Submits the form to the account forgotten service.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        private async Task SubmitAsync()
        {
            try
            {
                this.IsSubmitEnabled = false;
                this.ShowBusyStatus(Account.TextValidatingUsername, true);

                using (var api = new ClientApi())
                {
                    var response = await api.ForgotPasswordAsync(this, CancellationToken.None);

                    this.ClearStatus();

                    await this.ShowMessageBoxAsync(CancellationToken.None, response.Message, this.Title);
                    if (response?.IsValid == true)
                        this.Platform.Navigation.GoBack();
                }
            }
            catch (Exception ex)
            {
                await this.HandleExceptionAsync(ex, "Error during account forgot password.");
            }
            finally
            {
                this.CheckIfValid();
            }
        }

        #endregion
    }

    public partial class AccountForgotViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public AccountForgotViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class AccountForgotViewModel : Contoso.Core.ViewModels.AccountForgotViewModel
    {
        public AccountForgotViewModel()
        {
            this.Username = "TestUsername1";
        }
    }
}