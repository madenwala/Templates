﻿using Contoso.Core.Commands;
using Contoso.Core.Models;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using System.Threading;
using Contoso.Core.Services;

namespace Contoso.Core.ViewModels
{
    public partial class DebuggingViewModel : ViewModelBase
    {
        #region Properties
        
        public ICommand TestAppCrashCommand { get; private set; }

        private ModelList<BackgroundTaskRunInfo> _BackgroundTasksInfo = new ModelList<BackgroundTaskRunInfo>();
        public ModelList<BackgroundTaskRunInfo> BackgroundTasksInfo
        {
            get { return _BackgroundTasksInfo; }
            private set { this.SetProperty(ref _BackgroundTasksInfo, value); }
        }
        
        #endregion Properties

        #region Constructors

        public DebuggingViewModel()
        {
            this.Title = "Debugging";

            if (DesignMode.DesignModeEnabled)
                return;

            this.TestAppCrashCommand = new GenericCommand("TestAppCrashCommand", () => { throw new Exception("Test crash thrown!"); });
        }

        #endregion Constructors

        #region Methods

        protected override Task OnRefreshAsync(bool forceRefresh, CancellationToken ct)
        {
            if (forceRefresh)
            {
                this.BackgroundTasksInfo.Clear();
                foreach (var registration in BackgroundTaskRegistration.AllTasks)
                {
                    string key = "TASK_" + registration.Value.Name;
                    if (PlatformBase.GetService<StorageManager>().ContainsSetting(key, Windows.Storage.ApplicationData.Current.LocalSettings))
                    {
                        var info = PlatformBase.GetService<StorageManager>().LoadSetting<BackgroundTaskRunInfo>(key, Windows.Storage.ApplicationData.Current.LocalSettings);
                        if (info != null)
                        {
                            info.TaskName = registration.Value.Name;
                            this.BackgroundTasksInfo.Add(info);
                        }
                    }
                }
            }

            return Task.CompletedTask;
        }

        #endregion Methods
    }

    public partial class DebuggingViewModel
    {
        /// <summary>
        /// Self-reference back to this ViewModel. Used for designtime datacontext on pages to reference itself with the same "ViewModel" accessor used 
        /// by x:Bind and it's ViewModel property accessor on the View class. This allows you to do find-replace on views for 'Binding' to 'x:Bind'.
        [Newtonsoft.Json.JsonIgnore()]
        [System.Runtime.Serialization.IgnoreDataMember()]
        public DebuggingViewModel ViewModel { get { return this; } }
    }
}

namespace Contoso.Core.ViewModels.Designer
{
    public sealed class DebuggingViewModel : Contoso.Core.ViewModels.DebuggingViewModel
    {
        public DebuggingViewModel()
        {
        }
    }
}