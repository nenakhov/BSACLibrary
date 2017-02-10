using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

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

        private long total;
        private long current;
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
                //Разворачиваем список
                cBoxInput.IsDropDownOpen = true;
                cBoxInput.Items.Clear();
                //Приступаем к поиску
                string mask = "*.pdf"; //Ищем только .pdf файлы
                string source = @"\\192.168.1.1\Main\Transmission\Complete\Harry Potter 1-7 Reference Quality eBook Collection\"; //Путь к файлам
                //String source = @"D:\\Учеба\";
                gifAnim.Visibility = Visibility.Visible; //Показываем анимацией что программа не зависла
                try
                {
                    string[] files = Directory.GetFiles(source, mask, SearchOption.AllDirectories); //Записываем список всех файлов в массив

                    total = files.LongCount();
                    current = 0;

                    var watch = System.Diagnostics.Stopwatch.StartNew(); //Счетчик времени
                    //Запуск поиска фоном, исключаем зависание GUI
                    Task.Factory.StartNew(() => //Источник https://msdn.microsoft.com/en-us/library/dd997392.aspx
                        //Многопоточный цикл foreach, использует все доступные ядра/потоки процессора
                        Parallel.ForEach(files, file =>
                        {
                            try
                            {
                                //Console.WriteLine(new FileInfo(file).Name); //Имя файла 
                                //Console.WriteLine(file); //Полный путь к файлу
                                if (pdfSearch.SearchPdfFile(file, "Lupin") == true)
                                {
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate()
                                        {
                                            cBoxInput.Items.Add(new FileInfo(file).Name);
                                        });
                                    //Console.WriteLine(new FileInfo(file).Name + " Содержит");
                                }
                                    Interlocked.Increment(ref current);
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (ThreadStart)delegate()
                                    {
                                        if (current == total) //Прячем анимацию по завершению работы
                                        {
                                            gifAnim.Visibility = Visibility.Hidden;
                                            Console.WriteLine("Найдено " + files.Count() + " PDF файлов.");
                                            watch.Stop();
                                            var elapsedMs = watch.ElapsedMilliseconds;
                                            Console.WriteLine("Поиск занял " + (elapsedMs / 1000) + " секунд(у/ы)");
                                        }
                                    });
                            }
                            finally
                            {
                                //Interlocked.Increment(ref current);
                                //Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                //    (ThreadStart)delegate()
                                //{
                                //    //cBoxInput.Items.Add("Ищем...");
                                //    if (current == total) //Прячем анимацию по завершению работы
                                //    {
                                //        gifAnim.Visibility = Visibility.Hidden;
                                //        Console.WriteLine("Найдено " + files.Count() + " PDF файлов.");
                                //        watch.Stop();
                                //        var elapsedMs = watch.ElapsedMilliseconds;
                                //        Console.WriteLine("Поиск занял " + (elapsedMs / 1000) + " секунд(у/ы)");
                                //    }
                                //});
                            }
                        })
                    );
                }
                catch
                { }
            }
        }
    }
}
