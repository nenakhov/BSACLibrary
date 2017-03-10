using System.Windows;
using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BSACLibrary
{
    public static class PdfSearch
    {
        public static void SearchInPdfFile(PdfDescription file, string substring)
        {
            //Обнулим значение переменной
            file.FoundedText = null;
            try
            {
                using (var pdfReader = new PdfReader(file.FilePath))
                {
                    for (var page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        var currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                        var i = currentPageText.IndexOf(substring, StringComparison.CurrentCultureIgnoreCase);
                        if (i == -1)
                        {
                            continue;
                        }
                        //Отформатируем найденный текст
                        currentPageText = currentPageText.Substring(i, currentPageText.Length - i);
                        if (currentPageText.Length >= 400)
                        {
                            file.FoundedText = currentPageText.Substring(0, 399).Replace("\n", " ") + "...";
                        }
                        else
                        {   //Если на найденной странице символов меньше 400, отображаем ее целиком
                            file.FoundedText = currentPageText.Replace("\n", " ") + "...";
                        }
                        return;
                    }
                }
            }
            //Обработка возможных ошибок
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
