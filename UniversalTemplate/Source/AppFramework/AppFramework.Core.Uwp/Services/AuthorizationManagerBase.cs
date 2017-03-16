using AppFramework.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AppFramework.Core.Services
{
    public abstract class AuthorizationManagerBase : ServiceBase, IServiceSignout
    {
        #region Events

        public event EventHandler<bool> UserAuthenticatedStatusChanged;

        #endregion

        #region Properties

        private IUserInformation _currentUser;
        public IUserInformation CurrentUser
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
        
        protected internal virtual Task<IUserInformation> GetRefreshAccessToken(CancellationToken ct)
        {
            return Task.FromResult<IUserInformation>(null);
        }

        #endregion
    }
}