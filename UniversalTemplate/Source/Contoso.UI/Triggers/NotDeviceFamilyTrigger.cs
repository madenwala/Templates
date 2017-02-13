﻿using Contoso.Core;
using Windows.UI.Xaml;

namespace Contoso.UI.Triggers
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
                this.SetActive(Platform.DeviceFamily != value);
            }
        }
    }
}
