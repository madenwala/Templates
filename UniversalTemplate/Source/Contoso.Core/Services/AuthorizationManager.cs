﻿using Contoso.Core.Models;
using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace Contoso.Core.Services
{
    public partial class PlatformBase
    {
        /// <summary>
        /// Gets access to the cryptography provider of the platform currently executing.
        /// </summary>
        public AuthorizationManager AuthManager
        {
            get { return this.GetService<AuthorizationManager>(); }
            protected set { this.SetService<AuthorizationManager>(value); }
        }
    }

    public sealed class AuthorizationManager : ServiceBase, IServiceSignout
    {
        #region Variables
        
        private const string CREDENTIAL_USER_KEYNAME = "ContosoUser";
        private const string CREDENTIAL_ACCESSTOKEN_KEYNAME = "ContosoAccessToken";
        private const string CREDENTIAL_REFRESHTOKEN_KEYNAME = "ContosoAccessToken";

        #endregion

        #region Events

        public event EventHandler<bool> UserAuthenticatedStatusChanged;

        #endregion

        #region Properties

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

        private UserResponse _User;
        public UserResponse User
        {
            get { return _User; }
            private set { this.SetProperty(ref _User, value); }
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
        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(this.User?.AccessToken);
        }

        /// <summary>
        /// Notify any subscribers that the user authentication status has changed.
        /// </summary>
        private void NotifyUserAuthenticated()
        {
            this.UserAuthenticatedStatusChanged?.Invoke(null, this.IsAuthenticated());
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
            this.User = await Platform.Current.Storage.LoadFileAsync<UserResponse>(CREDENTIAL_USER_KEYNAME, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
            if (this.User != null)
            {
                this.User.AccessToken = this.AccessToken;
                this.User.RefreshToken = this.RefreshToken;
            }
            
            // Notify any subscribers that authentication status has changed
            this.NotifyUserAuthenticated();

            await base.OnInitializeAsync();
        }

        /// <summary>
        /// Sets the current user of the app.
        /// </summary>
        /// <param name="response"></param>
        internal async Task<bool> SetUserAsync(UserResponse response)
        {
            if (string.IsNullOrEmpty(response?.AccessToken))
            {
                await this.SignoutAsync();
                return false;
            }
            else
            {
                // Log user
                Platform.Current.Analytics.SetUsername(response.Email);

                // Store user data
                await Platform.Current.Storage.SaveFileAsync(CREDENTIAL_USER_KEYNAME, response, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
                Platform.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_ACCESSTOKEN_KEYNAME, response.AccessToken);
                Platform.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, CREDENTIAL_REFRESHTOKEN_KEYNAME, response.RefreshToken);

                // Set properties
                this.User = response;
                this.AccessToken = response.AccessToken;
                this.RefreshToken = response.RefreshToken;

                // Notify any subscribers that authentication status has changed
                this.NotifyUserAuthenticated();
                return this.IsAuthenticated();
            }
        }

        /// <summary>
        /// Signs the user out of the application and removes and credential data from storage / credential locker.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public async Task SignoutAsync()
        {
            await Platform.Current.Storage.SaveFileAsync(CREDENTIAL_USER_KEYNAME, null, ApplicationData.Current.RoamingFolder, SerializerTypes.Json);
            Platform.Current.Storage.SaveCredential(CREDENTIAL_USER_KEYNAME, null, null);
            this.User = null;
            this.AccessToken = null;
            this.RefreshToken = null;
            this.NotifyUserAuthenticated();
        }

        #endregion
    }
}