using System;
using System.Windows.Data;

namespace BSACLibrary
{
    //Отдельный класс для информации о каждом PDF файле
    public class PdfDescription
    {
        public int Id { get; set; }
        public string PublicationName { get; set; }
        public bool IsMagazine { get; set; }
        public DateTime Date { get; set; }
        public int IssueNumber { get; set; }
        public string FilePath { get; set; }
        public string FoundedText { get; set; }
    }

    //Класс для конвертирования bool значения в строку
    //Источник: http://geekswithblogs.net/codingbloke/archive/2010/05/28/a-generic-boolean-value-converter.aspx
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { private get; set; }
        public T TrueValue { private get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return FalseValue;
            }
            return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    public class BoolToStringConverter : BoolToValueConverter<String> { }
}
