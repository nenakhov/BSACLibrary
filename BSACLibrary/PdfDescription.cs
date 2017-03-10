using System;

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
}