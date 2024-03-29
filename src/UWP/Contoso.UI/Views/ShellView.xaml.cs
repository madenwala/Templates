﻿using AppFramework.Core;
using AppFramework.Core.Extensions;
using AppFramework.UI;
using AppFramework.UI.Views;
using Contoso.Core;
using Contoso.Core.ViewModels;
using Microsoft.Advertising.WinRT.UI;
using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseShellView : BaseView<ShellViewModel>
    {
    }
    
    public sealed partial class ShellView : BaseShellView
    {
        #region Constructors

        public ShellView()
        {
            this.InitializeComponent();

            SuspensionManager.RegisterFrame(bodyFrame, "ShellBodyFrame");

            this.Loaded += (sender, e) =>
            {
                bodyFrame.Navigated += BodyFrame_Navigated;
                this.UpdateSelectedMenuItem();
                this.Current_NotifyShellMenuToggle(null, false);
            };

            this.Unloaded += (sender, e) => 
            {
                bodyFrame.Navigated -= BodyFrame_Navigated;
            };

            Platform.Current.NotifyShellMenuToggle += Current_NotifyShellMenuToggle;
        }

        ~ShellView()
        {
            if (Platform.Current != null)
                Platform.Current.NotifyShellMenuToggle -= Current_NotifyShellMenuToggle;
        }

        #endregion

        #region Methods

        protected override void OnApplicationResuming()
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);
            bodyFrame.IsMenuHidden = svMain.DisplayMode == SplitViewDisplayMode.Overlay;

            base.OnApplicationResuming();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // Set the child frame to the navigation service so that it can appropriately perform navigation of pages to the desired frame.
            this.Frame.SetChildFrame(bodyFrame);
            bodyFrame.IsMenuHidden = svMain.DisplayMode == SplitViewDisplayMode.Overlay;

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Remove the bodyFrame as the childFrame
            this.Frame.SetChildFrame(null);
            bodyFrame.IsMenuHidden = false;
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new ShellViewModel());

            return base.OnLoadStateAsync(e);
        }

        /// <summary>
        /// Update the selected item in the shell navigation menu based on watching the body frame navigated event.
        /// </summary>
        private void UpdateSelectedMenuItem()
        {
            var view = bodyFrame.Content;
            if (view is SettingsView)
                btnSettings.IsChecked = true;
            else if (view is SearchView)
                btnSearch.IsChecked = true;
            else if (view is MainView || view is DetailsView)
                btnHome.IsChecked = true;
        }

        #endregion

        #region Event Handlers

        private async void BodyFrame_Navigated(object sender, NavigationEventArgs e)
        {
            // Update the selected item when a page navigation occurs in the body frame
            await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                this.Current_NotifyShellMenuToggle(null, false);
                this.UpdateSelectedMenuItem();
            });
        }

        private void Current_NotifyShellMenuToggle(object sender, bool? e)
        {
            if (svMain.DisplayMode != SplitViewDisplayMode.Inline && svMain.DisplayMode != SplitViewDisplayMode.CompactInline)
            {
                if(e == null)
                    this.ViewModel.IsMenuOpen = !this.ViewModel.IsMenuOpen;
                else
                    this.ViewModel.IsMenuOpen = e.Value;
            }
        }

        protected override void OnKeyUp(KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.GamepadMenu || e.Key == Windows.System.VirtualKey.Home)
            {
                this.Current_NotifyShellMenuToggle(null, null);
                e.Handled = true;
            }

            base.OnKeyUp(e);
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.IsMenuOpen = true;
            searchBox.Focus(FocusState.Programmatic);
        }

        #endregion
    }
}