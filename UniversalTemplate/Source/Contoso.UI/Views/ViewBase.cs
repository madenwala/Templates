using AppFramework.Core.ViewModels;

namespace Contoso.UI.Views
{
    public abstract class ViewBase<TViewModel> : AppFramework.Uwp.UI.Views.ViewBase<TViewModel> where TViewModel : ViewModelBase
    {
    }
}
