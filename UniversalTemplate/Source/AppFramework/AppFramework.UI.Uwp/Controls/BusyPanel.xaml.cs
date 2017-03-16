using AppFramework.Core.ViewModels;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    public sealed partial class BusyPanel : UserControl
    {
        #region Properties
        
        public ViewModelBase ViewModel
        {
            get { return (ViewModelBase)GetValue(ViewModelProperty); }
            set { SetValue(ViewModelProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ViewModel.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(ViewModelBase), typeof(BusyPanel), new PropertyMetadata(null));
        
        #endregion

        #region Constructors

        public BusyPanel()
        {
            this.InitializeComponent();
        }

        #endregion
    }
}
