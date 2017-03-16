using System;
using Windows.UI.Xaml.Data;

namespace AppFramework.UI.Uwp.Converters
{
    public class ValueToDoubleConverter : IValueConverter
    {
        public virtual object Convert(object value, Type targetType, object parameter, string language)
        {
            if (targetType == typeof(double))
            {
                double returnValue = 0;

                if (value is double)
                    returnValue = (double)value;
                else if (value != null)
                    double.TryParse(value.ToString(), out returnValue);

                return returnValue;
            }
            else if (targetType == typeof(string))
            {
                return value?.ToString();
            }
            else
                return value;
        }

        public virtual object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            double returnValue = 0;
            if (value != null)
                double.TryParse(value.ToString(), out returnValue);

            return returnValue;
        }
    }
}
