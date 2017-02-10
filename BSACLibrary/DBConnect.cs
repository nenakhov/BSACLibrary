using MySql.Data.MySqlClient;
using BSACLibrary.Properties;
using System.Windows;

namespace BSACLibrary
{
    public class DBConnect
    {
        public static void Connect()
        {
            //Устанавливаем соединение с БД
            MySqlConnection Connection = new MySqlConnection("server=" +
                Settings.Default.DBServerIP +
                ";port=" + Settings.Default.DBServerPort +
                //";uid=" + Settings.Default.DBUsername + 
                //";pwd=" + Settings.Default.DBPassword + 
                ";uid=root" +
                ";pwd=1544L" +
                ";");
            //";database=" + Settings.Default.DBName + ";";

            MySqlCommand Query = new MySqlCommand(); // С помощью этого объекта выполняются запросы к БД
            Query.Connection = Connection; // Присвоим объекту только что созданное соединение

            try
            {
                Connection.Open();

                if (Settings.Default.IsAdmin == true)
                {
                    //Создаем базу данных
                    Query.CommandText = "CREATE DATABASE IF NOT EXISTS " + // Если такая БД уже есть не выдаст ошибку 
                        "`" + Settings.Default.DBName +
                        "` CHARACTER SET cp1251 COLLATE cp1251_general_ci;";
                    Query.ExecuteNonQuery(); // Отправляем запрос

                    Query.CommandText = "USE " + Settings.Default.DBName + ";";
                    Query.ExecuteNonQuery();

                    //Запрос создание таблицы
                    Query.CommandText = "CREATE TABLE IF NOT EXISTS " + // Если такая таблица уже есть не выдаст ошибку 
                                        "`catalogue` (" + // Имя таблицы 
                                        "`id` INT(11) NOT NULL AUTO_INCREMENT, " + // Cамоинкрементирующееся поле id 
                                        "`publication` VARCHAR(60) NOT NULL, " + // Название журнала
                                        "`is_magazine` TINYINT(1) NOT NULL, " + // Является ли журналом или газетой
                                        "`date` DATE NOT NULL, " + // Дата издания
                                        "`issue_number` SMALLINT(6) NOT NULL, " + // Порядковый номер издания smallint до 32767
                                        "`file_path` VARCHAR(240) NOT NULL, " + // Ссылка на файл \\host\Lib\file.pdf 240 кол-во символов
                                        "PRIMARY KEY(`id`) " +
                                        ") ";
                    Query.ExecuteNonQuery(); // Отправляем запрос
                }

                //Закрываем соединение
                Connection.Close();
            }
            catch (MySqlException ex)
            {
                //Unable to connect to any of the specified MySQL hosts.
                if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                {
                    MessageBox.Show("Проверьте настройки подключения к MySQL");
                }
                else
                {
                    MessageBox.Show(ex.Message);
                }
                return;
            }
        }
    }
}
