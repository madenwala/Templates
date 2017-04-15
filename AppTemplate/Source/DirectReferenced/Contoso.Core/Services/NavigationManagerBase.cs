using AppFramework.Core.Commands;

namespace Contoso.Core.Services
{
    public abstract class NavigationManagerBase : AppFramework.Core.Services.NavigationManagerBase
    {
        #region Search

        public abstract void Search(object parameter = null);

        private CommandBase _navigateToSearchCommand = null;
        /// <summary>
        /// Command to navigate to the application's search view.
        /// </summary>
        public CommandBase NavigateToSearchCommand
        {
            get { return _navigateToSearchCommand ?? (_navigateToSearchCommand = new GenericCommand<string>("NavigateToSearchCommand", (e) => this.Search(e))); }
        }

        #endregion

        #region Account Management

        public abstract void AccountSignin(object parameter = null);

        private CommandBase _navigateToAccountSignInCommand = null;
        /// <summary>
        /// Command to navigate to the account sign in view.
        /// </summary>
        public CommandBase NavigateToAccountSignInCommand
        {
            get { return _navigateToAccountSignInCommand ?? (_navigateToAccountSignInCommand = new NavigationCommand("NavigateToAccountSignInCommand", this.AccountSignin)); }
        }

        public abstract void AccountSignup(object parameter = null);

        private CommandBase _navigateToAccountSignUpCommand = null;
        /// <summary>
        /// Command to navigate to the account sign up view.
        /// </summary>
        public CommandBase NavigateToAccountSignUpCommand
        {
            get { return _navigateToAccountSignUpCommand ?? (_navigateToAccountSignUpCommand = new NavigationCommand("NavigateToAccountSignUpCommand", this.AccountSignup)); }
        }

        public abstract void AccountForgot(object parameter = null);

        private CommandBase _navigateToAccountForgotCommand = null;
        /// <summary>
        /// Command to navigate to the account forgot crentials view.
        /// </summary>
        public CommandBase NavigateToAccountForgotCommand
        {
            get { return _navigateToAccountForgotCommand ?? (_navigateToAccountForgotCommand = new NavigationCommand("NavigateToAccountForgotCommand", this.AccountForgot)); }
        }

        #endregion

        #region Other

        public abstract void Item(object parameter);

        #endregion
    }
}