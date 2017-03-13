using AppFramework.Core;
using AppFramework.Core.Services;
using Windows.UI.Xaml;

namespace AppFramework.Uwp.UI.Triggers
{
    /// <summary>
    /// Trigger for when you need do perform device specific customizations.
    /// </summary>
    public class DeviceFamilyTrigger : StateTriggerBase
    {
        public DeviceFamily TargetDeviceFamily
        {
            set
            {
                this.SetActive(AppFramework.Core.Services.PlatformBase.DeviceFamily == value);
            }
        }
    }
}
