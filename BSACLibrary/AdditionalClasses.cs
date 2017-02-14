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
}
