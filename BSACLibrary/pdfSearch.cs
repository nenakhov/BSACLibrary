using iTextSharp.text.pdf;
using iTextSharp.text.pdf.parser;

namespace BSACLibrary
{
    public static class pdfSearch
    {
        public static bool SearchPdfFile(string fileName, string searchText)
        {
            PdfReader pdfReader = new PdfReader(fileName);
            for (int page = 1; page <= pdfReader.NumberOfPages; page++)
            {
                ITextExtractionStrategy strategy = new SimpleTextExtractionStrategy();
                string currentPageText = PdfTextExtractor.GetTextFromPage(pdfReader, page, strategy);
                if (currentPageText.Contains(searchText))
                {
                    pdfReader.Close();
                    return true;
                }
            }
            pdfReader.Close();
            return false;
        }
    }
}
