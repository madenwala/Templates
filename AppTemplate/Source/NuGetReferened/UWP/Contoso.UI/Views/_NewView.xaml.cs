using AppFramework.Core;
using Contoso.Core.ViewModels;
using System.Threading.Tasks;
using Windows.UI.Xaml.Navigation;

namespace Contoso.UI.Views
{
    public abstract class NewViewBase : ViewBase<NewViewModel>
    {
    }

    public sealed partial class NewView : NewViewBase
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