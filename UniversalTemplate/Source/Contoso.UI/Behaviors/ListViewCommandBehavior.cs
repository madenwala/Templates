using Microsoft.Toolkit.Uwp.UI.Controls;
using System.Windows.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Behaviors
{
    /// <summary>
    /// Creates an attached property for all ListViewBase controls allowing binding  a command object to it's ItemClick event.
    /// </summary>
    public static class ListViewCommandBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached("Command", typeof(ICommand),
            typeof(ListViewCommandBehavior), new PropertyMetadata(null, OnCommandPropertyChanged));

        public static void SetCommand(DependencyObject d, ICommand value)
        {
            d.SetValue(CommandProperty, value);
        }

        public static ICommand GetCommand(DependencyObject d)
        {
            return (ICommand)d.GetValue(CommandProperty);
        }

        private static void OnCommandPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (sender is ListViewBase)
            {
                var control = (ListViewBase)sender;

                // Remove the old click handler if there was a previous command
                if (args.OldValue != null)
                    control.ItemClick -= OnItemClick;

                if (args.NewValue != null)
                    control.ItemClick += OnItemClick;
            }
            else if (sender is AdaptiveGridView)
            {
                var control = (AdaptiveGridView)sender;

                // Remove the old click handler if there was a previous command
                if (args.OldValue != null)
                    control.ItemClick -= OnItemClick;

                if (args.NewValue != null)
                    control.ItemClick += OnItemClick;
            }
        }

        private static void OnItemClick(object sender, ItemClickEventArgs e)
        {
            if (sender is Control)
            {
                var control = (Control)sender;
                var command = GetCommand(control);

                if (command != null && command.CanExecute(e.ClickedItem))
                    command.Execute(e.ClickedItem);
            }
        }
    }
}