using AppFramework.Core;
using AppFramework.Core.ViewModels;
using System;
using Windows.UI.Xaml.Controls;

namespace AppFramework.Uwp.UI.Controls
{
    public sealed class Pivot : Windows.UI.Xaml.Controls.Pivot
    {
        public Pivot()
        {
            this.PivotItemLoading += Pivot_PivotItemLoading;
        }

        private async void Pivot_PivotItemLoading(Windows.UI.Xaml.Controls.Pivot sender, PivotItemEventArgs args)
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
                        PlatformBase.Current.Logger.LogError(ex, "Error while loading child ViewModel of type {1} in CollectionViewModel type {0}", parentVM.GetType().Name, vm.GetType().Name);
                    }
                }
            }
        }
    }
}
