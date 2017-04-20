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
            try
            {
                var t = new Contoso.Core.Strings.Resources();
                var u = Contoso.Core.Strings.Resources.ApplicationName;


                var work = new Action<CancellationToken>(async (ct) => await Platform.Current.TimedBackgroundWorkAsync(ct));
                Platform.Current.ExecuteBackgroundWork(taskInstance, work);
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                throw ex;
            }
        }
    }
}