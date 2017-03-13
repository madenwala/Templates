using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppFramework.Core.Commands;
using System.Reflection;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;
using AppFramework.Core.ViewModels;

namespace AppFramework.Core.Services
{
    public partial class NavigationManagerBase
    {
        #region Properties

        #region Core Navigation Commands

        private CommandBase _navigateToHomeCommand = null;
        /// <summary>
        /// Command to access backwards page navigation..
        /// </summary>
        public CommandBase NavigateToHomeCommand
        {
            get { return _navigateToHomeCommand ?? (_navigateToHomeCommand = new NavigationCommand("NavigateToHomeCommand", this.Home)); }
        }

        private CommandBase _navigateGoBackCommand = null;
        /// <summary>
        /// Command to access backwards page navigation..
        /// </summary>
        public CommandBase NavigateGoBackCommand
        {
            get { return _navigateGoBackCommand ?? (_navigateGoBackCommand = new NavigationCommand("NavigateGoBackCommand", () => this.GoBack(), this.CanGoBack)); }
        }

        private CommandBase _navigateGoForwardCommand = null;
        /// <summary>
        /// Command to access forard page navigation.
        /// </summary>
        public CommandBase NavigateGoForwardCommand
        {
            get { return _navigateGoForwardCommand ?? (_navigateGoForwardCommand = new NavigationCommand("NavigateGoForwardCommand", () => this.GoForward(), this.CanGoForward)); }
        }

        #endregion

        #region Page Commands

        private CommandBase _navigateToModelCommand = null;
        /// <summary>
        /// Command to access navigating to an instance of a model (Navigation manager handles actually forwarding to the appropriate view for 
        /// the model pass into a parameter. 
        /// </summary>
        public CommandBase NavigateToModelCommand
        {
            get { return _navigateToModelCommand ?? (_navigateToModelCommand = new NavigationCommand()); }
        }

        private CommandBase _navigateToSettingsCommand = null;
        /// <summary>
        /// Command to navigate to the settings view.
        /// </summary>
        public CommandBase NavigateToSettingsCommand
        {
            get { return _navigateToSettingsCommand ?? (_navigateToSettingsCommand = new NavigationCommand("NavigateToSettingsCommand", this.Settings)); }
        }

        #endregion

        #region Web Browser Commands

        private CommandBase _navigateToWebViewCommand = null;
        /// <summary>
        /// Command to navigate to the internal web view.
        /// </summary>
        public CommandBase NavigateToWebViewCommand
        {
            get { return _navigateToWebViewCommand ?? (_navigateToWebViewCommand = new WebViewCommand()); }
        }

        private CommandBase _navigateToWebBrowserCommand = null;
        /// <summary>
        /// Command to navigate to the external web browser.
        /// </summary>
        public CommandBase NavigateToWebBrowserCommand
        {
            get { return _navigateToWebBrowserCommand ?? (_navigateToWebBrowserCommand = new WebBrowserCommand()); }
        }

        #endregion

        #region Map Commands

        private CommandBase _navigateToMapExternalCommand = null;
        /// <summary>
        /// Command to access the external maps view.
        /// </summary>
        public CommandBase NavigateToMapExternalCommand
        {
            get { return _navigateToMapExternalCommand ?? (_navigateToMapExternalCommand = new MapExternalCommand()); }
        }

        private CommandBase _navigateToMapExternalDrivingCommand = null;
        /// <summary>
        /// Command to access the device's map driving directions view.
        /// </summary>
        public CommandBase NavigateToMapExternalDrivingCommand
        {
            get { return _navigateToMapExternalDrivingCommand ?? (_navigateToMapExternalDrivingCommand = new MapExternalCommand(MapExternalOptions.DrivingDirections)); }
        }

        private CommandBase _navigateToMapExternalWalkingCommand = null;
        /// <summary>
        /// Command to access the device's map walking directions view.
        /// </summary>
        public CommandBase NavigateToMapExternalWalkingCommand
        {
            get { return _navigateToMapExternalWalkingCommand ?? (_navigateToMapExternalWalkingCommand = new MapExternalCommand(MapExternalOptions.WalkingDirections)); }
        }

        #endregion

        #region Search Commands

        private CommandBase _navigateToSearchCommand = null;
        /// <summary>
        /// Command to navigate to the application's search view.
        /// </summary>
        public CommandBase NavigateToSearchCommand
        {
            get { return _navigateToSearchCommand ?? (_navigateToSearchCommand = new GenericCommand<string>("NavigateToSearchCommand", (e) => this.Search(e))); }
        }

        #endregion

        #region Account Commands

        private CommandBase _navigateToAccountSignInCommand = null;
        /// <summary>
        /// Command to navigate to the account sign in view.
        /// </summary>
        public CommandBase NavigateToAccountSignInCommand
        {
            get { return _navigateToAccountSignInCommand ?? (_navigateToAccountSignInCommand = new NavigationCommand("NavigateToAccountSignInCommand", this.AccountSignin)); }
        }

        private CommandBase _navigateToAccountSignUpCommand = null;
        /// <summary>
        /// Command to navigate to the account sign up view.
        /// </summary>
        public CommandBase NavigateToAccountSignUpCommand
        {
            get { return _navigateToAccountSignUpCommand ?? (_navigateToAccountSignUpCommand = new NavigationCommand("NavigateToAccountSignUpCommand", this.AccountSignup)); }
        }

        private CommandBase _navigateToAccountForgotCommand = null;
        /// <summary>
        /// Command to navigate to the account forgot crentials view.
        /// </summary>
        public CommandBase NavigateToAccountForgotCommand
        {
            get { return _navigateToAccountForgotCommand ?? (_navigateToAccountForgotCommand = new NavigationCommand("NavigateToAccountForgotCommand", this.AccountForgot)); }
        }

        #endregion

        #region Multi-Window

        private CommandBase _navigateToNewWindowCommand = null;
        /// <summary>
        /// Command to navigate to the account forgot crentials view.
        /// </summary>
        public CommandBase NavigateToNewWindowCommand
        {
            get
            {
                return PlatformBase.Current == null ? null : _navigateToNewWindowCommand ?? (_navigateToNewWindowCommand = new GenericCommand<ViewModelBase>("NavigateToNewWindowCommand", async (e) =>
                {
                    await this.NavigateInNewWindow(e.View.GetType(), e.ViewParameter);
                }));
            }
        }

        #endregion

        #region Feedback Commands

        private CommandBase _navigateToRateAppCommand = null;
        /// <summary>
        /// Command to navigate to the platform's rate application functionality.
        /// </summary>
        public CommandBase NavigateToRateAppCommand
        {
            get { return _navigateToRateAppCommand ?? (_navigateToRateAppCommand = new NavigationCommand("NavigateToRateAppCommand", async () => await this.RateApplicationAsync())); }
        }

        public bool IsFeedbackEnabled
        {
            // Microsoft Store Engagement and Monetization SDK
            // https://visualstudiogallery.msdn.microsoft.com/229b7858-2c6a-4073-886e-cbb79e851211/view/Reviews?sortBy=RatingDescending
            get
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher"))
                    return Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();
                else
                    return false;
            }
        }

        private CommandBase _navigateToFeedbackCommand = null;
        public CommandBase NavigateToFeedbackCommand
        {
            get
            {
                return _navigateToFeedbackCommand ?? (_navigateToFeedbackCommand = new GenericCommand("NavigateToFeedbackCommand", async () =>
                {
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher"))
                    {
                        PlatformBase.GetService<AnalyticsManager>().Event("FeedbackLauncher");
                        await Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
                    }
                    else
                        await Task.CompletedTask;
                }, () => this.IsFeedbackEnabled));
            }
        }

        #endregion

        #region Phone Commands

        private CommandBase _navigateToPhoneCommand = null;
        public CommandBase NavigateToPhoneCommand
        {
            get { return _navigateToPhoneCommand ?? (_navigateToPhoneCommand = new NavigationCommand("NavigateToPhoneCommand", PlatformBase.GetService<NavigationManagerBase>().Phone)); }
        }

        #endregion

        #region Twitter Commands

        private CommandBase _navigateToTwitterScreenNameCommand = null;
        public CommandBase NavigateToTwitterScreenNameCommand
        {
            get { return _navigateToTwitterScreenNameCommand ?? (_navigateToTwitterScreenNameCommand = new GenericCommand<string>("NavigateToTwitterScreenNameCommand", PlatformBase.GetService<NavigationManagerBase>().NavigateToTwitterScreenName)); }
        }

        #endregion

        #endregion
    }
}
