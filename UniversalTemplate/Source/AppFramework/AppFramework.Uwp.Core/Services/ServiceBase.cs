using AppFramework.Core.Models;
using System;
using System.Threading.Tasks;

namespace AppFramework.Core
{
}

namespace AppFramework.Core.Services
{
    public interface IServiceSignout
    {
        Task SignoutAsync();
    }

    public abstract class ServiceBase : ModelBase, IDisposable
    {
        #region Properties

        /// <summary>
        /// Indicates whether or not this adapter has been initialized by the PlatformAdaptereCore Initialization method.
        /// </summary>
        internal bool Initialized { get; private set; }

        #endregion Properties

        #region Methods

        /// <summary>
        /// Initialization logic which is called on launch of this application.
        /// </summary>
        internal async Task InitializeAsync()
        {
            await this.OnInitializeAsync();
            this.Initialized = true;
        }

        /// <summary>
        /// Custom initialization logic for this service.
        /// </summary>
        protected virtual Task OnInitializeAsync()
        {
            return Task.CompletedTask;
        }

        public virtual void Dispose()
        {
        }

        #endregion
    }
}