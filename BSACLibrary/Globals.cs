using BSACLibrary.Properties;

namespace BSACLibrary
{
        public static class Globals
    {
        static Globals()
        {
            connStr = "";
        } //Значения по умолчанию

        public static string connStr { get; private set; }

        //Метод для изменения глобальной переменной
        public static void SetConnStr()
        {
            connStr = ("server=" + Settings.Default.dbServerIP +
            ";port=" + Settings.Default.dbServerPort +
            ";uid=" + Settings.Default.dbUsername +
            ";pwd=" + Settings.Default.dbPassword +
            ";");
        }
    }
}
