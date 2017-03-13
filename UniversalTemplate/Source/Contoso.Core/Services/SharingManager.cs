using AppFramework.Core;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using AppFramework.Core.Strings;
using AppFramework.Core.ViewModels;
using Contoso.Core.Models;
using Contoso.Core.ViewModels;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Contoso.Core.Services
{
    public sealed partial class SharingManager: SharingManagerBase
    {
        #region Methods
        
        protected override void SetShareContent(DataRequest request, IModel model)
        {
            DataPackage requestData = request.Data;

            Platform.Current.Logger.Log(LogLevels.Information, "SetShareContent - Model: {0}", model?.GetType().Name);

            // Sharing is based on the model data that was passed in. Perform customized sharing based on the type of the model provided.
            if (model is WebBrowserViewModel)
            {
                var vm = model as WebBrowserViewModel;
                requestData.Properties.Title = vm.Title;
                requestData.SetWebLink(new Uri(vm.CurrentUrl, UriKind.Absolute));
            }
            else if (model is ItemModel)
            {
                var args = Platform.Current.GenerateModelArguments(model);
                var url = Platform.Current.AppInfo.GetDeepLink(args);
                var m = model as ItemModel;
                requestData.Properties.Title = m.LineOne;
                requestData.Properties.Description = m.LineTwo;
                requestData.Properties.ContentSourceApplicationLink = new Uri(url.Replace("//", ""), UriKind.Absolute);
                string body = m.LineOne + Environment.NewLine + m.LineTwo + Environment.NewLine + m.LineThree + Environment.NewLine + m.LineFour;
                requestData.SetText(body);
            }
            else
            {
                requestData.Properties.Title = Resources.ApplicationName;
                requestData.Properties.Description = Resources.ApplicationDescription;
                requestData.Properties.ContentSourceApplicationLink = new Uri(Platform.Current.AppInfo.StoreURL, UriKind.Absolute);
                string body = string.Format(Resources.ApplicationSharingBodyText, Resources.ApplicationName, Platform.Current.AppInfo.StoreURL);
                requestData.SetText(body);
            }
        }

        #endregion
    }
}