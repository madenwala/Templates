﻿using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.ViewModels;
using AppFramework.UI.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AppFramework.UI.Views
{
    /// <summary>
    /// Base class for all pages in your application.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract partial class ViewBase<TViewModel> : Page, IView, INotifyPropertyChanged, IViewScrollToTop where TViewModel : ViewModelBase
    {
        #region Properties

        private bool _isInitialized = false;

        private string _pageKey { get; set; }

        private TViewModel _ViewModel;
        /// <summary>
        /// Gets access to the view model instance associated to this page. Used for x:Bind capabilities in the page's XAML.
        /// </summary>
        public TViewModel ViewModel
        {
            get { return _ViewModel; }
            private set { this.SetProperty(ref _ViewModel, value); }
        }
        
        private bool _IsInView;
        /// <summary>
        /// True if the view is visible to the user else false.
        /// </summary>
        public bool IsInView
        {
            get { return _IsInView; }
            private set { this.SetProperty(ref _IsInView, value); }
        }

        public object ViewParameter
        {
            get
            {
                return this.ViewModel?.ViewParameter;
            }
        }

        /// <summary>
        /// Event for knowing when page properties change.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region Constructors

        public ViewBase()
        {
            this.NavigationCacheMode = NavigationCacheMode.Enabled;

            if (DesignMode.DesignModeEnabled)
                return;

            // Logging and analytics
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "New View Instance: {0}", this.GetType().Name);
            PlatformBase.CurrentCore.Analytics.NewPageView(this.GetType());

            // Wire up events
            this.Loaded += ViewBase_Loaded;
            this.KeyUp += ViewBase_KeyUp;
            this.KeyDown += ViewBase_KeyDown;

            if(PlatformBase.IsDebugMode)
                this.GotFocus += ViewBase_GotFocus;
        }

        #endregion

        #region Methods

        #region Navigation

        private void ViewBase_Loaded(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Update visibility of the BACK button in the titlebar area
            PlatformBase.CurrentCore.NavigationBase.UpdateTitleBarBackButton();

            this.OnLoaded(e);
        }

        protected virtual void OnLoaded(Windows.UI.Xaml.RoutedEventArgs e)
        {
        }

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            this.IsInView = true;

            try
            {
                this.InitializeInterstitialAds();

                // Set the datacontext of the frame so that it can appropriately show the busy panel or not when a view model requests it
                this.Frame.DataContext = this.ViewModel;

                PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "OnNavigatedTo: {0}\t Mode: {1}\t Parameter: {2}", e.SourcePageType.Name, e.NavigationMode, e.Parameter);

                // Chck for data in session state
                Dictionary<string, Object> dic = null;
                var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
                _pageKey = "Page-" + this.Frame.BackStackDepth;
                if (e.NavigationMode == NavigationMode.New)
                {
                    // Clear existing state for forward navigation when adding a new page to the
                    // navigation stack
                    var nextPageKey = _pageKey;
                    int nextPageIndex = this.Frame.BackStackDepth;
                    while (frameState.Remove(nextPageKey))
                    {
                        nextPageIndex++;
                        nextPageKey = "Page-" + nextPageIndex;
                    }

                    // Pass the navigation parameter to the new page
                    dic = new Dictionary<string, object>();
                }
                else
                {
                    // Pass the navigation parameter and preserved page state to the page, using
                    // the same strategy for loading suspended state and recreating pages discarded
                    // from cache
                    if(frameState.ContainsKey(_pageKey))
                        dic = (Dictionary<string, object>)frameState[_pageKey];
                    else
                        dic = new Dictionary<string, object>();
                }

                // Wrapper object for all the navigated to event data and session state
                var args = new LoadStateEventArgs(e, dic, _isInitialized);

                // Pass navigation event data back to the page so that it has a chance to do any custom logic with access to the page state dictionary.
                await this.OnLoadStateAsync(args);

                // Pass navigation event data on to the ViewModel
                if (this.ViewModel != null)
                    await this.ViewModel.LoadStateAsync(this, args);
            }
            catch(Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during {0}.OnNavigatedTo: {1} Parameter: {2}", e.SourcePageType.Name, e.NavigationMode, e.Parameter);
                throw ex;
            }
            finally
            {
                _isInitialized = true;
            }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        protected virtual Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        protected virtual Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            return Task.CompletedTask;
        }

        protected override async void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            this.IsInView = false;

            // User left the app, wire up the resuming event in case they come back and we can restore any custom state necessary
            if(e.NavigationMode == NavigationMode.Forward)
                Windows.UI.Xaml.Application.Current.Resuming += Application_Resuming;

            try
            {
                // Remove the view model instance from the frame's datacontext
                this.Frame.DataContext = null;

                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "OnNavigatedFrom: {0}\t Mode: {1}", e.SourcePageType.Name, e.NavigationMode);

                // Intialize page state for this page within the current frame
                var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
                var pageState = new Dictionary<string, object>();

                // Wrapper object for all the navigated from event data and session state
                var args = new SaveStateEventArgs(e, pageState);

                // Pass navigation event data on to the ViewModel
                if (this.ViewModel != null)
                    await this.ViewModel.SaveStateAsync(args);

                // Pass navigation event data back to the page so that it has a chance to do any custom logic with access to the page state dictionary.
                await this.OnSaveStateAsync(args);

                // Save session state for the page
                frameState[_pageKey] = pageState;

                this.TeardownInterstitialAds();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during {0}.OnNavigatedFrom: {1} Parameter: {2}", e.SourcePageType.Name, e.NavigationMode, e.Parameter);
                throw ex;
            }
        }

        /// <summary>
        /// Event handler for application resume
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Application_Resuming(object sender, object e)
        {
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "APPLICATION RESUMED ON VIEW {0}", this.GetType().Name);
            try
            {
                // Set the ViewModel again on resume of the app as the datacontext on UI elements may not have it after resume
                this.SetViewModel(this.ViewModel);
                this.OnApplicationResuming();
                PlatformBase.CurrentCore.ShellMenuClose();
            }
            catch(Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "ERROR during Application Resume on view: {0}", this.GetType().Name);
                throw ex;
            }
            finally
            {
                Windows.UI.Xaml.Application.Current.Resuming -= Application_Resuming;
            }
        }

        /// <summary>
        /// Perform any custom logic for when the OS resumes your app.
        /// </summary>
        protected virtual void OnApplicationResuming()
        {
            this.ViewModel?.OnApplicationResuming();
        }

        #endregion

        #region Back Requested

        /// <summary>
        /// Allows a view to prevent forward navigation if it needs to.
        /// </summary>
        /// <returns>True if forward navigation should be cancelled else false.</returns>
        public bool OnForwardNavigationRequested()
        {
            return false;
        }

        /// <summary>
        /// Allows a view to prevent back navigation if it needs to.
        /// </summary>
        /// <returns>True if back navigation should be cancelled else false.</returns>
        public virtual bool OnBackNavigationRequested()
        {
            if (PlatformBase.CurrentCore.NavigationBase.ParentFrame is ApplicationFrame frame)
                return frame.CheckIfAdsOpen();
            else
                return false;
        }

        #endregion

        #region Data Binding

        /// <summary>
        /// Sets a view model instance to the page for data binding and for x:Bind.
        /// </summary>
        /// <param name="vm"></param>
        protected void SetViewModel(TViewModel vm)
        {
            if (vm != null)
                vm.View = this;
            this.DataContext = this.Frame.DataContext = this.ViewModel = vm;
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                this.NotifyPropertyChanged(propertyName);
                return true;
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        #region ScrollToTop

        public virtual void ScrollToTop()
        {
            ScrollToTopHelper.ScrollToTop(this.Content);
        }

        #endregion

        #endregion

        #region Event Handlers

        private void ViewBase_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var element = Windows.UI.Xaml.Input.FocusManager.GetFocusedElement() as Windows.UI.Xaml.FrameworkElement;
            var name = element?.Name;
            if (string.IsNullOrEmpty(name)) name = "<NotNamed>";
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "{0}_GotFocus: {1} ({2}) received focus", this.GetType().Name, name, element?.GetType().Name);
        }

        private void ViewBase_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            var element = e.OriginalSource as FrameworkElement;
            var name = element?.Name;
            if (string.IsNullOrEmpty(name)) name = "<NotNamed>";
            var msg = string.Format("{0}_KeyUp: {1} was pressed while focus was on {2} ({3})", this.GetType().Name, e.Key, name, element?.GetType().Name);

            try
            {
                this.ViewModel.OnHandleKeyUp(e);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during " + msg);
            }

            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, msg);
        }

        private void ViewBase_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            try
            {
                this.ViewModel.OnHandleKeyDown(e);
            }
            catch (Exception ex)
            {
                var element = e.OriginalSource as FrameworkElement;
                var msg = string.Format("{0}_KeyDown: {1} was pressed while focus was on {2} ({3})", this.GetType().Name, e.Key, element?.Name, element?.GetType().Name);
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during " + msg);
            }
        }

        #endregion
    }
}