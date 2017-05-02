namespace Contoso.UI.Views
{
    public abstract class ViewBase<TViewModel> : AppFramework.UI.Views.ViewBase<TViewModel> where TViewModel : AppFramework.Core.ViewModels.ViewModelBase
    {
    }
}