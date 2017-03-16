using AppFramework.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace AppFramework.UI.Uwp.Controls
{
    public sealed class TaskPanel : ContentControl
    {
        #region Properties

        public INotifyTaskCompletion Task
        {
            get { return (INotifyTaskCompletion)GetValue(ContainerTemplateProperty); }
            set { SetValue(ContainerTemplateProperty, value); }
        }

        // Using a DependencyProperty as the backing store for BodyContainer.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ContainerTemplateProperty =
            DependencyProperty.Register(nameof(Task), typeof(INotifyTaskCompletion), typeof(TaskPanel), new PropertyMetadata(null, new PropertyChangedCallback(OnTaskChanged)));

        private static void OnTaskChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var control = sender as TaskPanel;
            if(control != null)
                control.DataContext = args.NewValue;
        }

        #endregion
    }
}