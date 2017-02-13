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
                if (currentPageTextLower.Contains(searchText))
                {
                    res.isFinded = true;
                    currentPageText = currentPageText.Replace("\n", " ");
                    if (currentPageText.Length >= 300)
                    {
                        res.textCut = currentPageText.Substring(0, 300);
                    }
                    else
                    {
                        res.textCut = currentPageText;
                    }
                    pdfReader.Close();
                    return res;
                }
            }
            pdfReader.Close();
            res.isFinded = false;
            return res;
        }
    }
}
