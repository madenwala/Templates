using AppFramework.Core.Models;
using AppFramework.Core.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppFramework.Core.Extensions;

namespace AppFramework.Core
{
    /// <summary>
    /// Base class used to manage application execution and to access all services available to the solution.
    /// Provides core app functionality for initializing and suspending your application,
    /// handling exceptions, and more.
    /// </summary>
    public abstract partial class BasePlatform : BaseModel
    {
        #region Variables

        private static Dictionary<Type, BaseService> _services = new Dictionary<Type, BaseService>();

        #endregion Variables

        #region Properties

        /// <summary>
        /// Provides access to application services.
        /// </summary>
        protected internal static BasePlatform CurrentCore { get; protected set; }

        /// <summary>
        /// Gets the current device family this app is executing on.
        /// </summary>
        public static DeviceFamily DeviceFamily { get; private set; }

        public static bool IsDebugMode { get; protected set; }
        public static bool IsDesignMode { get; protected set; }

        private BaseAppSettingsLocal _AppSettingsLocal;
        /// <summary>
        /// Gets local app settings for this app.
        /// </summary>
        protected internal BaseAppSettingsLocal AppSettingsLocalCore
        {
            get { return _AppSettingsLocal; }
            protected set { this.SetProperty(ref _AppSettingsLocal, value); }
        }

        private BaseAppSettingsRoaming _AppSettingsRoaming;
        /// <summary>
        /// Gets roaming app settings for this app.
        /// </summary>
        protected internal BaseAppSettingsRoaming AppSettingsRoamingCore
        {
            get { return _AppSettingsRoaming; }
            protected set { this.SetProperty(ref _AppSettingsRoaming, value); }
        }

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

        public bool IsFirstRunCheckEnabled { get; protected set; }

        public bool IsFirstRun { get; private set; }

        public bool IsFirstRunAfterUpdate { get; private set; }

        public Version PreviousAppVersion { get; private set; }

        #endregion Properties

        #region Constructors

        internal BasePlatform()
        {
            // Instantiate all the application services.
            this.Logger = new LoggingService();
            //this.Analytics = new AnalyticsManager();
            //this.Storage = new StorageManager();
            //this.Cryptography = new CryptographyProvider();
            //this.Geocode = new GeocodingService();
            //this.Ratings = new RatingsManager();
            //this.VoiceCommandManager = new VoiceCommandManager();
            //this.Jumplist = new JumplistManager();
            //this.WebAccountManager = new WebAccountManager();
            //this.EmailProvider = new EmailProvider();
            //this.SharingManager = new DefaultSharingManager();
            //this.Notifications = new DefaultNotificationsManager();
        }

        static BasePlatform()
        {
#if DEBUG
            IsDebugMode = true;
#else
            IsDebugMode = System.Diagnostics.Debugger.IsAttached;
#endif
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
            //////////this.Analytics.SetUser(this.AppInfo.UserID);

            // Execute only on first runs of the platform
            if (mode == InitializationModes.New)
            {
                ////////// Provide platform info to analytics
                ////////this.Analytics.Event("OS-Version", Microsoft.Toolkit.Uwp.Helpers.SystemInformation.OperatingSystemVersion);
                ////////this.Analytics.Event("CurrentCulture", System.Globalization.CultureInfo.CurrentCulture.Name);
                ////////this.Analytics.Event("CurrentUICulture", System.Globalization.CultureInfo.CurrentUICulture.Name);

            }
            this.Logger.Log(LogLevels.Debug, "Initializing services is complete!");

            ////////// Register all background agents
            ////////if (mode != InitializationModes.Background && this.BackgroundTasks != null)
            ////////    this.BackgroundRegistrationTask = this.BackgroundTasks.RegisterAllAsync();
        }

        protected virtual Task OnAppInitializingAsync(InitializationModes mode)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Initializes a service if not already initialized.
        /// </summary>
        /// <param name="service">Service instance to intialize.</param>
        private async Task CheckInitializationAsync(BaseService service)
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
        public abstract void SaveSettings();

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
        protected internal static T GetService<T>() where T : BaseService
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
        protected internal static void SetService<T>(T instance) where T : BaseService
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

        protected internal static bool ContainsService<T>() where T : BaseService
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
                //////////this.Analytics.Error(e, "Unhandled Exception");
            }
            catch { }

            try
            {
                BasePlatform.CurrentCore.Logger.LogErrorFatal(e);
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
            return dic.ToQueryString<string, string>();
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


        #region First Run

        internal async Task FirstRunCheck()
        {
            this.IsFirstRun = false;
            this.IsFirstRunAfterUpdate = false;
            ////////string lastAppVersion = this.Storage.LoadSetting<string>("LastAppVersion");
            ////////string currentAppVersion = this.AppInfo.VersionNumber.ToString();

            ////////if (string.IsNullOrEmpty(lastAppVersion))
            ////////{
            ////////    this.IsFirstRun = true;
            ////////    await this.OnAppFirstRunAsync();
            ////////    this.Storage.SaveSetting("LastAppVersion", currentAppVersion);
            ////////}
            ////////else if (lastAppVersion != currentAppVersion)
            ////////{
            ////////    BasePlatform.CurrentCore.Logger.Log(LogLevels.Warning, "App has been updated from version {0} to version {1}", lastAppVersion, currentAppVersion);
            ////////    this.IsFirstRunAfterUpdate = true;
            ////////    await this.OnAppFirstRunAfterUpdateAsync(new Version(lastAppVersion));
            ////////    this.Storage.SaveSetting("LastAppVersion", currentAppVersion);
            ////////}
        }

        protected virtual Task OnAppFirstRunAsync()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnAppFirstRunAfterUpdateAsync(Version previousVersion)
        {
            return Task.CompletedTask;
        }

        #endregion

        #region Web View

        ////////internal abstract WebViewModelBase CreateWebViewModel(WebViewArguments args);

        #endregion

        #endregion Methods
    }
}