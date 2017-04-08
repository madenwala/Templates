using Contoso.Core;
using System;
using System.Threading;
using Windows.ApplicationModel.Background;

namespace Contoso.BackgroundTasks
{
    public sealed class TimedWorkerTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            var work = new Action<CancellationToken>(async (ct) => await Platform.Current.TimedBackgroundWorkAsync(ct));
            Platform.Current.ExecuteBackgroundWork(taskInstance, work);
        }
    }
}