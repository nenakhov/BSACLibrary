using BSACLibrary.Properties;
using System.Windows;
using System.Windows.Controls;

namespace BSACLibrary
{
    public static class Initialize
    {
        public static void Init()
        {
            //Подключаемся к БД если заданы настройки
            DBConnect.Connect();

            //Если не включен режим администратора спрячем вкладку редактора
            MainWindow mWin = MainWindow.AppWindow;

            if (Settings.Default.isAdmin == false)
            {
                Style style = mWin.FindResource("RadioRightCorner") as Style;

                mWin.EditBtn.Visibility = Visibility.Hidden;
                mWin.NewspapersBtn.Style = style;

                Grid.SetColumn(mWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(mWin.MagazinesBtn, 3);
                Grid.SetColumn(mWin.NewspapersBtn, 3);
                Grid.SetColumnSpan(mWin.NewspapersBtn, 3);
            }
            else
            {
                Style style = mWin.FindResource("RadioNormalCorner") as Style;

                mWin.EditBtn.Visibility = Visibility.Visible;
                mWin.NewspapersBtn.Style = style;

                Grid.SetColumn(mWin.MagazinesBtn, 0);
                Grid.SetColumnSpan(mWin.MagazinesBtn, 2);
                Grid.SetColumn(mWin.NewspapersBtn, 2);
                Grid.SetColumnSpan(mWin.NewspapersBtn, 2);
            }
        }
    }
}
