using AppFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Notifications;
using Windows.UI.StartScreen;

namespace AppFramework.Core.Services
{
    public abstract partial class BaseNotificationsManager : BaseService, IServiceSignout
    {
        #region Methods

        /// <summary>
        /// Initialization logic which is called on launch of this application.
        /// </summary>
        protected override async Task OnInitializeAsync()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesEngagementManager"))
                await Microsoft.Services.Store.Engagement.StoreServicesEngagementManager.GetDefault().RegisterNotificationChannelAsync();
            await base.OnInitializeAsync();
        }

        public abstract Task<bool> CreateOrUpdateTileAsync(IModel model);

        public abstract void DisplayToast(IModel model);

        #region Public

        /// <summary>
        /// On signout of a user, clear tiles, toasts, notifications
        /// </summary>
        /// <returns></returns>
        public async Task SignoutAsync()
        {
            ToastNotificationManager.History.Clear();

            // Clear primary tile
            this.ClearTile(PlatformBase.CurrentCore.ViewModelCore);

            // Clear secondary tiles
            IReadOnlyList<SecondaryTile> list = await SecondaryTile.FindAllAsync();
            foreach (var tile in list)
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(tile.TileId).Clear();
        }

        /// <summary>
        /// Checks to see if a tile exists associated the the model specified. How it determines which tile to check against is determined by the
        /// class implementing this interface. If the platform does not support tiles, then the implementation should do nothing.
        /// </summary>
        /// <param name="model">Model which contains the data to find the tile.</param>
        /// <returns>True if a tile exists associated to the model specified or false if no tile was found.</returns>
        public bool HasTile(IModel model)
        {
            var tileID = PlatformBase.CurrentCore.GenerateModelTileID(model);
            if (tileID == string.Empty)
                return true;
            else if (!string.IsNullOrEmpty(tileID))
                return SecondaryTile.Exists(tileID);
            else
                return false;
        }

        /// <summary>
        /// Updates all tiles or any other UI features currently in use on the device.
        /// </summary>
        public async Task UpdateAllSecondaryTilesAsync(CancellationToken ct)
        {
            // Find all secondary tiles
            var list = await SecondaryTile.FindAllAsync();
            foreach (var tile in list)
            {
                var model = await PlatformBase.CurrentCore.GenerateModelFromTileIdAsync(tile.TileId, ct);
                ct.ThrowIfCancellationRequested();

                if (model != null)
                {
                    await this.CreateOrUpdateTileAsync(model);
                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        /// <summary>
        /// Deletes a tile associated to the model specified. How it determines which tile to delete is determined by the class implementing this
        /// interface. If the platform does not support tiles, then the implementation should do nothing.
        /// </summary>
        /// <param name="model">Model which contains the data necessary to find the tile to delete.</param>
        public async Task<bool> DeleteTileAsync(IModel model)
        {
            var tileID = PlatformBase.CurrentCore.GenerateModelTileID(model);
            if (!string.IsNullOrEmpty(tileID))
            {
                SecondaryTile tile = new SecondaryTile(tileID);
                return await tile.RequestDeleteAsync();
            }
            else
                throw new ArgumentException("Tile does not exist for model!");
        }

        /// <summary>
        /// Clears a tile associated to the model specified. How it determines which tile(s) to clear is determined by the class implementing this
        /// interface. If the platform does not support tiles, then the implementation should do nothing.
        /// </summary>
        /// <param name="model">Model which contains the data necessary to find the tile to clear.</param>
        public void ClearTile(IModel model)
        {
            var tileID = PlatformBase.CurrentCore.GenerateModelTileID(model);
            if (tileID == string.Empty)
                TileUpdateManager.CreateTileUpdaterForApplication().Clear();
            else if (!string.IsNullOrEmpty(tileID))
                TileUpdateManager.CreateTileUpdaterForSecondaryTile(tileID).Clear();
            else
                throw new ArgumentException("Tile does not exist for model!");
        }

        #endregion

        #region Private

        protected internal class TileVisualOptions
        {
            public TileVisualOptions()
            {
                this.Square71x71Logo = new Uri("ms-appx:///Assets/Square71x71Logo.png");
                this.Square150x150Logo = new Uri("ms-appx:///Assets/Square150x150Logo.png");
                this.Wide310x150Logo = new Uri("ms-appx:///Assets/Wide310x150Logo.png");
                this.Square310x310Logo = new Uri("ms-appx:///Assets/Square310x310Logo.png");
            }
            public Uri Square71x71Logo { get; set; }
            public Uri Square150x150Logo { get; set; }
            public Uri Wide310x150Logo { get; set; }
            public Uri Square310x310Logo { get; set; }
            public Rect Rect { get; set; }
            public Windows.UI.Popups.Placement PopupPlacement { get; set; }
        }

        protected async Task<bool> CreateOrUpdateSecondaryTileAsync(SecondaryTile tile, TileVisualOptions options)
        {
            if (tile == null)
                return false;

            tile.VisualElements.ShowNameOnSquare150x150Logo = true;

            tile.VisualElements.Square71x71Logo = options.Square71x71Logo ?? null;
            tile.VisualElements.Square150x150Logo = options.Square150x150Logo ?? null;

            if (!(Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons"))))
            {
                tile.VisualElements.Wide310x150Logo = options.Wide310x150Logo ?? null;
                tile.VisualElements.Square310x310Logo = options.Square310x310Logo ?? null;
                tile.VisualElements.ShowNameOnWide310x150Logo = true;
                tile.VisualElements.ShowNameOnSquare310x310Logo = true;
            }

            if (SecondaryTile.Exists(tile.TileId))
            {
                return await tile.UpdateAsync();
            }
            else
            {
                if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    if (options.Rect == null)
                        return await tile.RequestCreateAsync();
                    else
                        return await tile.RequestCreateForSelectionAsync(options.Rect, options.PopupPlacement);
                }
                else if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent(("Windows.Phone.UI.Input.HardwareButtons")))
                {
                    // OK, the tile is created and we can now attempt to pin the tile.
                    // Since pinning a secondary tile on Windows Phone will exit the app and take you to the start screen, any code after 
                    // RequestCreateForSelectionAsync or RequestCreateAsync is not guaranteed to run.  For an example of how to use the OnSuspending event to do
                    // work after RequestCreateForSelectionAsync or RequestCreateAsync returns, see Scenario9_PinTileAndUpdateOnSuspend in the SecondaryTiles.WindowsPhone project.
                    return await tile.RequestCreateAsync();
                }
            }

            return false;
        }

        #endregion

        #endregion
    }

    internal sealed class DefaultNotificationsManager : BaseNotificationsManager
    {
        internal DefaultNotificationsManager()
        {
        }

        public override Task<bool> CreateOrUpdateTileAsync(IModel model)
        {
            return Task.FromResult<bool>(false);
        }

        public override void DisplayToast(IModel model)
        {
        }
    }
}
