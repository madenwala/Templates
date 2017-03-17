using AppFramework.Core.ViewModels;

namespace Contoso.UI.Views
{
    public abstract class ViewBase<TViewModel> : AppFramework.UI.Views.ViewBase<TViewModel> where TViewModel : IViewModel
    {
    }
}
