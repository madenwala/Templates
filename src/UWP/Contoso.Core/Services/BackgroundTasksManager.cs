using AppFramework.Core.Services;
using Windows.ApplicationModel.Background;

namespace Contoso.Core.Services
{
    public sealed class BackgroundTasksManager : BaseBackgroundTasksManager
    {
        protected override void Registrations()
        {
            // Register each of your background tasks here:
            this.RegisterBackgroundTaskAsync("Contoso.BackgroundTasks.TimedWorkerTask)", "ContosoTimeTriggerTask", new TimeTrigger(15, false), null);
        }
    }
}