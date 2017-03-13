using AppFramework.Core.ViewModels;
using AppFramework.Uwp.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class DetailsViewBase : ViewBase<DetailsViewModel>
    {
    }

    public sealed partial class DetailsView : DetailsViewBase
    {
        public DetailsView()
        {
            this.InitializeComponent();
            //this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new DetailsViewModel());

            return base.OnLoadStateAsync(e);
        }
    }
}
