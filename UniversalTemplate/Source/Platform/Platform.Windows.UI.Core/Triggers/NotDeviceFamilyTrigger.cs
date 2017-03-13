using Contoso.Core;
using Contoso.Core.Services;
using Windows.UI.Xaml;

namespace AppFramework.Uwp.UI.Triggers
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
                this.SetActive(Contoso.Core.Services.PlatformBase.DeviceFamily != value);
            }
        }
    }
}
