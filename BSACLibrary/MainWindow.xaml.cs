using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Controls.Primitives;

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

        public ObservableCollection<string> NamesList { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> YearsList { get; set; } = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();
            AppWindow = this;
            //Инициализация подключения к БД и др. процессов в фоне
            Initialize.Init();
        }

        public List<PdfDescription> FilesList
        {
            get
            {
                return _filesList;
            }
            set
            {
                _filesList = value;

                //Cортировка всех названий по алфавиту
                _filesList = _filesList.AsParallel().OrderBy(x => x.PublicationName).ToList();

                foreach(var file in _filesList)
                {
                    //Заполним список наименований во вкладке редактора заново
                    //Исключим повторяющиеся записи
                    if (AddPublNameCmbBox.Items.Contains(file.PublicationName) == false)
                    {
                        AddPublNameCmbBox.Items.Add(file.PublicationName);
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

        //Клик мышью по полю ввода поискового запроса
        private void SrchTxtBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если список содержит в себе элементы
            if (SearchListBox.Items.Count > 0)
            {
                //Скроем выпадающий список с результатами
                SearchListBox.Visibility = Visibility.Visible;
            }
        }

        private void SearchListBox_MouseLeave(object sender, MouseEventArgs e)
        {
            //Скроем выпадающий список если мышь увели за его пределы
            SearchListBox.Visibility = Visibility.Collapsed;
        }

        //Было закрыто главное окно программы
        private void Window_Closed(object sender, EventArgs e)
        {
            //Остановим выполнение программы
            Application.Current.Shutdown();
        }

        //Клик мышью по кнопке "Добавить запись"
        private void AddEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            //Проверим заполнили ли поля "Название, Дата выхода, Номер издания"
            if (string.IsNullOrEmpty(AddPublNameCmbBox.Text) == false &&
                string.IsNullOrEmpty(AddDatePicker.Text) == false &&
                string.IsNullOrEmpty(AddIssueNmbTxtBox.Text) == false)
            {
                //Сформируем и отправим соответствующий SQL-запрос в БД
                _query = "INSERT INTO " + Settings.Default.dbTableName + " VALUES('" +
                         null + "', '" +
                         AddPublNameCmbBox.Text.Replace(@"'", @"\'") + "', '" +
                         Convert.ToInt16(AddRadioBtnMagaz.IsChecked) + "', '" +
                         Convert.ToDateTime(AddDatePicker.Value).ToString("yyyy-MM-dd") + "', '" +
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

        //Указание расположения pdf файла при добавлении
        private void AddOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            //Запишим данные в переменную
            AddFilePathTxtBox.Text = FilePathSet();
        }

        //Изменилась выделенная запись в таблице редактора
        private void DbDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Если что-то было выбрано
            if (DbDataGrid.SelectedIndex >= 0)
            {
                //Сделаем кнопки редактировать/удалить активными
                EditEntryBtn.IsEnabled = true;
                DelEntryBtn.IsEnabled = true;
                //Присвоим всем полям соответствующие значения в колонке редактирования
                if (DbDataGrid.SelectedItem is DataRowView row)
                {
                    EditIdTxtBox.Text = Convert.ToString(row[0]);
                    EditPublName.Text = Convert.ToString(row[1]);
                    //Конвертация нужна т.к. в БД для экономии памяти это значение хранится в виде 0,1
                    if (Convert.ToBoolean(row[2]))
                    {
                        EditRadioBtnMagaz.IsChecked = true;
                    }
                    else
                    {
                        EditRadioBtnNewsp.IsChecked = true;
                    }

                    EditDatePicker.Value = Convert.ToDateTime(row[3]);
                    EditIssueNmbTxtBox.Text = Convert.ToString(row[4]);
                    EditFilePathTxtBox.Text = Convert.ToString(row[5]);
                }
            }
            //Сняли выделение
            else
            {
                //Сделаем кнопки редактировать/удалить неактивными
                EditEntryBtn.IsEnabled = false;
                DelEntryBtn.IsEnabled = false;
            }
        }

        //Указание расположения pdf файла при редактировнии
        private void EditOpenFileBtn_Click(object sender, RoutedEventArgs e)
        {
            //Запишим данные в переменную
            EditFilePathTxtBox.Text = FilePathSet();
        }

        //Функция вызова диалога выбора файла
        private string FilePathSet()
        {
            var openFileDialog = new OpenFileDialog { Filter = "PDF files (*.pdf)|*.pdf|All files (*.*)|*.*" };
            if (openFileDialog.ShowDialog() == true)
            {
                return(openFileDialog.FileName);
            }
            return null;
        }

        //Клик мышью по кнопке "Удалить запись"
        private void DelEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DbDataGrid.SelectedIndex != -1)
            {
                try
                {
                    var result = MessageBox.Show("Вы уверены что хотите удалить запись? Действие необратимо.", "Удаление",
                        MessageBoxButton.OKCancel);
                    switch (result)
                    {
                        case MessageBoxResult.OK:
                            _query = "DELETE FROM " + Settings.Default.dbTableName +
                                     " WHERE id = '" + EditIdTxtBox.Text +
                                     "';";
                            DbQueries.Execute(_query);
                            DbQueries.UpdateDataGrid();
                            break;
                        case MessageBoxResult.Cancel:
                            break;
                    }
                    //Удалим из списка предложенных названий, если такого имени не осталось в БД 
                    //Удалили последнюю запись
                    string name = EditPublName.Text;
                    if (_filesList.AsParallel().Any(file => file.PublicationName == name) == false)
                    {
                        AddPublNameCmbBox.Items.Remove(EditPublName.Text);
                        AddPublNameCmbBox.Text = null;
                        AddPublNameCmbBox.SelectedIndex = -1;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Клик мышью по кнопке "Редактировать запись"
        private void EditEntryBtn_Click(object sender, RoutedEventArgs e)
        {
            if (DbDataGrid.SelectedIndex >= 0)
            {
                var result = MessageBox.Show("Вы уверены что хотите изменить запись? Действие необратимо.", "Изменение",
                    MessageBoxButton.OKCancel);
                switch (result)
                {
                    case MessageBoxResult.OK:
                        _query = "UPDATE " + Settings.Default.dbTableName +
                                 " SET publication='" + EditPublName.Text.Replace("'", "''") +
                                 "',is_magazine='" + Convert.ToInt16(EditRadioBtnMagaz.IsChecked) +
                                 "',date='" + Convert.ToDateTime(EditDatePicker.Value).ToString("yyyy-MM-dd") +
                                 "',issue_number='" + EditIssueNmbTxtBox.Text +
                                 "',file_path='" + EditFilePathTxtBox.Text.Replace(@"\", @"\\").Replace("'", "''") +
                                 "' WHERE id='" + EditIdTxtBox.Text + "';";
                        DbQueries.Execute(_query);
                        DbQueries.UpdateDataGrid();
                        break;
                    case MessageBoxResult.Cancel:
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
            {
                SearchListBox.Visibility = Visibility.Collapsed;
            }
        }

        //Обработка клика по выпадающему списку результатов поиска
        private void SearchListBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            //Если был выбран любой элемент списка
            if (SearchListBox.SelectedIndex >= 0)
            {
                try
                {
                    //Приводим объект из списка к типу pdfDescription
                    if (SearchListBox.Items[SearchListBox.SelectedIndex] is PdfDescription selectedFile)
                    {
                        //Откроем соответствующий файл
                        OnNavigate(this, new RequestNavigateEventArgs(new Uri(selectedFile.FilePath), null));
                    }
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
                        FileName = e.Uri.OriginalString,
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

        //Метод формирующий текстовые блоки с названием и номером издания
        private static TextBlock AddTextBlock(string publicationName, string filePath, int issueNumber)
        {
            //Cформируемый текстовый блок с названием издания и его номером
            var newTextBlock = new TextBlock();
            var addString = publicationName + " №" + issueNumber + "; ";
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
                var newToolTip = new ToolTip()
                {
                    Content = "Нет цифровой копии",
                    Placement = PlacementMode.Bottom
                };
                newTextBlock.ToolTip = newToolTip;
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
                //Очищаем элементы списка
                SearchListBox.Items.Clear();
                SearchListBox.Visibility = Visibility.Collapsed;

                if (SrchTxtBox.Text.Length < 3)
                {
                    //Добавляем элемент
                    SearchListBox.Items.Add("Минимальная длина поискового запроса 3 символа.");
                    SearchListBox.Visibility = Visibility.Visible;
                    return;
                }

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
                                {
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart) delegate
                                        {
                                            //Добавим в результаты поиска название издания
                                            //И часть текста, начинающегося с поискового запроса
                                            SearchListBox.Items.Add(file);
                                            //Откроем выпадающий список
                                            SearchListBox.Visibility = Visibility.Visible;
                                        });
                                }

                                Interlocked.Increment(ref _current);
                                //Если поиск завершился
                                if (_current == _total)
                                {
                                    Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                                        (ThreadStart)delegate
                                       {
                                            //Обнуляем счетчки
                                            _total = 0;
                                            _current = 0;
                                            _substring = null;
                                        //Прячем анимацию по завершению работы
                                        GifAnim.Visibility = Visibility.Collapsed;
                                        //Если ничего не найдено
                                        if (SearchListBox.Items.Count == 0)
                                        {
                                            //Добавляем элемент
                                            SearchListBox.Items.Add("По вашему запросу ничего не найдено.");
                                            SearchListBox.Visibility = Visibility.Visible;
                                        }
                                    });
                                }
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

        private void NamesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбрано ниодного элемента в списке
            if (NamesListBox.SelectedIndex == -1)
            {
                return;
            }
            //Очистим список годов выхода
            YearsList.Clear();
            YearsList.Add("<<<ВСЕ>>>");

            //Cортировка по году выхода
            foreach(var file in _filesList.AsParallel().OrderBy(x => x.Date.Year).ToList())
            {
                //Если это газета но выбрана вкладка "ЖУРНАЛЫ" перейдем к следующей итерации
                if (MagazinesBtn.IsChecked == true && file.IsMagazine == false)
                {
                    continue;
                }
                //Если это журнал но выбрана вкладка "ГАЗЕТЫ" перейдем к следующей итерации
                if (NewspapersBtn.IsChecked == true && file.IsMagazine)
                {
                    continue;
                }
                //Если выбрали <<<ВСЕ>>> в названии издания
                if (NamesListBox.SelectedIndex == 0 &&
                    //Если этот год еще не добавили в список
                    YearsList.AsParallel().Contains(file.Date.Year.ToString()) == false)
                {
                    YearsList.Add(file.Date.Year.ToString());
                }
                //Если выбрано определенное издание
                else if (NamesListBox.SelectedItem.ToString() == file.PublicationName &&
                            //И такой год еще не добавляли
                            YearsList.AsParallel().Contains(file.Date.Year.ToString()) == false)
                {
                    YearsList.Add(file.Date.Year.ToString());
                }
            }
            //Выбираем <<<ВСЕ>>> в списке по умолчанию
            YearsListBox.SelectedIndex = 0;
        }

        private void YearsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //Не выбранно ниодного элемента в списке
            if (NamesListBox.SelectedIndex == -1 || YearsListBox.SelectedIndex == -1)
            {
                return;
            }
            //Очистим панель со списком изданий.
            ResultWrapPanel.Children.Clear();
            //Отсортируем список по имени и номеру
            foreach(var file in _filesList.AsParallel().OrderBy(x => x.PublicationName).ThenBy(x => x.IssueNumber).ToList())
            {
                //Если это газета но выбрана вкладка "ЖУРНАЛЫ" перейдем к следующей итерации
                if (MagazinesBtn.IsChecked == true && file.IsMagazine == false)
                {
                    continue;
                }
                //Если это журнал но выбрана вкладка "ГАЗЕТЫ" перейдем к следующей итерации
                if (NewspapersBtn.IsChecked == true && file.IsMagazine)
                {
                    continue;
                }
                //Если выбрано какое-то издание, не ВСЕ
                if (NamesListBox.SelectedIndex != 0 &&
                    //И если выбранное издание != file.publication_name пропустим текущую итерацию
                    NamesListBox.SelectedItem.ToString() != file.PublicationName)
                {
                    continue;
                }
                //Или если выбранный год != file.date.Year пропустим текущую итерацию
                if (YearsListBox.SelectedIndex != 0 &&
                    YearsListBox.SelectedItem.ToString() != file.Date.Year.ToString())
                {
                    continue;
                }
                //Заполним панель полученными гиперссылками
                ResultWrapPanel.Children.Add(AddTextBlock(file.PublicationName, file.FilePath,
                    file.IssueNumber));
            }
            CountLabel.Content = PrintRecordCount(ResultWrapPanel.Children.Count);
        }

        //Просклоняем слово "номер"
        private static string PrintRecordCount(int i)
        {
            if (i > 0)
            {
                string sEnd = null;
                //C помощью остатка от деления на 10 определим последнюю цифру в числе номеров
                var iLast = i % 10;
                int[] numList = {0, 5, 6, 7, 8 , 9};
                //номерОВ (от 11 до 19 включительно, а также в случае окончания на цифры в массиве)
                if ((i >= 11 && i <= 19) || numList.Contains(iLast))
                {
                    sEnd = "ов";
                }
                //номерА (во всех остальных случаях)
                else if (iLast != 1)
                {
                    sEnd = "а";
                }
                //номер (один, двадцать один, и т.д.)
                return ("Всего " + i + " номер" + sEnd + ".");
            }
            return ("В базе данных отсутсвуют записи.");
        }

        //Выбор вкладки журналы/газеты
        private void MainToggle_Checked(object sender, RoutedEventArgs e)
        {
            NamesList.Clear();
            NamesList.Add("<<<ВСЕ>>>");
            foreach(var file in _filesList)
            {
                if (MagazinesBtn.IsChecked == true && file.IsMagazine)
                {
                    if (NamesList.AsParallel().Contains(file.PublicationName))
                    {
                        continue;
                    }
                }
                else if (NewspapersBtn.IsChecked == true && file.IsMagazine == false)
                {
                    if (NamesList.AsParallel().Contains(file.PublicationName))
                    {
                        continue;
                    }
                }
                else
                {
                    continue;
                }
                NamesList.Add(file.PublicationName);
            }
            NamesListBox.SelectedIndex = 0;
        }
    }
}