using BSACLibrary.Properties;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace BSACLibrary
{
    public static class QueryExecute
    {
        public static void Connect(string query)
        {
            MySqlConnection conn = null;
            string connStr = "";

            //Формируем строку для подключения
            connStr = ("server=" + Settings.Default.dbServerIP +
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
