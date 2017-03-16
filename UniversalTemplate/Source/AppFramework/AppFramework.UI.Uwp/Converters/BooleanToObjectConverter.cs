using System;

namespace AppFramework.UI.Converters
{

    public abstract class BooleanToObjectConverter<T> : ValueToBooleanConverter
    {
        public T TrueValue { get; set; }
        public T FalseValue { get; set; }

        public override object Convert(object value, Type targetType, object parameter, string language)
        {
            bool boolean = (bool)base.Convert(value, targetType, parameter, language);
            return boolean ? this.TrueValue : this.FalseValue;
        }

        public object ConvertBackCore(object value, Type targetType, object parameter)
        {
            throw new NotImplementedException();
        }
    }
}
