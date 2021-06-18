using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseSearchView : BaseView<SearchViewModel>
    {
    }

    public sealed partial class SearchView : BaseSearchView
    {
        public SearchView()
        {
            this.InitializeComponent();
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new SearchViewModel());

            return base.OnLoadStateAsync(e);
        }

        public override void ScrollToTop()
        {
            list.ScrollToTop();
            base.ScrollToTop();
        }
    }
}