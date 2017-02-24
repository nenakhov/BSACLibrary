using System.Windows;
using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BSACLibrary
{
    public static class PdfSearch
    {
        public static pdfDescription SearchInPdfFile(pdfDescription file, string substring)
        {
            //Обнулим значение переменной
            file.founded_text = null;
            try
            {
                using (PdfReader pdfReader = new PdfReader(file.file_path))
                {
                    for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                    {
                        ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                        string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                        string currentPageTextLower = currentPageText.ToLower();
                        int i = currentPageTextLower.IndexOf(substring);
                        if (i != -1)
                        {
                            //Отформатируем найденный текст
                            currentPageText = currentPageText.Substring(i, currentPageText.Length - i);
                            if (currentPageText.Length >= 400)
                            {
                                file.founded_text = currentPageText.Substring(0, 399).Replace("\n", " ") + "...";
                            }
                            else
                            {   //Если на найденной странице символов меньше 400, отображаем ее целиком
                                file.founded_text = currentPageText.Replace("\n", " ") + "...";
                            }
                            return file;
                        }
                    }
                    return file;
                }
            }
            //Обработка возможных ошибок
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return file;
            }
        }
    }
}
