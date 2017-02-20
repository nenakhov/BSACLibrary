using BSACLibrary.Properties;
using MySql.Data.MySqlClient;
using System.Data;
using System.Windows;

namespace BSACLibrary
{
    public class QueryExecute
    {
        private MySqlConnection conn = null;
        private MySqlCommand Query = null;
        MainWindow mWin = MainWindow.AppWindow;

        public bool Connect()
        {
            try
            {
                conn = new MySqlConnection(Globals.connStr);
                conn.Open();

                Query = new MySqlCommand();
                Query.Connection = conn;
                //Выбираем БД с которой будем работать
                Query.CommandText = "USE " + Settings.Default.dbName + ";";
                //Отправляем запрос
                Query.ExecuteNonQuery();

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
                return false;
            }
        }

        public void Execute(string query, bool update)
        {
            try
            {
                if (query != null)
                { 
                    Query = new MySqlCommand();
                    Query.Connection = conn;
                    Query.CommandText = query;
                    Query.ExecuteNonQuery();
                }

                if (update == true)
                {
                    query = "SELECT id,publication,is_magazine,date,issue_number,file_path FROM " + Settings.Default.dbTableName + ";";
                    MySqlDataAdapter dataAdapt = new MySqlDataAdapter(query, conn);
                    DataSet ds = new DataSet();
                    dataAdapt.Fill(ds, "dbBinding");

                    mWin.dbDataGrid.DataContext = ds;
                }
            }
            //В этом блоке перехватываем возможные ошибки
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
    }
}
