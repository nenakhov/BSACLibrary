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
            //Если заданы настройки подключения
            if ((string.IsNullOrEmpty(Settings.Default.dbUsername) == false) && string.IsNullOrEmpty(Settings.Default.dbPassword) == false)
            {
                //Если включен режим администратора
                if (Settings.Default.isAdmin == true)
                {
                    //Отобразим вкладку редактора
                    Style style = mWin.FindResource("RadioNormalCorner") as Style;

                    mWin.EditBtn.Visibility = Visibility.Visible;
                    mWin.EditBtn.IsChecked = true;
                    mWin.NewspapersBtn.Style = style;

                    Grid.SetColumn(mWin.MagazinesBtn, 0);
                    Grid.SetColumnSpan(mWin.MagazinesBtn, 2);
                    Grid.SetColumn(mWin.NewspapersBtn, 2);
                    Grid.SetColumnSpan(mWin.NewspapersBtn, 2);
                    //Создаем БД и таблицу в ней, если они еще не были созданы
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
                //Иначе спрячем вкладку редактора и применим соответствующее оформление
                else
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
                try
                {
                    //Обновим массив со списком pdf файлов
                    UpdateFilesDescription();
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
