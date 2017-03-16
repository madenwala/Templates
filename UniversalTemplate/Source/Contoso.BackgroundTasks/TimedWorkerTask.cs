﻿using AppFramework.Core;
using AppFramework.Core.Models;
using Contoso.Core;
using System;
using System.Threading;
using Windows.ApplicationModel.Background;

namespace Contoso.BackgroundTasks
{
    public sealed class TimedWorkerTask : IBackgroundTask
    {
        // Stores information about the status of this background task execution and if it succeeded or not.
        private BackgroundTaskRunInfo _info = new BackgroundTaskRunInfo();

        public async void Run(IBackgroundTaskInstance taskInstance)
        {
#if !DEBUG
            // Check if the app is alread in the foreground and if so, don't run the agent
            if (AgentSync.IsApplicationLaunched())
                return;
#endif
            // Get a deferral, to prevent the task from closing prematurely 
            // while asynchronous code is still running.
            BackgroundTaskDeferral deferral = taskInstance.GetDeferral();

            // Initialize the app
            await Platform.Current.AppInitializingAsync(InitializationModes.Background);
            Platform.Current.Logger.Log(LogLevels.Information, "Starting background task '{0}'...", taskInstance.Task.Name);

            CancellationTokenSource cts = new CancellationTokenSource();

            taskInstance.Canceled += (sender, reason) =>
            {
                Platform.Current.Logger.Log(LogLevels.Warning, "Background task '{0}' is being cancelled due to '{1}'...", taskInstance.Task.Name, reason);

                // Store info on why this task was cancelled
                _info.CancelReason = reason.ToString();
                _info.EndTime = DateTime.UtcNow;

                // Cancel/dispose the token
                cts?.Cancel();
                cts?.Dispose();
            };

            try
            {
                // Execute the background work
                _info.StartTime = DateTime.UtcNow;
                await Platform.Current.TimedBackgroundWorkAsync(BackgroundWorkCost.CurrentBackgroundWorkCost, cts.Token);

                // Task ran without error
                _info.RunSuccessfully = true;
                Platform.Current.Logger.Log(LogLevels.Information, "Completed execution of background task '{0}'!", taskInstance.Task.Name);
            }
            catch(OperationCanceledException)
            {
                // Task was aborted via the cancelation token
                Platform.Current.Logger.Log(LogLevels.Warning, "Background task '{0}' had an OperationCanceledException with reason '{1}'.", taskInstance.Task.Name, _info.CancelReason);
            }
            catch (Exception ex)
            {
                // Task threw an exception, store/log the error details
                _info.ExceptionDetails = ex.ToString();
                Platform.Current.Logger.LogErrorFatal(ex, "Background task '{0}' failed with exception to run to completion: {1}", taskInstance.Task.Name, ex.Message);
            }
            finally
            {
                _info.EndTime = DateTime.UtcNow;

                // Store the task status information
                Platform.Current.Storage.SaveSetting("TASK_" + taskInstance.Task.Name, _info, Windows.Storage.ApplicationData.Current.LocalSettings);

                // Shutdown the task
                Platform.Current.AppSuspending();
                deferral.Complete();
            }
        }
    }
}
