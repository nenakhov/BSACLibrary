using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data;
using MySql.Data.MySqlClient;
using BSACLibrary.Properties;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            //Подключаемся к БД
            DBConnect.Connect();
        }
        //Обработка клика на выпадающем списке
        private void cBoxInput_MouseUp(object sender, MouseButtonEventArgs e)
        {
            //Проверяем изменялся ли текст после запуска программы
            if (cBoxInput.Text == "Поиск в каталоге. Например, \"802.11g\"")
            {
                //Делаем поле ввода пустым
                cBoxInput.Text = "";
                //Изменяем цвет текста в поле ввода по умолчанию на черный
                //SolidColorBrush закрашивает область сплошным цветом.
                cBoxInput.Foreground = new SolidColorBrush(Colors.Black); 
            }
        }

        private void OptionsWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открываем окно настроек
            OptionsWindow oWin = new OptionsWindow();
            oWin.Owner = this;
            //ShowDialog в отличии от Show позволяет запретить повторный запуск этого же окна
            oWin.ShowDialog();
        }

        private void AboutWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открытие окна "О программе"
            AboutWindow aWin = new AboutWindow();
            aWin.Owner = this;
            aWin.ShowDialog();
        }

        private void HelpFile_Open(object sender, RoutedEventArgs e)
        {
            //Ссылка на документ FAQ
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Закрываем программу
            App.Current.Shutdown();
        }
        //Реакция на нажатие клавиши в строке поиска
        private void cBoxInput_KeyDown(object sender, KeyEventArgs e)
        {
            //Проверяем какая клавиша была нажата
            //Если это Enter
            if (e.Key == Key.Return)
            {
                //Приступаем к поиску
                //Разворачиваем список
                cBoxInput.IsDropDownOpen = true;
                for (int i = 1; i <= 500; i++)
                {
                    Console.WriteLine("1");
                }   
            }
            else
            {
                //Иначе
            }
        }
    }
}
