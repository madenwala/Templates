using AppFramework.Core.Commands;
using AppFramework.Core.Extensions;
using AppFramework.Core.Models;
using AppFramework.Core.ViewModels;
using AppFramework.UI.Controls;
using AppFramework.UI.Views;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.Core.Services
{
    public abstract class BaseNavigationManager : BaseService, IServiceSignout
    {
        #region Variables

        public static Dictionary<int, CoreApplicationView> AppWindows { get; private set; }

        #endregion Variables

        #region Properties

        /// <summary>
        /// Gets or sets the frame inside a Window. If not set, the ParentFrame will be returned (the frame inside a Window object).
        /// </summary>
        public Frame Frame
        {
            get
            {
                var frame = Window.Current.Content as Frame;
                return frame.GetChildFrame() ?? frame;
            }
        }

        /// <summary>
        /// Gets access to the Frame inside a Window object.
        /// </summary>
        public Frame ParentFrame
        {
            get
            {
                return Window.Current.Content as Frame;
            }
        }

        /// <summary>
        /// Indicates if the Window currently has a child frame. Used for SplitView pages where there is a child frame.
        /// </summary>
        public bool IsChildFramePresent
        {
            get
            {
                return this.ParentFrame != this.Frame;
            }
        }

        #endregion

        #region Constructors

        static BaseNavigationManager()
        {
            if (Windows.ApplicationModel.DesignMode.DesignModeEnabled)
                return;
            AppWindows = new Dictionary<int, CoreApplicationView>();
        }

        #endregion

        #region Methods

        #region Navigation

        protected void Navigate(Type pageType, object parameter = null)
        {
            this.Navigate(this.Frame, pageType, parameter);
        }

        protected async void Navigate(Frame frame, Type pageType, object parameter = null)
        {
            if (frame == null)
                frame = this.Frame;

            if (frame != null)
            {
                var view = frame.Content as IView;
                if (frame.Content?.GetType() == pageType && view != null && object.Equals(view.ViewParameter, parameter))
                {
                    PlatformBase.CurrentCore.ShellMenuClose();
                    view.ScrollToTop();
                    if (frame.DataContext is ViewModelBase vm)
                        await vm.RefreshAsync(false);
                    return;
                }
                frame.Navigate(pageType, this.SerializeParameter(parameter));
            }
        }

        /// <summary>
        /// Serializes a parameter to string if not a primitive type so that app suspension can properly happen.
        /// </summary>
        /// <param name="obj">Parameter object to serialize.</param>
        /// <returns></returns>
        private object SerializeParameter(object obj)
        {
            return NavigationParameterSerializer.Serialize(obj);
        }

        /// <summary>
        /// Indicates whether or not a back navigation can occur. Will also check to see if the frame contains a WebView and if the WebView can go back as well.
        /// </summary>
        /// <returns>True if a back navigation can occur else false.</returns>
        public bool CanGoBack()
        {
            if (this.Frame == null)
                return false;
            else
            {
                if (this.Frame.DataContext is WebViewModelBase && (this.Frame.DataContext as WebViewModelBase).BrowserCanGoBack())
                    return true;
                else
                    return this.Frame.CanGoBack || this.ParentFrame.CanGoBack;
            }
        }

        /// <summary>
        /// Indicates whether or not a forward navigation can occur. Will also check to see if the frame contains a WebView and if the WebView can go forward as well.
        /// </summary>
        /// <returns>True if a forward navigation can occur else false.</returns>
        public bool CanGoForward()
        {
            if (this.Frame == null)
                return false;
            else
            {
                if (this.Frame.DataContext is WebViewModelBase && (this.Frame.DataContext as WebViewModelBase).BrowserCanGoForward())
                    return true;
                else
                    return this.Frame.CanGoForward || this.ParentFrame.CanGoForward;
            }
        }

        /// <summary>
        /// Navigates back one page. Will also check to see if the frame contains a WebView and if the WebView can go back, it will perform back on that WebView instead.
        /// </summary>
        /// <returns>True if a back navigation occurred else false.</returns>
        public bool GoBack()
        {
            if (this.IsChildFramePresent && this.Frame.CanGoBack)
            {
                if (this.ViewModelHandleBackNavigation(this.Frame))
                {
                    return false;
                }
                else
                {
                    this.Frame.GoBack();
                    return true;
                }
            }

            if (this.ParentFrame.CanGoBack)
            {
                if (this.ViewModelHandleBackNavigation(this.ParentFrame))
                {
                    return false;
                }
                else
                {
                    this.ParentFrame.GoBack();
                    return true;
                }
            }

            return false;
        }

        private CommandBase _GoBackCommand = null;
        /// <summary>
        /// Command to access backwards page navigation..
        /// </summary>
        public CommandBase GoBackCommand
        {
            get { return _GoBackCommand ?? (_GoBackCommand = new NavigationCommand("NavigateGoBackCommand", () => this.GoBack(), this.CanGoBack)); }
        }

        /// <summary>
        /// Navigates forward one page. Will also check to see if the frame contains a WebView and if the WebView can go forward, it will perform forward on that WebView instead.
        /// </summary>
        /// <returns>True if a forward navigation occurred else false.</returns>
        public bool GoForward()
        {
            // Check if the frame contains a WebView and if it can go forward
            if (this.Frame.DataContext is WebViewModelBase)
            {
                var vm = this.Frame.DataContext as WebViewModelBase;
                if (vm.BrowserCanGoForward())
                {
                    vm.BrowserGoForward();
                    return true;
                }
            }

            // Check the child frame to go forward
            if (this.IsChildFramePresent && this.Frame.CanGoForward)
            {
                if (this.ViewModelHandleForwardNavigation(this.Frame))
                {
                    return false;
                }
                else
                {
                    this.Frame.GoForward();
                    return true;
                }
            }

            // Finally check the parent frame to go forward
            if (this.ParentFrame.CanGoForward)
            {
                if (this.ViewModelHandleForwardNavigation(this.ParentFrame))
                {
                    return false;
                }
                else
                {
                    this.ParentFrame.GoForward();
                    return true;
                }
            }

            // Nothing can go forward, return false.
            return false;
        }

        private CommandBase _GoForwardCommand = null;
        /// <summary>
        /// Command to access forard page navigation.
        /// </summary>
        public CommandBase GoForwardCommand
        {
            get { return _GoForwardCommand ?? (_GoForwardCommand = new NavigationCommand("NavigateGoForwardCommand", () => this.GoForward(), this.CanGoForward)); }
        }

        /// <summary>
        /// Checks a ViewModels to see if it will allow a nagivation back.
        /// </summary>
        /// <param name="frame">Frame to check.</param>
        /// <returns>True if allowed else false.</returns>
        private bool ViewModelHandleBackNavigation(Frame frame)
        {
            if (frame.DataContext is ViewModelBase)
            {
                var vm = frame.DataContext as ViewModelBase;
                return vm.BackNavigationRequested();
            }
            else
                return false;
        }

        /// <summary>
        /// Checks a ViewModels to see if it will allow a nagivation forward.
        /// </summary>
        /// <param name="frame">Frame to check.</param>
        /// <returns>True if allowed else false.</returns>
        private bool ViewModelHandleForwardNavigation(Frame frame)
        {
            if (frame.DataContext is ViewModelBase)
            {
                var vm = frame.DataContext as ViewModelBase;
                return vm.ForwardNavigationRequested();
            }
            else
                return false;
        }

        /// <summary>
        /// Clears the navigation backstack of the window.
        /// </summary>
        public void ClearBackstack()
        {
            this.Frame?.BackStack.Clear();
            this.ParentFrame?.BackStack.Clear();
            this.UpdateTitleBarBackButton();
        }

        /// <summary>
        /// Removes the previous page in the navigation backstack.
        /// </summary>
        public void RemovePreviousPage()
        {
            if (this.IsChildFramePresent && this.Frame.BackStack.Count > 1)
            {
                this.Frame.BackStack.RemoveAt(this.Frame.BackStack.Count - 1);
                return;
            }
            if (this.ParentFrame.BackStack.Count > 1)
                this.ParentFrame.BackStack.RemoveAt(this.ParentFrame.BackStack.Count - 1);
        }

        /// <summary>
        /// Updates the navigate back button in the app window's title bar.
        /// </summary>
        public void UpdateTitleBarBackButton()
        {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = this.CanGoBack() ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
        }

        #endregion Navigation

        #region Activation/Deactivation

        /// <summary>
        /// Initialization logic which is called on launch of this application.
        /// </summary>
        protected override Task OnInitializeAsync()
        {
            // Register for phone hardware back button press
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
                Windows.Phone.UI.Input.HardwareButtons.BackPressed += HardwareButtons_BackPressed;

            /// Register the main window of the application
            this.RegisterCoreWindow();

            return base.OnInitializeAsync();
        }

        /// <summary>
        /// Registers the window with all window events.
        /// </summary>
        private void RegisterCoreWindow()
        {
            SystemNavigationManager.GetForCurrentView().BackRequested += ViewBase_BackRequested;
            Window.Current.CoreWindow.Dispatcher.AcceleratorKeyActivated += CoreDispatcher_AcceleratorKeyActivated;
            Window.Current.CoreWindow.PointerPressed += this.CoreWindow_PointerPressed;
        }

        /// <summary>
        /// Handle activation of the event and any navigation necessary.
        /// </summary>
        /// <param name="e">Activation args.</param>
        /// <param name="rootFrame">Root frame of the app.</param>
        public void HandleActivation(IActivatedEventArgs e, Frame rootFrame)
        {
            var handled = false;

            try
            {
                PlatformBase.CurrentCore.Analytics.Event("HandleActivation", e.Kind);

                switch (e.Kind)
                {
                    case ActivationKind.Launch:
                        var launchArg = e as LaunchActivatedEventArgs;
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Calling OnActivation({0})  TileID: {1}  Arguments: {2}", e?.GetType().Name, launchArg.TileId, launchArg.Arguments);
                        handled = this.OnActivation(launchArg);
                        break;

                    case ActivationKind.VoiceCommand:
                        var voiceArgs = e as VoiceCommandActivatedEventArgs;
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Calling OnActivation({0})", e?.GetType().Name);
                        handled = this.OnActivation(voiceArgs);
                        break;

                    case ActivationKind.ToastNotification:
                        var toastArgs = e as ToastNotificationActivatedEventArgs;
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Calling OnActivation({0})  Arguments: {1}", e?.GetType().Name, toastArgs.Argument);
                        handled = this.OnActivation(toastArgs);
                        break;

                    case ActivationKind.Protocol:
                        var protocolArgs = e as ProtocolActivatedEventArgs;
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Calling OnActivation({0})  Arguments: {1}", e?.GetType().Name, protocolArgs.Uri.ToString());
                        handled = this.OnActivation(protocolArgs);
                        break;

                    default:
                        PlatformBase.CurrentCore.Logger.LogError(new Exception(string.Format("Can't call OnActivation({0}) as it's not implemented!", e.Kind)));
                        handled = false;
                        break;
                }

                if (handled == false || rootFrame?.Content == null)
                    this.Home();

                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "Completed Navigation.HandleActivation({0}) on RootFrame: {1} --- OnActivation Handled? {2}", e?.GetType().Name, rootFrame?.Content?.GetType().Name, handled);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error during App Navigation.HandleActivation({0}) on RootFrame: {1}", e?.GetType().Name, rootFrame?.Content?.GetType().Name);
                throw ex;
            }
        }

        /// <summary>
        /// Handles actions from primary and secondary tiles and jump lists.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnActivation(LaunchActivatedEventArgs e)
        {
            var handled = this.HandleArgumentsActivation(e.Arguments);

            if (handled == false && PlatformBase.CurrentCore.InitializationMode == InitializationModes.Restore)
                handled = true;

            return handled;
        }

        /// <summary>
        /// Handles activations from toasts activations.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnActivation(ToastNotificationActivatedEventArgs e)
        {
            return this.HandleArgumentsActivation(e.Argument);
        }

        /// <summary>
        /// Handles protocol activation i.e. contoso:4
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnActivation(ProtocolActivatedEventArgs e)
        {
            return this.HandleArgumentsActivation(e.Uri.AbsoluteUri.Replace(PlatformBase.CurrentCore.AppInfo.ProtocolPrefix, "").Split(':').Last());
        }

        /// <summary>
        /// Handles voice commands from Cortana integration
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private bool OnActivation(VoiceCommandActivatedEventArgs e)
        {
            var info = new VoiceCommandInfo(e.Result);
            return OnVoiceActivation(info);
        }

        protected virtual bool OnVoiceActivation(VoiceCommandInfo info)
        {
            return false;
        }

        private bool HandleArgumentsActivation(string arguments)
        {
            if (string.IsNullOrWhiteSpace(arguments))
                return false;

            PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "HandleArgumentsActivation: {0}", arguments);

            try
            {
                var dic = GeneralFunctions.ParseQuerystring(arguments);
                return this.OnHandleArgumentsActivation(arguments, dic);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Could not parse argument '{0}' passed into app.", arguments);
                return false;
            }
        }

        protected virtual bool OnHandleArgumentsActivation(string arguments, IDictionary<string, string> dic)
        {
            return false;
        }

        /// <summary>
        /// Exits an application.
        /// </summary>
        public void Exit()
        {
            PlatformBase.CurrentCore.Analytics.Event("ApplicationExit");
            Application.Current.Exit();
        }

        #endregion

        #region Authentication

        /// <summary>
        /// On signout of a user, close all secondary windows that might be open.
        /// </summary>
        /// <returns></returns>
        public async Task SignoutAsync()
        {
            try
            {
                // Close all secondary windows
                if (AppWindows.Count > 1)
                {
                    var currentView = CoreApplication.GetCurrentView();
                    var currentWindowID = ApplicationView.GetApplicationViewIdForWindow(currentView.CoreWindow);

                    var ids = AppWindows.Keys.ToArray().Where(w => w != currentWindowID);
                    foreach (var otherWindowID in ids)
                        await ApplicationViewSwitcher.SwitchAsync(currentWindowID, otherWindowID, ApplicationViewSwitchingOptions.ConsolidateViews);
                }
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Could not close all secondary windows on Signout!");
                throw ex;
            }
        }

        #endregion

        #region Maps

        public void MapExternal(ILocationModel loc, string label = null, MapExternalOptions mapOption = MapExternalOptions.Normal, double zoomLevel = 16)
        {
            if (loc == null)
                throw new ArgumentNullException(nameof(loc));

            PlatformBase.CurrentCore.Analytics.Event("MapExternal-" + mapOption.ToString());

            label = System.Net.WebUtility.HtmlEncode(label ?? loc.LocationDisplayName);
            string url = null;

            switch (mapOption)
            {
                case MapExternalOptions.DrivingDirections:
                    {
                        url = string.Format("ms-drive-to:?destination.latitude={0}&destination.longitude={1}&destination.name={2}", loc.Latitude.ToString(CultureInfo.InvariantCulture), loc.Longitude.ToString(CultureInfo.InvariantCulture), label);
                        var t = Launcher.LaunchUriAsync(new Uri(url));
                        break;
                    }

                case MapExternalOptions.WalkingDirections:
                    {
                        url = string.Format("ms-walk-to:?destination.latitude={0}&destination.longitude={1}&destination.name={2}", loc.Latitude.ToString(CultureInfo.InvariantCulture), loc.Longitude.ToString(CultureInfo.InvariantCulture), label);
                        var t = Launcher.LaunchUriAsync(new Uri(url));
                        break;
                    }

                default:
                    {
                        url = string.Format("bingmaps:?collection=point.{0}_{1}_{2}&lvl={3}", loc.Latitude.ToString(CultureInfo.InvariantCulture), loc.Longitude.ToString(CultureInfo.InvariantCulture), label, zoomLevel.ToString(CultureInfo.InvariantCulture));
                        var t = Launcher.LaunchUriAsync(new Uri(url));
                        break;
                    }
            }
        }

        private CommandBase _MapExternalCommand = null;
        /// <summary>
        /// Command to access the external maps view.
        /// </summary>
        public CommandBase MapExternalCommand
        {
            get { return _MapExternalCommand ?? (_MapExternalCommand = new MapExternalCommand()); }
        }

        private CommandBase _MapExternalDrivingCommand = null;
        /// <summary>
        /// Command to access the device's map driving directions view.
        /// </summary>
        public CommandBase MapExternalDrivingCommand
        {
            get { return _MapExternalDrivingCommand ?? (_MapExternalDrivingCommand = new MapExternalCommand(MapExternalOptions.DrivingDirections)); }
        }

        private CommandBase _MapExternalWalkingCommand = null;
        /// <summary>
        /// Command to access the device's map walking directions view.
        /// </summary>
        public CommandBase MapExternalWalkingCommand
        {
            get { return _MapExternalWalkingCommand ?? (_MapExternalWalkingCommand = new MapExternalCommand(MapExternalOptions.WalkingDirections)); }
        }

        #endregion

        #region Phone

        public virtual void Phone(object model)
        {
            throw new NotImplementedException("Phone virtual method has not been overridden!");
        }

        public void Phone(string phoneNumber, string displayName = null)
        {
            phoneNumber = GeneralFunctions.ConvertPhoneLettersToNumbers(phoneNumber);
            Windows.ApplicationModel.Calls.PhoneCallManager.ShowPhoneCallUI(phoneNumber, displayName);
        }

        private CommandBase _PhoneCommand = null;
        public CommandBase PhoneCommand
        {
            get { return _PhoneCommand ?? (_PhoneCommand = new NavigationCommand("NavigateToPhoneCommand", this.Phone)); }
        }

        #endregion

        #region Web

        public void WebView(string webAddress)
        {
            this.WebView(new WebViewArguments(webAddress));
        }

        public void WebView(string webAddress, bool showNavigationBar, string title = null)
        {
            this.WebView(new WebViewArguments(webAddress, showNavigationBar, title));
        }
        
        public void WebView(WebViewArguments args)
        {
            this.Navigate(typeof(UI.Views.WebView), args);
        }

        /// <summary>
        /// Navigates to an external web browser.
        /// </summary>
        /// <param name="webAddress">URL to navigate to.</param>
        public void WebBrowser(string webAddress)
        {
            PlatformBase.CurrentCore.Analytics.Event("NavigateToWebBrowser");
            var t = Launcher.LaunchUriAsync(new Uri(webAddress, UriKind.Absolute));
        }

        private CommandBase _WebViewCommand = null;
        /// <summary>
        /// Command to navigate to the internal web view.
        /// </summary>
        public CommandBase WebViewCommand
        {
            get { return _WebViewCommand ?? (_WebViewCommand = new WebViewCommand()); }
        }

        private CommandBase _WebBrowserCommand = null;
        /// <summary>
        /// Command to navigate to the external web browser.
        /// </summary>
        public CommandBase WebBrowserCommand

        {
            get { return _WebBrowserCommand ?? (_WebBrowserCommand = new WebBrowserCommand()); }
        }

        #endregion

        #region App Links

        public void NavigateToTwitterScreenName(string screenname)
        {
            string url = "twitter:";
            if (!string.IsNullOrEmpty(screenname))
                url += "//user?screen_name=" + (screenname.StartsWith("@") ? screenname.Substring(1) : screenname);

            PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"Launching Twitter @{screenname} -- {url}");
            var t = Launcher.LaunchUriAsync(new Uri(url, UriKind.Absolute));
        }

        #endregion

        #region Secondary Windows

        /// <summary>
        /// Navigates to a page specified in the navigation request object.
        /// </summary>
        /// <param name="request">Request object instance.</param>
        public void Navigate(NavigationRequest request)
        {
            if (request != null)
                this.Frame.Navigate(Type.GetType(request.ViewType), this.SerializeParameter(request.ViewParameter));
        }

        protected virtual void SecondaryWindow(NavigationRequest request)
        {
            this.Navigate(typeof(SecondaryWindowView), request);
        }

        /// <summary>
        /// Launches another window with the specified page type.
        /// </summary>
        /// <param name="viewType">Type of the page requested in the secondary window.</param>
        /// <param name="parameter">Page parameter to pass to the new page instance.</param>
        /// <returns>Awaitable task is returned.</returns>
        public virtual async Task NewWindow(Type viewType, object parameter)
        {
            try
            {
                // Create a new window
                var view = CoreApplication.CreateNewView();
                int windowID = 0;
                await view.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    windowID = ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread());

                    // Register the new window
                    this.RegisterCoreWindow();

                    // Create a frame for the new window
                    var frame = new ApplicationFrame(true);
                    Window.Current.Content = frame;

                    // Navigate to a page within the new window based on the parameters of this method
                    this.SecondaryWindow(new NavigationRequest(viewType, this.SerializeParameter(parameter)));

                    // Show the new window
                    Window.Current.Activate();
                    ApplicationView.GetForCurrentView().Consolidated += View_Consolidated;

                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"Launched new window");
                });

                // Run this on the last dispatcher so the windows get positioned correctly
                bool viewShown;
                await AppWindows[AppWindows.Keys.First()].Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                {
                    viewShown = await ApplicationViewSwitcher.TryShowAsStandaloneAsync(windowID);
                });

                // Track the new window
                AppWindows.Add(windowID, view);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Could not create new window for view type {0}.", viewType.Name);
                throw ex;
            }
        }

        private void View_Consolidated(ApplicationView sender, ApplicationViewConsolidatedEventArgs args)
        {
            var windowID = ApplicationView.GetApplicationViewIdForWindow(CoreWindow.GetForCurrentThread());
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"Closed secondary window with ID {windowID}");
            AppWindows.Remove(windowID);
            ApplicationView.GetForCurrentView().Consolidated -= View_Consolidated;
            Window.Current.Close();
        }

        private CommandBase _NewWindowCommand = null;
        /// <summary>
        /// Command to navigate to the account forgot crentials view.
        /// </summary>
        public CommandBase NewWindowCommand
        {
            get
            {
                return _NewWindowCommand ?? (_NewWindowCommand = new GenericCommand<ViewModelBase>("NavigateToNewWindowCommand", async (e) =>
                {
                    if(e != null && e.View != null)
                        await this.NewWindow(e.View.GetType(), e.ViewParameter);
                    else
                        PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"Could not open a new window with parameter {e.GetType().FullName} because it is not a ViewModelBase or ViewModelBase.View is null.");
                }));
            }
        }

        #endregion

        #region Feedback Commands

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

        private CommandBase _FeedbackCommand = null;
        public CommandBase FeedbackCommand
        {
            get
            {
                return _FeedbackCommand ?? (_FeedbackCommand = new GenericCommand("NavigateToFeedbackCommand", async () =>
                {
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher"))
                    {
                        PlatformBase.CurrentCore.Analytics.Event("FeedbackLauncher");
                        await Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
                    }
                    else
                        await Task.CompletedTask;
                }, () => this.IsFeedbackEnabled));
            }
        }

        #endregion

        #region Twitter Commands

        private CommandBase _TwitterScreenNameCommand = null;
        public CommandBase TwitterScreenNameCommand
        {
            get { return _TwitterScreenNameCommand ?? (_TwitterScreenNameCommand = new GenericCommand<string>("NavigateToTwitterScreenNameCommand", this.NavigateToTwitterScreenName)); }
        }

        #endregion

        #region Common Pages

        public abstract void Home(object parameter = null);

        private CommandBase _HomeCommand = null;
        /// <summary>
        /// Command to access backwards page navigation..
        /// </summary>
        public CommandBase HomeCommand
        {
            get { return _HomeCommand ?? (_HomeCommand = new NavigationCommand("NavigateToHomeCommand", this.Home)); }
        }

        public abstract void Model(object model);

        private CommandBase _ModelCommand = null;
        /// <summary>
        /// Command to access navigating to an instance of a model (Navigation manager handles actually forwarding to the appropriate view for 
        /// the model pass into a parameter. 
        /// </summary>
        public CommandBase ModelCommand
        {
            get { return _ModelCommand ?? (_ModelCommand = new NavigationCommand()); }
        }

        public abstract void Settings(object parameter = null);

        private CommandBase _SettingsCommand = null;
        /// <summary>
        /// Command to navigate to the settings view.
        /// </summary>
        public CommandBase SettingsCommand
        {
            get { return _SettingsCommand ?? (_SettingsCommand = new NavigationCommand("NavigateToSettingsCommand", this.Settings)); }
        }

        public abstract void PrivacyPolicy(object parameter = null);

        private CommandBase _PrivacyPolicyCommand = null;
        /// <summary>
        /// Command to navigate to the application's privacy policy view.
        /// </summary>
        public CommandBase PrivacyPolicyCommand
        {
            get { return _PrivacyPolicyCommand ?? (_PrivacyPolicyCommand = new NavigationCommand("NavigateToPrivacyPolicyCommand", this.PrivacyPolicy)); }
        }

        public abstract void TermsOfService(object parameter = null);

        private CommandBase _TermsOfServiceCommand = null;
        /// <summary>
        /// Command to navigate to the application's terms of service view.
        /// </summary>
        public CommandBase TermsOfServiceCommand
        {
            get { return _TermsOfServiceCommand ?? (_TermsOfServiceCommand = new NavigationCommand("NavigateToTermsOfServiceCommand", this.TermsOfService)); }
        }

        #endregion

        #endregion

        #region Event Handlers

        private void ViewBase_BackRequested(object sender, BackRequestedEventArgs e)
        {
            e.Handled = this.GoBack();
        }

        /// <summary>
        /// Invoked when the hardware back button is pressed. For Windows Phone only.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void HardwareButtons_BackPressed(object sender, Windows.Phone.UI.Input.BackPressedEventArgs e)
        {
            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "Hardware back button pressed.");
            e.Handled = this.GoBack();
        }

        /// <summary>
        /// Invoked on every keystroke, including system keys such as Alt key combinations, when
        /// this page is active and occupies the entire window.  Used to detect keyboard navigation
        /// between pages even when the page itself doesn't have focus.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void CoreDispatcher_AcceleratorKeyActivated(CoreDispatcher sender, AcceleratorKeyEventArgs e)
        {
            var virtualKey = e.VirtualKey;

            // Only investigate further when Left, Right, or the dedicated Previous or Next keys
            // are pressed
            if ((e.EventType == CoreAcceleratorKeyEventType.SystemKeyDown
                || e.EventType == CoreAcceleratorKeyEventType.KeyDown) &&
                (
                virtualKey == VirtualKey.Left || virtualKey == VirtualKey.Right
                || virtualKey == VirtualKey.GoBack || virtualKey == VirtualKey.GoForward
                )
                )
            {
                var coreWindow = Window.Current.CoreWindow;
                var downState = CoreVirtualKeyStates.Down;
                bool menuKey = (coreWindow.GetKeyState(VirtualKey.Menu) & downState) == downState;
                bool controlKey = (coreWindow.GetKeyState(VirtualKey.Control) & downState) == downState;
                bool shiftKey = (coreWindow.GetKeyState(VirtualKey.Shift) & downState) == downState;
                bool noModifiers = !menuKey && !controlKey && !shiftKey;
                bool onlyAlt = menuKey && !controlKey && !shiftKey;


                if (((int)virtualKey == 166 && noModifiers) ||
                    (virtualKey == VirtualKey.Left && onlyAlt))
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"Windows accelerator keyboard key pressed to go back: {virtualKey}");
                    e.Handled = this.GoBack();
                }
                else if (((int)virtualKey == 167 && noModifiers) ||
                    (virtualKey == VirtualKey.Right && onlyAlt))
                {

                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"Windows accelerator key pressed to go forward: {virtualKey}");
                    // When the next key or Alt+Right are pressed navigate forward
                    e.Handled = this.GoForward();
                }
            }
        }

        /// <summary>
        /// Invoked on every mouse click, touch screen tap, or equivalent interaction when this
        /// page is active and occupies the entire window.  Used to detect browser-style next and
        /// previous mouse button clicks to navigate between pages.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void CoreWindow_PointerPressed(CoreWindow sender, PointerEventArgs e)
        {
            var properties = e.CurrentPoint.Properties;

            // Ignore button chords with the left, right, and middle buttons
            if (properties.IsLeftButtonPressed || properties.IsRightButtonPressed ||
                properties.IsMiddleButtonPressed) return;

            // If back or foward are pressed (but not both) navigate appropriately
            bool backPressed = properties.IsXButton1Pressed;
            bool forwardPressed = properties.IsXButton2Pressed;
            if (backPressed ^ forwardPressed)
            {
                if (backPressed)
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Windows accelerator mouse key pressed to go back.");
                    e.Handled = this.GoBack();
                }
                if (forwardPressed)
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Windows accelerator mouse key pressed to go forward.");
                    e.Handled = this.GoForward();
                }
            }
        }

        #endregion
    }

    #region Classes

    internal static class NavigationParameterSerializer
    {
        /// <summary>
        /// Serializes an object if its a non-primitive type.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static object Serialize(object obj)
        {
            if (obj == null)
                return null;
            else if (obj.GetType().GetTypeInfo().IsEnum)
                // Convert enums to int
                return (int)obj;
            else if (TypeUtility.IsPrimitive(obj.GetType()))
                // Return primitive types as-is
                return obj;
            else
            {
                // Only serialize non-primitive types to string
                var dic = new Dictionary<string, string>();
                dic.Add("Type", obj.GetType().AssemblyQualifiedName);
                dic.Add("Parameter", Serializer.Serialize(obj, SerializerTypes.Json));
                return GeneralFunctions.CreateQuerystring(dic);
            }
        }

        /// <summary>
        /// Deserializes an object if its a string and has serialization info else returns the object as-is.
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public static object Deserialize(object parameter)
        {
            if (parameter is string)
            {
                var p = parameter.ToString();
                if (p.StartsWith("Type="))
                {
                    var dic = GeneralFunctions.ParseQuerystring(p);
                    var type = Type.GetType(dic["Type"]);
                    var data = dic["Parameter"];
                    return Newtonsoft.Json.JsonConvert.DeserializeObject(data, type);
                }
            }

            return parameter;
        }
    }

    #endregion
}

namespace AppFramework.Core.Models
{
    public sealed class WebViewArguments
    {
        #region Properties

        private string _webAddress;
        public string WebAddress
        {
            get
            {
                // if the URL is a twitter handle, forward to Twitter.com
                if (!string.IsNullOrEmpty(_webAddress) && _webAddress.StartsWith("@") && _webAddress.Length > 1)
                    return "https://twitter.com/" + _webAddress.Substring(1);
                else
                    return _webAddress;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                    value = value.Trim();

                _webAddress = value;
            }
        }

        public string Title { get; set; }

        public bool ShowNavigationBar { get; set; }

        #endregion

        #region Constructors

        public WebViewArguments(string webAddress = null, bool showNavigationBar = false, string title = null)
        {
            this.WebAddress = webAddress;
            this.ShowNavigationBar = showNavigationBar;
            this.Title = title;
        }

        #endregion
    }

    /// <summary>
    /// Represents navigation instructions that can be serialized and performed at a later time.
    /// </summary>
    public sealed class NavigationRequest
    {
        public NavigationRequest(Type viewType = null, object viewParameter = null)
        {
            this.ViewType = viewType?.AssemblyQualifiedName;
            this.ViewParameter = viewParameter;
        }

        /// <summary>
        /// Full type name of a view/page that needs to be instantiated.
        /// </summary>
        public string ViewType { get; set; }

        /// <summary>
        /// Object instance to pass in as a parameter.
        /// </summary>
        public object ViewParameter { get; set; }
    }
}