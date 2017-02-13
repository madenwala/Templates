using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    /// <summary>
    /// User control used in the ShellView page's SplitView control's pane.  Each instance represents a navigation button for the main navigation menu.
    /// </summary>
    public sealed class SplitViewButton : RadioButton
    {
        public SplitViewButton()
        {
            this.DefaultStyleKey = typeof(SplitViewButton);
        }

        // Using a DependencyProperty as the backing store for Symbol.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolProperty = DependencyProperty.Register(nameof(Symbol), typeof(Symbol), typeof(SplitViewButton), new PropertyMetadata(Symbol.Accept));
        public Symbol Symbol
        {
            get { return (Symbol)GetValue(SymbolProperty); }
            set { SetValue(SymbolProperty, value); }
        }

        public Visibility SymbolVisibility
        {
            get { return (Visibility)GetValue(SymbolVisibilityProperty); }
            set { SetValue(SymbolVisibilityProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolVisibility.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolVisibilityProperty =
            DependencyProperty.Register(nameof(SymbolVisibility), typeof(Visibility), typeof(SplitViewButton), new PropertyMetadata(Visibility.Visible));

        public bool SymbolAlwaysVisible
        {
            get { return (bool)GetValue(SymbolAlwaysVisibleProperty); }
            set { SetValue(SymbolAlwaysVisibleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SymbolAlwaysVisible.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SymbolAlwaysVisibleProperty =
            DependencyProperty.Register(nameof(SymbolAlwaysVisible), typeof(bool), typeof(SplitViewButton), new PropertyMetadata(false, new PropertyChangedCallback(OnIconChanged)));


        public object Icon
        {
            get { return (object)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Icon.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(object), typeof(SplitViewButton), new PropertyMetadata(null, new PropertyChangedCallback(OnIconChanged)));
                        
        private static void OnIconChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var ctrl = sender as SplitViewButton;
            if (ctrl != null)
                ctrl.SymbolVisibility = ctrl.Icon == null || ctrl.SymbolAlwaysVisible ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
