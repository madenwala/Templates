using AppFramework.Core;
using AppFramework.Core.Models;
using Contoso.Core.Data;
using Contoso.Core.Models;
using Contoso.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;

namespace Contoso.Core
{
    //public partial class Platform
    //{
    //    /// <summary>
    //    /// Gets access to the cryptography provider of the platform currently executing.
    //    /// </summary>
    //    public new AuthorizationManager AuthManager
    //    {
    //        get { return GetService<AuthorizationManager>(); }
    //        set { SetService(value); }
    //    }
    //}
}

namespace Contoso.Core.Services
{
    public sealed class AuthorizationManager : AppFramework.Core.Services.AuthorizationManagerBase
    {
        #region Variables

        private const string CREDENTIAL_USER_KEYNAME = "ContosoUser";
        private const string CREDENTIAL_ACCESSTOKEN_KEYNAME = "ContosoAccessToken";
        private const string CREDENTIAL_REFRESHTOKEN_KEYNAME = "ContosoAccessToken";

        #endregion

        #region Properties
        
        public new UserResponse CurrentUser
        {
            get { return base.CurrentUser as UserResponse; }
            private set { base.CurrentUser = value; }
        }

        private string _AccessToken;
        public string AccessToken
        {
            get { return _AccessToken; }
            private set { this.SetProperty(ref _AccessToken, value); }
        }

        private string _RefreshToken;
        public string RefreshToken
        {
            get { return _RefreshToken; }
            private set { this.SetProperty(ref _RefreshToken, value); }
        }

        #endregion

        #region Constructors

        internal AuthorizationManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates whether or not a user is authenticated into this app.
        /// </summary>
        /// <returns>True if the user is authenticated else false.</returns>
        public override bool IsAuthenticated()
        {
            var user = this.CurrentUser as UserResponse;
            return !string.IsNullOrEmpty(user?.AccessToken);
        }

        /// <summary>
        /// Initialization logic which is called on launch of this application.
        /// </summary>
        protected override async Task OnInitializeAsync()
        {
            // Retrieve the access token from the credential locker
            string access_token_value = null;
            if (Platform.Current.Storage.LoadCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_ACCESSTOKEN_KEYNAME, ref access_token_value))
                this.AccessToken = access_token_value;

            // Retrieve the refresh token from the credential locker
            string refresh_token_value = null;
            if (Platform.Current.Storage.LoadCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_REFRESHTOKEN_KEYNAME, ref refresh_token_value))
                this.RefreshToken = refresh_token_value;

            // Retrieve the user profile data from settings
            this.CurrentUser = await Platform.Current.Storage.LoadFileAsync<UserResponse>(CREDENTIAL_USER_KEYNAME, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
            if (this.CurrentUser != null)
            {
                this.CurrentUser.AccessToken = this.AccessToken;
                this.CurrentUser.RefreshToken = this.RefreshToken;
                Platform.Current.Analytics.SetUser(this.CurrentUser);
            }

            // Notify any subscribers that authentication status has changed
            this.NotifyUserAuthenticated();

            await base.OnInitializeAsync();
        }

        /// <summary>
        /// Sets the current user of the app.
        /// </summary>
        /// <param name="userInformation"></param>
        public override async Task<bool> SetUserAsync(IUserInformation userInformation)
        {
            var user = userInformation as UserResponse;

            if (string.IsNullOrEmpty(user?.AccessToken))
            {
                await this.SignoutAsync();
                return false;
            }
            else
            {
                // Log user
                PlatformBase.Current.Analytics.SetUser(user);

                // Store user data
                await PlatformBase.Current.Storage.SaveFileAsync(CREDENTIAL_USER_KEYNAME, user, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
                PlatformBase.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_ACCESSTOKEN_KEYNAME, user.AccessToken);
                PlatformBase.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_REFRESHTOKEN_KEYNAME, user.RefreshToken);

                // Set properties
                this.CurrentUser = user;
                this.AccessToken = user.AccessToken;
                this.RefreshToken = user.RefreshToken;

                // Notify any subscribers that authentication status has changed
                this.NotifyUserAuthenticated();
                return this.IsAuthenticated();
            }
        }

        /// <summary>
        /// Signs the user out of the application and removes and credential data from storage / credential locker.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public override async Task SignoutAsync()
        {
            await PlatformBase.Current.Storage.SaveFileAsync(CREDENTIAL_USER_KEYNAME, null, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
            PlatformBase.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, null, null);
            this.CurrentUser = null;
            this.AccessToken = null;
            this.RefreshToken = null;
            this.NotifyUserAuthenticated();
        }

        protected override async Task<IUserInformation> GetRefreshAccessToken(CancellationToken ct)
        {
            try
            {
                using (ClientApi api = new ClientApi(true))
                {
                    return await api.AuthenticateAsync(this.CurrentUser.AccessToken, ct);
                }
            }
            catch (Exception ex)
            {
                Platform.Current.Logger.LogError(ex, "Could not retrieve refresh token.");
                return null;
            }
        }

        #endregion
    }
}