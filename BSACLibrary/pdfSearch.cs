using System.Windows;
using System;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BSACLibrary
{
    public static class pdfSearch
    {
        public static pdfSearchResponse SearchPdfFile(string fileName, string searchText)
        {
            PdfReader pdfReader = new PdfReader(fileName);
            pdfSearchResponse res = new pdfSearchResponse();
            try
            {

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
                            res.textCut = currentPageText.Substring(0, 399).Replace("\n", " ");
                        }
                        else
                        {   //Если на найденной странице символов меньше 400, отображаем ее целиком
                            res.textCut = currentPageText.Replace("\n", " ");
                        }
                        res.isFinded = true;
                        return res;
                    }
                }
                res.isFinded = false;
                return res;
            }
            //Обработка возможных ошибок
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
            //При любом результате выполнения программы выше, закроем открытый файл
            finally
            {
                pdfReader.Close();
            }
        }
    }
}
