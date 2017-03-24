using System;
using System.Globalization;
using System.Windows.Data;

namespace BSACLibrary
{
    //Источник http://stackoverflow.com/questions/356194/datatrigger-where-value-is-not-null
    public class IsNullConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
