using AppFramework.Core.Commands;
using AppFramework.Core.Data;
using AppFramework.Core.Extensions;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Devices.Geolocation;
using Windows.Networking.Connectivity;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace AppFramework.Core.ViewModels
{
    public abstract class ViewModelBase : ModelBase, IDisposable, IViewModel
    {
        #region Constants

        private const int E_WINHTTP_TIMEOUT = unchecked((int)0x80072ee2);
        private const int E_WINHTTP_NAME_NOT_RESOLVED = unchecked((int)0x80072ee7);
        private const int E_WINHTTP_CANNOT_CONNECT = unchecked((int)0x80072efd);
        private const int E_WINHTTP_CONNECTION_ERROR = unchecked((int)0x80072efe);

        #endregion

        #region Variables

        private DispatcherTimer _statusTimer = null;
        private CancellationTokenSource _cts;

        #endregion Variables

        #region Properties

        /// <summary>
        /// Gets access to all the platform services.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        internal PlatformBase PlatformBase { get { return PlatformBase.CurrentCore; } }

        /// <summary>
        /// Gets access to the dispatcher for this view or application.
        /// </summary>
        private CoreDispatcher Dispatcher
        {
            get
            {
                if (this.View != null)
                    return this.View.Dispatcher;

                var coreWindow1 = CoreWindow.GetForCurrentThread();
                if (coreWindow1 != null)
                    return coreWindow1.Dispatcher;

                var coreWindow2 = CoreApplication.MainView.CoreWindow;
                if (coreWindow2 != null)
                    return coreWindow2.Dispatcher;

                throw new InvalidOperationException("Dispatcher not accessible!");
            }
        }

        private Page _View;
        /// <summary>
        /// Gets or sets access to the page instance associated to this ViewModel instance.
        /// </summary>
        public Page View
        {
            get { return _View; }
            set { _View = value; }
        }

        /// <summary>
        /// Gets access to the parameter passed to the page.
        /// </summary>
        public object ViewParameter { get; private set; }

        /// <summary>
        /// True or false indicating whether or not the view model has been called at least by a view bound to this view model.
        /// </summary>
        protected internal bool IsInitialized { get; private set; }

        /// <summary>
        /// Gets or sets the title for the view specified by this view model.
        /// </summary>
        private string _Title = Strings.Resources.TextNotApplicable;
        public virtual string Title
        {
            get { return _Title; }
            protected set { this.SetProperty(ref _Title, value); }
        }

        /// <summary>
        /// Gets a boolean indicating whether or not the view instance is in a child frame of the window.
        /// </summary>
        protected bool IsViewInChildFrame
        {
            get { return this.PlatformBase.NavigationBase.IsChildFramePresent; }
        }

        /// <summary>
        /// Get a boolean indicating whether or not this view should show a home button.
        /// </summary>
        public bool ShowHomeButton
        {
            get
            {
                return PlatformBase.CurrentCore.ViewModelCore.IsInitialized == false
                    && this.IsUserAuthenticated
                    && this.PlatformBase.NavigationBase.CanGoBack() == false;
            }
        }

        #endregion Properties

        #region Constructors

        public ViewModelBase()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            if(PlatformBase.CurrentCore.Geolocation != null)
                PlatformBase.CurrentCore.Geolocation.LocationChanged += Geolocation_LocationChanged;
        }

        static ViewModelBase()
        {
            if (PlatformBase.CurrentCore.Geolocation != null)
            {
                CurrentLocationTask = new NotifyTaskCompletion<Geoposition>(async (arg) => await PlatformBase.CurrentCore.Geolocation.GetSingleCoordinateAsync(false, 0, arg));
                CurrentLocationTask.Refresh(true, CancellationToken.None);
            }
        }

        #endregion

        #region Methods

        #region Navigation Methods

        internal bool BackNavigationRequested()
        {
            if (this.OnBackNavigationRequested() == true || (this.View as IView)?.OnBackNavigationRequested() == true)
                return true;
            else
                return false;
        }

        internal bool ForwardNavigationRequested()
        {
            if (this.OnForwardNavigationRequested() == true || (this.View as IView)?.OnForwardNavigationRequested() == true)
                return true;
            else
                return false;
        }

        /// <summary>
        /// Allows a view model to prevent back navigation if it needs to.
        /// </summary>
        /// <returns>True if on back navigation should be cancelled else false.</returns>
        protected internal virtual bool OnBackNavigationRequested()
        {
            return false;
        }

        /// <summary>
        /// Allows a view model to prevent forward navigation if it needs to.
        /// </summary>
        /// <returns>True if on forward navigation should be cancelled else false.</returns>
        protected internal virtual bool OnForwardNavigationRequested()
        {
            return false;
        }

        #endregion

        #region Load/Save State Methods

        /// <summary>
        /// Called by a view's OnNavigatedTo event, LoadState allows view models to perform initialization logic on initial or subsequent views of this page and view model instance.
        /// </summary>
        /// <param name="view">View that is being shown.</param>
        /// <param name="e">Arguments containing navigation and page state data.</param>
        /// <returns>Awaitable task is returned.</returns>
        internal async Task LoadStateAsync(Page view, LoadStateEventArgs e)
        {
            // Full screen on Xbox
            if (PlatformBase.DeviceFamily == DeviceFamily.Xbox && Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().IsFullScreenMode == false)
            {
                var isFullScreen = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "{0}: TryEnterFullScreenMode returned {1}", this.GetType().Name, isFullScreen);
            }

            // Store properties and subscribe to events
            this.View = view;
            this.ViewParameter = e.Parameter;
            this.View.GotFocus += View_GotFocus;

            var auth = PlatformBase.GetService<AuthorizationManagerBase>();
            if (auth != null)
            {
                this.IsUserAuthenticated = auth.IsAuthenticated();
                auth.UserAuthenticatedStatusChanged += AuthenticationManager_UserAuthenticated;
            }

            if (this.RequiresAuthorization && auth?.IsReauthenticationNeeded == true)
            {
                if(this.IsUserAuthenticated && (RefreshAccessTokenTask == null || RefreshAccessTokenTask.IsSuccessfullyCompleted == false))
                {
                    // User is authenticated
                    if (RefreshAccessTokenTask == null)
                    {
                        this.ShowBusyStatus(Strings.Account.TextAuthenticating, true);
                        RefreshAccessTokenTask = new NotifyTaskCompletion<bool>(async (ct) => await this.RefreshAccessTokenAsync(ct));
                    }
                    RefreshAccessTokenTask.Refresh(false, CancellationToken.None);
                    await this.WaitAllAsync(CancellationToken.None, RefreshAccessTokenTask.Task);
                    this.ClearStatus();
                }

                // Check if user is authorized to view this page or the refresh token returned false to indicate not logged in
                if (this.IsUserAuthenticated == false || RefreshAccessTokenTask?.Result == false)
                {
                    await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                    {
                        await this.UserSignoutAsync(true);
                    });
                    return;
                }
            }

            // Update properties with values from page state if found
            if (!this.IsInitialized && e.PageState.Count > 0)
            {
                foreach (var pair in _preservePropertiesList)
                    this.LoadPropertyFromState(e, pair.Value);
            }

            // Notify load state on inherited view model instances
            await this.OnLoadStateAsync(e);

            // Refresh Data
            await this.RefreshAsync();

            NetworkInformation.NetworkStatusChanged += NetworkInformation_NetworkStatusChanged;
            this.IsInitialized = true;
        }

        /// <summary>
        /// Inherited view model instances perform any load state logic.
        /// </summary>
        /// <param name="e">Event args with all navigation data.</param>
        /// <returns>Awaitable task is returned.</returns>
        protected virtual Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Inherited view model instances perform any save state logic.
        /// </summary>
        /// <param name="e">Event args with all navigation data.</param>
        /// <returns>Awaitable task is returned.</returns>
        protected virtual Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Called by a view's OnNavigatedFrom event, SaveState allows view models to perform suspend logic whenever the page is navigated away from.
        /// </summary>
        /// <param name="e">Event args with all navigation data.</param>
        /// <returns>Awaitable task is returned.</returns>
        internal async Task SaveStateAsync(SaveStateEventArgs e)
        {
            await this.OnSaveStateAsync(e);

            NetworkInformation.NetworkStatusChanged -= NetworkInformation_NetworkStatusChanged;
            this.View.GotFocus -= View_GotFocus;
            this.View = null;

            try
            {
                // Save all tracked properties to state
                foreach (var pair in _preservePropertiesList)
                    this.SavePropertyToState(e, pair.Value);
            }
            catch(Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Failed to save properties to {0} page state.", this.GetType().Name);
            }

            // Logout of user authentication service
            var auth = PlatformBase.GetService<AuthorizationManagerBase>();
            if(auth != null)
                auth.UserAuthenticatedStatusChanged -= AuthenticationManager_UserAuthenticated;

            // Dispose the viewmodel if navigating backwards
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.Back)
                this.Dispose();
        }

        protected internal virtual async void OnApplicationResuming()
        {
            try
            {
                await this.RefreshAsync();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"Error trying to call RefreshAsync from {this.GetType().Name}.OnApplicationResume()");
                throw ex;
            }
        }

        #endregion

        #region View Focused

        private void View_GotFocus(object sender, RoutedEventArgs e)
        {
            try
            {
                this.OnViewGotFocus();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error while performing OnViewGotFocus on view model '{0}' with parameters: {1}", this.GetType().Name, this.ViewParameter);
            }
        }

        /// <summary>
        /// Executes when the focused view loses focus.
        /// </summary>
        /// <returns></returns>
        protected virtual void OnViewGotFocus()
        {
        }

        #endregion

        #region Exception Handling

        protected Task HandleExceptionAsync(Exception ex, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
        {
            return this.HandleExceptionAsync(CancellationToken.None, ex, message, callerName);
        }

        protected async Task HandleExceptionAsync(CancellationToken ct, Exception ex, string message = null, [System.Runtime.CompilerServices.CallerMemberName] string callerName = null)
        {
            if (ex == null)
                throw new ArgumentNullException(nameof(ex));

            if (ex is UnauthorizedAccessException || ex.HResult == -2145844847)
            {
                this.ShowStatus(Strings.Account.TextUnauthorizedUser);
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"{this.GetType().Name}.{callerName} - Unauthorized user exception (Parameters: {this.ViewParameter}) - {message}");
                if (PlatformBase.ContainsService<AuthorizationManagerBase>())
                    await PlatformBase.GetService<AuthorizationManagerBase>().SetUserAsync(null);
                await this.ShowMessageBoxAsync(ct, Strings.Account.TextUnauthorizedUser, Strings.Account.TextUnauthorizedUserTitle);
            }
            else if (ex is UserFriendlyException)
            {
                this.ClearStatus();
                var bex = ex as UserFriendlyException;
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "{0}.{1} - UserFriendlyException - {4} ({0} Parameters: {2}) {3}", this.GetType().Name, callerName, this.ViewParameter, message, bex.UserMessage);

                switch(bex.DisplayStyle)
                {
                    case UserFriendlyException.DisplayStyles.NonBlockingMessage:
                        this.ShowStatus(bex.UserMessage);
                        break;

                    case UserFriendlyException.DisplayStyles.BlockingMessage:
                        this.ShowStatus(bex.UserMessage, true);
                        break;

                    case UserFriendlyException.DisplayStyles.MessageBox:
                        var t = this.ShowMessageBoxAsync(ct, bex.UserMessage, this.Title);
                        break;

                    default:
                        throw new NotImplementedException($"UserFriendlyException display style of '{bex.DisplayStyle}' has not been implemented.");
                }
            }
            else if (ex is OperationCanceledException || ex is TaskCanceledException)
            {
                this.ShowTimedStatus(Strings.Resources.TextCancellationRequested, 3000);
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "{0}.{1} - Operation canceled ({0} Parameters: {2}) {3}", this.GetType().Name, callerName, this.ViewParameter, message);
            }
            else
            {
                if (Data.ClientApiBase.IsNoInternetException(ex))
                {
                    this.ShowStatus(Strings.Resources.TextNoInternet);
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "{0}.{1} - No internet ({0} Parameters: {2}) {3}", this.GetType().Name, callerName, this.ViewParameter, message);
                }
                else
                {
                    this.ShowStatus(Strings.Resources.TextErrorGeneric);
                    PlatformBase.CurrentCore.Logger.LogError(ex, "{0}.{1} - threw {2} {3} ({0} Parameters: {4}) {5}", this.GetType().Name, callerName, ex.GetType().Name, ex.HResult, this.ViewParameter, message);
                }
            }
        }

        #endregion

        #region Status Methods

        #region Status Properties

        private bool _StatusIsBusy = false;
        /// <summary>
        /// Gets whether or not the view model is in busy status.
        /// </summary>
        public bool StatusIsBusy
        {
            get { return _StatusIsBusy; }
            private set { this.SetProperty(ref _StatusIsBusy, value); }
        }

        private bool _StatusIsBlocking;
        /// <summary>
        /// Gets whether or not the busy status indicator for this view model should be blocking the UI view.
        /// </summary>
        public bool StatusIsBlocking
        {
            get { return _StatusIsBlocking; }
            private set { this.SetProperty(ref _StatusIsBlocking, value); }
        }

        private double? _StatusProgressValue = null;
        /// <summary>
        /// Gets the numerical value of the progress bar that should be displayed by busy status indicator.
        /// </summary>
        public double? StatusProgressValue
        {
            get { return _StatusProgressValue; }
            protected set { this.SetProperty(ref _StatusProgressValue, value); }
        }

        private string _StatusText;
        /// <summary>
        /// Gets the text that should be displayed by the busy status indicator.
        /// </summary>
        public string StatusText
        {
            get { return _StatusText; }
            private set { this.SetProperty(ref _StatusText, value); }
        }

        private bool _StatusIsBlockingCancelable = false;
        public bool StatusIsBlockingCancelable
        {
            get { return _StatusIsBlockingCancelable; }
            private set { this.SetProperty(ref _StatusIsBlockingCancelable, value); }
        }

        private CommandBase _statusIsBlockingCancelableCommand = null;
        public CommandBase StatusIsBlockingCancelableCommand
        {
            get { return _statusIsBlockingCancelableCommand ?? (_statusIsBlockingCancelableCommand = new GenericCommand(this.GetType().Name + "_StatusIsBlockingCancelableCommand", () => this.CancelStatus())); }
        }

        #endregion

        /// <summary>
        /// Copies status data from another ViewModel.
        /// </summary>
        /// <param name="vm">ViewModel to copy status data from.</param>
        protected void CopyStatus(ViewModelBase vm)
        {
            if (vm != null)
            {
                this.StatusIsBlocking = vm.StatusIsBlocking;
                this.StatusIsBlockingCancelable = vm.StatusIsBlockingCancelable;
                this.StatusIsBusy = vm.StatusIsBusy;
                this.StatusProgressValue = vm.StatusProgressValue;
                this.StatusText = vm.StatusText;
            }
            else
            {
                this.StatusIsBlocking = false;
                this.StatusIsBlockingCancelable = false;
                this.StatusIsBusy = false;
                this.StatusProgressValue = null;
                this.StatusText = null;
            }
        }

        /// <summary>
        /// Show just a status message to the user for a indefinite period of time.
        /// </summary>
        /// <param name="message">Message to display on the UI</param>
        protected void ShowStatus(string message, bool statusIsBlocking = false)
        {
            this.StatusIsBusy = false;
            this.StatusIsBlocking = statusIsBlocking;
            this.StatusIsBlockingCancelable = false;
            this.StatusText = message;
        }

        /// <summary>
        /// Show the busy UI to the user for an indefinite period of time.
        /// </summary>
        /// <param name="message">Message to display on the UI</param>
        /// <param name="isBlocking">True if full screen blocking UI else false.</param>
        protected void ShowBusyStatus(string message = null, bool isBlocking = false, bool isBlockingCancelable = false)
        {
            this.ShowTimedStatus(message, 0);
            this.StatusIsBusy = true;
            this.StatusIsBlocking = isBlocking;
            this.StatusIsBlockingCancelable = isBlockingCancelable;
        }

        /// <summary>
        /// Show the busy UI to the user for a definitive period of time.
        /// </summary>
        /// <param name="message">Message to display on the UI</param>
        /// <param name="milliseconds">Time in milliseconds to display the message.</param>
        protected void ShowTimedStatus(string message, int milliseconds = 5000, bool isBlocking = false)
        {
            this.StatusIsBusy = false;
            this.StatusIsBlocking = isBlocking;
            this.StatusIsBlockingCancelable = false;
            this.StatusText = message;

            if (DesignMode.DesignModeEnabled)
                return;

            this.ShutdownTimer();
            if (milliseconds > 0)
            {
                _statusTimer = new DispatcherTimer();
                _statusTimer.Interval = TimeSpan.FromMilliseconds(milliseconds);
                _statusTimer.Tick += (s, e) =>
                {
                    this.ClearStatus();
                    this.ShutdownTimer();
                };
                _statusTimer.Start();
            }
        }

        /// <summary>
        /// Clears any status messages on the UI.
        /// </summary>
        /// <param name="obj"></param>
        protected void ClearStatus(object obj)
        {
            this.ShutdownTimer();
            this.InvokeOnUIThread(this.ClearStatus);
        }

        /// <summary>
        /// Clears any status messages on the UI.
        /// </summary>
        protected void ClearStatus()
        {
            this.StatusText = string.Empty;
            this.StatusProgressValue = null;
            this.StatusIsBusy = false;
            this.StatusIsBlocking = false;
            this.StatusIsBlockingCancelable = false;
        }

        /// <summary>
        /// Called when the user hits the cancel button on the blocking status UI. Overridable so inherited VMs can customize experience when user cancels.
        /// </summary>
        protected virtual void CancelStatus()
        {
        }

        private void ShutdownTimer()
        {
            if (_statusTimer != null)
            {
                _statusTimer.Stop();
                _statusTimer = null;
            }
        }

        #endregion Status Methods

        #region Property State Methods

        /// <summary>
        /// Notify subscribers on the UI thread that a property changed occurred.
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="property"></param>
        protected void NotifyPropertyChangedOnUI<TProperty>(Expression<Func<TProperty>> property)
        {
            this.InvokeOnUIThread(() => this.NotifyPropertyChanged(property));
        }

        private Dictionary<string, PropertyInfo> _preservePropertiesList = new Dictionary<string, PropertyInfo>();

        /// <summary>
        /// Registers properties to save/retrieve from page state during tombstoning.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        protected void PreservePropertyState<T>(Expression<Func<T>> property)
        {
            var pi = this.GetPropertyInfo(property);
            if (pi != null && !_preservePropertiesList.ContainsKey(pi.Name))
                _preservePropertiesList.Add(pi.Name, pi);
        }

        /// <summary>
        /// Loads a property with data from page state if found.
        /// </summary>
        /// <param name="e">Load state event args for the view.</param>
        /// <param name="key">Name of the property.</param>
        /// <param name="pi">Property info object.</param>
        private void LoadPropertyFromState(LoadStateEventArgs e, PropertyInfo pi)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            // Check if value exists in state bag
            if (e.PageState != null && e.PageState.ContainsKey(pi.Name))
            {
                var storedValue = e.PageState[pi.Name];
                if (storedValue != null)
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "{0} - Restoring property {1} from page state of value {2}", this.GetType().Name, pi.Name, storedValue);
                    this.SetPropertyValue(pi, storedValue);
                }
            }
        }

        /// <summary>
        /// Saves a property to page state.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="pi"></param>
        private void SavePropertyToState(SaveStateEventArgs e, PropertyInfo pi)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));
            if (pi == null)
                throw new ArgumentNullException(nameof(pi));

            var value = pi.GetValue(this);

            // Add key/value to state bag
            if (e.PageState.ContainsKey(pi.Name))
                e.PageState[pi.Name] = value;
            else
                e.PageState.Add(pi.Name, value);

            PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, "{0} - Saved property {1} to page state of value {2}", this.GetType().Name, pi.Name, value);
        }

        #endregion Property State Methods

        #region Dispatcher Methods

        /// <summary>
        /// Runs a function on the currently executing platform's UI thread.
        /// </summary>
        /// <param name="action">Code to be executed on the UI thread</param>
        /// <param name="priority">Priority to indicate to the system when to prioritize the execution of the code</param>
        /// <returns>Task representing the code to be executing</returns>
        protected void InvokeOnUIThread(Action action, CoreDispatcherPriority priority = CoreDispatcherPriority.Normal)
        {
            if (this.Dispatcher == null || this.Dispatcher.HasThreadAccess)
            {
                action();
            }
            else
            {
                // Execute asynchronously on the thread the Dispatcher is associated with.
                var task = this.Dispatcher.RunAsync(priority, () => action());
            }
        }

        #endregion

        #region MessageBox Methods

        /// <summary>
        /// Displays a message box dialog.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Awaitable call which returns the index of the button clicked.</returns>
        protected internal Task<int> ShowMessageBoxAsync(CancellationToken ct, string message)
        {
            return this.ShowMessageBoxAsync(ct, message, PlatformBase.CurrentCore.AppInfo.AppName);
        }

        /// <summary>
        /// Displays a message box dialog.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of the message box.</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Awaitable call which returns the index of the button clicked.</returns>
        protected internal Task<int> ShowMessageBoxAsync(CancellationToken ct, string message, string title)
        {
            return this.ShowMessageBoxAsync(ct, message, title, null, 0);
        }

        /// <summary>
        /// Displays a message box dialog.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="buttonNames">List of buttons to display.</param>
        /// <param name="defaultIndex">Index of the default button of the dialog box.</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Awaitable call which returns the index of the button clicked.</returns>
        protected internal Task<int> ShowMessageBoxAsync(CancellationToken ct, string message, IList<string> buttonNames = null, int defaultIndex = 0)
        {
            return this.ShowMessageBoxAsync(ct, message, PlatformBase.CurrentCore.AppInfo.AppName, buttonNames, defaultIndex);
        }

        /// <summary>
        /// Displays a message box dialog.
        /// </summary>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Title of the message box.</param>
        /// <param name="buttonNames">List of buttons to display.</param>
        /// <param name="defaultIndex">Index of the default button of the dialog box.</param>
        /// <param name="ct">Cancelation token</param>
        /// <returns>Awaitable call which returns the index of the button clicked.</returns>
        public async Task<int> ShowMessageBoxAsync(CancellationToken ct, string message, string title, IList<string> buttonNames = null, int defaultIndex = 0)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentException("The specified message cannot be null or empty.", "message");

            // Set a default title if no title was specified.
            if (string.IsNullOrWhiteSpace(title))
                title = PlatformBase.CurrentCore.AppInfo.AppName;

            int result = defaultIndex;
            MessageDialog dialog = new MessageDialog(message, title);

            // Show all the button names specified or just an OK label if no names were specified.
            if (buttonNames != null && buttonNames.Count > 0)
                foreach (string button in buttonNames)
                    dialog.Commands.Add(new UICommand(button, new UICommandInvokedHandler((o) => result = buttonNames.IndexOf(button))));
            else
                dialog.Commands.Add(new UICommand(Strings.Resources.TextOk, new UICommandInvokedHandler((o) => result = 0)));

            // Set the default button of the dialog
            dialog.DefaultCommandIndex = (uint)defaultIndex;

            // Show on the appropriate thread
            if (this.Dispatcher == null || this.Dispatcher.HasThreadAccess)
            {
                await dialog.ShowAsync().AsTask(ct);
                return result;
            }
            else
            {
                var tcs = new TaskCompletionSource<int>();

                // Execute asynchronously on the thread the Dispatcher is associated with.
                await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
                {
                    await dialog.ShowAsync().AsTask(ct);
                    tcs.TrySetResult(result);
                });
                return tcs.Task.Result;
            }
        }

        #endregion

        #region Refresh Async

        private CommandBase _RefreshCommand;
        /// <summary>
        /// Gets a command instance for refreshing the page.
        /// </summary>
        public CommandBase RefreshCommand
        {
            get
            {
                return _RefreshCommand ?? (_RefreshCommand = new GenericCommand("RefreshCommand", async () =>
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"User pressed refresh on {this.GetType().Name} with paramemter {this.ViewParameter?.ToString()}");
                    this.UserForcedRefresh = true;
                    var dic = new Dictionary<string, string>();
                    dic.Add("ViewModel", this.GetType().Name);
                    dic.Add("Parameter", this.ViewParameter?.ToString());
                    PlatformBase.CurrentCore.Analytics.Event("UserRefreshed", dic);
                    await this.RefreshAsync(true);
                }, () => this.IsRefreshEnabled));
            }
        }

        /// <summary>
        /// Gets or sets whether or not the user forced a refresh on the view.
        /// </summary>
        public bool UserForcedRefresh { get; internal set; }
        
        private bool _IsRefreshEnabled = true;
        /// <summary>
        /// Gets or sets whether or not the refresh button is enabled or not.
        /// </summary>
        public bool IsRefreshEnabled
        {
            get { return _IsRefreshEnabled; }
            private set
            {
                if (this.SetProperty(ref _IsRefreshEnabled, value))
                    this.RefreshCommand.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// Refreshes data on the entire page. 
        /// </summary>
        /// <returns>Task for the operation.</returns>
        public Task RefreshAsync()
        {
            return this.RefreshAsync(!this.IsInitialized);
        }

        /// <summary>
        /// Refreshes data on the entire page. 
        /// </summary>
        /// <param name="forceRefresh">Flag indicating a force refresh else refresh only if necessary.</param>
        /// <returns>Task for the operation.</returns>
        public async Task RefreshAsync(bool forceRefresh)
        {
            if (_cts != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Debug, $"Cannot refresh {this.GetType().Name} again because it's currently being refreshed.");
                return;
            }

            try
            {
                this.IsRefreshEnabled = false;
                _cts = new CancellationTokenSource();
                await this.OnRefreshAsync(forceRefresh, _cts.Token);
                this.HasLocationChanged = false;
                this.ClearStatus();
            }
            catch(TaskCanceledException)
            {
                PlatformBase.Logger.Log(LogLevels.Warning, $"RefreshAsync cancelled on {this.GetType().Name}");
                this.ClearStatus();
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, $"Exception while trying to refresh {this.GetType().Name}.");
                await this.HandleExceptionAsync(ex);
            }
            finally
            {
                this.UserForcedRefresh = false;
                this.IsRefreshEnabled = true;
                this.RefreshReset();
            }
        }

        /// <summary>
        /// Inherited view models can implement their own logic as to what happens when a page refresh is requested.
        /// </summary>
        /// <param name="forceRefresh"></param>
        /// <param name="ct"></param>
        /// <returns></returns>
        protected virtual Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        // Clean up after a refresh occurs.
        private void RefreshReset()
        {
            if(_cts?.IsCancellationRequested == false)
                _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
        }

        public virtual void Dispose()
        {
            this.RefreshReset();
        }

        #endregion

        #region Caching

        private static string APP_CACHE_PATH = StorageManager.DATA_CACHE_FOLDER_NAME + "\\{0}_{1}.data";

        /// <summary>
        /// Fills a property with data from the app cache if available.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected Task<T> LoadFromCacheAsync<T>(Expression<Func<T>> property, string identifier = null)
        {
            if (property == null)
                return Task.FromResult<T>(default(T));

            var key = this.GetPropertyInfo(property).Name + identifier;
            return this.LoadFromCacheAsync<T>(key);
        }

        /// <summary>
        /// Fills a property with data from the app cache if available.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        protected internal async Task<T> LoadFromCacheAsync<T>(string key)
        {
            try
            {
                return await PlatformBase.CurrentCore.Storage.LoadFileAsync<T>(string.Format(APP_CACHE_PATH, this.GetType().Name, key), Windows.Storage.ApplicationData.Current.TemporaryFolder);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error retrieving '{0}' from cache data.", key);
                return default(T);
            }
        }

        /// <summary>
        /// Stores a property's data to the app cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="identifier"></param>
        /// <returns></returns>
        protected async Task SaveToCacheAsync<T>(Expression<Func<T>> property, string identifier = null)
        {
            if (property == null)
                return;

            var pi = this.GetPropertyInfo(property);
            var key = pi.Name + identifier;

            T data = default(T);
            try
            {
                data = (T)pi.GetValue(this);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error retrieving data from '{0}' property to save to cache.", pi.Name);
            }
            await this.SaveToCacheAsync<T>(key, data);
        }

        /// <summary>
        /// Stores a property's data to the app cache.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">Unique identifier for the data</param>
        /// <param name="data">The data to store</param>
        /// <returns></returns>
        protected internal async Task SaveToCacheAsync<T>(string key, T data)
        {
            try
            {
                await PlatformBase.CurrentCore.Storage.SaveFileAsync(string.Format(APP_CACHE_PATH, this.GetType().Name, key), data, Windows.Storage.ApplicationData.Current.TemporaryFolder);
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Error saving '{0}' to cache data.", key);
            }
        }

        #endregion

        #region Tasks

        /// <summary>
        /// Waits for a list of task to complete before continuing execution.
        /// </summary>
        /// <param name="tasks">List of tasks to execute and wait for all to complete.</param>
        /// <returns>Awaitable task is returned.</returns>
        protected async Task WaitAllAsync(CancellationToken ct, params Task[] tasks)
        {
            if (tasks == null || tasks.Where(w => w != null).Count() == 0)
                return;

            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();

            var t = Task.Factory.StartNew(() =>
            {
                try
                {
                    Task.WaitAll(tasks.Where(w => w != null).ToArray(), ct);
                    tcs.TrySetResult(null);
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            }, ct);

            await tcs.Task;
        }

        #endregion

        #region Keyboard Methods

        protected internal virtual void OnHandleKeyUp(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
        }
        protected internal virtual void OnHandleKeyDown(Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
        }

        #endregion

        #region User Authentication

        private bool _IsUserAuthenticated = false;
        /// <summary>
        /// Gets whether or not a user is authenticated in this application.
        /// </summary>
        public bool IsUserAuthenticated
        {
            get { return _IsUserAuthenticated; }
            private set { this.SetProperty(ref _IsUserAuthenticated, value); }
        }

        private static NotifyTaskCompletion<bool> RefreshAccessTokenTask { get; set; }

        /// <summary>
        /// True or false indicating whether or not this view model requires the user to be authenticated.
        /// </summary>
        protected bool RequiresAuthorization { get; set; }

        private async Task<bool> RefreshAccessTokenAsync(CancellationToken ct)
        {
            var auth = PlatformBase.GetService<AuthorizationManagerBase>();
            if (auth != null)
            {
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "Attempting to refresh access token...");
                try
                {
                    var user = await auth.GetRefreshAccessToken(ct);
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "...access token refresh complete!");
                    if(user != null)
                        return await PlatformBase.GetService<AuthorizationManagerBase>().SetUserAsync(user);
                }
                catch(UnauthorizedAccessException)
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"UnauthorizedAccessException was caught by {this.GetType().FullName}.RefreshAccessTokenAsync");
                    return false;
                }
                catch(Exception ex)
                {
                    if (ClientApiBase.IsNoInternetException(ex))
                    {
                        PlatformBase.CurrentCore.Logger.LogError(ex, "Could not refresh access token due to no internet. Allowing users to pass refresh token check.");
                        return true;
                    }
                    else
                    {
                        PlatformBase.CurrentCore.Logger.LogError(ex, "Error while trying to refresh access token.");
                        return false;
                    }
                }
            }

            return true;
        }

        private void AuthenticationManager_UserAuthenticated(object sender, bool e)
        {
            if (e == false)
                RefreshAccessTokenTask = null;

            // Subscribes to user authentication changed event from AuthorizationManager
            this.InvokeOnUIThread(async () =>
            {
                this.IsUserAuthenticated = e;
                try
                {
                    await this.OnUserAuthenticatedChanged();
                }
                catch (Exception ex)
                {
                    PlatformBase.CurrentCore.Logger.LogError(ex, "Error while performing OnUserAuthenticatedChanged on view model '{0}' with parameters: {1}", this.GetType().Name, this.ViewParameter);
                }
            });
        }

        protected virtual Task OnUserAuthenticatedChanged()
        {
            return Task.CompletedTask;
        }

        #endregion

        #region User Signout

        private CommandBase _navigateToAccountSignoutCommand = null;
        /// <summary>
        /// Command to sign out of the application.
        /// </summary>
        public CommandBase NavigateToAccountSignoutCommand
        {
            get { return _navigateToAccountSignoutCommand ?? (_navigateToAccountSignoutCommand = new GenericCommand("NavigateToAccountSignoutCommand", async () => await this.UserSignoutAsync(), () => !_isSignoutInProgress)); }
        }

        private static bool _isSignoutInProgress = false;

        /// <summary>
        /// Signs a users out of the application.
        /// </summary>
        /// <param name="isSilent">True to prompt the user else false to sign out immediately.</param>
        /// <returns>Awaitable task is returned.</returns>
        protected async Task UserSignoutAsync(bool isSilent = false)
        {
            if (_isSignoutInProgress)
                return;

            int result = 0;

            if (isSilent == false)
                result = await this.ShowMessageBoxAsync(CancellationToken.None, Strings.Account.TextSignoutConfirmation, Strings.Account.TextSignout, new string[] { Strings.Account.TextSignout, Strings.Resources.TextCancel }, 1);

            if (result == 0)
            {
                try
                {
                    _isSignoutInProgress = true;
                    this.NavigateToAccountSignoutCommand.RaiseCanExecuteChanged();

                    this.ShowBusyStatus(Strings.Account.TextSigningOut, true);
                    PlatformBase.CurrentCore.Analytics.Event("AccountSignout");

                    // Allow the app core to signout
                    await PlatformBase.CurrentCore.SignoutAllAsync();

                    // Navigate home after successful signout
                    this.PlatformBase.NavigationBase.Home();
                }
                catch (Exception ex)
                {
                    PlatformBase.CurrentCore.Logger.LogError(ex, "Error during ViewModelBase.SignoutAsync");
                    throw ex;
                }
                finally
                {
                    _isSignoutInProgress = false;
                    this.NavigateToAccountSignoutCommand.RaiseCanExecuteChanged();
                    this.ClearStatus();
                }
            }
        }

        #endregion

        #region Location

        protected bool HasLocationChanged { get; private set; }

        private void Geolocation_LocationChanged(object sender, Services.LocationChangedEventArgs e)
        {
            this.HasLocationChanged = true;
        }

        private static NotifyTaskCompletion<Geoposition> CurrentLocationTask { get; set; }

        protected Task<ILocationModel> GetCurrentLocationAsync(bool highAccuracy, CancellationToken ct)
        {
            CurrentLocationTask.Refresh(this.UserForcedRefresh, ct);
            return this.WaitForCurrentLocationAsync(ct);
        }

        protected async Task<ILocationModel> WaitForCurrentLocationAsync(CancellationToken ct, bool isBlocking = true)
        {
            if (!CurrentLocationTask.IsCompleted)
            {
                this.ShowBusyStatus(Strings.Location.TextDeterminingLocation, isBlocking);
                await this.WaitAllAsync(ct, CurrentLocationTask.Task);
                this.ClearStatus();
            }

            return CurrentLocationTask.Task.Result?.Coordinate?.AsLocationModel();
        }

        #endregion

        #region Network Access Changed

        private async void NetworkInformation_NetworkStatusChanged(object sender)
        {
            try
            {

                var profile = NetworkInformation.GetInternetConnectionProfile();
                bool isConnected = profile?.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess;

                PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, $"NetworkStatusChanged - IsConnected: {isConnected}");

                // On network access connected, execute a soft refresh to ensure everything loads automatically
                if (isConnected)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () => await this.RefreshAsync());
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Logger.LogError(ex, "Failure during NetworkStatusChanged event on {0}", this.GetType().Name);
            }
        }

        #endregion

        #region Scrolling

        private CommandBase _viewScrollToTopCommand = null;
        public CommandBase ViewScrollToTopCommand
        {
            get { return _viewScrollToTopCommand ?? (_viewScrollToTopCommand = new GenericCommand("ViewScrollToTopCommand_" + this.GetType().Name, this.ViewScrollToTop)); }
        }

        internal virtual void ViewScrollToTop()
        {
            var view = this.View as IView;
            if (view != null)
                view.ScrollToTop();
        }

        #endregion

        #region Interstitial Ads

        protected void ShowInterstitialAd()
        {
            if(this.View is IView view)
                view.ShowInterstitialAd();
        }

        #endregion

        #endregion Methods
    }
}

namespace AppFramework.Core
{
    #region Classes

    /// <summary>
    /// Represents the method that will handle the <see cref="NavigationHelper.LoadState"/>event
    /// </summary>
    public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
    /// <summary>
    /// Represents the method that will handle the <see cref="NavigationHelper.SaveState"/>event
    /// </summary>
    public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

    /// <summary>
    /// Class used to hold the event data required when a page attempts to load state.
    /// </summary>
    public sealed class LoadStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the navigation event args passed to the OnNavigatingTo event of the page.
        /// </summary>
        public NavigationEventArgs NavigationEventArgs { get; private set; }

        /// <summary>
        /// A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.
        /// </summary>
        public IDictionary<string, object> PageState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadStateEventArgs"/> class.
        /// </summary>
        /// <param name="navigationParameter">
        /// The parameter value passed to <see cref="Frame.Navigate(Type, Object)"/> 
        /// when this page was initially requested.
        /// </param>
        /// <param name="pageState">
        /// A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.
        /// </param>
        internal LoadStateEventArgs(NavigationEventArgs e, IDictionary<string, object> pageState, bool isViewInitialized)
            : base()
        {
            this.NavigationEventArgs = e;
            this.PageState = pageState;
            this.Parameter = NavigationParameterSerializer.Deserialize(e.Parameter); // Deserializes the parameter from the navigation event if necessary and stores instance
            this.IsViewInitialized = isViewInitialized;
        }

        /// <summary>
        /// Gets the deserialized instance of the parameter passed to this page.
        /// </summary>
        public object Parameter { get; private set; }

        public bool IsViewInitialized { get; private set; }
    }

    /// <summary>
    /// Class used to hold the event data required when a page attempts to save state.
    /// </summary>
    public sealed class SaveStateEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the navigation event args passed to the OnNavigatedFrom event of the page.
        /// </summary>
        public NavigationEventArgs NavigationEventArgs { get; private set; }

        /// <summary>
        /// An empty dictionary to be populated with serializable state.
        /// </summary>
        public IDictionary<string, object> PageState { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SaveStateEventArgs"/> class.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        internal SaveStateEventArgs(NavigationEventArgs e, IDictionary<string, object> pageState)
            : base()
        {
            this.NavigationEventArgs = e;
            this.PageState = pageState;
        }
    }

    #endregion
}