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
using System.Windows.Navigation;
using System.Windows.Threading;
using BSACLibrary.Properties;
using Microsoft.Win32;

namespace BSACLibrary
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        //Создаем необходимые переменные
        public static MainWindow AppWindow;
        private List<PdfDescription> _filesList = new List<PdfDescription>();
        private string _substring, _query;
        private int _total, _current;

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            //Инициализация подключения к БД и др. процессов в фоне
            Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                    (ThreadStart) Initialize.Init)
            );
        }

        public List<PdfDescription> FilesList
        {
            set
            {
                _filesList = value;
                //Очистим выпадающий список из уже существующих наименований во вкладке редактора
                AddPublNameCmbBox.Items.Clear();
                //Очистим списки во вкладках газеты/журналы
                MzNameListBox.Items.Clear();
                MzYearListBox.Items.Clear();

                NpNameListBox.Items.Clear();
                NpYearListBox.Items.Clear();
                //Обнулим строки результатов поиска
                MzLabel.Content = null;
                MzWrapPanel.Children.Clear();

                NpLabel.Content = null;
                NpWrapPanel.Children.Clear();

                //Добавим по одному элементу "ВСЕ" в каждый список
                MzNameListBox.Items.Add("<<<ВСЕ>>>");
                NpNameListBox.Items.Add("<<<ВСЕ>>>");
                //Выделим его по умолчанию
                MzNameListBox.SelectedIndex = 0;
                NpNameListBox.SelectedIndex = 0;

                //Cортировка всех названий по алфавиту
                _filesList = _filesList.OrderBy(x => x.PublicationName).ToList();

                foreach (var file in _filesList)
                {
                    //Заполним список наименований во вкладке редактора заново
                    //Исключим повторяющиеся записи
                    if (AddPublNameCmbBox.Items.Contains(file.PublicationName) == false)
                        AddPublNameCmbBox.Items.Add(file.PublicationName);
                    //Заполним список изданий во вкладке ЖУРНАЛЫ
                    if (file.IsMagazine)
                    {
                        if (MzNameListBox.Items.Contains(file.PublicationName) == false)
                            MzNameListBox.Items.Add(file.PublicationName);
                    }
                    //Заполним список изданий во вкладке ГАЗЕТЫ
                    else
                    {
                        if (NpNameListBox.Items.Contains(file.PublicationName) == false)
                            NpNameListBox.Items.Add(file.PublicationName);
                    }
                }
            }
        }

        private void OptionsWindow_Open(object sender, RoutedEventArgs e)
        {
            //Открываем окно настроек
            var oWin = new OptionsWindow {Owner = this};
            //ShowDialog в отличии от Show позволяет запретить повторный запуск этого же окна
            oWin.ShowDialog();
        }

        //Открытие окна "О программе" по нажатию соответствующего пункта меню
        private void AboutWindow_Open(object sender, RoutedEventArgs e)
        {
            var aWin = new AboutWindow {Owner = this};
            aWin.ShowDialog();
        }

        //Вызов из файла помощи из меню программы
        private void HelpFile_Open(object sender, RoutedEventArgs e)
        {
            try
            {
                OnNavigate(this,
                    new RequestNavigateEventArgs(new Uri(AppDomain.CurrentDomain.BaseDirectory + "readme.pdf"), null));
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        //Обработка клика по полю ввода
        private void SrchTxtBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SearchListBox.Items.Count > 0)
                SearchListBox.Visibility = Visibility.Visible;
        }

        private void searchListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //Скрыть выпадающий список если мышь увели за его пределы
            SearchListBox.Visibility = Visibility.Hidden;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            //Закрываем программу
            Application.Current.Shutdown();
        }

        private void AddEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(AddPublNameCmbBox.Text) == false &&
                string.IsNullOrEmpty(AddDatePicker.Text) == false &&
                string.IsNullOrEmpty(AddIssueNmbTxtBox.Text) == false)
            {
                _query = "INSERT INTO " + Settings.Default.dbTableName + " VALUES('" +
                         null + "', '" +
                         AddPublNameCmbBox.Text.Replace(@"'", @"\'") + "', '" +
                         Convert.ToInt16(AddRadioBtnMagaz.IsChecked) + "', '" +
                         Convert.ToDateTime(AddDatePicker.SelectedDate).ToString("yyyy-MM-dd") + "', '" +
                         AddIssueNmbTxtBox.Text + "', '" +
                         AddFilePathTxtBox.Text.Replace(@"\", @"\\").Replace("'", "''") +
                         "');";
                DbQueries.Execute(_query);
                DbQueries.UpdateDataGrid();
            }
            else
            {
                MessageBox.Show("Поля не могут быть пустыми", "Заполните поля", MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void AddOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"};
            if (openFileDialog.ShowDialog() == true)
                AddFilePathTxtBox.Text = openFileDialog.FileName;
        }

        //Обработка выбора строки в таблице редактора
        private void DbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DbDataGrid.SelectedIndex >= 0)
            {
                EditEntryBtn.IsEnabled = true;
                DelEntryBtn.IsEnabled = true;

                var row = DbDataGrid.SelectedItem as DataRowView;
                if (row != null)
                {
                    EditIdTxtBox.Text = Convert.ToString(row[0]);
                    EditPublName.Text = Convert.ToString(row[1]);

                    if (Convert.ToBoolean(row[2]))
                        EditRadioBtnMagaz.IsChecked = true;
                    else
                        EditRadioBtnNewsp.IsChecked = true;

                    EditDatePicker.SelectedDate = Convert.ToDateTime(row[3]);
                    EditIssueNmbTxtBox.Text = Convert.ToString(row[4]);
                    EditFilePathTxtBox.Text = Convert.ToString(row[5]);
                }
            }
            else
            {
                EditEntryBtn.IsEnabled = false;
                DelEntryBtn.IsEnabled = false;
            }
        }

        private void EditOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog {Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*"};
            if (openFileDialog.ShowDialog() == true)
                EditFilePathTxtBox.Text = openFileDialog.FileName;
        }

        private void DelEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DbDataGrid.SelectedIndex >= 0)
            {
                var result = MessageBox.Show("Вы уверены что хотите удалить запись? Действие необратимо.", "Удаление",
                    MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _query = "DELETE FROM " + Settings.Default.dbTableName +
                                 " WHERE id = '" + EditIdTxtBox.Text +
                                 "';";
                        DbQueries.Execute(_query);
                        DbQueries.UpdateDataGrid();
                        break;
                    case MessageBoxResult.No:
                        break;
                }
            }
        }

        private void EditEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DbDataGrid.SelectedIndex >= 0)
            {
                var result = MessageBox.Show("Вы уверены что хотите изменить запись? Действие необратимо.", "Изменение",
                    MessageBoxButton.YesNo);
                switch (result)
                {
                    case MessageBoxResult.Yes:
                        _query = "UPDATE " + Settings.Default.dbTableName +
                                 " SET publication='" + EditPublName.Text.Replace("'", "''") +
                                 "',is_magazine='" + Convert.ToInt16(EditRadioBtnMagaz.IsChecked) +
                                 "',date='" + Convert.ToDateTime(EditDatePicker.SelectedDate).ToString("yyyy-MM-dd") +
                                 "',issue_number='" + EditIssueNmbTxtBox.Text +
                                 "',file_path='" + EditFilePathTxtBox.Text.Replace(@"\", @"\\").Replace("'", "''") +
                                 "' WHERE id='" + EditIdTxtBox.Text + "';";
                        DbQueries.Execute(_query);
                        DbQueries.UpdateDataGrid();
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
            if (SearchListBox.IsMouseOver == false)
                SearchListBox.Visibility = Visibility.Hidden;
        }

        //Обработка клика по выпадающему списку результатов поиска
        private void SearchListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если был выбран любой элемент списка
            if (SearchListBox.SelectedIndex >= 0)
            {
                try
                {
                    //Приводим объект из списку к типу pdfDescription
                    var selectedFile = SearchListBox.Items[SearchListBox.SelectedIndex] as PdfDescription;
                    //Откроем соответствующий файл
                    if (selectedFile != null)
                        OnNavigate(this, new RequestNavigateEventArgs(new Uri(selectedFile.FilePath), null));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                SearchListBox.SelectedIndex = -1;
            }
        }

        private static void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                //Откроем соответствующий файл
                var proc = new Process
                {
                    StartInfo =
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    }
                };
                proc.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }

        private void NpNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбрано ниодного элемента в списке
            if (NpNameListBox.SelectedIndex == -1)
                return;
            //Очистим список годов выхода
            NpYearListBox.Items.Clear();
            NpYearListBox.Items.Add("<<<ВСЕ>>>");
            //Cортировка по году выхода
            var sortedDate = _filesList.OrderBy(x => x.Date.Year).ToList();

            foreach (var file in sortedDate)
            {
                //Если является журналом перейдем к следующей итерации
                if (file.IsMagazine)
                    continue;
                //Если выбрали ВСЕ в названии издания
                if (NpNameListBox.SelectedIndex == 0 &&
                    //Если этот год еще не добавили в список
                    NpYearListBox.Items.Contains(file.Date.Year) == false)
                    NpYearListBox.Items.Add(file.Date.Year);
                //Если выбрана определенная газета
                else if (NpNameListBox.SelectedItem.ToString() == file.PublicationName &&
                         //И такой год еще не добавляли
                         NpYearListBox.Items.Contains(file.Date.Year) == false)
                    NpYearListBox.Items.Add(file.Date.Year);
            }
            //Выбираем ВСЕ в списке по умолчанию
            NpYearListBox.SelectedIndex = 0;
        }

        private void NpYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбранно ниодного элемента в списке
            if (NpNameListBox.SelectedIndex == -1 || NpYearListBox.SelectedIndex == -1)
                return;
            //Очистим панель со списком газет.
            NpWrapPanel.Children.Clear();
            //Отсортируем список по имени и номеру
            var sortedByNameAndNmb = _filesList.OrderBy(x => x.PublicationName).ThenBy(x => x.IssueNumber).ToList();
            var i = 0;

            foreach (var file in sortedByNameAndNmb)
            {
                //Если не является газетой пропустим текущую итерацию
                if (file.IsMagazine)
                    continue;

                //Если выбрано какое-то издание, не ВСЕ
                if (NpNameListBox.SelectedIndex != 0 &&
                    //И если выбранное издание != file.publication_name пропустим текущую итерацию
                    NpNameListBox.SelectedItem.ToString() != file.PublicationName)
                    continue;

                //Или если выбранный год != file.date.Year пропустим текущую итерацию
                if (NpYearListBox.SelectedIndex != 0 &&
                    NpYearListBox.SelectedItem.ToString() != file.Date.Year.ToString())
                    continue;
                //Заполним панель полученными гиперссылками
                NpWrapPanel.Children.Add(AddTextBlock(file.PublicationName, file.FilePath, file.IssueNumber));
                //Инкрементируем кол-во найденных записей
                i++;
            }
            if (i > 0)
                NpLabel.Content = "Всего " + i + " номер(а, ов).";
            else
                NpLabel.Content = "В базе данных отсутсвуют сведения.";
        }

        private void MzNameListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбрано ниодного элемента в списке
            if (MzNameListBox.SelectedIndex == -1)
                return;
            //Очистим список годов выхода
            MzYearListBox.Items.Clear();
            MzYearListBox.Items.Add("<<<ВСЕ>>>");
            //Cортировка по году выхода
            var sortedDate = _filesList.OrderBy(x => x.Date.Year).ToList();

            foreach (var file in sortedDate)
            {
                //Если является не журналом перейдем к следующей итерации
                if (file.IsMagazine == false)
                    continue;
                //Если выбрали ВСЕ в названии издания
                if (MzNameListBox.SelectedIndex == 0 &&
                    //Если этот год еще не добавили в список
                    MzYearListBox.Items.Contains(file.Date.Year) == false)
                    MzYearListBox.Items.Add(file.Date.Year);
                //Если выбрана определенная газета
                else if (MzNameListBox.SelectedItem.ToString() == file.PublicationName &&
                         //И такой год еще не добавляли
                         MzYearListBox.Items.Contains(file.Date.Year) == false)
                    MzYearListBox.Items.Add(file.Date.Year);
            }
            //Выбираем ВСЕ в списке по умолчанию
            MzYearListBox.SelectedIndex = 0;
        }

        private void MzYearListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбранно ниодного элемента в списке
            if (MzNameListBox.SelectedIndex == -1 || MzYearListBox.SelectedIndex == -1)
                return;
            //Очистим панель со списком газет.
            MzWrapPanel.Children.Clear();
            //Отсортируем список по имени и номеру
            var sortedByNameAndNmb = _filesList.OrderBy(x => x.PublicationName).ThenBy(x => x.IssueNumber).ToList();
            var i = 0;

            foreach (var file in sortedByNameAndNmb)
            {
                //Если не является журналом пропустим текущую итерацию
                if (file.IsMagazine == false)
                    continue;

                //Если выбрано какое-то издание, не ВСЕ
                if (MzNameListBox.SelectedIndex != 0 &&
                    //И если выбранное издание != file.publication_name пропустим текущую итерацию
                    MzNameListBox.SelectedItem.ToString() != file.PublicationName)
                    continue;

                //Или если выбранный год != file.date.Year пропустим текущую итерацию
                if (MzYearListBox.SelectedIndex != 0 &&
                    MzYearListBox.SelectedItem.ToString() != file.Date.Year.ToString())
                    continue;
                //Заполним панель полученными гиперссылками
                MzWrapPanel.Children.Add(AddTextBlock(file.PublicationName, file.FilePath, file.IssueNumber));
                //Инкрементируем кол-во найденных записей
                i++;
            }
            if (i > 0)
                MzLabel.Content = "Всего " + i + " номер(а, ов).";
            else
                MzLabel.Content = "В базе данных отсутсвуют записи.";
        }

        //Метод формирующий текстовые блоки с названием и номером издания
        private TextBlock AddTextBlock(string publicationName, string filePath, int issueNumber)
        {
            //Cформируемый текстовый блок с названием издания и его номером
            var newTextBlock = new TextBlock();
            var addString = publicationName + " №" + issueNumber + ";    ";
            //При наличии .pdf создаем гиперссылку на  файл
            if (string.IsNullOrEmpty(filePath) == false)
            {
                var newHyperLink = new Hyperlink();
                newHyperLink.Inlines.Add(addString);
                newHyperLink.NavigateUri = new Uri(filePath);
                newHyperLink.RequestNavigate += OnNavigate;
                newTextBlock.Inlines.Add(newHyperLink);
            }
            else
            {
                //Если к файлу не указан путь => соответственно он не был оцифрован
                newTextBlock.ToolTip = "Нет цифровой копии";
                newTextBlock.Inlines.Add(addString);
            }
            return newTextBlock;
        }

        //Источник: http://stackoverflow.com/questions/1268552/how-do-i-get-a-textbox-to-only-accept-numeric-input-in-wpf
        //Регулярное выражение для проверки вводимых символов в поля редактора
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            var regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Реакция на нажатие клавиши в строке поиска
        private void SrchTxtBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            //Проверяем какая клавиша была нажата
            if (e.Key == Key.Return && _total == 0 && _current == 0)
                //Если это Enter и если поиск не выполняется в данный момент
            {
                if (SrchTxtBox.Text.Length < 3)
                {
                    //Минимальная длина поискового запроса -3 символа
                    PpSrchTxtBox.IsOpen = true;
                    return;
                }
                //Очищаем элементы списка
                SearchListBox.Items.Clear();
                SearchListBox.Visibility = Visibility.Hidden;

                if (_filesList == null)
                {
                    MessageBox.Show("В базе данных отсутствуют .pdf файлы.", "Ошибка", MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    return;
                }

                //Показываем анимацией что программа не зависла
                GifAnim.Visibility = Visibility.Visible;
                try
                {
                    //Задаем начальное значение переменных, поисковый запрос переведем в нижний регистр букв
                    _total = _filesList.Count;
                    _substring = SrchTxtBox.Text.ToLower();

                    //Запуск поиска фоном, исключаем зависание GUI
                    Task.Factory.StartNew(() => //Источник https://msdn.microsoft.com/en-us/library/dd997392.aspx
                        //Многопоточный цикл foreach, использует все доступные ядра/потоки процессора
                            Parallel.ForEach(_filesList, file =>
                            {
                                //Если в БД путь к файлу не задан пропустим его и перейдем к следующему
                                if (string.IsNullOrEmpty(file.FilePath))
                                {
                                    //Инкрементируем переменную отвечающую за количество уже просмотренных файлов
                                    Interlocked.Increment(ref _current);
                                    //return = continue в случае с Parallel.ForEach
                                    return;
                                }
                                //Поиск строки запроса в pdf файле
                                PdfSearch.SearchInPdfFile(file, _substring);
                                //Если строка нашлась
                                if (string.IsNullOrEmpty(file.FoundedText) == false)
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart) delegate
                                        {
                                            //Откроем выпадающий список
                                            SearchListBox.Visibility = Visibility.Visible;
                                            //Добавим в результаты поиска название издания
                                            //И часть текста, начинающегося с поискового запроса
                                            SearchListBox.Items.Add(file);
                                        });
                                Interlocked.Increment(ref _current);
                                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                    (ThreadStart) delegate
                                    {
                                        //Если поиск завершился
                                        if (_current == _total)
                                        {
                                            //Обнуляем счетчки
                                            _total = 0;
                                            _current = 0;
                                            _substring = null;
                                            //Прячем анимацию по завершению работы
                                            GifAnim.Visibility = Visibility.Hidden;
                                            //Если ничего не найдено
                                            if (SearchListBox.Items.Count == 0)
                                            {
                                                //Добавляем элемент
                                                SearchListBox.Visibility = Visibility.Visible;
                                                SearchListBox.Items.Add("По вашему запросу ничего не найдено.");
                                            }
                                        }
                                    });
                            })
                    );
                    //Установим фокус на выпадающий список результатов
                    FocusManager.SetFocusedElement(this, SearchListBox);
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