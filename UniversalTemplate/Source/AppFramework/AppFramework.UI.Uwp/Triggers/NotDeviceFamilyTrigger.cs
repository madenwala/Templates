using AppFramework.Core;
using Windows.UI.Xaml;

namespace AppFramework.UI.Uwp.Triggers
{
    /// <summary>
    /// Trigger for when you need do perform specific customizations NOT on the specified device family.
    /// </summary>
    public class NotDeviceFamilyTrigger : StateTriggerBase
    {
        public DeviceFamily TargetDeviceFamily
        {
            set
            {
                this.SetActive(PlatformBase.DeviceFamily != value);
            }
        }
    }
}
