using Contoso.Core.Commands;
using Contoso.Core.Models;
using System;
using Windows.ApplicationModel.DataTransfer;

namespace Contoso.Core.Services
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public SharingManager SharingManager
        {
            get { return this.GetService<SharingManager>(); }
            protected set { this.SetService<SharingManager>(value); }
        }
    }

    public sealed partial class SharingManager : ServiceBase
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

        #region Constructors

        internal SharingManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Share a model of data with Windows.
        /// </summary>
        /// <param name="model"></param>
        public void Share(IModel model)
        {
            Platform.Current.Analytics.Event("Share");

            DataTransferManager.GetForCurrentView().DataRequested += (sender, e) =>
            {
                try
                {
                    this.SetShareContent(e.Request, model ?? Platform.Current.ViewModel);
                }
                catch (Exception ex)
                {
                    Platform.Current.Logger.LogError(ex, "Error in OnDataRequested");
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