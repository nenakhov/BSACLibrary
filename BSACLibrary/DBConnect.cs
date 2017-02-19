using MySql.Data.MySqlClient;
using BSACLibrary.Properties;
using System.Windows;

namespace BSACLibrary
{
    public class DBConnect
    {
        public static void Connect()
        {
            MySqlConnection conn = null;
            string connStr = "";

            //Формируем строку для подключения
            connStr= ("server=" + Settings.Default.dbServerIP +
            ";port=" + Settings.Default.dbServerPort +
            ";uid=" + Settings.Default.dbUsername + 
            ";pwd=" + Settings.Default.dbPassword + 
            ";");
            //";database=" + Settings.Default.DBName + ";";

            try
            {
                conn = new MySqlConnection(connStr);
                //С помощью этого объекта выполняются запросы к БД
                MySqlCommand Query = new MySqlCommand();
                //Присваиваем объекту созданное соединение
                Query.Connection = conn;
                //Устанавливаем соединение с БД
                conn.Open();

                //Если выбран режим администратора создаем БД и таблицу в ней, если они еще не были созданы
                if (Settings.Default.isAdmin == true)
                {
                    //Создаем базу данных
                    Query.CommandText = "CREATE DATABASE IF NOT EXISTS " + // Если такая БД уже есть не будет ошибки
                        "`" + Settings.Default.dbName +
                        "` CHARACTER SET cp1251 COLLATE cp1251_general_ci;";
                    //Отправляем запрос
                    Query.ExecuteNonQuery();

                    //Выбираем БД с которой будем работать
                    Query.CommandText = "USE " + Settings.Default.dbName + ";";
                    //Отправляем запрос
                    Query.ExecuteNonQuery();

                    //Запрос создание таблицы
                    Query.CommandText = "CREATE TABLE IF NOT EXISTS `" + // Если такая таблица уже есть не будет ошибки
                                        Settings.Default.dbTableName + // Имя таблицы 
                                        "` (" +
                                        "`id` INT(6) NOT NULL AUTO_INCREMENT, " + // Cамоинкрементирующееся поле id 
                                        "`publication` VARCHAR(256) NOT NULL, " + // Название журнала
                                        "`is_magazine` TINYINT(1) NOT NULL, " + // Является ли журналом или газетой
                                        "`date` DATE NOT NULL, " + // Дата издания
                                        "`issue_number` INT(6) NOT NULL, " + // Порядковый номер издания
                                        "`file_path` VARCHAR(256) NOT NULL, " + // Ссылка на файл \\host\Lib\file.pdf 240 кол-во символов
                                        "PRIMARY KEY(`id`) " +
                                        ") ";
                    //Отправляем запрос
                    Query.ExecuteNonQuery();
                }
            }
            //В этом блоке перехватываем возможные ошибки в процессе соединения
            catch (MySqlException ex)
            {
                //Unable to connect to any of the specified MySQL hosts.
                if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                {
                    MessageBox.Show("Проверьте настройки подключения к MySQL", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            finally
            {
                if (conn != null) 
                    {
                        //Закрываем соединение
                        conn.Close();
                    }
            }
        }
    }
}
