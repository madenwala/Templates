using AppFramework.Core.Models;
using AppFramework.Core.Services;

namespace AppFramework.Core.Commands
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class ShareCommand : GenericCommand<IModel>
    {
        #region Constructors

        /// <summary>
        /// Creates a new command instance for sharing IModel objects to other apps.
        /// </summary>
        public ShareCommand()
            : base("ShareCommand", PlatformBase.Current.SharingManager.Share)
        {
        }

        #endregion
    }
}