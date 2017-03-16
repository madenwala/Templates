﻿using System;
using Windows.UI.Xaml.Data;

namespace AppFramework.UI.Converters
{
    public sealed class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (parameter != null && parameter is string)
            {
                string format = parameter.ToString();
                return string.Format(format, value).Trim();
            }
            else
                return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
