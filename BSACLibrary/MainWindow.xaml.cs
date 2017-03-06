using BSACLibrary.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Создаем необходимые переменные
        public static MainWindow AppWindow;
        private int total, current;
        private string substring, query;
        private List<pdfDescription> filesList = new List<pdfDescription>();
        public List<pdfDescription> FilesList
        {
            get
            {
                return filesList;
            }
            set
            {
                filesList = value;
                //Очистим выпадающий список из уже существующих наименований во вкладке редактора
                addPublNameCmbBox.Items.Clear();
                magazinesNameListBox.Items.Clear();
                newspappersNameListBox.Items.Clear();

                magazinesNameListBox.Items.Add("<<<ВСЕ>>>");
                newspappersNameListBox.Items.Add("<<<ВСЕ>>>");

                //Cортировка всех названий по алфавиту
                filesList = filesList.OrderBy(x => x.publication_name).ToList();

                foreach (pdfDescription file in filesList)
                {
                    //Заполним список наименований во вкладке редактора заново
                    //Исключим повторяющиеся записи
                    if (addPublNameCmbBox.Items.Contains(file.publication_name) == false)
                    {
                        addPublNameCmbBox.Items.Add(file.publication_name);
                    }
                    //Заполним список изданий во вкладке ЖУРНАЛЫ
                    if (magazinesNameListBox.Items.Contains(file.publication_name) == false && file.is_magazine)
                    {
                        magazinesNameListBox.Items.Add(file.publication_name);
                    }
                    //Заполним список изданий во вкладке ГАЗЕТЫ
                    else if (newspappersNameListBox.Items.Contains(file.publication_name) == false && file.is_magazine == false)
                    {
                        newspappersNameListBox.Items.Add(file.publication_name);
                    }
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            //Инициализация подключения к БД и др. процессов в фоне
            Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart)delegate ()
                    {
                        Initialize.Init();
                    })
            );
        }

        private void OptionsWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открываем окно настроек
            OptionsWindow oWin = new OptionsWindow();
            oWin.Owner = this;
            //ShowDialog в отличии от Show позволяет запретить повторный запуск этого же окна
            oWin.ShowDialog();
        }

        //Открытие окна "О программе" по нажатию соответствующего пункта меню
        private void AboutWindow_Open(object sender, RoutedEventArgs e)
        {
            AboutWindow aWin = new AboutWindow();
            aWin.Owner = this;
            aWin.ShowDialog();
        }

        //Вызов из файла помощи из меню программы
        private void HelpFile_Open(object sender, RoutedEventArgs e)
        {
            try
            {
                OnNavigate(this, new RequestNavigateEventArgs(new Uri(AppDomain.CurrentDomain.BaseDirectory + "readme.pdf"), null));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
            else if (searchListBox.Items.Count > 0) searchListBox.Visibility = Visibility.Visible;
        }

        private void searchListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //Скрыть выпадающий список если мышь увели за его пределы
            searchListBox.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Закрываем программу
            Application.Current.Shutdown();
        }

        private void addEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(addPublNameCmbBox.Text) == false && string.IsNullOrEmpty(addDatePicker.Text) == false && string.IsNullOrEmpty(addIssueNmbTxtBox.Text) == false)
            {
                query = "INSERT INTO " + Settings.Default.dbTableName + " VALUES('" +
                    null + "', '" +
                    addPublNameCmbBox.Text.Replace(@"'", @"\'") + "', '" + 
                    Convert.ToInt16(addRadioBtnMagaz.IsChecked) + "', '" +
                    Convert.ToDateTime(addDatePicker.SelectedDate).ToString("yyyy-MM-dd") + "', '" + 
                    addIssueNmbTxtBox.Text + "', '" +
                    addFilePathTxtBox.Text.Replace(@"\", @"\\").Replace("'", "''") + 
                    "');";
                DBQueries.Execute(query);
                DBQueries.UpdateDataGrid();
            }
            else
            {
                MessageBox.Show("Поля не могут быть пустыми", "Заполните поля", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void addOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) addFilePathTxtBox.Text = openFileDialog.FileName;
        }

        //Обработка выбора строки в таблице редактора
        private void dbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dbDataGrid.SelectedIndex >= 0)
            {
                this.editEntryBtn.IsEnabled = true;
                this.delEntryBtn.IsEnabled = true;

                DataRowView row = dbDataGrid.SelectedItem as DataRowView;
                editIdTxtBox.Text = Convert.ToString(row[0]);
                editPublName.Text = Convert.ToString(row[1]);

                if (Convert.ToBoolean(row[2]) == true) editRadioBtnMagaz.IsChecked = true;
                else editRadioBtnNewsp.IsChecked = true;

                editDatePicker.SelectedDate = Convert.ToDateTime(row[3]);
                editIssueNmbTxtBox.Text = Convert.ToString(row[4]);
                editFilePathTxtBox.Text = Convert.ToString(row[5]);
            }
            else
            {
                this.editEntryBtn.IsEnabled = false;
                this.delEntryBtn.IsEnabled = false;
            }
        }

        private void editOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*";
            if (openFileDialog.ShowDialog() == true) editFilePathTxtBox.Text = openFileDialog.FileName;
        }

        private void delEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dbDataGrid.SelectedIndex >= 0)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены что хотите удалить запись? Действие необратимо.", "Удаление", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        query = "DELETE FROM " + Settings.Default.dbTableName +
                             " WHERE id = '" + editIdTxtBox.Text +
                             "';";
                        DBQueries.Execute(query);
                        DBQueries.UpdateDataGrid();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void editEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (dbDataGrid.SelectedIndex >= 0)
            {
                MessageBoxResult result = MessageBox.Show("Вы уверены что хотите изменить запись? Действие необратимо.", "Изменение", MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        query = "UPDATE " + Settings.Default.dbTableName +
                            " SET publication='" + editPublName.Text.Replace("'", "''") +
                            "',is_magazine='" + Convert.ToInt16(editRadioBtnMagaz.IsChecked) +
                            "',date='" + Convert.ToDateTime(editDatePicker.SelectedDate).ToString("yyyy-MM-dd") +
                            "',issue_number='" + editIssueNmbTxtBox.Text +
                            "',file_path='" + editFilePathTxtBox.Text.Replace(@"\", @"\\").Replace("'", "''") +
                            "' WHERE id='" + editIdTxtBox.Text + "';";
                        DBQueries.Execute(query);
                        DBQueries.UpdateDataGrid();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }
        
        //Обработка клика по окну программы
        private void Window_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            //Если выпадающий список результатов поиска был открыт, то закроем его.
            //Необходимо для того чтобы при щелчке на любом другом элементе программы список прятался
            if (searchListBox.IsMouseOver == false) searchListBox.Visibility = Visibility.Hidden;
        }

        //Обработка клика по выпадающему списку результатов поиска
        private void searchListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если был выбран любой элемент списка
            if (searchListBox.SelectedIndex >= 0)
            {
                try
                {
                    //Приводим объект из списку к типу pdfDescription
                    pdfDescription selectedFile = searchListBox.Items[searchListBox.SelectedIndex] as pdfDescription;
                    //Откроем соответствующий файл
                    OnNavigate(this, new RequestNavigateEventArgs(new Uri(selectedFile.file_path), null));
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                searchListBox.SelectedIndex = -1;
            }
        }

        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            //Откроем соответствующий файл
            Process proc = new Process();
            proc.StartInfo.FileName = e.Uri.AbsoluteUri;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();

            e.Handled = true;
        }

        private void newspappersNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (newspappersNameListBox.SelectedIndex == -1) return;

            newspappersYearListBox.Items.Clear();
            newspappersYearListBox.Items.Add("<<<ВСЕ>>>");
            //Cортировка по году выхода
            List<pdfDescription> sortedDate = filesList.OrderBy(x => x.date.Year).ToList();

            if (newspappersNameListBox.SelectedIndex == 0)
            {
                //ВСЕ
                foreach (pdfDescription file in sortedDate)
                {
                    if (file.is_magazine) continue;
                    if (newspappersYearListBox.Items.Contains(file.date.Year) == false)
                    {
                        newspappersYearListBox.Items.Add(file.date.Year);
                    }
                }
            }
            else
            {
                //Выбранная газета
                foreach (pdfDescription file in sortedDate)
                {
                    if (file.is_magazine) continue;
                    if (newspappersNameListBox.SelectedItem.ToString() == file.publication_name && newspappersYearListBox.Items.Contains(file.date.Year) == false)
                    {
                        newspappersYearListBox.Items.Add(file.date.Year);
                    }
                }
            }
        }

        private void newspappersYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (newspappersNameListBox.SelectedIndex == -1 || newspappersYearListBox.SelectedIndex == -1) return;

            newspappersWrap.Children.Clear();
            List<pdfDescription> sortedByNameAndNumber = filesList.OrderBy(x => x.publication_name).ThenBy(x => x.issue_number).ToList();
            //Все
            if (newspappersYearListBox.SelectedIndex == 0)
            {
                foreach (pdfDescription file in sortedByNameAndNumber)
                {
                    if (file.is_magazine) continue;
                    if (newspappersNameListBox.SelectedIndex != 0 && newspappersNameListBox.SelectedItem.ToString() != file.publication_name) continue;
                    TextBlock newTextBlock = new TextBlock();
                    Hyperlink newHyperLink = new Hyperlink();
                    newHyperLink.Inlines.Add(file.publication_name + " №" + file.issue_number + ";   ");
                    newHyperLink.NavigateUri = new Uri(file.file_path);
                    newHyperLink.RequestNavigate += OnNavigate;
                    newTextBlock.Inlines.Add(newHyperLink);
                    newspappersWrap.Children.Add(newTextBlock);
                }
            }
            //Выбранный год
            else
            {
                foreach (pdfDescription file in sortedByNameAndNumber)
                {
                    if (file.is_magazine) continue;
                    //Доработать
                    if (newspappersNameListBox.SelectedIndex != 0)
                    {
                        if (newspappersNameListBox.SelectedItem.ToString() != file.publication_name) continue;
                    }
                    if (newspappersYearListBox.SelectedItem.ToString() != file.date.Year.ToString()) continue;
                    TextBlock newTextBlock = new TextBlock();
                    Hyperlink newHyperLink = new Hyperlink();
                    newHyperLink.Inlines.Add(file.publication_name + " №" + file.issue_number + ";   ");
                    newHyperLink.NavigateUri = new Uri(file.file_path);
                    newHyperLink.RequestNavigate += OnNavigate;
                    newTextBlock.Inlines.Add(newHyperLink);
                    newspappersWrap.Children.Add(newTextBlock);
                }
            }
        }

        private void magazinesNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void magazinesYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        //Источник: http://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        //Регулярное выражение для проверки вводимых символов в поля редактора
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
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

                if (filesList == null)
                {
                    MessageBox.Show("В базе данных отсутствуют .pdf файлы.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                //Показываем анимацией что программа не зависла
                gifAnim.Visibility = Visibility.Visible;
                try
                {   
                    //Очистим коллекцию с путями к найденным файлам
                    //Задаем начальное значение переменных, поисковый запрос переведем в нижний регистр букв
                    total = filesList.Count();
                    substring = tBoxInput.Text.ToLower();

                    //Запуск поиска фоном, исключаем зависание GUI
                    Task.Factory.StartNew(() => //Источник https://msdn.microsoft.com/en-us/library/dd997392.aspx
                        //Многопоточный цикл foreach, использует все доступные ядра/потоки процессора
                        Parallel.ForEach(filesList, file =>
                        {
                            //Если в БД путь к файлу не задан пропустим его и перейдем к следующему
                            if (string.IsNullOrEmpty(file.file_path))
                            {
                                //Инкрементируем переменную отвечающую за количество пройденных файлом
                                Interlocked.Increment(ref current);
                                return;
                            }
                            //Поиск строки запроса в pdf файле
                            PdfSearch.SearchInPdfFile(file, substring);
                        //Если строка нашлась
                        if (string.IsNullOrEmpty(file.founded_text) == false)
                            {
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (ThreadStart)delegate ()
                                    {
                                        //Делаем выпадающий список видимым
                                        searchListBox.Visibility = Visibility.Visible;
                                        ////Добавляем найденный файл в результаты поиска
                                        ////Его название и часть текста, начинающегося с поискового запроса
                                        searchListBox.Items.Add(file);
                                    });
                            }
                            Interlocked.Increment(ref current);
                            Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                (ThreadStart)delegate ()
                                {
                                    //Если поиск завершился
                                    if (current == total)
                                    {
                                        //Обнуляем счетчки
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
                                            searchListBox.Items.Add("По вашему запросу ничего не найдено.");
                                        }
                                    }
                                });
                        })
                    );
                    //Установим фокус на выпадающий список результатов
                    FocusManager.SetFocusedElement(this, searchListBox);
                }
                catch (Exception ex)
                {
                    //Вывод сообщения о возникшей ошибке
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}
