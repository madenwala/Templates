using Contoso.Core.ViewModels;

namespace Contoso.UI.Controls
{
    /// <summary>
    /// Base class for custom controls.  Provides plumbing for view model support and x:Bind just like the ViewBase.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class ViewControlBase<TViewModel> : UserControlBase where TViewModel : ViewModelBase
    {
        #region Properties

        private TViewModel _ViewModel;
        public TViewModel ViewModel
        {
            get { return _ViewModel; }
            protected set { this.SetProperty(ref _ViewModel, value); }
        }

        #endregion

        #region Constructors

        public ViewControlBase()
        {
            this.DataContextChanged += (sender, args) =>
            {
                this.ViewModel = (TViewModel)this.DataContext;
            };
        }

        #endregion
    }
}