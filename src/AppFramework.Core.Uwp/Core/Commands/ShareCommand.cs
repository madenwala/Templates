using AppFramework.Core.Models;

namespace AppFramework.Core.Commands
{
    /// <summary>
    /// Command for sharing content to the OS.
    /// </summary>
    public sealed class ShareCommand : GenericCommand<IModel>
    {
        #region Constructors

        /// <summary>
        /// Creates a new command instance for sharing IModel objects to other apps.
        /// </summary>
        public ShareCommand()
            : base("ShareCommand", BasePlatform.CurrentCore.SharingManager.Share)
        {
        }

        #endregion
    }
}