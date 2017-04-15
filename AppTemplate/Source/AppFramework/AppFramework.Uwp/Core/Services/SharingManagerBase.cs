using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using AppFramework.Core.Strings;
using AppFramework.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace AppFramework.Core
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public SharingManagerBase SharingManager
        {
            get { return GetService<SharingManagerBase>(); }
            protected set { SetService(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    public abstract class SharingManagerBase : ServiceBase
    {
        #region Properties

        private CommandBase _shareCommand = null;
        /// <summary>
        /// Command to navigate to the share functionality.
        /// </summary>
        public CommandBase ShareCommand
        {
            get { return _shareCommand ?? (_shareCommand = new ShareCommand()); }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Method to populate DataRequest object which is then shared with the OS sharing feature.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        protected abstract void SetShareContent(DataRequest request, IModel model);

        /// <summary>
        /// Share a model of data with Windows.
        /// </summary>
        /// <param name="model"></param>
        public void Share(IModel model)
        {
            PlatformBase.CurrentCore.Analytics.Event("Share");

            DataTransferManager.GetForCurrentView().DataRequested += (sender, e) =>
            {
                try
                {
                    PlatformBase.CurrentCore.Logger.Log(LogLevels.Information, "SetShareContent - Model: {0}", model?.GetType().Name);

                    if (model is WebBrowserViewModel)
                    {
                        var vm = model as WebBrowserViewModel;
                        e.Request.Data.Properties.Title = vm.Title;
                        e.Request.Data.SetWebLink(new Uri(vm.CurrentUrl, UriKind.Absolute));
                    }
                    else
                    {
                        try
                        {
                            var args = PlatformBase.CurrentCore.GenerateModelArguments(model);
                            var url = PlatformBase.CurrentCore.AppInfo.GetDeepLink(args);
                            if(url != null)
                                e.Request.Data.Properties.ContentSourceApplicationLink = new Uri(url.Replace("//", ""), UriKind.Absolute);

                            this.SetShareContent(e.Request, model ?? PlatformBase.CurrentCore.ViewModel);
                        }
                        catch(Exception ex)
                        {
                            PlatformBase.CurrentCore.Logger.LogError(ex, "Failure while calling SetShareContent");
                            throw ex;
                        }
                    }

                    if (string.IsNullOrEmpty(e.Request.Data.Properties.Title))
                    {
                        e.Request.Data.Properties.Title = PlatformBase.CurrentCore.AppInfo.AppName;
                        e.Request.Data.Properties.Description = PlatformBase.CurrentCore.AppInfo.AppDescription;
                        e.Request.Data.Properties.ContentSourceApplicationLink = new Uri(PlatformBase.CurrentCore.AppInfo.StoreURL, UriKind.Absolute);
                        string body = string.Format(Resources.ApplicationSharingBodyText, PlatformBase.CurrentCore.AppInfo.AppName, PlatformBase.CurrentCore.AppInfo.StoreURL);
                        e.Request.Data.SetText(body);
                    }
                }
                catch (Exception ex)
                {
                    PlatformBase.CurrentCore.Logger.LogError(ex, "Error in OnDataRequested");
#if DEBUG
                    e.Request.FailWithDisplayText(ex.ToString());
#else
                    e.Request.FailWithDisplayText(Strings.Resources.TextErrorGeneric);
#endif
                }
            };
            DataTransferManager.ShowShareUI();
        }

        #endregion Sharing
    }

    internal sealed class DefaultSharingManager : SharingManagerBase
    {
        internal DefaultSharingManager()
        {
        }

        /// <summary>
        /// Shares the sz
        /// </summary>
        /// <param name="request"></param>
        /// <param name="model"></param>
        protected override void SetShareContent(DataRequest request, IModel model)
        {
            DataPackage dataPackage = request.Data;
            dataPackage.Properties.Title = PlatformBase.CurrentCore.AppInfo.AppName;
            dataPackage.Properties.Description = PlatformBase.CurrentCore.AppInfo.AppDescription;
            // TODO localize
            string body = $"Download {PlatformBase.CurrentCore.AppInfo.AppName} from the Windows Store!";
            dataPackage.SetText(body);
        }
    }
}