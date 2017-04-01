﻿using AppFramework.Core.Models;
using AppFramework.Core.Services;
using Contoso.Core.Models;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Contoso.Core.Services
{
    internal sealed class SharingManager: SharingManagerBase
    {
        #region Methods
        
        protected override void SetShareContent(DataRequest request, IModel model)
        {
            DataPackage dataPackage = request.Data;

            // Sharing is based on the model data that was passed in. Perform customized sharing based on the type of the model provided.
            if (model is ItemModel)
            {
                var args = Platform.Current.GenerateModelArguments(model);
                var url = Platform.Current.AppInfo.GetDeepLink(args);
                var m = model as ItemModel;
                dataPackage.Properties.Title = m.LineOne;
                dataPackage.Properties.Description = m.LineTwo;
                dataPackage.Properties.ContentSourceApplicationLink = new Uri(url.Replace("//", ""), UriKind.Absolute);
                string body = m.LineOne + Environment.NewLine + m.LineTwo + Environment.NewLine + m.LineThree + Environment.NewLine + m.LineFour;
                dataPackage.SetText(body);
            }
        }

        #endregion
    }
}