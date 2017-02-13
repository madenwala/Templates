using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.System;

namespace Contoso.Core.Services
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the app info service of the platform currently executing.
        /// </summary>
        public AppInfoProvider AppInfo
        {
            get { return this.GetService<AppInfoProvider>(); }
            protected set { this.SetService<AppInfoProvider>(value); }
        }
    }

    /// <summary>
    /// Base class providing access to the application currently executing specific to the platform this app is executing on.
    /// </summary>
    public sealed partial class AppInfoProvider : ServiceBase
    {
        #region Variables

        public const string PROTOCOL_PREFIX = "contoso://";

        private LicenseInformation _licenseInfo = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets version number of the application currently executing.
        /// </summary>
        public Version VersionNumber
        {
            get
            {
                return Windows.ApplicationModel.Package.Current.Id.Version.ToVersion();
            }
        }
        
        /// <summary>
        /// Creates a deep link to your application with the specified arguments.
        /// </summary>
        /// <param name="arguments">Dictionary of different parameters to create a query string for the arguments.</param>
        /// <returns>String representing the deep link.</returns>
        public string GetDeepLink(Dictionary<string, string> arguments = null)
        {
            return this.GetDeepLink(GeneralFunctions.CreateQuerystring(arguments));
        }

        /// <summary>
        /// Creates a deep link to your application with the specified arguments.
        /// </summary>
        /// <param name="arguments">String representation of the arguments.</param>
        /// <returns>String representing the deep link.</returns>
        public string GetDeepLink(string arguments)
        {
            return PROTOCOL_PREFIX + arguments;
        }

        /// <summary>
        /// Gets the deep link URL to where this application can be downloaded from.
        /// </summary>
        public string StoreURL
        {
            get { return string.Format("ms-windows-store:PDP?PFN={0}", Windows.ApplicationModel.Package.Current.Id.FamilyName); }
        }

        /// <summary>
        /// Gets whether or not this application is running in trial mode.
        /// </summary>
        public bool IsTrial
        {
            get { return _licenseInfo.IsTrial; }
        }

        /// <summary>
        /// Gets whether or not this application trial is expired.
        /// </summary>
        public bool IsTrialExpired
        {
            get { return (_licenseInfo.IsActive == false); }
        }

        /// <summary>
        /// Gets the DateTime of when this application's trial will expire.
        /// </summary>
        public DateTime TrialExpirationDate
        {
            get { return _licenseInfo.ExpirationDate.DateTime; }
        }

        public string UserID { get; private set; }

        #endregion Properties

        #region Constructors

        public AppInfoProvider()
        {
            try
            {
                _licenseInfo = CurrentApp.LicenseInformation;
            }
            catch
            {
                _licenseInfo = CurrentAppSimulator.LicenseInformation;
            }
//#if DEBUG
//            _licenseInfo = CurrentAppSimulator.LicenseInformation;
//#else
//            try
//            {
//                _licenseInfo = CurrentApp.LicenseInformation;
//            }
//            catch
//            {
//                _licenseInfo = CurrentAppSimulator.LicenseInformation;
//            }
//#endif
        }

        #endregion

        #region Methods

        protected override async Task OnInitializeAsync()
        {
            if (Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.SystemIdentification"))
            {
                //var users = await User.FindAllAsync(UserType.LocalUser, UserAuthenticationStatus.LocallyAuthenticated);
                //var user = users.FirstOrDefault();
                //if (user != null)
                //    this.UserID = user.NonRoamableId;

                var info = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                if (info?.Id != null)
                    this.UserID = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(info.Id);
            }

            await this.InitializeAddOnsAsync();

            await base.OnInitializeAsync();
        }

        private async Task<PurchaseResults> PurchaseAddOnAsync(string featureName)
        {
            try
            {
                return await CurrentApp.RequestProductPurchaseAsync(featureName);
            }
            catch
            {
                return await CurrentAppSimulator.RequestProductPurchaseAsync(featureName);
            }
        }

        private bool HasLicense(string featureName)
        {
            try
            {
                return _licenseInfo.ProductLicenses[featureName].IsActive;
            }
            catch
            {
                return false;
            }
        }

        #endregion
    }
}
