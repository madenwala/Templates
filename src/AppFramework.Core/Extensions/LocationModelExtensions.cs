using AppFramework.Core.Models;

namespace AppFramework.Core.Extensions
{
    public static class LocationModelExtensions
    {
        /// <summary>
        /// Gets a friendly display for the distance away.
        /// </summary>
        public static string GetDistanceAwayDisplay(this ILocationModel model)
        {
            return "TODO n/a";
            // TODO return string.Format(Strings.Location.TextDistanceAwayDisplay, model.DistanceAway.ToString("N"), BaseLocationModel.IsMetric ? Strings.Location.TextKilomentersShort : Strings.Location.TextMilesShort);
        }
    }
}