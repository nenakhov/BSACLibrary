using BSACLibrary.Properties;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
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
        public static MainWindow AppWindow;
        //Зададим начальные значения для переменных
        private int total = 0, current = 0;
        private string substring = null, query = null;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            //Инициализация подключения к БД и др. процессов
            Initialize.Init();
        }
    private void OptionsWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открываем окно настроек
            OptionsWindow oWin = new OptionsWindow();
            //ShowDialog в отличии от Show позволяет запретить повторный запуск этого же окна
            oWin.ShowDialog();
        }

        private void AboutWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открытие окна "О программе"
            AboutWindow aWin = new AboutWindow();
            aWin.ShowDialog();
        }

        private void HelpFile_Open(object sender, RoutedEventArgs e)
        {
            //Ссылка на документ FAQ
        }
        //Обработка клика по полю ввода
        private void tBoxInput_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
             if (tBoxInput.Text == "Поиск информации по ключевому слову или фразе. Например, \"802.11ac\"")
                {
                    //Делаем поле ввода пустым
                    tBoxInput.Text = null;
                    //Изменяем цвет текста в поле ввода по умолчанию на черный
                    //SolidColorBrush закрашивает область сплошным цветом.
                    tBoxInput.Foreground = new SolidColorBrush(Colors.Black);
                }
            else if (searchListBox.Items.Count > 0)
                searchListBox.Visibility = Visibility.Visible;
        }

        private void searchListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //Скрыть если мышь увели за пределы
            searchListBox.Visibility = Visibility.Hidden;
        }
        private void Window_Closed(object sender, EventArgs e)
        {
            //Закрываем программу
            App.Current.Shutdown();
        }

        private void addEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (addPublName.Text != "" && addDatePicker.Text != "" && addIssueNmbTxtBox.Text != "")
            {
                query = "INSERT INTO " + Settings.Default.dbTableName + " VALUES('" +
                    null + "', '" +
                    addPublName.Text + "', '" + 
                    Convert.ToInt16(addRadioBtnMagaz.IsChecked) + "', '" +
                    Convert.ToDateTime(addDatePicker.SelectedDate).ToString("yyyy-MM-dd") + "', '" + 
                    addIssueNmbTxtBox.Text + "', '" +
                    addFilePathTxtBox.Text.Replace(@"\", @"\\") + 
                    "');";
                QueryExecute addEntry = new QueryExecute();
                if (addEntry.Connect() == true)
                {
                    addEntry.Execute(query, true);
                    addEntry.Disconnect();
                }
            }
            else
            {
                MessageBox.Show("Заполните поля");
            }
        }

        private void addOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            if (openFileDialog.ShowDialog() == true)
            {
                addFilePathTxtBox.Text = openFileDialog.FileName;
            }
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Maximized:
                    //Очистим выпадающий список при разворачивании окна
                    //Избавимся от графических "артефактов"
                    searchListBox.Items.Clear();
                    searchListBox.Visibility = Visibility.Hidden;
                    break;
                case WindowState.Minimized:
                    break;
                case WindowState.Normal:
                    break;
            }
    }

        //Реакция на нажатие клавиши в строке поиска
        private void tBoxInput_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Проверяем какая клавиша была нажата
            if ((e.Key == Key.Return) && (total == 0) && (current == 0))
            //Если это Enter и если поиск не выполняется в данный момент
            {
                if (tBoxInput.Text.Length < 3)
                {
                    tBoxInput.Text = "Минимальная длина поискового запроса - 3 символа";
                    return;
                }
                //Очищаем элементы списка
                searchListBox.Items.Clear();
                searchListBox.Visibility = Visibility.Hidden;
                //Приступаем к поиску
                string mask = "*.pdf"; //Ищем только .pdf файлы
                //string source = @"\\10.90.4.67\doc\Aurora описание\AuroraИнстрНарус\"; //Путь к файлам
                string source = @"\\192.168.1.1\Main\Transmission\Complete\Harry Potter 1-7 Reference Quality eBook Collection\"; //Путь к файлам
                //string source = @"D:/";
                //Показываем анимацией что программа не зависла
                gifAnim.Visibility = Visibility.Visible;
                try
                {
                    string[] files = Directory.GetFiles(source, mask, SearchOption.AllDirectories); //Записываем список всех файлов в массив

                    total = files.Count();
                    substring = tBoxInput.Text.ToLower();

                    //Запуск поиска фоном, исключаем зависание GUI
                    Task.Factory.StartNew(() => //Источник https://msdn.microsoft.com/en-us/library/dd997392.aspx
                                                //Многопоточный цикл foreach, использует все доступные ядра/потоки процессора
                        Parallel.ForEach(files, file =>
                        {
                            //Console.WriteLine(new FileInfo(file).Name); //Имя файла 
                            //Console.WriteLine(file); //Полный путь к файлу

                            pdfSearchResponse searchResponse = pdfSearch.SearchPdfFile(file, substring);
                            if (searchResponse.isFinded == true)
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (ThreadStart)delegate ()
                                    {
                                        //Делаем выпадающий список видимым
                                        searchListBox.Visibility = Visibility.Visible;
                                        //Форматирование текста, перенос строк
                                        TextBlock txtBlock = new TextBlock();
                                        txtBlock.MaxWidth = this.ActualWidth - 75;
                                        txtBlock.TextWrapping = TextWrapping.Wrap;
                                        //Добавляем элемент
                                        txtBlock.Inlines.Add(new FileInfo(file).Name + "\n");
                                        txtBlock.Inlines.Add(new Run(searchResponse.textCut + "...") { Foreground = Brushes.Gray, FontSize = 12 });
                                        searchListBox.Items.Add(txtBlock);
                                    });
                            }
                            Interlocked.Increment(ref current);
                            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    //Если поиск завершился
                                    if (current == total)
                                    {
                                        //Обнуляем счетчики
                                        total = 0;
                                        current = 0;
                                        substring = null;
                                        //Прячем анимацию по завершению работы
                                        gifAnim.Visibility = Visibility.Hidden;
                                        //Если ничего не найдено
                                        if (searchListBox.Items.Count == 0)
                                        {
                                            //Добавляем элемент
                                            searchListBox.Visibility = Visibility.Visible;
                                            searchListBox.Items.Add("Совпадений нет.");
                                        }
                                    }
                                });
                        })
                    );
                }
                catch (Exception ex)
                {
                    //Игнорируем возможные исключения
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
