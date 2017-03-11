using Contoso.Core;
using Contoso.Core.Services;
using Contoso.Core.ViewModels;
using System;

namespace Contoso.UI.Controls
{
    public sealed class Pivot : Windows.UI.Xaml.Controls.Pivot
    {
        public Pivot()
        {
            this.PivotItemLoading += Pivot_PivotItemLoading;
        }

        private async void Pivot_PivotItemLoading(Windows.UI.Xaml.Controls.Pivot sender, Windows.UI.Xaml.Controls.PivotItemEventArgs args)
        {
            var parentVM = this.DataContext as CollectionViewModelBase;
            if (parentVM != null)
            {
                if (args.Item.DataContext is ViewModelBase)
                {
                    var vm = args.Item.DataContext as ViewModelBase;
                    try
                    {
                        await parentVM.SetCurrentAsync(vm);
                    }
                    catch (Exception ex)
                    {
                        Platform.Current.Logger.LogError(ex, "Error while loading child ViewModel of type {1} in CollectionViewModel type {0}", parentVM.GetType().Name, vm.GetType().Name);
                    }
                }
            }
        }
    }
}
