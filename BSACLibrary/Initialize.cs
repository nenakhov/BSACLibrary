using BSACLibrary.Properties;
using System;
using System.Windows;
using System.Windows.Controls;

namespace BSACLibrary
{
    public static class Initialize
    {
        private static readonly MainWindow MWin = MainWindow.AppWindow;

        public static void Init()
        {
            //Если не заданы настройки подключения
            if (string.IsNullOrEmpty(Settings.Default.dbUsername) || string.IsNullOrEmpty(Settings.Default.dbPassword)) return;
                
            //Если включен режим администратора
            if (Settings.Default.isAdmin)
            {
                //Отобразим вкладку редактора
                var style = MWin.FindResource("RadioNormalCorner") as Style;

                MWin.EditBtn.Visibility = Visibility.Visible;
                MWin.EditBtn.IsChecked = true;
                MWin.NewspapersBtn.Style = style;

                Grid.SetColumn(MWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(MWin.MagazinesBtn, 2);
                Grid.SetColumn(MWin.NewspapersBtn, 2);
                Grid.SetColumnSpan(MWin.NewspapersBtn, 2);
                //Создаем БД и таблицу в ней, если они еще не были созданы
                try
                {
                    //Отправляем запрос на создание БД и таблицы в ней
                    DbQueries.DataBaseCreate();
                    //Отправляем запрос на обновление таблицы
                    DbQueries.UpdateDataGrid();
                }
                catch (Exception ex)
                {
                    //Unable to connect to any of the specified MySQL hosts.
                    MessageBox.Show(
                        ex.Message == "Unable to connect to any of the specified MySQL hosts."
                            ? "Нет соединения с сервером MySQL"
                            : ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            //Иначе спрячем вкладку редактора и применим соответствующее оформление
            else
            {
                var style = MWin.FindResource("RadioRightCorner") as Style;

                MWin.EditBtn.Visibility = Visibility.Hidden;
                MWin.NewspapersBtn.Style = style;
                MWin.MagazinesBtn.IsChecked = true;

                Grid.SetColumn(MWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(MWin.MagazinesBtn, 3);
                Grid.SetColumn(MWin.NewspapersBtn, 3);
                Grid.SetColumnSpan(MWin.NewspapersBtn, 3);
            }
            //Обновим массив со списком pdf файлов
            UpdateFilesDescriptions();
        }
        public static void UpdateFilesDescriptions()
        {
            //Выбираем из БД путь ко всем имеющимся pdf файлам
            try
            {
                //Записываем список всех файлов в массив
                MWin.FilesList = DbQueries.ExecuteAndReadFilesDescription("SELECT * FROM " + Settings.Default.dbTableName + ";");
            }
            catch (Exception ex)
            {
                //Unable to connect to any of the specified MySQL hosts.
                MessageBox.Show(
                    ex.Message == "Unable to connect to any of the specified MySQL hosts."
                        ? "Нет соединения с сервером MySQL"
                        : ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
