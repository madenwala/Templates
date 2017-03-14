﻿using AppFramework.Core;
using AppFramework.Core.Services;
using Contoso.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class SecondaryWindowViewBase : ViewBase<ViewModelBase>
    {
    }

    public sealed partial class SecondaryWindowView : SecondaryWindowViewBase
    {
        #region Constructors

        public SecondaryWindowView()
        {
            this.InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnApplicationResuming()
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);

            base.OnApplicationResuming();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Remove the bodyFrame as the childFrame
            this.Frame.SetChildFrame(null);
        }

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New && e.Parameter is NavigationRequest)
                Platform.Current.Navigation.NavigateTo(e.Parameter as NavigationRequest);

            await base.OnLoadStateAsync(e);
        }

        #endregion
    }
}
