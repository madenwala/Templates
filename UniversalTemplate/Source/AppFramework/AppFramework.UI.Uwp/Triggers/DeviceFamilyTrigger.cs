using AppFramework.Core;
using Windows.UI.Xaml;

namespace AppFramework.UI.Uwp.Triggers
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
                this.SetActive(PlatformBase.DeviceFamily == value);
            }
        }
    }
}
