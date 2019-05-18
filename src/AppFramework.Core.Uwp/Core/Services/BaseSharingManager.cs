using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using AppFramework.Core.Strings;
using AppFramework.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace AppFramework.Core.Services
{
    public abstract class BaseSharingManager : BaseService
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
            BasePlatform.CurrentCore.Analytics.Event("Share");

            DataTransferManager.GetForCurrentView().DataRequested += (sender, e) =>
            {
                try
                {
                    BasePlatform.CurrentCore.Logger.Log(LogLevels.Information, "SetShareContent - Model: {0}", model?.GetType().Name);

                    if (model is WebViewModelBase)
                    {
                        var vm = model as WebViewModelBase;
                        e.Request.Data.Properties.Title = vm.Title;
                        e.Request.Data.SetWebLink(new Uri(vm.CurrentUrl, UriKind.Absolute));
                    }
                    else
                    {
                        try
                        {
                            var args = BasePlatform.CurrentCore.GenerateModelArguments(model);
                            var url = BasePlatform.CurrentCore.AppInfo.GetDeepLink(args);
                            if (url != null)
                            {
                                e.Request.Data.Properties.ContentSourceApplicationLink = new Uri(url.Replace("//", ""), UriKind.Absolute);
                                e.Request.Data.Properties.ContentSourceWebLink = new Uri(url.Replace("//", ""), UriKind.Absolute);
                            }

                            this.SetShareContent(e.Request, model ?? BasePlatform.CurrentCore.ViewModelCore);
                        }
                        catch(Exception ex)
                        {
                            BasePlatform.CurrentCore.Logger.LogError(ex, "Failure while calling SetShareContent");
                            throw ex;
                        }
                    }

                    if (string.IsNullOrEmpty(e.Request.Data.Properties.Title))
                    {
                        e.Request.Data.Properties.Title = BasePlatform.CurrentCore.AppInfo.AppName;
                        e.Request.Data.Properties.Description = BasePlatform.CurrentCore.AppInfo.AppDescription;
                        e.Request.Data.Properties.ContentSourceApplicationLink = new Uri(BasePlatform.CurrentCore.AppInfo.StoreURL, UriKind.Absolute);
                        string body = string.Format(Resources.ApplicationSharingBodyText, BasePlatform.CurrentCore.AppInfo.AppName, BasePlatform.CurrentCore.AppInfo.StoreURL);
                        e.Request.Data.SetText(body);
                    }
                }
                catch (Exception ex)
                {
                    BasePlatform.CurrentCore.Logger.LogError(ex, "Error in OnDataRequested");

                    if (BasePlatform.IsDebugMode)
                        e.Request.FailWithDisplayText(ex.ToString());
                    else
                        e.Request.FailWithDisplayText(Strings.Resources.TextErrorGeneric);

                }
            };
            DataTransferManager.ShowShareUI();
        }

        #endregion Sharing
    }

    internal sealed class DefaultSharingManager : BaseSharingManager
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
            request.Data.Properties.Title = BasePlatform.CurrentCore.AppInfo.AppName;
            request.Data.Properties.Description = BasePlatform.CurrentCore.AppInfo.AppDescription;
            // TODO localize
            string body = $"Download {BasePlatform.CurrentCore.AppInfo.AppName} from the Windows Store! {BasePlatform.CurrentCore.AppInfo.StoreURL}";
            request.Data.SetText(body);
        }
    }
}