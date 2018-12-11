using AppFramework.Core.Models;
using Contoso.Core;
using Contoso.Core.Models;
using Contoso.Core.Services;
using Contoso.Core.ViewModels;
using Contoso.UI.Views;
using System;

namespace Contoso.UI.Services
{
    /// <summary>
    /// NavigationManager instance with implementations specific to this application.
    /// </summary>
    internal sealed class NavigationManager : NavigationManagerBase
    {
        #region Navigation Methods

        public override void Home(object parameter = null)
        {
            if(Platform.Current.AuthManager?.IsAuthenticated() != true)
            {
                this.Navigate(this.ParentFrame, typeof(WelcomeView), parameter, true);
            }
            else
            {
                // User is authenticated
                if (this.ParentFrame.Content == null || !(this.ParentFrame.Content is ShellView))
                {
                    NavigationRequest navParam = parameter as NavigationRequest ?? new NavigationRequest(typeof(MainView), parameter);
                    this.Navigate(this.ParentFrame, typeof(ShellView), navParam, true);
                }
                else
                {
                    this.Navigate(typeof(MainView), parameter, true);
                }
            }
        }

        public override void Model(object parameter)
        {
            if (parameter == null)
                throw new ArgumentNullException(nameof(parameter));

            if (parameter is ItemModel)
                this.Item(parameter);
            else
                throw new NotImplementedException("Navigation not implemented for type " + parameter.GetType().Name);
        }

        public override void Settings(object parameter = null)
        {
            this.Navigate(typeof(SettingsView), parameter);
        }

        public override void Search(object parameter)
        {
            bool removePrevious = false;

            // If the current page is a search page, remove that page before displaying a new search results page
            if (this.Frame.CurrentSourcePageType == typeof(SearchView))
                removePrevious = true;

            this.Navigate(typeof(SearchView), parameter);

            if (removePrevious)
                this.RemovePreviousPage();
        }

        public override void AccountSignin(object parameter = null)
        {
            this.Navigate(this.ParentFrame, typeof(AccountSignInView), parameter);
        }

        public override void AccountSignup(object parameter = null)
        {
            this.Navigate(this.ParentFrame, typeof(AccountSignUpView), parameter);
        }

        public override void AccountForgot(object parameter = null)
        {
            this.Navigate(this.ParentFrame, typeof(AccountForgotView), parameter);
        }

        public override void Item(object parameter)
        {
            if (this.IsChildFramePresent)
                this.Navigate(typeof(DetailsView), parameter);
            else
                this.Home(new NavigationRequest(typeof(DetailsView), parameter));
        }

        public override void Phone(object model)
        {
            if (model is ItemModel item)
                this.Phone(item.PhoneNumber);
        }

        public override void PrivacyPolicy(object parameter = null)
        {
            this.Settings(SettingsViews.PrivacyPolicy);
        }

        public override void TermsOfService(object parameter = null)
        {
            this.Settings(SettingsViews.TermsOfService);
        }

        #endregion
    }
}