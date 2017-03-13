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
        private bool _settingsIsLocalDataDirty = false;
        private bool _settingsIsRoamingDataDirty = false;

        #endregion Variables

        #region Properties

        private MainViewModelBase _ViewModel;
        /// <summary>
        /// Gets the MainViewModel global instance for the application.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public MainViewModelBase ViewModel
        {
            get { return _ViewModel; }
            protected set { this.SetProperty(ref _ViewModel, value); }
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

        private AppSettingsLocal _AppSettingsLocal;
        /// <summary>
        /// Gets local app settings for this app.
        /// </summary>
        public AppSettingsLocal AppSettingsLocal
        {
            get { return _AppSettingsLocal; }
            private set { this.SetProperty(ref _AppSettingsLocal, value); }
        }

        private AppSettingsRoaming _AppSettingsRoaming;
        /// <summary>
        /// Gets roaming app settings for this app.
        /// </summary>
        public AppSettingsRoaming AppSettingsRoaming
        {
            get { return _AppSettingsRoaming; }
            private set { this.SetProperty(ref _AppSettingsRoaming, value); }
        }

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

        protected PlatformBase()
        {
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

            this.AppSettingsLocal = this.Storage.LoadSetting<AppSettingsLocal>("AppSettingsLocal", ApplicationData.Current.LocalSettings) ?? new AppSettingsLocal();
            this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            this.AppSettingsRoaming = this.Storage.LoadSetting<AppSettingsRoaming>("AppSettingsRoaming", ApplicationData.Current.RoamingSettings) ?? new AppSettingsRoaming();
            this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;

            // Initializes all service
            foreach (var service in _services)
            {
                this.Logger.Log(LogLevels.Debug, "Initializing service '{0}'...", service.Key);
                await this.CheckInitializationAsync(service.Value);
            }

            // Record the userID to analytics
            this.Analytics.SetUser(GetService<AppInfoProviderBase>().UserID);

            // Execute only on first runs of the platform
            if (mode == InitializationModes.New)
            {
                // Provide platform languages to analytics
                this.Analytics.Event("CurrentCulture", System.Globalization.CultureInfo.CurrentCulture.Name);
                this.Analytics.Event("CurrentUICulture", System.Globalization.CultureInfo.CurrentUICulture.Name);
            }
            this.Logger.Log(LogLevels.Debug, "Initializing services is complete!");

            // Register all background agents
            if (mode != InitializationModes.Background)
                this.BackgroundRegistrationTask = this.BackgroundTasks.RegisterAllAsync();
        }

        /// <summary>
        /// Global suspension of the app and any custom logic to execute on suspend of the app.
        /// </summary>
        public virtual void AppSuspending()
        {
            this.Logger.Log(LogLevels.Warning, "APP SUSPENDING - Initialization mode was {0}", this.InitializationMode);

            // Save app settings
            this.SaveSettings();

            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;
        }

        /// <summary>
        /// Saves any app settings only if the data had changed.
        /// </summary>
        public void SaveSettings(bool forceSave = false)
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

        /// <summary>
        /// Reset all the app settings back to their defaults.
        /// </summary>
        protected void ResetAppSettings()
        {
            if (this.AppSettingsLocal != null)
                this.AppSettingsLocal.PropertyChanged -= AppSettingsLocal_PropertyChanged;
            if (this.AppSettingsRoaming != null)
                this.AppSettingsRoaming.PropertyChanged -= AppSettingsRoaming_PropertyChanged;

            _settingsIsLocalDataDirty = true;
            _settingsIsRoamingDataDirty = true;

            this.AppSettingsLocal = new AppSettingsLocal();
            this.AppSettingsLocal.PropertyChanged += AppSettingsLocal_PropertyChanged;
            this.AppSettingsRoaming = new AppSettingsRoaming();
            this.AppSettingsRoaming.PropertyChanged += AppSettingsRoaming_PropertyChanged;

            this.SaveSettings();
        }

        /// <summary>
        /// Retrieve an instance of a type registered as a platform service.
        /// </summary>
        /// <typeparam name="T">Type reference of the service to retrieve.</typeparam>
        /// <returns>Instance of type T if it was already initialized or null if not found.</returns>
        internal static T GetService<T>() where T : ServiceBase
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
        internal static void SetService<T>(T instance) where T : ServiceBase
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

        protected void CheckForFullLogging()
        {
            if (this.AppSettingsRoaming.EnableFullLogging)
                this.Logger.CurrentLevel = LogLevels.Debug;
            else
                this.Logger.CurrentLevel = LogLevels.Warning;
        }

        #region Application Core

        /// <summary>
        /// Global unhandled exception handler for your application.
        /// </summary>
        /// <param name="e"></param>
        /// <returns>True if the exception was handled else false.</returns>
        public bool AppUnhandledException(Exception e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // If the Native debugger is in use, give us a clue in the Output window at least
                System.Diagnostics.Debug.WriteLine("Unhandled exception:" + e.Message);
                System.Diagnostics.Debug.WriteLine(e.StackTrace);

                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }

            // Only log this when the debugger is not attached and you're in RELEASE mode
            try
            {
                PlatformBase.GetService<AnalyticsManager>().Error(e, "Unhandled Exception");
            }
            catch { }

            try
            {
                PlatformBase.GetService<LoggingService>().LogErrorFatal(e);
            }
            catch (Exception exLog)
            {
                System.Diagnostics.Debug.WriteLine("Exception logging to Logger in AppUnhandledException!");
                System.Diagnostics.Debug.WriteLine(exLog.ToString());
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
            await this.OnInitialize();
            this.ShellMenuClose();
        }

        /// <summary>
        /// Signs a user out of the app.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public virtual Task OnInitialize()
        {
            return Task.CompletedTask;
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
