using BSACLibrary.Properties;
using System.Windows;
using System.Windows.Controls;

namespace BSACLibrary
{
    public static class Initialize
    {
        public static void Init()
        {
            //Переопределим глобальные переменные
            Globals.SetConnStr();

            //Если не включен режим администратора спрячем вкладку редактора
            MainWindow mWin = MainWindow.AppWindow;

            if (Settings.Default.isAdmin == false)
            {
                Style style = mWin.FindResource("RadioRightCorner") as Style;

                mWin.EditBtn.Visibility = Visibility.Hidden;
                mWin.NewspapersBtn.Style = style;
                mWin.MagazinesBtn.IsChecked = true;

                Grid.SetColumn(mWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(mWin.MagazinesBtn, 3);
                Grid.SetColumn(mWin.NewspapersBtn, 3);
                Grid.SetColumnSpan(mWin.NewspapersBtn, 3);
            }
            else
            {
                //Либо же наоборот, отобразим ее по умолчанию
                Style style = mWin.FindResource("RadioNormalCorner") as Style;

                mWin.EditBtn.Visibility = Visibility.Visible;
                mWin.EditBtn.IsChecked = true;
                mWin.NewspapersBtn.Style = style;

                Grid.SetColumn(mWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(mWin.MagazinesBtn, 2);
                Grid.SetColumn(mWin.NewspapersBtn, 2);
                Grid.SetColumnSpan(mWin.NewspapersBtn, 2);

                QueryExecute addEntry = new QueryExecute();
                if (addEntry.Connect() == true)
                {
                    //Подключаемся к БД если заданы настройки
                    if ((Settings.Default.dbUsername != "") && (Settings.Default.dbPassword != ""))
                    {
                        //Если выбран режим администратора создаем БД и таблицу в ней, если они еще не были созданы
                        string query;
                        //Создаем базу данных
                        query = "CREATE DATABASE IF NOT EXISTS " + // Если такая БД уже есть не будет ошибки
                            "`" + Settings.Default.dbName +
                            "` CHARACTER SET cp1251 COLLATE cp1251_general_ci;";
                        //Отправляем запрос
                        addEntry.Execute(query, false);
                        //Выбираем БД с которой будем работать
                        query = "USE " + Settings.Default.dbName + ";";
                        //Отправляем запрос
                        addEntry.Execute(query, false);
                        //Запрос создание таблицы
                        query = "CREATE TABLE IF NOT EXISTS `" + //Если такая таблица уже есть не будет ошибки
                                            Settings.Default.dbTableName + //Имя таблицы 
                                            "` (" +
                                            "`id` INT(6) NOT NULL AUTO_INCREMENT, " + //Автоинкрементирующееся поле id 
                                            "`publication` VARCHAR(255) NOT NULL, " + //Название журнала
                                            "`is_magazine` TINYINT(1) NOT NULL, " + //Является ли журналом или газетой
                                            "`date` DATE NOT NULL, " + //Дата издания
                                            "`issue_number` INT(6) NOT NULL, " + //Порядковый номер издания
                                            "`file_path` VARCHAR(255) NOT NULL, " + //Ссылка на файл \\host\Lib\file.pdf 255 кол-во символов
                                            "PRIMARY KEY(`id`) " +
                                            ") ";
                        //Отправляем запрос
                        addEntry.Execute(query, false);
                        //Отправляем запрос
                    }
                    addEntry.Execute(null, true);
                    //Закрываем соединение
                    addEntry.Disconnect();
                }
            }
        }
    }
}
