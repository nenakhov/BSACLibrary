using BSACLibrary.Properties;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace BSACLibrary
{
    public static class DBQueries
    {

        private static MainWindow mWin = MainWindow.AppWindow;
        private static string connectionString = ("Server=" + Settings.Default.dbServerIP +
            "; Port=" + Settings.Default.dbServerPort +
            "; Uid=" + Settings.Default.dbUsername +
            "; Pwd=" + Settings.Default.dbPassword +
            "; CharSet=cp1251; ConvertZeroDateTime=True;");

        //Метод выполняющий SQL запрос, не возвращает результата
        public static void Execute(string query)
        {
            //Созданое таким образом соединение будет автоматически закрыто
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //Открываем соединение
                conn.Open();
                //Выбор БД для использования по умолчанию
                conn.ChangeDatabase(Settings.Default.dbTableName);

                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }

        //Отдельный метод для обновления DataGrid таблицы в редакторе
        public static void UpdateDataGrid()
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //Открываем соединение
                conn.Open();
                conn.ChangeDatabase(Settings.Default.dbTableName);

                //Отправка запроса на обновление списка изданий из БД
                SqlDataAdapter newDataAdapter = new SqlDataAdapter("SELECT id,publication,is_magazine,date,issue_number,file_path FROM " + Settings.Default.dbTableName, conn);

                DataSet newDataSet = new DataSet();
                newDataAdapter.Fill(newDataSet, "dbBinding");
                //Заполняем таблицу в редакторе
                mWin.dbDataGrid.DataContext = newDataSet;
                //Обновим информацию о файлах для поиска
                Initialize.UpdateFilesDescription();
            }
        }

        //Метод выполняющий команду и возвращающий результат в виде массива
        public static List<pdfDescription> ExecuteAndReadFilesDescription(string query)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                //Открываем соединение
                conn.Open();
                conn.ChangeDatabase(Settings.Default.dbTableName);

                SqlCommand cmd = new SqlCommand(query, conn);
                //cmd.ExecuteNonQuery();
                SqlDataReader Reader = cmd.ExecuteReader();

                //Cоздаем необходимые переменные
                List<pdfDescription> newList = new List<pdfDescription>();

                //Присвоими полученные от SQL значения соответсвующему классу
                pdfDescription curFile = new pdfDescription();
                while (Reader.Read())
                {
                    curFile.id = Convert.ToInt32(Reader.GetString(0));
                    curFile.publication = Reader.GetString(1);
                    curFile.is_magazine = Convert.ToBoolean(Reader.GetString(2));
                    curFile.date = Convert.ToDateTime(Reader.GetString(3));
                    curFile.issue_number = Convert.ToInt32(Reader.GetString(4));
                    curFile.file_path = Reader.GetString(5);

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
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();

                //Запрос для создания базы данных
                //Если такая БД уже есть ошибки не будет (IF NOT EXISTS)
                string query = "CREATE DATABASE IF NOT EXISTS " +
                                            "`" + Settings.Default.dbName +
                                            "` CHARACTER SET cp1251 COLLATE cp1251_general_ci;";
                //Отправляем запрос
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();

                //Выбираем БД с которой будем работать
                conn.ChangeDatabase(Settings.Default.dbTableName);

                //Запрос для создания таблицы
                //Если такая таблица уже есть в БД ошибки не будет (IF NOT EXISTS)
                query = "CREATE TABLE IF NOT EXISTS `" +
                                    Settings.Default.dbTableName + //Имя таблицы 
                                    "` (" +
                                    "`id` INT(6) NOT NULL AUTO_INCREMENT, " + //Автоинкрементирующееся поле id 
                                    "`publication` VARCHAR(255) NOT NULL, " + //Название издания
                                    "`is_magazine` TINYINT(1) NOT NULL, " + //Является ли журналом или газетой
                                    "`date` DATETIME NOT NULL, " + //Дата выхода издания
                                    "`issue_number` INT(6) NOT NULL, " + //Порядковый номер издания
                                    "`file_path` VARCHAR(255) NOT NULL, " + //Ссылка на файл
                                    "PRIMARY KEY(`id`) " +
                                    ") ";
                //Отправляем запрос
                cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
