using AppFramework.Core.Models;
using AppFramework.Core.Services;

namespace AppFramework.Core.Commands
{
    /// <summary>
    /// Command for launching an external maps app passing an ILocationModel command parameter instance passed to this command.
    /// </summary>
    public sealed class MapExternalCommand : GenericCommand<ILocationModel>
    {
        #region Constructors

        public MapExternalCommand(MapExternalOptions option = MapExternalOptions.Normal)
            : base("MapExternalCommand-" + option, (loc) => PlatformCore.Core.NavigationBase.MapExternal(loc, loc?.LocationDisplayName, option))
        {
        }

        #endregion
    }
}