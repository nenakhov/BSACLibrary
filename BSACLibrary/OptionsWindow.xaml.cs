using System.Windows;
using BSACLibrary.Properties;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;
using System;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow
    {
        public OptionsWindow()
        {
            InitializeComponent();
            //Cчитываем значения переменных из файла конфигурации
            TxtBoxIp.Text = Settings.Default.dbServerIP;
            TxtBoxPort.Text = Settings.Default.dbServerPort.ToString();
            TxtBoxDbName.Text = Settings.Default.dbName;
            TxtBoxDbTableName.Text = Settings.Default.dbTableName;
            TxtBoxUsr.Text = Settings.Default.dbUsername;
            PwdBoxPwd.Password = Settings.Default.dbPassword;
            ChkBoxAdm.IsChecked = Settings.Default.isAdmin;
            //chkBoxAdm.IsChecked = false;
        }

        //Закрываем окно по нажатии "Отмена"
        private void CancelBt_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
        
        //Клик по кнопке ОК
        private void OkBt_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Присваиваем значения переменным
                Settings.Default.dbServerIP = TxtBoxIp.Text;
                Settings.Default.dbServerPort = Convert.ToInt32(TxtBoxPort.Text);
                Settings.Default.dbName = TxtBoxDbName.Text;
                Settings.Default.dbTableName = TxtBoxDbTableName.Text;
                Settings.Default.dbUsername = TxtBoxUsr.Text;
                Settings.Default.dbPassword = PwdBoxPwd.Password;
                if (ChkBoxAdm.IsChecked != null)
                {
                    Settings.Default.isAdmin = ChkBoxAdm.IsChecked.Value;
                }
                //Cохраняем переменные в файл конфигурации
                Settings.Default.Save();
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //Повторно инициализируем GUI и переподключаемся к бд
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)Initialize.Init)
            );

            CancelBt_Click(sender, e);
        }

        //Запрет на ввод специальных символов решает проблему возможности SQL-инъекции
        private void AllowedInput_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9_]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        //Запрет на ввод всего кроме цифр
        private void Numeric_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
