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
            protected set { SetService<SharingManagerBase>(value); }
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

        protected abstract void SetShareContent(DataRequest request, IModel model);

        /// <summary>
        /// Share a model of data with Windows.
        /// </summary>
        /// <param name="model"></param>
        public void Share(IModel model)
        {
            PlatformBase.Current.Analytics.Event("Share");

            DataTransferManager.GetForCurrentView().DataRequested += (sender, e) =>
            {
                try
                {
                    PlatformBase.Current.Logger.Log(LogLevels.Information, "SetShareContent - Model: {0}", model?.GetType().Name);

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
                            this.SetShareContent(e.Request, model ?? PlatformBase.Current.ViewModel);
                        }
                        catch(Exception ex)
                        {
                            PlatformBase.Current.Logger.LogError(ex, "Failure while calling SetShareContent");
                            throw ex;
                        }
                    }

                    if (string.IsNullOrEmpty(e.Request.Data.Properties.Title))
                    {
                        e.Request.Data.Properties.Title = PlatformBase.Current.AppInfo.AppName;
                        e.Request.Data.Properties.Description = Resources.ApplicationDescription;
                        e.Request.Data.Properties.ContentSourceApplicationLink = new Uri(PlatformBase.Current.AppInfo.StoreURL, UriKind.Absolute);
                        string body = string.Format(Resources.ApplicationSharingBodyText, PlatformBase.Current.AppInfo.AppName, PlatformBase.Current.AppInfo.StoreURL);
                        e.Request.Data.SetText(body);
                    }
                }
                catch (Exception ex)
                {
                    PlatformBase.Current.Logger.LogError(ex, "Error in OnDataRequested");
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
}