using System;
using System.Windows.Data;

namespace BSACLibrary
{
    //Отдельный класс возврата более чем одной переменной от функции поиска
    public class pdfSearchResponse
    {
        public bool isFinded { get; set; }
        public string textCut { get; set; }
    }
    //Отдельный класс для информации о каждом PDF файле
    public class pdfDescription
    {
        public int id { get; set; }
        public string publication { get; set; }
        public bool is_magazine { get; set; }
        public string date { get; set; }
        public int issue_number { get; set; }
        public string file_path { get; set; }
    }

    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<String> { }
}
