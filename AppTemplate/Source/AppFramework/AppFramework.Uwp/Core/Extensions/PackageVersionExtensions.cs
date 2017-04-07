using System;
using Windows.ApplicationModel;

namespace AppFramework.Core.Extensions
{
    internal static class PackageVersionExtensions
    {
        /// <summary>
        /// Converts this PackageVersion instance into a Version instance.
        /// </summary>
        /// <returns>Version instance with the version numbers.</returns>
        public static Version ToVersion(this PackageVersion pv)
        {
            return new Version(pv.Major, pv.Minor, pv.Build, pv.Revision);
        }
    }
}