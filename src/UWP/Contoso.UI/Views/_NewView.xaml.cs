using AppFramework.Core;
using AppFramework.UI.Views;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class BaseNewView : BaseView<NewViewModel>
    {
    }

    public sealed partial class NewView : BaseNewView
    {
        public NewView()
        {
            this.InitializeComponent();
        }

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New || this.ViewModel == null)
                this.SetViewModel(new NewViewModel());

            await base.OnLoadStateAsync(e);
        }
    }
}