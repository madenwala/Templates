using AppFramework.Core.Models;
using AppFramework.Core.ViewModels;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.System.Profile;

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Singleton object which holds instances to all the services in this application.
    /// Also provides core app functionality for initializing and suspending your application,
    /// handling exceptions, and more.
    /// </summary>
    public partial class PlatformBase
    {
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

        #endregion

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

        #endregion
    }
}
