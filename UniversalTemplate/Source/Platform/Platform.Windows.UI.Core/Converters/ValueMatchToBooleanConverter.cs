using System;
using Windows.UI.Xaml.Data;

namespace AppFramework.Uwp.UI.Converters
{
    public class ValueMatchToBooleanConverter : IValueConverter
    {
        public bool InvertValue { get; set; }

        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolean;

            if (value == null)
                boolean = value == parameter;
            else if (value is string)
                boolean = value.ToString().Equals(parameter?.ToString() ?? "", StringComparison.CurrentCultureIgnoreCase);
            else
                boolean = value.Equals(parameter);

            if (this.InvertValue)
                return !boolean;
            else
                return boolean;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}