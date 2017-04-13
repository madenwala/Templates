using AppFramework.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class WebBrowserViewBase : ViewBase<WebViewModel>
    {
    }

    public sealed partial class WebBrowserView : WebBrowserViewBase
    {
        public WebBrowserView()
        {
            InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Disabled;
        }

        protected override Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
            {
                WebViewModel vm = null;
                if (e.NavigationEventArgs.Parameter is WebViewModel)
                    vm = e.NavigationEventArgs.Parameter as WebViewModel;
                else
                    vm = new WebViewModel();
                this.SetViewModel(vm);
            }

            return base.OnLoadStateAsync(e);
        }
    }
}