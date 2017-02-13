using Contoso.Core.Models;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml.Controls;

namespace Contoso.Core.Services
{
    public abstract partial class NavigationManagerBase
    {
        #region Abstract Methods

        protected abstract Frame CreateFrame();

        protected abstract bool OnActivation(LaunchActivatedEventArgs e);

        protected abstract bool OnActivation(ToastNotificationActivatedEventArgs e);

        protected abstract bool OnActivation(VoiceCommandActivatedEventArgs e);

        protected abstract bool OnActivation(ProtocolActivatedEventArgs e);

        protected abstract void NavigateToSecondaryWindow(NavigationRequest request);

        public abstract void Home(object parameter = null);

        public abstract void NavigateTo(object model);

        protected abstract void WebView(object parameter);

        public abstract void AccountSignin(object parameter = null);

        public abstract void AccountSignup(object parameter = null);

        public abstract void AccountForgot(object parameter = null);

        public abstract void Settings(object parameter = null);

        public abstract void Search(object parameter = null);

        public abstract void Item(object parameter);

        #endregion

        #region Custom Methods

        public void Phone(object model)
        {
            if (model is ItemModel)
            {
                this.Phone((model as ItemModel).PhoneNumber);
            }
        }

        #endregion
    }
}