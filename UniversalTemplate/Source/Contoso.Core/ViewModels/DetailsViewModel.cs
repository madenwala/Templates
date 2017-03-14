using AppFramework.Core;
using AppFramework.Core.Commands;
using AppFramework.Core.Models;
using AppFramework.Core.Services;
using AppFramework.Core.Strings;
using Contoso.Core.Data;
using Contoso.Core.Models;
using System;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;

namespace Contoso.Core.ViewModels
{
    public partial class DetailsViewModel : ViewModelBase
    {
        #region Properties

        public override string Title
        {
            get { return this.Item?.LineOne ?? base.Title; }
            protected set { base.Title = value; }
        }

        private string _ID;
        public string ID
        {
            get { return _ID; }
            private set { this.SetProperty(ref _ID, value); }
        }
        
        private ItemModel _Item;
        public ItemModel Item
        {
            get { return _Item; }
            protected set
            {
                if (this.SetProperty(ref _Item, value))
                    this.ID = value?.ID;
                else
                    this.Item?.CopyFrom(value);
                
                this.NotifyPropertyChanged(() => this.Title);
            }
        }
        
        private bool _IsDownloadEnabled = true;
        public bool IsDownloadEnabled
        {
            get { return _IsDownloadEnabled; }
            private set { this.SetProperty(ref _IsDownloadEnabled, value); }
        }

        public PinTileCommand PinTileCommand { get; private set; }
        public UnpinTileCommand UnpinTileCommand { get; private set; }
        public CommandBase DownloadCommand { get; private set; }

        #endregion

        #region Constructors

        public DetailsViewModel(string id = null)
        {
            if (DesignMode.DesignModeEnabled)
                return;

            this.RequiresAuthorization = true;

            this.Title = Resources.TextNotApplicable;
            this.ID = id;
            this.RequiresAuthorization = true;
            this.PinTileCommand = new PinTileCommand();
            this.UnpinTileCommand = new UnpinTileCommand();
            this.PinTileCommand.OnSuccessAction = () => this.UnpinTileCommand.RaiseCanExecuteChanged();
            this.UnpinTileCommand.OnSuccessAction = () => this.PinTileCommand.RaiseCanExecuteChanged();
            this.DownloadCommand = new GenericCommand("DownloadCommand", async () => await this.DownloadAsync(), () => this.IsDownloadEnabled);
        }

        #endregion

        #region Methods

        protected override async Task OnLoadStateAsync(LoadStateEventArgs e)
        {
            this.View.GotFocus += View_GotFocus;

            if (e.Parameter is ItemModel)
            {
                this.Item = e.Parameter as ItemModel;
            }
            else if (e.Parameter != null)
            {
                if (e.Parameter.ToString() != this.Item?.ID)
                    this.Item = null;
                this.ID = e.Parameter.ToString();
            }
            else
            {
                this.Item = null;
                this.ID = null;
            }

            await base.OnLoadStateAsync(e);
        }

        protected override Task OnSaveStateAsync(SaveStateEventArgs e)
        {
            this.View.GotFocus -= View_GotFocus;
            return base.OnSaveStateAsync(e);
        }

        private void View_GotFocus(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            this.PinTileCommand.RaiseCanExecuteChanged();
            this.UnpinTileCommand.RaiseCanExecuteChanged();
        }

        protected override async Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(this.ID))
                throw new UserFriendlyException("No item was requested to be displayed.");

            if (forceRefresh || this.Item == null)
            {
                try
                {
                    if (this.Item == null)
                        this.Title = Resources.TextLoading;

                    this.ShowBusyStatus(Resources.TextLoading, this.Item == null);
                    using (var api = new ClientApi())
                    {
                        this.Item = await api.GetItemByID(this.ID, ct);
                    }
                }
                finally
                {
                    await this.RefreshUIAsync();
                }
            }

            if (this.Item == null)
            {
                this.Title = Resources.TextNotApplicable;
                throw new ArgumentException("No item to display.");
            }
        }

        private async Task RefreshUIAsync()
        {
            this.PinTileCommand.RaiseCanExecuteChanged();
            this.UnpinTileCommand.RaiseCanExecuteChanged();

            if (this.Item != null)
            {
                // Check if tile exists, clear old notifications, update for new notifications
                if (this.Platform.Notifications.HasTile(this.Item))
                {
                    this.Platform.Notifications.ClearTile(this.Item);
                    await this.Platform.Notifications.CreateOrUpdateTileAsync(this.Item);
                }
                
                if (!this.IsInitialized)
                {
                    var t = this.Platform.Jumplist.AddItemAsync(new JumpItemInfo()
                    {
                        Name = this.Item.LineOne,
                        Description = this.Item.LineTwo,
                        Arguments = this.Platform.GenerateModelArguments(this.Item)
                    });
                }
            }
        }

        private async Task DownloadAsync()
        {
            try
            {
                this.IsDownloadEnabled = false;
                
                for (double p = 0; p <= 100; p++)
                {
                    this.ShowBusyStatus(Resources.TextDownloading);
                    this.StatusProgressValue = p;
                    await Task.Delay(100);
                }
                this.ClearStatus();
            }
            catch (Exception ex)
            {
                this.HandleException(ex, "Error while downloading!");
                this.ShowTimedStatus("Couldn't download data. Try again later.");
            }
            finally
            {
                this.IsDownloadEnabled = true;
            }
        }

        #endregion
    }

    public partial class DetailsViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public DetailsViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class DetailsViewModel : Contoso.Core.ViewModels.DetailsViewModel
    {
        public DetailsViewModel()
            : base()
        {
            this.Item = new ItemModel()
            {
                ID = "0",
                LineOne = "PinLine1",
                LineTwo = "PinLine2",
                LineThree = "PinLine3",
                LineFour = "PinLine4",
            };
        }
    }
}