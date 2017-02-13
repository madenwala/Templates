using System;
using Windows.UI.Xaml;

namespace Contoso.UI.Converters
{
    public sealed class ValueMatchToVisibilityConverter : ValueMatchToBooleanConverter
    {
        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolean = (bool)base.Convert(value, targetType, parameter, language);
            return boolean ? Visibility.Visible : Visibility.Collapsed;
        }

        public override object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}