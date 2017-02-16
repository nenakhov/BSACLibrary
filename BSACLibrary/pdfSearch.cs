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
                    {
                        res.textCut = currentPageText.Replace("\n", " ");
                    }
                    pdfReader.Close();
                    res.isFinded = true;
                    return res;
                }
            }
            pdfReader.Close();
            res.isFinded = false;
            return res;
        }
    }
}
