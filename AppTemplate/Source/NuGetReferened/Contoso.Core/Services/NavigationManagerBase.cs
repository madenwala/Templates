using AppFramework.Core.Commands;

namespace Contoso.Core.Services
{
    public abstract class NavigationManagerBase : AppFramework.Core.Services.NavigationManagerBase
    {
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

        public void Help()
        {
            this.WebView("http://www.microsoft.com");
        }

        private CommandBase _HelpCommand = null;
        public CommandBase HelpCommand
        {
            get { return Platform.Current == null ? null : _HelpCommand ?? (_HelpCommand = new NavigationCommand("NavigateToHelpCommand", Platform.Current.Navigation.Help)); }
        }

        #endregion
    }
}