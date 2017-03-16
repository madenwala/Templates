using AppFramework.Core.ViewModels;

namespace Contoso.UI.Views
{
    public abstract class ViewBase<TViewModel> : AppFramework.UI.Uwp.Views.ViewBase<TViewModel> where TViewModel : ViewModelBase
    {
    }
}
