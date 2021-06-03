﻿using AppFramework.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class AccountSignUpViewBase : ViewBase<AccountSignUpViewModel>
    {
    }

    public sealed partial class AccountSignUpView : AccountSignUpViewBase
    {
        public AccountSignUpView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new AccountSignUpViewModel());

            return base.OnLoadStateAsync(e);
        }

        protected override void OnLoaded(RoutedEventArgs e)
        {
            txtFirstName.Focus(FocusState.Programmatic);
            base.OnLoaded(e);
        }
    }
}