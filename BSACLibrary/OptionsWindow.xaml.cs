using System.Windows;
using BSACLibrary.Properties;
using System.Windows.Input;
using System.Text.RegularExpressions;
using System.Windows.Threading;
using System.Threading;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for OptionsWindow.xaml
    /// </summary>
    public partial class OptionsWindow : Window
    {
        public OptionsWindow()
        {
            InitializeComponent();
            //Cчитываем значения переменных из файла конфигурации
            txtBoxIP.Text = Settings.Default.dbServerIP;
            txtBoxPort.Text = Settings.Default.dbServerPort;
            txtBoxDBName.Text = Settings.Default.dbName;
            txtBoxDBTableName.Text = Settings.Default.dbTableName;
            txtBoxUsr.Text = Settings.Default.dbUsername;
            pwdBoxPwd.Password = Settings.Default.dbPassword;
            chkBoxAdm.IsChecked = Settings.Default.isAdmin;
            //chkBoxAdm.IsChecked = false;
        }

        //Закрываем окно по нажатии "Отмена"
        private void CancelBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        
        //Клик по кнопке ОК
        private void OkBt_Click(object sender, RoutedEventArgs e)
        {
            //Присваиваем значения переменным
            Settings.Default.dbServerIP = txtBoxIP.Text;
            Settings.Default.dbServerPort = txtBoxPort.Text;
            Settings.Default.dbName = txtBoxDBName.Text;
            Settings.Default.dbTableName = txtBoxDBTableName.Text;
            Settings.Default.dbUsername = txtBoxUsr.Text;
            Settings.Default.dbPassword = pwdBoxPwd.Password;
            Settings.Default.isAdmin = chkBoxAdm.IsChecked.Value;
            //Cохраняем переменные в файл конфигурации
            Settings.Default.Save();

            //Повторно инициализируем GUI и переподключаемся к бд
            System.Threading.Tasks.Task.Factory.StartNew(() =>
                Dispatcher.BeginInvoke(DispatcherPriority.Normal,
                (ThreadStart)delegate ()
                {
                    Initialize.Init();
                })
            );

            CancelBt_Click(sender, e);
        }

        //Запрет на ввод специальных символов решает проблему возможности SQL-инъекции
        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^a-zA-Z0-9_]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
