﻿using AppFramework.Core.Models;
using AppFramework.Core.Services;
using AppFramework.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;
using Windows.System.Profile;
using Windows.UI.ViewManagement;

namespace AppFramework.Core
{
    /// <summary>
    /// Base class used to manage application execution and to access all services available to the solution.
    /// Provides core app functionality for initializing and suspending your application,
    /// handling exceptions, and more.
    /// </summary>
    public abstract partial class PlatformBase : ModelBase
    {
        #region Variables

        private static Dictionary<Type, ServiceBase> _services = new Dictionary<Type, ServiceBase>();

        #endregion Variables

        #region Properties

        /// <summary>
        /// Provides access to application services.
        /// </summary>
        protected internal static PlatformBase CurrentCore { get; protected set; }

        /// <summary>
        /// Gets the current device family this app is executing on.
        /// </summary>
        public static DeviceFamily DeviceFamily { get; private set; }

        public static bool IsDebugMode { get; protected set; }

        private AppSettingsLocalBase _AppSettingsLocal;
        /// <summary>
        /// Gets local app settings for this app.
        /// </summary>
        protected internal AppSettingsLocalBase AppSettingsLocalCore
        {
            get { return _AppSettingsLocal; }
            protected set { this.SetProperty(ref _AppSettingsLocal, value); }
        }

        private AppSettingsRoamingBase _AppSettingsRoaming;
        /// <summary>
        /// Gets roaming app settings for this app.
        /// </summary>
        protected internal AppSettingsRoamingBase AppSettingsRoamingCore
        {
            get { return _AppSettingsRoaming; }
            protected set { this.SetProperty(ref _AppSettingsRoaming, value); }
        }

        /// <summary>
        /// Gets whether or not the current device is Windows Desktop.
        /// </summary>
        public bool IsDesktop { get { return DeviceFamily == DeviceFamily.Desktop; } }

        /// <summary>
        /// Gets whether or not the current device is Windows Mobile.
        /// </summary>
        public bool IsMobile { get { return DeviceFamily == DeviceFamily.Mobile; } }

        /// <summary>
        /// Gets whether or not the current device is Xbox.
        /// </summary>
        public bool IsXbox { get { return DeviceFamily == DeviceFamily.Xbox; } }

        /// <summary>
        /// Gets whether or not the current device is executing on Windows Mobile Continuum Desktop.
        /// </summary>
        public bool IsMobileContinuumDesktop { get { return this.IsMobile && UIViewSettings.GetForCurrentView().UserInteractionMode == UserInteractionMode.Mouse; } }

        /// <summary>
        /// Indicates the initialization mode of this app instance.
        /// </summary>
        public InitializationModes InitializationMode { get; private set; }

        private Task _BackgroundRegistrationTask;
        /// <summary>
        /// Gets the task object which registers all the background tasks in the application.
        /// </summary>
        public Task BackgroundRegistrationTask
        {
            get { return _BackgroundRegistrationTask; }
            private set { this.SetProperty(ref _BackgroundRegistrationTask, value); }
        }

        private ViewModelBase _ViewModelCore;
        protected internal ViewModelBase ViewModelCore
        {
            get { return _ViewModelCore; }
            protected set { this.SetProperty(ref _ViewModelCore, value); }
        }

        #endregion Properties

        #region Constructors

        internal PlatformBase()
        {
            // Instantiate all the application services.
            this.Logger = new LoggingService();
            this.Analytics = new AnalyticsManager();
            this.Storage = new StorageManager();
            this.Cryptography = new CryptographyProvider();
            this.Geocode = new GeocodingService();
            this.Ratings = new RatingsManager();
            this.VoiceCommandManager = new VoiceCommandManager();
            this.Jumplist = new JumplistManager();
            this.WebAccountManager = new WebAccountManager();
            this.EmailProvider = new EmailProvider();
            this.SharingManager = new DefaultSharingManager();
            this.Notifications = new DefaultNotificationsManager();
        }

        static PlatformBase()
        {
#if DEBUG
            IsDebugMode = true;
#else
            IsDebugMode = System.Diagnostics.Debugger.IsAttached;
#endif

            Debug.WriteLine("DeviceFamily: " + AnalyticsInfo.VersionInfo.DeviceFamily);

            // Determine what device this is running on and store the appropriate enumeration representing that device
            switch (AnalyticsInfo.VersionInfo.DeviceFamily)
            {
                case "Windows.Desktop":
                    DeviceFamily = DeviceFamily.Desktop;
                    break;
                case "Windows.Mobile":
                    DeviceFamily = DeviceFamily.Mobile;
                    break;
                case "Windows.Xbox":
                    DeviceFamily = DeviceFamily.Xbox;
                    break;
                case "Windows.IoT":
                    DeviceFamily = DeviceFamily.IoT;
                    break;
                default:
                    DeviceFamily = DeviceFamily.Unknown;
                    break;
            }
        }

        #endregion

        #region Methods

        #region App Initializing

        /// <summary>
        /// Global initialization of the app and loads all app settings and initializes all services.
        /// </summary>
        /// <param name="mode">Specifies the mode of this app instance and how it's executing.</param>
        /// <returns>Awaitable task is returned.</returns>
        public virtual async Task AppInitializingAsync(InitializationModes mode)
        {
            this.InitializationMode = mode;
            this.Logger.Log(LogLevels.Warning, "APP INITIALIZING - Initialization mode is {0}", this.InitializationMode);
            
            // Initializes all service
            foreach (var service in _services)
            {
                this.Logger.Log(LogLevels.Debug, "Initializing service '{0}'...", service.Key);
                await this.CheckInitializationAsync(service.Value);
            }

            //// Record the userID to analytics
            //this.Analytics.SetUser(this.AppInfo.UserID);

            // Execute only on first runs of the platform
            if (mode == InitializationModes.New)
            {
                // Provide platform info to analytics
                this.Analytics.Event("OS-Version", Microsoft.Toolkit.Uwp.Helpers.SystemInformation.OperatingSystemVersion);
                this.Analytics.Event("CurrentCulture", System.Globalization.CultureInfo.CurrentCulture.Name);
                this.Analytics.Event("CurrentUICulture", System.Globalization.CultureInfo.CurrentUICulture.Name);
            }
            this.Logger.Log(LogLevels.Debug, "Initializing services is complete!");

            // Register all background agents
            if (mode != InitializationModes.Background && this.BackgroundTasks != null)
                this.BackgroundRegistrationTask = this.BackgroundTasks.RegisterAllAsync();
        }

        protected virtual Task OnAppInitializingAsync(InitializationModes mode)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes a service if not already initialized.
        /// </summary>
        /// <param name="service">Service instance to intialize.</param>
        private async Task CheckInitializationAsync(ServiceBase service)
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));

            try
            {
                if (service.Initialized == false)
                    await service.InitializeAsync();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, $"Failed to execute CheckInitializationAsync for {service.GetType().Name}");
                throw ex;
            }
        }

        #endregion

        #region App Suspending

        /// <summary>
        /// Global suspension of the app and any custom logic to execute on suspend of the app.
        /// </summary>
        public virtual void AppSuspending()
        {
            this.Logger.Log(LogLevels.Warning, "APP SUSPENDING - Initialization mode was {0}", this.InitializationMode);

            try
            {
                this.OnAppSuspending();
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while excuting OnAppSuspending");
                throw ex;
            }
        }

        protected virtual void OnAppSuspending()
        {
        }

        #endregion

        #region Signout / Reset Session

        /// <summary>
        /// Saves any app settings only if the data had changed.
        /// </summary>
        public abstract void SaveSettings(bool forceSave = false);

        /// <summary>
        /// Logic performed during sign out of a user in this application.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        internal virtual async Task SignoutAllAsync()
        {
            var services = _services.Values.Where(w => w is IServiceSignout);
            var list = new List<Task>();

            foreach (var service in services)
                list.Add(((IServiceSignout)service).SignoutAsync());

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    Task.WaitAll(list.ToArray());
                }
                catch (Exception ex)
                {
                    this.Logger.LogError(ex, "Error while trying to call SignoutAsync on each of the platform services.");
                    throw;
                }
            });

            this.ShellMenuClose();
        }

        #endregion

        #region Services

        /// <summary>
        /// Retrieve an instance of a type registered as a platform service.
        /// </summary>
        /// <typeparam name="T">Type reference of the service to retrieve.</typeparam>
        /// <returns>Instance of type T if it was already initialized or null if not found.</returns>
        protected internal static T GetService<T>() where T : ServiceBase
        {
            if (_services.ContainsKey(typeof(T)))
                return (T)_services[typeof(T)];
            else
            {
                var value = _services.Values.FirstOrDefault(f => f is T);
                if (value != null)
                    return (T)value;
                else
                    return default(T);
            }
        }

        /// <summary>
        /// Registers and intializes an instance of an adapter.
        /// </summary>
        /// <typeparam name="T">Type reference of the service to register and initialize.</typeparam>
        protected internal static void SetService<T>(T instance) where T : ServiceBase
        {
            // Shutdown the old instance of T
            var services = _services.Values.Where(f => f is T).ToArray();
            foreach (var service in services)
            {
                Type key = _services.FirstOrDefault(f => f.Value == service).Key;
                _services.Remove(key);
            }

            _services.Add(typeof(T), instance);
        }

        protected internal static bool ContainsService<T>() where T : ServiceBase
        {
            if (_services.ContainsKey(typeof(T)))
                return true;
            else
            {
                var value = _services.Values.FirstOrDefault(f => f is T);
                if (value != null)
                    return true;
                else
                    return false;
            }
        }

        #endregion

        #region Error Handling

        /// <summary>
        /// Global unhandled exception handler for your application.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>True if the exception was handled else false.</returns>
        internal bool AppUnhandledException(Exception e)
        {
            if (Debugger.IsAttached)
            {
                // If the Native debugger is in use, give us a clue in the Output window at least
                Debug.WriteLine("Unhandled exception:" + e.Message);
                Debug.WriteLine(e.StackTrace);

                // An unhandled exception has occurred; break into the debugger
                Debugger.Break();
            }

            // Only log this when the debugger is not attached and you're in RELEASE mode
            try
            {
                PlatformBase.CurrentCore.Analytics.Error(e, "Unhandled Exception");
            }
            catch { }

            try
            {
                PlatformBase.CurrentCore.Logger.LogErrorFatal(e);
            }
            catch (Exception exLog)
            {
                Debug.WriteLine("Exception logging to Logger in AppUnhandledException!");
                Debug.WriteLine(exLog.ToString());
            }

            return false;
        }

        #endregion

        #region Generate Models

        /// <summary>
        /// Creates a querystring parameter string from a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a querystring.</param>
        /// <returns>Query string representing the model provided.</returns>
        public string GenerateModelArguments(IModel model)
        {
            if (model == null)
                return null;

            Dictionary<string, string> dic = new Dictionary<string, string>();
            dic.Add("model", model.GetType().Name);

            // For each model you want to support, you'll add any custom properties 
            // to the dictionary based on the type of object
            if (model is IUniqueModel m)
                dic.Add("ID", m.ID);

            try
            {
                this.OnGenerateModelArguments(dic, model);
            }
            catch(Exception ex)
            {
                this.Logger.LogError(ex, "Error generating querystring with OnGenerateModelArguments implementation.");
                throw ex;
            }

            // Create a querystring from the dictionary collection
            return GeneralFunctions.CreateQuerystring(dic);
        }

        /// <summary>
        /// Creates a querystring parameter string from a model instance.
        /// </summary>
        /// <param name="dic">Dictionary of parameters to add to the querystring.</param>
        /// <param name="model">Model to convert into a querystring.</param>
        /// <returns>Query string representing the model provided.</returns>
        protected virtual void OnGenerateModelArguments(Dictionary<string, string> dic, IModel model)
        {
        }

        /// <summary>
        /// Generates a unique tile ID used for secondary tiles based on a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a unique tile ID.</param>
        /// <returns>String representing a unique tile ID for the model else null if not supported.</returns>
        public abstract string GenerateModelTileID(IModel model);

        /// <summary>
        /// Converts a tile ID back into an object instance.
        /// </summary>
        /// <param name="tileID">Tile ID to retrieve an object instance for.</param>
        /// <param name="ct">Cancelation token.</param>
        /// <returns>Object instance if found else null.</returns>
        public virtual Task<IModel> GenerateModelFromTileIdAsync(string tileID, CancellationToken ct)
        {
            return Task.FromResult<IModel>(null);
        }

        #endregion

        #region Split View Menu

        /// <summary>
        /// Event which notifies the shell to open or close the menu.
        /// </summary>
        public event EventHandler<bool?> NotifyShellMenuToggle;

        public void ShellMenuOpen()
        {
            this.NotifyShellMenuToggle?.Invoke(null, true);
        }

        public void ShellMenuClose()
        {
            this.NotifyShellMenuToggle?.Invoke(null, false);
        }

        public void ShellMenuToggle()
        {
            this.NotifyShellMenuToggle?.Invoke(null, null);
        }

        #endregion

        #region Background Tasks

        /// <summary>
        /// Executes background task work. This call wraps the work with error handling, deferrals, and instantiating all the appropriate services needed by AppFramework.
        /// </summary>
        /// <param name="taskInstance">Background task instance.</param>
        /// <param name="work">The work that needs to be performed.</param>
        public async void ExecuteBackgroundWork(IBackgroundTaskInstance taskInstance, Action<CancellationToken> work)
        {
            if (!PlatformBase.IsDebugMode)
            {
                // Check if the app is alread in the foreground and if so, don't run the agent
                if (AgentSync.IsApplicationLaunched())
                    return;
            }

            BackgroundTaskRunInfo _info = new BackgroundTaskRunInfo();

            // Get a deferral, to prevent the task from closing prematurely 
            // while asynchronous code is still running.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            try
            {
                // Execute the background work
                _info.StartTime = DateTime.UtcNow;

                // Initialize the app
                await PlatformBase.CurrentCore.AppInitializingAsync(InitializationModes.Background);
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "Starting background task '{0}'...", taskInstance.Task.Name);

                CancellationTokenSource cts = new CancellationTokenSource();

                taskInstance.Canceled += (sender, reason) =>
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Background task '{0}' is being cancelled due to '{1}'...", taskInstance.Task.Name, reason);

                    // Store info on why this task was cancelled
                    _info.CancelReason = reason.ToString();
                    _info.EndTime = DateTime.UtcNow;

                    // Cancel/dispose the token
                    cts?.Cancel();
                    cts?.Dispose();
                };

                work(cts.Token);

                // Task ran without error
                _info.RunSuccessfully = true;
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "Completed execution of background task '{0}'!", taskInstance.Task.Name);
            }
            catch (OperationCanceledException)
            {
                // Task was aborted via the cancelation token
                PlatformBase.CurrentCore.Logger.Log(LogLevels.Warning, "Background task '{0}' had an OperationCanceledException with reason '{1}'.", taskInstance.Task.Name, _info.CancelReason);
            }
            catch (Exception ex)
            {
                // Task threw an exception, store/log the error details
                _info.ExceptionDetails = ex.ToString();
                PlatformBase.CurrentCore.Logger.LogErrorFatal(ex, "Background task '{0}' failed with exception to run to completion: {1}", taskInstance.Task.Name, ex.Message);
            }
            finally
            {
                _info.EndTime = DateTime.UtcNow;

                // Store the task status information
                PlatformBase.CurrentCore.Storage.SaveSetting("TASK_" + taskInstance.Task.Name, _info, ApplicationData.Current.LocalSettings);

                // Shutdown the task
                PlatformBase.CurrentCore.AppSuspending();
                deferral.Complete();
            }
        }

        #endregion

        #endregion Methods
    }

    public abstract class PlatformBase<VM, L, R> : PlatformBase
        where VM : ViewModelBase, new()
        where L : AppSettingsLocalBase
        where R : AppSettingsRoamingBase
    {
        #region Variables

        private bool _settingsIsLocalDataDirty = false;
        private bool _settingsIsRoamingDataDirty = false;

        #endregion

        #region Properties

        private VM _ViewModel;
        /// <summary>
        /// Gets the MainViewModel global instance for the application.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public VM ViewModel
        {
            get { return _ViewModel; }
            set { this.SetProperty(ref _ViewModel, value); }
        }

        /// <summary>
        /// Gets local app settings for this app.
        /// </summary>
        public L AppSettingsLocal
        {
            get { return base.AppSettingsLocalCore as L; }
        }

        /// <summary>
        /// Gets roaming app settings for this app.
        /// </summary>
        public R AppSettingsRoaming
        {
            get { return base.AppSettingsRoamingCore as R; }
        }

        #endregion

        #region Constructors

        public PlatformBase()
        {
        }

        #endregion

        #region Methods

        public override async Task AppInitializingAsync(InitializationModes mode)
        {
            await base.AppInitializingAsync(mode);

            if (this.AppSettingsLocal == null)
            {
                base.AppSettingsLocalCore = this.Storage.LoadSetting<L>("AppSettingsLocal", ApplicationData.Current.LocalSettings) ?? Activator.CreateInstance<L>();
                this.NotifyPropertyChanged(() => this.AppSettingsLocal);
                this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            }
            if (this.AppSettingsRoaming == null)
            {
                base.AppSettingsRoamingCore = this.Storage.LoadSetting<R>("AppSettingsRoaming", ApplicationData.Current.RoamingSettings) ?? Activator.CreateInstance<R>();
                this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
                this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;
            }

            this.CheckForFullLogging();

            if (this.ViewModel == null)
                this.ViewModelCore = this.ViewModel = Activator.CreateInstance<VM>();

            if (mode == InitializationModes.New)
            {
                // Check for previous app crashes
                if (this.IsXbox == false)
                {
                    // TODO Should automatically send error reports to cloud on XBOX
                    await this.Logger.CheckForFatalErrorReportsAsync(this.ViewModel);
                }

                // Check to see if the user should be prompted to rate the application
                await this.Ratings.CheckForRatingsPromptAsync(this.ViewModel);
            }

            try
            {
                await this.OnAppInitializingAsync(mode);
            }
            catch (Exception ex)
            {
                this.Logger.LogError(ex, "Error while excuting OnAppInitializingAsync");
                throw ex;
            }
        }

        public override void AppSuspending()
        {
            try
            {
                base.AppSuspending();
            }
            finally
            {
                // Save app settings
                this.SaveSettings();
            }

            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;
        }

        internal override async Task SignoutAllAsync()
        {
            await base.SignoutAllAsync();

            // Instantiate a new instance of settings and the MainViewModel 
            // to ensure no previous user data is shown on the UI.
            this.ResetAppSettings();

            this.ViewModelCore = this.ViewModel = Activator.CreateInstance<VM>();
        }

        /// <summary>
        /// Reset all the app settings back to their defaults.
        /// </summary>
        private void ResetAppSettings()
        {
            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;

            _settingsIsLocalDataDirty = true;
            _settingsIsRoamingDataDirty = true;

            base.AppSettingsLocalCore = Activator.CreateInstance<L>();
            this.NotifyPropertyChanged(() => this.AppSettingsLocal);
            this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            base.AppSettingsRoamingCore = Activator.CreateInstance<R>();
            this.NotifyPropertyChanged(() => this.AppSettingsRoaming);
            this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;

            this.SaveSettings();
            this.CheckForFullLogging();
        }

        /// <summary>
        /// Saves any app settings only if the data had changed.
        /// </summary>
        public override void SaveSettings(bool forceSave = false)
        {
            if (_settingsIsLocalDataDirty || forceSave)
            {
                this.Storage.SaveSetting(nameof(AppSettingsLocal), this.AppSettingsLocal, ApplicationData.Current.LocalSettings);
                _settingsIsLocalDataDirty = false;
            }

            if (_settingsIsRoamingDataDirty || forceSave)
            {
                this.Storage.SaveSetting(nameof(AppSettingsRoaming), this.AppSettingsRoaming, ApplicationData.Current.RoamingSettings);
                _settingsIsRoamingDataDirty = false;
            }
        }

        private void CheckForFullLogging()
        {
            if (this.AppSettingsRoaming.EnableFullLogging)
                this.Logger.CurrentLevel = LogLevels.Debug;
            else
                this.Logger.CurrentLevel = LogLevels.Warning;
        }

        /// <summary>
        /// Generates a unique tile ID used for secondary tiles based on a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a unique tile ID.</param>
        /// <returns>String representing a unique tile ID for the model else null if not supported.</returns>
        public override string GenerateModelTileID(IModel model)
        {
            if (model == this.ViewModel)
                return string.Empty;
            else if (model is IUniqueModel imodel)
                return $"{model.GetType().Name}_{imodel.ID}";
            else
                return null;
        }

        #endregion

        #region Event Handlers

        private void AppSettingsLocal_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _settingsIsLocalDataDirty = true;
        }

        private void AppSettingsRoaming_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _settingsIsRoamingDataDirty = true;

            if (e.PropertyName == nameof(this.AppSettingsRoaming.EnableFullLogging))
                this.CheckForFullLogging();
        }

        #endregion Events
    }
}