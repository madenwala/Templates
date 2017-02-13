using Contoso.Core;
using Windows.UI.Xaml;

namespace Contoso.UI.Triggers
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
                this.SetActive(Platform.DeviceFamily == value);
            }
        }
    }
}
