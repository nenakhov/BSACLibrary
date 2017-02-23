using BSACLibrary.Properties;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text.RegularExpressions;
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
        int total = 0, current = 0;
        string substring = null, query = null;
        public List<pdfDescription> filesList = new List<pdfDescription>();
        List<string> foundedFiles = new List<string>();

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
            //ShowDialog в отличии от Show позволяет запретить повторный запуск этого же окна
            oWin.ShowDialog();
        }

        //Открытие окна "О программе" по нажатию соответствующего пункта меню
        private void AboutWindow_Open(object sender, RoutedEventArgs e)
        {
            AboutWindow aWin = new AboutWindow();
            aWin.ShowDialog();
        }

        //Вызов из файла помощи из меню программы
        private void HelpFile_Open(object sender, RoutedEventArgs e)
        {
            try
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo.FileName = AppDomain.CurrentDomain.BaseDirectory + "readme.pdf";
                proc.StartInfo.UseShellExecute = true;
                proc.Start();
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
                    addPublName.Text.Replace(@"'", @"\'") + "', '" + 
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
                    //Откроем соответствующий файл
                    System.Diagnostics.Process proc = new System.Diagnostics.Process();
                    proc.StartInfo.FileName = foundedFiles[searchListBox.SelectedIndex];
                    proc.StartInfo.UseShellExecute = true;
                    proc.Start();
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                searchListBox.SelectedIndex = -1;
            }
        }

        //Метод для очистки выпадающего списка при изменении размера окна
        private void Window_SizeChanged(object sender, EventArgs e)
        {
            //Избавимся от графических "артефактов"
            searchListBox.Items.Clear();
            searchListBox.Visibility = Visibility.Hidden;
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
                    foundedFiles.Clear();
                    //Задаем начальное значение переменных, поисковый запрос переведем в нижний регистр букв
                    total = filesList.Count();
                    substring = tBoxInput.Text.ToLower();

                    //Запуск поиска фоном, исключаем зависание GUI
                    Task.Factory.StartNew(() => //Источник https://msdn.microsoft.com/en-us/library/dd997392.aspx
                        //Многопоточный цикл foreach, использует все доступные ядра/потоки процессора
                        Parallel.ForEach(filesList, file =>
                        {
                            //Если в БД путь к файлу не задан пропустим его и перейдем к следующему
                            if (file.file_path.Length <= 0)
                            {
                                //Инкрементируем переменную отвечающую за количество пройденных файлом
                                Interlocked.Increment(ref current);
                                return;
                            }
                            //Поиск строки запроса в pdf файле
                            pdfSearchResponse searchResponse = PdfSearch.SearchInPdfFile(file.file_path, substring);
                            //Если строка нашлась
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
                                        //Добавляем найденный файл в результаты поиска
                                        //Его название и часть текста, начинающегося с поискового запроса
                                        txtBlock.Inlines.Add(file.publication + "\n");
                                        txtBlock.Inlines.Add(new Run(searchResponse.textCut + "...") { Foreground = Brushes.Gray, FontSize = 12 });
                                        searchListBox.Items.Add(txtBlock);
                                        //Так же добавим найденный файл в коллекцию
                                        foundedFiles.Add(file.file_path);
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
