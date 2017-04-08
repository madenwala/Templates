using AppFramework.Core.Models;
using AppFramework.Core.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppFramework.Core
{
    public partial class PlatformCore
    {
        /// <summary>
        /// Gets access to the cryptography provider of the platform currently executing.
        /// </summary>
        public AuthorizationManagerBase AuthManager
        {
            get { return GetService<AuthorizationManagerBase>(); }
            protected set { SetService(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    public abstract class AuthorizationManagerBase : ServiceBase, IServiceSignout
    {
        #region Events

        /// <summary>
        /// Event used to notify subscribed objects of when a user logs in or out.
        /// </summary>
        public event EventHandler<bool> UserAuthenticatedStatusChanged;

        #endregion

        #region Properties

        private IUserInformation _currentUser;
        /// <summary>
        /// Gets or sets the current user object representing a logged in user.
        /// </summary>
        public virtual IUserInformation CurrentUser
        {
            get { return _currentUser; }
            protected set { this.SetProperty(ref _currentUser, value); }
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
        public abstract Task<bool> SetUserAsync(IUserInformation user);

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
        protected internal abstract Task<IUserInformation> GetRefreshAccessToken(CancellationToken ct);

        #endregion
    }
}