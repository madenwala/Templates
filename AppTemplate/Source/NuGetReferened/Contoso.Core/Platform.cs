using AppFramework.Core;
using AppFramework.Core.Extensions;
using AppFramework.Core.Models;
using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Services;
using Contoso.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Contoso.Core
{
    public sealed partial class Platform : PlatformBase<MainViewModel, AppSettingsLocal, AppSettingsRoaming, WebViewModel>
    {
        #region Properties

        /// <summary>
        /// Provides access to application services.
        /// </summary>
        public static Platform Current { get { return PlatformBase.CurrentCore as Platform; } private set { PlatformBase.CurrentCore = value; } }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public AppInfoProvider AppInfo
        {
            get { return GetService<AppInfoProvider>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the cryptography provider of the platform currently executing.
        /// </summary>
        public AuthorizationManager AuthManager
        {
            get { return GetService<AuthorizationManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the geocoding service adapter implement of the platform currently executing.
        /// </summary>
        public BackgroundTasksManager BackgroundTasks
        {
            get { return GetService<BackgroundTasksManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the notifications service of the platform currently executing. Provides you the ability to display toasts or manage tiles or etc on the executing platform.
        /// </summary>
        internal NotificationsManager Notifications
        {
            get { return GetService<NotificationsManager>(); }
            private set { SetService(value); }
        }

        /// <summary>
        /// Gets the ability to navigate to different parts of an application specific to the platform currently executing.
        /// </summary>
        public NavigationManagerBase Navigation
        {
            get { return GetService<NavigationManagerBase>(); }
            set { SetService(value); }
        }

        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public SharingManager SharingManager
        {
            get { return GetService<SharingManager>(); }
            private set { SetService(value); }
        }
        
        #endregion

        #region Constructors

        static Platform()
        {
#if DEBUG
            IsDebugMode = true;
#else
            IsDebugMode = System.Diagnostics.Debugger.IsAttached;
#endif
            Platform.Current = new Platform();
        }

        private Platform()
        {
            // Instantiate all the application services.
            this.AuthManager = new AuthorizationManager();
            this.BackgroundTasks = new BackgroundTasksManager();
            this.AppInfo = new AppInfoProvider();
            this.Notifications = new NotificationsManager();
            this.SharingManager = new SharingManager();
            this.Geolocation = new AppFramework.Core.Services.GeolocationService();

            this.IsFirstRunCheckEnabled = true;
        }

        #endregion

        #region Application Core

        /// <summary>
        /// Logic performed during initialization of the application.
        /// </summary>
        /// <param name="mode">Mode indicates how this app instance is being run.</param>
        /// <returns>Awaitable task is returned.</returns>
        protected override Task OnAppInitializingAsync(InitializationModes mode)
        {
            // Your custom app logic which you want to run on launch of
            // your app should be placed here.
            return Task.CompletedTask;
        }

        /// <summary>
        /// Logic performed during suspend of the application.
        /// </summary>
        protected override void OnAppSuspending()
        {
            // Your custom app logic which you want to run on suspend of
            // your app should be placed here.
        }

        #endregion

        #region Overrides

        /// <summary>
        /// Creates a querystring parameter string from a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a querystring.</param>
        /// <returns>Query string representing the model provided.</returns>
        protected override void OnGenerateModelArguments(Dictionary<string, string> dic, IModel model)
        {
            // For each model you support, add any custom properties 
            // to the dictionary based on the type of object
            if (model is ItemModel itemModel)
            {
                dic.Add("LineOne", itemModel.LineOne);
            }
        }

        /// <summary>
        /// Generates a unique tile ID used for secondary tiles based on a model instance.
        /// </summary>
        /// <param name="model">Model to convert into a unique tile ID.</param>
        /// <returns>String representing a unique tile ID for the model else null if not supported.</returns>
        public override string GenerateModelTileID(IModel model)
        {
            // For each model you want to support, you'll want to customize the ID that gets generated to be unique.
            if (model is MainViewModel || model is ShellViewModel)
            {
                return string.Empty;
            }
            else if (model is ItemModel item)
            {
                return "ItemModel_" + item.ID;
            }
            else
                return null;
        }

        /// <summary>
        /// Converts a tile ID back into an object instance.
        /// </summary>
        /// <param name="tileID">Tile ID to retrieve an object instance for.</param>
        /// <param name="ct">Cancelation token.</param>
        /// <returns>Object instance if found else null.</returns>
        public override async Task<IModel> GenerateModelFromTileIdAsync(string tileID, CancellationToken ct)
        {
            try
            {
                if (tileID.StartsWith("ItemModel_"))
                {
                    var id = tileID.Split('_').Last();
                    using (var api = new ClientApi())
                    {
                        return await api.GetItemByID(id, ct);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Couldn't generate model from tileID = {0}", tileID), ex);
            }

            throw new NotImplementedException(string.Format("App has not implemented creating a model from tileID = {0}", tileID));
        }

        #endregion

        #region Background Tasks

        /// <summary>
        /// Work that should be performed from the background agent.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public async Task TimedBackgroundWorkAsync(CancellationToken ct)
        {
            try
            {
                // Perform work that needs to be done on a background task/agent...
                if (this.AuthManager?.IsAuthenticated() != true)
                    return;

                this.Notifications.DisplayToast(this.ViewModel);

                // SAMPLE - Load data from your API, do any background work here.
                using (var api = new ClientApi())
                {
                    var data = await api.GetItems(ct);
                    if (data != null)
                    {
                        var items = data.ToObservableCollection();
                        if (items.Count > 0)
                        {
                            var index = DateTime.Now.Second % items.Count;
                            this.Notifications.DisplayToast(items[index]);
                        }
                    }

                    ct.ThrowIfCancellationRequested();

                    if (BackgroundWorkCost.CurrentBackgroundWorkCost <= BackgroundWorkCostValue.Medium)
                    {
                        // Update primary tile
                        await this.Notifications.CreateOrUpdateTileAsync(new ModelList<ItemModel>(data));

                        ct.ThrowIfCancellationRequested();

                        // Update all tiles pinned from this application
                        await this.Notifications.UpdateAllSecondaryTilesAsync(ct);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorFatal(ex, "Failed to complete BackgroundWork from background task due to: {0}", ex.Message);
                throw ex;
            }
        }

        #endregion
    }
}