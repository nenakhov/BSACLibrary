using System;
using System.Collections.Generic;
using System.Data;
using BSACLibrary.Properties;
using MySql.Data.MySqlClient;
using System.Windows.Threading;
using System.Threading;

namespace BSACLibrary
{
    public static class DbQueries
    {
        private static readonly MainWindow MWin = MainWindow.AppWindow;

        //Метод возвращает строку подключения к БД
        private static string ConnectionString()
        {
            var stringBuilder = new MySqlConnectionStringBuilder
            {
                Server = Settings.Default.dbServerIP,
                Port = Settings.Default.dbServerPort,
                UserID = Settings.Default.dbUsername,
                Password = Settings.Default.dbPassword,
                CharacterSet = "utf8",
                ConvertZeroDateTime = true
            };
            return stringBuilder.ConnectionString;
        }

        //Метод выполняющий SQL запрос, не возвращает результата
        public static void Execute(string query)
        {
            //Созданое таким образом соединение будет автоматически закрыто
            using (var conn = new MySqlConnection(ConnectionString()))
            {
                //Открываем соединение
                conn.Open();
                //Выбор БД для использования по умолчанию
                conn.ChangeDatabase(Settings.Default.dbName);

                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }

        //Отдельный метод для обновления DataGrid таблицы в редакторе
        public static void UpdateDataGrid()
        {
            using (var conn = new MySqlConnection(ConnectionString()))
            {
                //Открываем соединение
                conn.Open();
                conn.ChangeDatabase(Settings.Default.dbName);

                //Отправка запроса на обновление списка изданий из БД
                var newDataAdapter =
                    new MySqlDataAdapter(
                        "SELECT id,publication,is_magazine,date,issue_number,file_path FROM " +
                        Settings.Default.dbTableName, conn);

                var newDataSet = new DataSet();
                newDataAdapter.Fill(newDataSet, "dbBinding");
                //Заполняем таблицу в редакторе
                MWin.Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate
                    {
                        MWin.DbDataGrid.DataContext = newDataSet;
                        //Обновим информацию о файлах для поиска
                        Initialize.UpdateFilesDescriptions();
                    });
            }
        }

        //Метод выполняющий команду и возвращающий результат в виде массива
        public static List<PdfDescription> ExecuteAndReadFilesDescription(string query)
        {
            using (var conn = new MySqlConnection(ConnectionString()))
            {
                //Открываем соединение
                conn.Open();
                conn.ChangeDatabase(Settings.Default.dbName);

                var cmd = new MySqlCommand(query, conn);
                var reader = cmd.ExecuteReader();

                //Cоздаем необходимые переменные
                var newList = new List<PdfDescription>();

                while (reader.Read())
                {
                    //Присвоими полученные от SQL значения соответсвующему классу
                    var curFile = new PdfDescription
                    {
                        Id = Convert.ToInt32(reader.GetString(0)),
                        PublicationName = reader.GetString(1),
                        IsMagazine = Convert.ToBoolean(reader.GetString(2)),
                        Date = Convert.ToDateTime(reader.GetString(3)),
                        IssueNumber = Convert.ToInt32(reader.GetString(4)),
                        FilePath = reader.GetString(5)
                    };

                    //Запишем данные в массив
                    newList.Add(curFile);
                }
                return newList;
            }
        }

        //Метод для создания БД и таблицы, если они отсутсвуют
        public static void DataBaseCreate()
        {
            //Откроем новое соединение
            using (var conn = new MySqlConnection(ConnectionString()))
            {
                conn.Open();

                //Запрос для создания базы данных
                var query = "CREATE DATABASE IF NOT EXISTS " +
                            "`" + Settings.Default.dbName +
                            "` CHARACTER SET utf8 COLLATE utf8_general_ci;";
                //Отправляем запрос
                var cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Выбираем БД с которой будем работать
                conn.ChangeDatabase(Settings.Default.dbName);

                //Запрос для создания таблицы
                query = "CREATE TABLE IF NOT EXISTS `" +
                        Settings.Default.dbTableName + //Имя таблицы 
                        "` (" +
                        "`id` INT(6) NOT NULL AUTO_INCREMENT, " + //Автоинкрементирующееся поле id 
                        "`publication` VARCHAR(255) NOT NULL, " + //Название издания
                        "`is_magazine` TINYINT(1) NOT NULL, " + //Журнал или газета
                        "`date` DATETIME NOT NULL, " + //Дата выхода издания
                        "`issue_number` INT(6) NOT NULL, " + //Сквозной номер издания
                        "`file_path` VARCHAR(255) NOT NULL, " + //Расположение файла
                        "PRIMARY KEY(`id`) " +
                        ") ";
                //Отправляем запрос
                cmd = new MySqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}