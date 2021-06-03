using AppFramework.Core.Commands;
using AppFramework.Core.Extensions;
using AppFramework.Core.Models;
using Contoso.Core.Models;
using Contoso.Core.ViewModels;
using System;
using System.Collections.Generic;

namespace Contoso.Core.Services
{
    public abstract class NavigationManagerBase : AppFramework.Core.Services.NavigationManagerBase
    {
        #region Handle Activation Methods

        protected override bool OnVoiceActivation(VoiceCommandInfo info)
        {
            switch (info.VoiceCommandName)
            {
                case "showByName":
                    // Access the value of the {destination} phrase in the voice command
                    string name = info.GetSemanticInterpretation("Name");
                    this.Item(name);
                    return true;

                default:
                    // If we can't determine what page to launch, go to the default entry point.
                    return base.OnVoiceActivation(info);
            }
        }

        protected override bool OnHandleArgumentsActivation(string arguments, IDictionary<string, string> dic)
        {
            if (dic.ContainsKey("model"))
            {
                if (dic.ContainsKeyAndValue("model", nameof(MainViewModel)))
                {
                    this.Home();
                    return true;
                }
                else if (dic.ContainsKeyAndValue("model", nameof(ItemModel)))
                {
                    this.Item(dic["ID"]);
                    return true;
                }
                throw new NotImplementedException(string.Format("No action implemented for model type {0}", dic["model"]));
            }
            else
            {
                this.Item(arguments.Replace("/", ""));
                return true;
            }
        }

        #endregion

        #region Search

        public abstract void Search(object parameter = null);

        private CommandBase _SearchCommand = null;
        /// <summary>
        /// Command to navigate to the application's search view.
        /// </summary>
        public CommandBase SearchCommand
        {
            get { return _SearchCommand ?? (_SearchCommand = new GenericCommand<string>("NavigateToSearchCommand", (e) => this.Search(e))); }
        }

        #endregion

        #region Account Management

        public abstract void AccountSignin(object parameter = null);

        private CommandBase _AccountSignInCommand = null;
        /// <summary>
        /// Command to navigate to the account sign in view.
        /// </summary>
        public CommandBase AccountSignInCommand
        {
            get { return _AccountSignInCommand ?? (_AccountSignInCommand = new NavigationCommand("NavigateToAccountSignInCommand", this.AccountSignin)); }
        }

        public abstract void AccountSignup(object parameter = null);

        private CommandBase _AccountSignUpCommand = null;
        /// <summary>
        /// Command to navigate to the account sign up view.
        /// </summary>
        public CommandBase AccountSignUpCommand
        {
            get { return _AccountSignUpCommand ?? (_AccountSignUpCommand = new NavigationCommand("NavigateToAccountSignUpCommand", this.AccountSignup)); }
        }

        public abstract void AccountForgot(object parameter = null);

        private CommandBase _AccountForgotCommand = null;
        /// <summary>
        /// Command to navigate to the account forgot crentials view.
        /// </summary>
        public CommandBase AccountForgotCommand
        {
            get { return _AccountForgotCommand ?? (_AccountForgotCommand = new NavigationCommand("NavigateToAccountForgotCommand", this.AccountForgot)); }
        }

        #endregion

        #region Other

        public abstract void Item(object parameter);

        #endregion
    }
}