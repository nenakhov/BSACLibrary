using System;
using System.Globalization;
using System.Windows.Data;

namespace BSACLibrary
{

    //Класс для конвертирования bool значения в строку
    //Источник: http://geekswithblogs.net/codingbloke/archive/2010/05/28/a-generic-boolean-value-converter.aspx
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { private get; set; }
        public T TrueValue { private get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return FalseValue;
            }

            return (bool) value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value?.Equals(TrueValue) ?? false;
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<string>
    {
    }
}