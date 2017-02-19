using MySql.Data.MySqlClient;
using System.Windows;

namespace BSACLibrary
{
    public static class QueryExecute
    {
        public static void Connect(string query)
        {
            MySqlConnection conn = null;

            try
            {
                conn = new MySqlConnection(Globals.connStr);

                //С помощью этого объекта выполняются запросы к БД
                MySqlCommand Query = new MySqlCommand();

                //Присваиваем объекту созданное соединение
                Query.Connection = conn;

                //Устанавливаем соединение с БД
                conn.Open();

                Query.CommandText = query;
                //Отправляем запрос
                Query.ExecuteNonQuery();
            }
            //В этом блоке перехватываем возможные ошибки в процессе соединения
            catch (MySqlException ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
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
