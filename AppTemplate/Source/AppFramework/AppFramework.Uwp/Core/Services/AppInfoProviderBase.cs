using AppFramework.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.Store;
using Windows.Storage;

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Base class providing access to the application currently executing specific to the platform this app is executing on.
    /// </summary>
    public abstract class AppInfoProviderBase : ServiceBase
    {
        #region Variables

        private LicenseInformation _licenseInfo = null;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the AppID of the application.
        /// </summary>
        public string AppID { get { return Windows.ApplicationModel.Package.Current.Id.ProductId; } }

        public virtual string AppName { get { return Windows.ApplicationModel.Package.Current.Id.Name; } }

        public abstract string AppDescription { get; }

        public abstract string AppSupportEmailAddress { get; }

        public abstract string AppSupportTwitterAddress { get; }

        public abstract string ProtocolPrefix { get; }

        /// <summary>
        /// Gets version number of the application currently executing.
        /// </summary>
        public Version VersionNumber { get { return Windows.ApplicationModel.Package.Current.Id.Version.ToVersion(); } }

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

        public AppInfoProviderBase()
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
            if (string.IsNullOrEmpty(this.UserID) && Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.SystemIdentification"))
            {
                //var users = await User.FindAllAsync(UserType.LocalUser, UserAuthenticationStatus.LocallyAuthenticated);
                //var user = users.FirstOrDefault();
                //if (user != null)
                //    this.UserID = user.NonRoamableId;

                var info = Windows.System.Profile.SystemIdentification.GetSystemIdForPublisher();
                if (info?.Id != null)
                    this.UserID = Windows.Security.Cryptography.CryptographicBuffer.EncodeToBase64String(info.Id);
            }

            await base.OnInitializeAsync();
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
            return ProtocolPrefix + arguments;
        }

        /// <summary>
        /// Gets the version number of the AppFramework package being used in this app.
        /// </summary>
        /// <returns>Version number of AppFramework package</returns>
        public async Task<string> GetAppFrameworkVersionAsync()
        {
            try
            {
                var assembly = typeof(AppInfoProviderBase).GetTypeInfo().Assembly.GetName().Name;
                string filename = $"ms-appx:///{assembly}/VERSION.txt";
                Uri appUri = new Uri(filename);//File name should be prefixed with 'ms-appx:///Assets/* 
                StorageFile file = StorageFile.GetFileFromApplicationUriAsync(appUri).AsTask().ConfigureAwait(false).GetAwaiter().GetResult();
                return await file.ReadAllTextAsync();
            }
            catch (Exception)
            {
                return Strings.Resources.TextNotApplicable;
            }
        }

        #region Store AddOns

        protected async Task<bool> PurchaseFeatureAsync(string featureName)
        {
            var result = await this.RequestProductPurchaseAsync(featureName);

            await this.OnInitializeAsync();

            PlatformBase.CurrentCore.Analytics.Event("STORE_PURCHASE_STATUS-" + featureName, result.Status.ToString());
            switch (result.Status)
            {
                case ProductPurchaseStatus.AlreadyPurchased:
                case ProductPurchaseStatus.Succeeded:
                    return true;

                case ProductPurchaseStatus.NotPurchased:
                    return false;

                case ProductPurchaseStatus.NotFulfilled:
                default:
                    await PlatformBase.CurrentCore.ViewModelCore.ShowMessageBoxAsync(CancellationToken.None, string.Format("Could not purchase Pro-Feature Pack add-on right now. Store return back an error. Please try again later."), "Failed to purchase add-on");
                    return false;
            }
        }

        private async Task<PurchaseResults> RequestProductPurchaseAsync(string featureName)
        {
            try
            {
                PlatformBase.CurrentCore.Analytics.Event("PurchaseAddOn", featureName);
                return await CurrentApp.RequestProductPurchaseAsync(featureName);
            }
            catch
            {
                return await CurrentAppSimulator.RequestProductPurchaseAsync(featureName);
            }
            finally
            {
                this.HasLicense(featureName);
            }
        }

        protected bool HasLicense(string featureName)
        {
            try
            {
                var hasLicense = _licenseInfo.ProductLicenses[featureName].IsActive;

                if (hasLicense)
                    this.FeaturePurchased(featureName);

                return hasLicense || this.FeaturedPreviouslyPurchased(featureName);
            }
            catch
            {
                return false;
            }
        }

        private void FeaturePurchased(string featureName)
        {
            PlatformBase.CurrentCore.Storage.SaveSetting("InAppPurchase-" + featureName, DateTime.UtcNow, ApplicationData.Current.RoamingSettings);
        }

        private bool FeaturedPreviouslyPurchased(string featureName)
        {
            try
            {
                var key = "InAppPurchase-" + featureName;
                if (PlatformBase.CurrentCore.Storage.ContainsSetting(key, ApplicationData.Current.RoamingSettings))
                {
                    var date = PlatformBase.CurrentCore.Storage.LoadSetting<DateTime>(key, ApplicationData.Current.RoamingSettings);
                    if (date.AddDays(7) > DateTime.UtcNow)
                        return true;
                    else
                        return false;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                PlatformBase.CurrentCore.Analytics.Error(ex, $"Failed to check if '{featureName}' feature was previously purchased.");
                return false;
            }
        }

        #endregion Store AddOns

        #endregion
    }
}