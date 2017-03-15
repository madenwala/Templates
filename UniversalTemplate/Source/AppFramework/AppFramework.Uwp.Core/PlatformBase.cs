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
        private Type _mainViewModelType;

        #endregion Variables

        #region Properties

        public event EventHandler OnAppSettingsReset;

        private ViewModelBase _ViewModel;
        /// <summary>
        /// Gets the MainViewModel global instance for the application.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public ViewModelBase ViewModel
        {
            get { return _ViewModel; }
            protected set { this.SetProperty(ref _ViewModel, value); }
        }

        private AppSettingsLocalBase _AppSettingsLocal;
        /// <summary>
        /// Gets local app settings for this app.
        /// </summary>
        public AppSettingsLocalBase AppSettingsLocal
        {
            get { return _AppSettingsLocal; }
            protected internal set { this.SetProperty(ref _AppSettingsLocal, value); }
        }

        private AppSettingsRoamingBase _AppSettingsRoaming;
        /// <summary>
        /// Gets roaming app settings for this app.
        /// </summary>
        public AppSettingsRoamingBase AppSettingsRoaming
        {
            get { return _AppSettingsRoaming; }
            protected internal set { this.SetProperty(ref _AppSettingsRoaming, value); }
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

        #endregion Properties

        #region Constructors

        protected PlatformBase(Type mainViewModelType)
        {
            _mainViewModelType = mainViewModelType;

            // Instantiate all the application services.
            this.Logger = new LoggingService();
            this.Analytics = new AnalyticsManager();
            this.Storage = new StorageManager();
            this.AuthManager = new AuthorizationManager();
            this.Cryptography = new CryptographyProvider();
            this.Geocode = new GeocodingService();
            this.Geolocation = new GeolocationService();
            this.Ratings = new RatingsManager();
            this.VoiceCommandManager = new VoiceCommandManager();
            this.Jumplist = new JumplistManager();
            this.WebAccountManager = new WebAccountManager();
        }

        static PlatformBase()
        {
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

        /// <summary>
        /// Provides access to application services.
        /// </summary>
        public static PlatformBase Current { get; set; }

        /// <summary>
        /// Gets the current device family this app is executing on.
        /// </summary>
        public static DeviceFamily DeviceFamily { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        /// Global initialization of the app and loads all app settings and initializes all services.
        /// </summary>
        /// <param name="mode">Specifies the mode of this app instance and how it's executing.</param>
        /// <returns>Awaitable task is returned.</returns>
        public virtual async Task AppInitializingAsync(InitializationModes mode)
        {
            this.InitializationMode = mode;
            this.Logger.Log(LogLevels.Warning, "APP INITIALIZING - Initialization mode is {0}", this.InitializationMode);

            this.AppSettingsInitializing();

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
            if (mode != InitializationModes.Background)
                this.BackgroundRegistrationTask = this.BackgroundTasks.RegisterAllAsync();

            if (this.ViewModel == null)
                this.ViewModel = Activator.CreateInstance(_mainViewModelType) as ViewModelBase;
            
            if (mode == InitializationModes.New)
            {
                // Check for previous app crashes
                await this.Logger.CheckForFatalErrorReportsAsync(this.ViewModel);

                // Check to see if the user should be prompted to rate the application
                await this.Ratings.CheckForRatingsPromptAsync(this.ViewModel);
            }
        }

        internal abstract void AppSettingsInitializing();

        /// <summary>
        /// Global suspension of the app and any custom logic to execute on suspend of the app.
        /// </summary>
        public virtual void AppSuspending()
        {
            this.Logger.Log(LogLevels.Warning, "APP SUSPENDING - Initialization mode was {0}", this.InitializationMode);

            // Save app settings
            this.SaveSettings();
        }

        /// <summary>
        /// Saves any app settings only if the data had changed.
        /// </summary>
        public abstract void SaveSettings(bool forceSave = false);

        /// <summary>
        /// Reset all the app settings back to their defaults.
        /// </summary>
        internal abstract void ResetAppSettings();

        /// <summary>
        /// Retrieve an instance of a type registered as a platform service.
        /// </summary>
        /// <typeparam name="T">Type reference of the service to retrieve.</typeparam>
        /// <returns>Instance of type T if it was already initialized or null if not found.</returns>
        private static T GetService<T>() where T : ServiceBase
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
        private static void SetService<T>(T instance) where T : ServiceBase
        {
            // Check if T is already registered
            if (_services.ContainsKey(typeof(T)))
            {
                // Shutdown the old instance of T
                var service = _services[typeof(T)];
                _services.Remove(typeof(T));
            }

            _services.Add(typeof(T), instance);
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

        internal void FireOnAppSettingsResetEvent()
        {
            this.OnAppSettingsReset?.Invoke(null, null);
        }

        #region Application Core

        /// <summary>
        /// Global unhandled exception handler for your application.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>True if the exception was handled else false.</returns>
        public bool AppUnhandledException(Exception e)
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
                PlatformBase.Current.Analytics.Error(e, "Unhandled Exception");
            }
            catch { }

            try
            {
                PlatformBase.Current.Logger.LogErrorFatal(e);
            }
            catch (Exception exLog)
            {
                Debug.WriteLine("Exception logging to Logger in AppUnhandledException!");
                Debug.WriteLine(exLog.ToString());
            }

            return false;
        }

        /// <summary>
        /// Logic performed during sign out of a user in this application.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public async Task SignoutAllAsync()
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

            // Instantiate a new instance of settings and the MainViewModel 
            // to ensure no previous user data is shown on the UI.
            this.ResetAppSettings();

            this.ViewModel = Activator.CreateInstance(_mainViewModelType) as ViewModelBase;
            this.ShellMenuClose();
        }
        
        #endregion

        #region Generate Models

        /// <summary>
        /// Creates a querystring parameter string from a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a querystring.</param>
        /// <returns>Query string representing the model provided.</returns>
        public abstract string GenerateModelArguments(IModel model);

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
        public abstract Task<IModel> GenerateModelFromTileIdAsync(string tileID, CancellationToken ct);

        #endregion

        #region Split View Menu

        /// <summary>
        /// Event which notifies the shell to open or close the menu.
        /// </summary>
        public event EventHandler<bool> NotifyShellMenuToggle;

        public void ShellMenuOpen()
        {
            this.NotifyShellMenuToggle?.Invoke(null, true);
        }

        public void ShellMenuClose()
        {
            this.NotifyShellMenuToggle?.Invoke(null, false);
        }

        #endregion

        #endregion Methods
    }

    public abstract class PlatformNewBase<VM, L, R> : PlatformBase
        where VM : ViewModelBase
        where L : AppSettingsLocalBase
        where R : AppSettingsRoamingBase
    {
        private bool _settingsIsLocalDataDirty = false;
        private bool _settingsIsRoamingDataDirty = false;

        public PlatformNewBase() : base(typeof(VM))
        {
        }

        internal override void AppSettingsInitializing()
        {
            if (this.AppSettingsLocal == null)
            {
                this.AppSettingsLocal = this.Storage.LoadSetting<L>("AppSettingsLocal", ApplicationData.Current.LocalSettings) ?? Activator.CreateInstance<L>();
                this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            }
            if (this.AppSettingsRoaming == null)
            {
                this.AppSettingsRoaming = this.Storage.LoadSetting<R>("AppSettingsRoaming", ApplicationData.Current.RoamingSettings) ?? Activator.CreateInstance<R>();
                this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;
            }

            this.CheckForFullLogging();
            this.FireOnAppSettingsResetEvent();
        }

        /// <summary>
        /// Reset all the app settings back to their defaults.
        /// </summary>
        internal override void ResetAppSettings()
        {
            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;

            _settingsIsLocalDataDirty = true;
            _settingsIsRoamingDataDirty = true;

            this.AppSettingsLocal = Activator.CreateInstance<L>();
            this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            this.AppSettingsRoaming = Activator.CreateInstance<R>();
            this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;

            this.SaveSettings();
            this.CheckForFullLogging();
            this.FireOnAppSettingsResetEvent();
        }

        /// <summary>
        /// Saves any app settings only if the data had changed.
        /// </summary>
        public override void SaveSettings(bool forceSave = false)
        {
            if (_settingsIsLocalDataDirty || forceSave)
            {
                this.Storage.SaveSetting("AppSettingsLocal", this.AppSettingsLocal, ApplicationData.Current.LocalSettings);
                _settingsIsLocalDataDirty = false;
            }

            if (_settingsIsRoamingDataDirty || forceSave)
            {
                this.Storage.SaveSetting("AppSettingsRoaming", this.AppSettingsRoaming, ApplicationData.Current.RoamingSettings);
                _settingsIsRoamingDataDirty = false;
            }
        }

        public override void AppSuspending()
        {
            base.AppSuspending();
            
            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;
        }

        private void CheckForFullLogging()
        {
            if (this.AppSettingsRoaming.EnableFullLogging)
                this.Logger.CurrentLevel = LogLevels.Debug;
            else
                this.Logger.CurrentLevel = LogLevels.Warning;
        }

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