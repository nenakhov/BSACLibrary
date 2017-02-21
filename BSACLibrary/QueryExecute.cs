using BSACLibrary.Properties;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows;

namespace BSACLibrary
{
    public class QueryExecute : IDisposable
    {
        MySqlConnection conn = null;
        MySqlCommand Query = null;
        MainWindow mWin = MainWindow.AppWindow;

        public bool Connect()
        {
            try
            {
                conn = new MySqlConnection(Globals.connStr);
                conn.Open();

                //Выбираем БД с которой будем работать
                string query = "USE " + Settings.Default.dbName + ";";

                Query = new MySqlCommand(query, conn);
                
                //Отправляем запрос
                Query.ExecuteNonQuery();

                //Отмечаем в глобальной переменной что есть связь с БД
                Globals.isConnected = true;
                return true;
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
                //Отметим что нет соединения с БД
                Globals.isConnected = false;
                return false;
            }
        }

        //Метод выполняющий SQL запрос, не возвращает результата
        public void Execute(string query, bool update)
        {
            try
            {
                if (query != null)
                { 
                    Query = new MySqlCommand(query, conn);
                    Query.ExecuteNonQuery();
                }

                if (update == true)
                {
                    //Отправка запроса на обновление списка изданий из БД
                    query = "SELECT id,publication,is_magazine,date,issue_number,file_path FROM " + Settings.Default.dbTableName + ";";
                    MySqlDataAdapter dataAdapt = new MySqlDataAdapter(query, conn);
                    DataSet ds = new DataSet();
                    dataAdapt.Fill(ds, "dbBinding");
                    //Заполняем таблицу в редакторе
                    mWin.dbDataGrid.DataContext = ds;
                    //Обновим информацию о файлах для поиска
                    Initialize.UpdateFilesDescription();
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        //Метод выполняющий команду и возвращающий результат в виде массива
        public List<pdfDescription> ExecuteAndReadFilesDescription(string query)
        {
            try
            {
                if (query != null)
                {
                    Query = new MySqlCommand(query, conn);
                    Query.ExecuteNonQuery();
                    MySqlDataReader Reader;
                    Reader = Query.ExecuteReader();
                    //Cоздаем необходимые переменные
                    List<pdfDescription> newList = new List<pdfDescription>();

                    while (Reader.Read())
                    {
                        //Присвоими полученные от MySQL значения соответсвующему классу
                        pdfDescription curFile = new pdfDescription();

                        curFile.id = Convert.ToInt32(Reader.GetString(0));
                        curFile.publication = Reader.GetString(1);
                        curFile.is_magazine = Convert.ToBoolean(Reader.GetString(2));
                        curFile.date = Convert.ToDateTime(Reader.GetString(3));
                        curFile.issue_number = Convert.ToInt32(Reader.GetString(4));
                        curFile.file_path = Reader.GetString(5);

                        //И запишем данные в массив
                        newList.Add(curFile);
                    }
                    return newList;
                }
                else return null;
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public void Disconnect()
        {
            try
            {
                if (conn != null)
                {
                    //Закрываем соединение
                    conn.Close();
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                Query.Dispose();
                conn.Dispose();
            }
        }
    }
}
