using BSACLibrary.Properties;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BSACLibrary
{
    public static class Initialize
    {
        private static MainWindow mWin = MainWindow.AppWindow;

        public static void Init()
        {
            //Переопределим глобальные переменные
            Globals.SetConnectionString();

            //Если не включен режим администратора спрячем вкладку редактора
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

                //Если выбран режим администратора создаем БД и таблицу в ней, если они еще не были созданы
                //Подключаемся к БД если заданы настройки
                if ((Settings.Default.dbUsername != "") && (Settings.Default.dbPassword != ""))
                {
                    try
                    {
                        //Отправляем запрос на создание БД и таблицы в ней
                        DBQueries.DataBaseCreate();
                        //Отправляем запрос на обновление таблицы
                        DBQueries.UpdateDataGrid();
                    }
                    catch (Exception ex)
                    {
                        //Unable to connect to any of the specified MySQL hosts.
                        if (ex.Message == "Unable to connect to any of the specified MySQL hosts.")
                        {
                            MessageBox.Show("Нет соединения с сервером MySQL", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        return;
                    }
                }
            }
            //Обновим массив со списком pdf файлов
            UpdateFilesDescription();
        }
        public static void UpdateFilesDescription()
        {
            //Выбираем из БД путь ко всем имеющимся pdf файлам
            try
            { 
                //Записываем список всех файлов в массив
                mWin.filesList = DBQueries.ExecuteAndReadFilesDescription("SELECT * FROM " + Settings.Default.dbTableName + ";");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
