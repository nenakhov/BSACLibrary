using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

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

                String mask = "*.pdf";
                String source = @"\\192.168.1.1\Main\Transmission\Complete\Harry Potter 1-7 Reference Quality eBook Collection\";
                //String source = @"D:\\Учеба\";
                try
                {
                    String[] files = Directory.GetFiles(source, mask, SearchOption.AllDirectories);

                    var watch = System.Diagnostics.Stopwatch.StartNew();

                    Parallel.ForEach(files, (string file, ParallelLoopState state) =>
                    {
                        //Console.WriteLine(new FileInfo(file).Name); //имя файла 
                        //Console.WriteLine(file); //путь + имя 
                        if (pdfSearch.SearchPdfFile(file, "Lupin called") == true)
                        {
                            Console.WriteLine(new FileInfo(file).Name + " Содержит"); //имя файла 
                                                                                      //state.Stop(); 
                        }
                    });

                    Console.WriteLine("Найдено " + files.Count() + " PDF файлов.");
                    watch.Stop();
                    var elapsedMs = watch.ElapsedMilliseconds;
                    Console.WriteLine("Поиск занял " + (elapsedMs / 1000) + " секунд");
                }
                catch
                { }

            //Разворачиваем список
            cBoxInput.IsDropDownOpen = true;
            }
            else
            {
                //Иначе
            }
        }
    }
}
