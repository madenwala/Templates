using AppFramework.Core.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;
using System.Threading;
using Windows.UI.Xaml.Input;
using AppFramework.Core.Services;

namespace AppFramework.Core.ViewModels
{
    /// <summary>
    /// ViewModelBase for views that need to display multiple separate sub-views which might have their own ViewModel instances. This 
    /// CollectionViewModelBase can contain multiple ViewModels and set a current view model so that the frame can show appropriate 
    /// status data specific to the current view model.
    /// </summary>
    public abstract class CollectionViewModelBase : ViewModelBase
    {
        #region Variables
        
        private LoadStateEventArgs _loadState;
        private ModelList<ViewModelBase> _viewModelsLoaded = new ModelList<ViewModelBase>();

        #endregion

        #region Properties
        
        private ViewModelBase _CurrentViewModel;
        /// <summary>
        /// Gets access to the current selected view model.
        /// </summary>
        public ViewModelBase CurrentViewModel
        {
            get { return _CurrentViewModel ?? this.ViewModels.FirstOrDefault(); }
            private set { this.SetProperty(ref _CurrentViewModel, value); }
        }

        private ModelList<ViewModelBase> _ViewModels = new ModelList<ViewModelBase>();
        /// <summary>
        /// Gets access to the collection of sub-viewmodels.
        /// </summary>
        protected ModelList<ViewModelBase> ViewModels
        {
            get { return _ViewModels; }
            private set { this.SetProperty(ref _ViewModels, value); }
        }

        #endregion

        #region Constructors

        public CollectionViewModelBase()
        {
            if (DesignMode.DesignModeEnabled)
                return;
        }

        #endregion

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            _loadState = e;
            _viewModelsLoaded.Clear();
            await base.OnLoadStateAsync(e);

            // Subsequent navigations to a page that uses a CollectionViewModelBase might already have an initialized page and thus not fire the appropriate 
            // event on the already loaded first view. This check is to manually pass the load state event to the current in focuse ViewModel.
            if (e.NavigationEventArgs.NavigationMode == NavigationMode.New && e.IsViewInitialized)
                await this.SetCurrentAsync(this.CurrentViewModel);
        }

        protected override async Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            try
            {
                // Call load on each sub-ViewModel in this collection when displaying this page
                foreach (var vm in this.ViewModels)
                    if (vm != null && vm.IsInitialized)
                        await vm.SaveStateAsync(e);
            }
            catch (Exception ex)
            {
                PlatformBase.Current.Logger.LogError(ex, "Error during CollectionViewModelBase.OnSaveStateAsync calling each individual child ViewModel.SaveStateAsync");
                throw;
            }

            await base.OnSaveStateAsync(e);
        }

        protected override async Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (this.CurrentViewModel != null)
            {
                this.CurrentViewModel.UserForcedRefresh = this.UserForcedRefresh;
                await this.CurrentViewModel.RefreshAsync(forceRefresh);
            }
            await base.OnRefreshAsync(forceRefresh, ct);
        }

        /// <summary>
        /// Sets the current ViewModel that is active and visible.
        /// </summary>
        /// <param name="vm"></param>
        public async Task SetCurrentAsync(ViewModelBase vm)
        {
            if (vm == this)
                return;

            if (this.CurrentViewModel != null)
            {
                this.CurrentViewModel.PropertyChanged -= CurrentVM_PropertyChanged;
                this.ClearStatus();
            }

            this.CurrentViewModel = vm;

            if (this.CurrentViewModel != null)
            {
                PlatformBase.Current.Logger.Log(LogLevels.Debug, "CollectionViewModelBase.SetCurrent to {0}", vm);
                this.CurrentViewModel.PropertyChanged += CurrentVM_PropertyChanged;
                if (this.CurrentViewModel is WebBrowserViewModel)
                    this.CopyStatus(null);
                else
                    this.CopyStatus(this.CurrentViewModel);

                if (!_viewModelsLoaded.Contains(this.CurrentViewModel))
                {
                    // Call load state on the sub-viewmodel once its requested to be set to curent.
                    await this.CurrentViewModel.LoadStateAsync(this.View, _loadState);
                    _viewModelsLoaded.Add(this.CurrentViewModel);
                }
                else
                    await this.CurrentViewModel.RefreshAsync(false);
            }

            // Update global navigation
            PlatformBase.Current.Navigation.NavigateGoBackCommand.RaiseCanExecuteChanged();
            PlatformBase.Current.Navigation.NavigateGoForwardCommand.RaiseCanExecuteChanged();
        }

        protected internal override bool OnBackNavigationRequested()
        {
            return this.CurrentViewModel?.OnBackNavigationRequested() ?? base.OnBackNavigationRequested();
        }

        protected internal override bool OnForwardNavigationRequested()
        {
            return this.CurrentViewModel?.OnForwardNavigationRequested() ?? base.OnForwardNavigationRequested();
        }

        protected internal override void ViewScrollToTop()
        {
            foreach (var vm in this.ViewModels)
                if (vm != null && vm.IsInitialized)
                    vm.ViewScrollToTop();

            base.ViewScrollToTop();
        }

        protected override void CancelStatus()
        {
            this.CurrentViewModel?.StatusIsBlockingCancelableCommand.Execute(null);
            base.CancelStatus();
        }

        public override void OnHandleKeyDown(KeyRoutedEventArgs e)
        {
            this.CurrentViewModel?.OnHandleKeyDown(e);
            base.OnHandleKeyDown(e);
        }

        public override void OnHandleKeyUp(KeyRoutedEventArgs e)
        {
            this.CurrentViewModel?.OnHandleKeyUp(e);
            base.OnHandleKeyUp(e);
        }

        #endregion

        #region Event Handlers

        private void CurrentVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // Pass the current viewmodel's status properties on to the parent collection view models' status properties.
            switch (e.PropertyName)
            {
                case nameof(this.StatusIsBusy):
                case nameof(this.StatusIsBlocking):
                case nameof(this.StatusIsBlockingCancelable):
                case nameof(this.StatusProgressValue):
                case nameof(this.StatusText):
                    if (this.CurrentViewModel is WebBrowserViewModel)
                        this.CopyStatus(null);
                    else
                        this.CopyStatus(this.CurrentViewModel);
                    break;
            }
        }

        #endregion
    }
}
