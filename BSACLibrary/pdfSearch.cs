using System.Windows;
using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BSACLibrary
{
    public static class PdfSearch
    {
        public static pdfSearchResponse SearchInPdfFile(string fileName, string searchText)
        {
            pdfSearchResponse res = new pdfSearchResponse();

            try
            {
                PdfReader pdfReader = new PdfReader(fileName);
                
                for (int page = 1; page <= pdfReader.NumberOfPages; page++)
                {
                    ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                    string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                    string currentPageTextLower = currentPageText.ToLower();
                    int i = currentPageTextLower.IndexOf(searchText);
                    if (i != -1)
                    {
                        //Отформатируем найденный текст
                        currentPageText = currentPageText.Substring(i, currentPageText.Length - i);
                        if (currentPageText.Length >= 400)
                        {
                            res.founded_text = currentPageText.Substring(0, 399).Replace("\n", " ");
                        }
                        else
                        {   //Если на найденной странице символов меньше 400, отображаем ее целиком
                            res.founded_text = currentPageText.Replace("\n", " ");
                        }
                        res.isFinded = true;
                        break;
                    }
                }
                pdfReader.Close();
                return res;
            }
            //Обработка возможных ошибок
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return res;
            }
        }
    }
}
