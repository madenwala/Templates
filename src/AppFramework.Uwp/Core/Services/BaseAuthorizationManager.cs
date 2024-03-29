﻿using AppFramework.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppFramework.Core.Services
{
    public abstract class BaseAuthorizationManager : BaseService, IServiceSignout
    {
        #region Events

        /// <summary>
        /// Event used to notify subscribed objects of when a user logs in or out.
        /// </summary>
        public event EventHandler<bool> UserAuthenticatedStatusChanged;

        #endregion

        #region Properties

        internal protected bool IsReauthenticationNeeded { internal get; set; }

        #endregion

        #region Constructors

        internal BaseAuthorizationManager()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        /// Indicates whether or not a user is authenticated into this app.
        /// </summary>
        /// <returns>True if the user is authenticated else false.</returns>
        public abstract bool IsAuthenticated();

        /// <summary>
        /// Notify any subscribers that the user authentication status has changed.
        /// </summary>
        protected void NotifyUserAuthenticated()
        {
            this.UserAuthenticatedStatusChanged?.Invoke(null, this.IsAuthenticated());
        }

        /// <summary>
        /// Sets the current user of the app.
        /// </summary>
        /// <param name="user"></param>
        public abstract Task<bool> SetUserAsync(IAuthenticatedUserProfile user);

        /// <summary>
        /// Signs the user out of the application and removes and credential data from storage / credential locker.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public abstract Task SignoutAsync();

        /// <summary>
        /// Gets a refresh token on launches of this app if the user was previously authenticated.
        /// </summary>
        /// <param name="ct">Cancellation token.</param>
        /// <returns>User object representing the logged in user.</returns>
        protected internal abstract Task<IAuthenticatedUserProfile> GetRefreshAccessToken(CancellationToken ct);

        #endregion
    }

    public abstract class BaseAuthorizationManager<U> : BaseAuthorizationManager where U : IAuthenticatedUserProfile
    {
        #region Properties

        private U _currentUser;
        /// <summary>
        /// Gets or sets the current user object representing a logged in user.
        /// </summary>
        public U CurrentUser
        {
            get { return _currentUser; }
            protected set { this.SetProperty(ref _currentUser, value); }
        }

        #endregion
    }
}