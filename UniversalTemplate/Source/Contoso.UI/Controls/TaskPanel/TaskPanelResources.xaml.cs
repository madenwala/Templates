﻿using Contoso.Core.Models;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    public partial class TaskPanelResources
    {
        public TaskPanelResources()
        {
            this.InitializeComponent();
        }

        private void btnTryAgain_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            var btn = sender as Button;
            if(btn?.DataContext is INotifyTaskCompletion)
            {
                var task = btn.DataContext as INotifyTaskCompletion;
                task.Refresh();
            }
        }
    }
}