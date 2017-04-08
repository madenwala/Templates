using AppFramework.Core.Commands;
using AppFramework.Core.Services;
using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.System;

namespace AppFramework.Core
{
    public partial class PlatformCore
    {
        /// <summary>
        /// Gets access to the geocoding service adapter implement of the platform currently executing.
        /// </summary>
        public BackgroundTasksManagerBase BackgroundTasks
        {
            get { return GetService<BackgroundTasksManagerBase>(); }
            protected set { SetService(value); }
        }
    }
}

namespace AppFramework.Core.Services
{
    /// <summary>
    /// Task manager is responsible for registering and unregistering all background tasks used by this application.
    /// </summary>
    public abstract class BackgroundTasksManagerBase : ServiceBase, IServiceSignout
    {
        #region Properties

        public bool AreTasksRegistered { get; set; }
        
        private CommandBase _ManageBackgroundTasksCommand = null;
        /// <summary>
        /// Manage background apps from the Windows Settings app.
        /// </summary>
        public CommandBase ManageBackgroundTasksCommand
        {
            // Deep linking to Settings app sections: https://msdn.microsoft.com/en-us/library/windows/apps/mt228342.aspx
            get { return _ManageBackgroundTasksCommand ?? (_ManageBackgroundTasksCommand = new GenericCommand("ManageBackgroundTasksCommand", async () => await Launcher.LaunchUriAsync(new Uri("ms-settings:privacy-backgroundapps")))); }
        }

        #endregion

        #region Methods

        protected abstract void Registrations();

        /// <summary>
        /// Registers all background tasks related to this application.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public async Task RegisterAllAsync()
        {
            try
            {
                PlatformCore.Current.Logger.Log(LogLevels.Debug, "Registering background tasks...");

                // Keep track of the previous version of the app. If the app has been updated, we must first remove the previous task registrations and then re-add them.
                var previousVersion = PlatformCore.Current.Storage.LoadSetting<string>("PreviousAppVersion");
                if (previousVersion != PlatformCore.Current.AppInfo.VersionNumber.ToString())
                {
                    this.RemoveAll();
                    PlatformCore.Current.Storage.SaveSetting("PreviousAppVersion", PlatformCore.Current.AppInfo.VersionNumber.ToString());
                }

                // Propmts users to give access to run background tasks.
                var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
                if (backgroundAccessStatus == BackgroundAccessStatus.AlwaysAllowed || backgroundAccessStatus == BackgroundAccessStatus.AllowedSubjectToSystemPolicy)
                {
                    try
                    {
                        this.Registrations();

                        // Flag that registration was completed
                        this.AreTasksRegistered = true;
                    }
                    catch (Exception ex)
                    {
                        PlatformCore.Current.Logger.LogError(ex, "Failed to register background tasks.");
                    }
                }
                else
                {
                    // User did not give the app access to run background tasks
                    PlatformCore.Current.Logger.Log(LogLevels.Information, "Could not register tasks because background access status is '{0}'.", backgroundAccessStatus);
                    this.AreTasksRegistered = false;
                }

                PlatformCore.Current.Logger.Log(LogLevels.Debug, "Completed registering background tasks!");
            }
            catch (Exception ex)
            {
                PlatformCore.Current.Logger.LogError(ex, "Error during BackgroundTaskManager.RegisterAllAsync()");
            }
        }

        /// <summary>
        /// Indicates whether or not the app has permissions to run background tasks.
        /// </summary>
        /// <returns>True if allowed else false.</returns>
        public bool CheckIfAllowed()
        {
            var status = BackgroundExecutionManager.GetAccessStatus();

            var allowed = status == BackgroundAccessStatus.AlwaysAllowed || status == BackgroundAccessStatus.AllowedSubjectToSystemPolicy;
            
            if (allowed == false)
                this.AreTasksRegistered = false;

            return allowed;
        }

        /// <summary>
        /// Removes background task registrations when the user signs out of the app.
        /// </summary>
        /// <returns>Awaitable task is returned.</returns>
        public Task SignoutAsync()
        {
            this.RemoveAll();
            return Task.CompletedTask;
        }

        /// <summary>
        /// Removes all registered background tasks related to this application.
        /// </summary>
        private void RemoveAll()
        {
            this.Remove(null);
            BackgroundExecutionManager.RemoveAccess();
        }

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        protected BackgroundTaskRegistration RegisterBackgroundTaskAsync(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition = null)
        {
            if (string.IsNullOrWhiteSpace(taskEntryPoint))
                throw new ArgumentNullException(nameof(taskEntryPoint));
            if (string.IsNullOrWhiteSpace(taskName))
                throw new ArgumentNullException(nameof(taskName));
            if (trigger == null)
                throw new ArgumentNullException(nameof(trigger));

            try
            {
                // Remove if existing
                this.Remove(taskName);

                BackgroundTaskBuilder builder = new BackgroundTaskBuilder();

                if (condition != null)
                {
                    builder.AddCondition(condition);

                    // If the condition changes while the background task is executing then it will
                    // be canceled.
                    builder.CancelOnConditionLoss = true;
                }

                builder.Name = taskName;
                builder.TaskEntryPoint = taskEntryPoint;

                builder.SetTrigger(trigger);

                var registrationTask = builder.Register();
                return registrationTask;
            }
            catch (Exception ex)
            {
                PlatformCore.Current.Logger.LogError(ex, "Error while trying to register task '{0}': {1}", taskName, ex.Message);
                return null;
            }
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        private void Remove(string name)
        {
            // Loop through all background tasks and unregister all tasks with matching name
            foreach (var taskKeyPair in BackgroundTaskRegistration.AllTasks)
            {
                if (string.IsNullOrEmpty(name) || taskKeyPair.Value.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase))
                {
                    taskKeyPair.Value.Unregister(true);
                    PlatformCore.Current.Storage.SaveSetting("TASK_" + taskKeyPair.Key, null, Windows.Storage.ApplicationData.Current.LocalSettings);
                    PlatformCore.Current.Logger.Log(LogLevels.Debug, "TaskManager removed background task '{0}'", name);
                }
            }
        }

        #endregion
    }
}
