using Contoso.Core.Models;
using Contoso.Core.Services;

namespace Contoso.Core.Commands
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