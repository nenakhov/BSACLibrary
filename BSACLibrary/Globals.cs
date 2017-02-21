using BSACLibrary.Properties;

namespace BSACLibrary
{
    /// <summary>
    /// В этом классе будем описывать глобальные переменные которые могут понадобиться
    /// </summary>
    public static class Globals
    {
        public static string connStr { get; private set; }
        public static bool isConnected { get; set; }

        static Globals()
        {
            connStr = null;
            isConnected = false;
        } //Значения по умолчанию

        //Метод для изменения глобальной переменной
        //Формирует строку для подключения к MySQL
        public static void SetConnStr()
        {
            connStr = ("server=" + Settings.Default.dbServerIP +
            ";port=" + Settings.Default.dbServerPort +
            ";uid=" + Settings.Default.dbUsername +
            ";pwd=" + Settings.Default.dbPassword +
            ";charset=cp1251;convert zero datetime=true;");
        }
    }
}
