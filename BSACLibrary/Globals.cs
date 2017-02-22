using BSACLibrary.Properties;

namespace BSACLibrary
{
    /// <summary>
    /// В этом классе будем описывать глобальные переменные которые могут понадобиться
    /// </summary>
    public static class Globals
    {
        public static string connectionString { get; set; }
        public static bool isConnected { get; set; }

        static Globals()
        {
            //Значения по умолчанию
            connectionString = ("server=" + Settings.Default.dbServerIP +
            ";port=" + Settings.Default.dbServerPort +
            ";uid=" + Settings.Default.dbUsername +
            ";pwd=" + Settings.Default.dbPassword +
            ";charset=cp1251;convert zero datetime=true;");
            isConnected = false;
        }

        //Метод для изменения глобальной переменной
        //Формирует строку для подключения к MySQL
        public static void SetConnectionString()
        {
            connectionString = ("server=" + Settings.Default.dbServerIP +
            ";port=" + Settings.Default.dbServerPort +
            ";uid=" + Settings.Default.dbUsername +
            ";pwd=" + Settings.Default.dbPassword +
            ";charset=cp1251;convert zero datetime=true;");
        }
    }
}
