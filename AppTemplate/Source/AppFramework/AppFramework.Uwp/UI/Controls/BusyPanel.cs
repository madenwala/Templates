using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Controls
{
    public sealed class BusyPanel : Control
    {
        #region Properties
        
        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }
        public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(nameof(IsActive), typeof(bool), typeof(BusyPanel), new PropertyMetadata(false));


        public string StatusText
        {
            get { return (string)GetValue(StatusTextProperty); }
            set { SetValue(StatusTextProperty, value); }
        }
        public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(nameof(StatusText), typeof(string), typeof(BusyPanel), new PropertyMetadata(null));


        public Style StatusTextStyle
        {
            get { return (Style)GetValue(StatusTextStyleProperty); }
            set { SetValue(StatusTextStyleProperty, value); }
        }
        public static readonly DependencyProperty StatusTextStyleProperty = DependencyProperty.Register(nameof(StatusTextStyle), typeof(Style), typeof(BusyPanel), new PropertyMetadata(null));

        public ICommand CustomButtonCommand
        {
            get { return (ICommand)GetValue(CustomButtonCommandProperty); }
            set { SetValue(CustomButtonCommandProperty, value); }
        }
        public static readonly DependencyProperty CustomButtonCommandProperty = DependencyProperty.Register(nameof(CustomButtonCommand), typeof(ICommand), typeof(BusyPanel), new PropertyMetadata(null));


        public string CustomButtonText
        {
            get { return (string)GetValue(CustomButtonTextProperty); }
            set { SetValue(CustomButtonTextProperty, value); }
        }
        public static readonly DependencyProperty CustomButtonTextProperty = DependencyProperty.Register(nameof(CustomButtonText), typeof(string), typeof(BusyPanel), new PropertyMetadata("[Cancel]"));

        public Visibility CustomButtonVisibility
        {
            get { return (Visibility)GetValue(CustomButtonVisibilityProperty); }
            set { SetValue(CustomButtonVisibilityProperty, value); }
        }
        public static readonly DependencyProperty CustomButtonVisibilityProperty = DependencyProperty.Register(nameof(CustomButtonVisibility), typeof(Visibility), typeof(BusyPanel), new PropertyMetadata(Visibility.Collapsed));

        #endregion

        public BusyPanel()
        {
            this.DefaultStyleKey = typeof(BusyPanel);
        }
    }
}
