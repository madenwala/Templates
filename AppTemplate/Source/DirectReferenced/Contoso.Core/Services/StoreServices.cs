using AppFramework.Core.Commands;
using AppFramework.Core.Services;
using Contoso.Core.Services;
using System;
using System.Threading.Tasks;

namespace Contoso.Core
{
    public partial class Platform
    {
        /// <summary>
        /// Gets the ability to navigate to different parts of an application specific to the platform currently executing.
        /// </summary>
        public StoreServices StoreServices
        {
            get { return GetService<StoreServices>(); }
            private set { SetService(value); }
        }
    }
}

namespace Contoso.Core.Services
{
    public sealed class StoreServices : ServiceBase
    {
        /// <summary>
        /// Initialization logic which is called on launch of this application.
        /// </summary>
        protected override async Task OnInitializeAsync()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesEngagementManager"))
                await Microsoft.Services.Store.Engagement.StoreServicesEngagementManager.GetDefault().RegisterNotificationChannelAsync();
            await base.OnInitializeAsync();
        }

        #region Feedback Commands

        public bool IsFeedbackEnabled
        {
            // Microsoft Store Engagement and Monetization SDK
            // https://visualstudiogallery.msdn.microsoft.com/229b7858-2c6a-4073-886e-cbb79e851211/view/Reviews?sortBy=RatingDescending
            get
            {
                if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher"))
                    return Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.IsSupported();
                else
                    return false;
            }
        }

        private CommandBase _navigateToFeedbackCommand = null;
        public CommandBase NavigateToFeedbackCommand
        {
            get
            {
                return _navigateToFeedbackCommand ?? (_navigateToFeedbackCommand = new GenericCommand("NavigateToFeedbackCommand", async () =>
                {
                    if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher"))
                    {
                        Platform.Current.Analytics.Event("FeedbackLauncher");
                        await Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault().LaunchAsync();
                    }
                    else
                        await Task.CompletedTask;
                }, () => this.IsFeedbackEnabled));
            }
        }

        #endregion
    }
}
