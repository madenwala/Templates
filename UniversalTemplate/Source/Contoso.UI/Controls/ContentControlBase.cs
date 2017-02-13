﻿using Contoso.Core.ViewModels;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Contoso.UI.Controls
{
    /// <summary>
    /// Base class for custom controls.  Provides plumbing for view model support and x:Bind just like the ViewBase.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class ContentControlBase<TViewModel> : ContentControl where TViewModel : ViewModelBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private TViewModel _ViewModel;
        public TViewModel ViewModel
        {
            get { return _ViewModel; }
            protected set { this.SetProperty(ref _ViewModel, value); }
        }

        public ContentControlBase()
        {
            this.DataContextChanged += ViewControlBase_DataContextChanged;
        }

        private void ViewControlBase_DataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
            this.ViewModel = (TViewModel)this.DataContext;
        }

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (System.Collections.Generic.EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }
            else
            {
                storage = value;
                this.NotifyPropertyChanged(propertyName);
                return true;
            }
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
