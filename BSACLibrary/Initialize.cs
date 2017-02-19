using BSACLibrary.Properties;
using System.Windows;
using System.Windows.Controls;

namespace BSACLibrary
{
    public static class Initialize
    {
        public static void Init()
        {
            if ((Settings.Default.dbUsername != "") && (Settings.Default.dbPassword != ""))
                {
                    //Подключаемся к БД если заданы настройки
                    DBConnect.Connect();
                }

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
            }
        }
    }
}
