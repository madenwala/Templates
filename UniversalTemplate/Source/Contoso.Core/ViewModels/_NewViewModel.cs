using AppFramework.Core.Strings;
using AppFramework.Core.ViewModels;
using Contoso.Core.Data;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public partial class NewViewModel : ViewModelBase
    {
        #region Properties

        /// <summary>
        /// Gets the title to be displayed on the view consuming this ViewModel.
        /// </summary>
        public override string Title
        {
            get { return Resources.ApplicationName; }
        }

        #endregion

        #region Constructors

        public NewViewModel()
        {
            if (DesignMode.DesignModeEnabled)
                return;

            this.RequiresAuthorization = true;
        }

        #endregion

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (!this.IsInitialized)
            {
            }

            await base.OnLoadStateAsync(e);
        }
        
        protected override async Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (forceRefresh)
            {
                this.ShowBusyStatus(Resources.TextLoading, true);

                using (var client = new ClientApi())
                {
                    // DO WORK HERE
                    await Task.Delay(1500);

                    ct.ThrowIfCancellationRequested();
                }
            }
        }

        protected override Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            return base.OnSaveStateAsync(e);
        }

        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    public partial class NewViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        /// </summary>
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public NewViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class NewViewModel : Contoso.Core.ViewModels.NewViewModel
    {
        public NewViewModel()
        {
        }
    }
}