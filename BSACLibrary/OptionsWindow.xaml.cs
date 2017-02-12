using System.Windows;
using BSACLibrary.Properties;

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
            txtBoxIP.Text = Settings.Default.DBServerIP;
            txtBoxPort.Text = Settings.Default.DBServerPort;
            txtBoxDBName.Text = Settings.Default.DBName;
            txtBoxUsr.Text = Settings.Default.DBUsername;
            pwdBoxPwd.Password = Settings.Default.DBPassword;
            chkBoxAdm.IsChecked = Settings.Default.IsAdmin;
            //chkBoxAdm.IsChecked = false;
        }
        //Закрываем окно по нажатии "Отмена"
        private void CancelBt_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        private void OkBt_Click(object sender, RoutedEventArgs e)
        {
            //Присваиваем значения переменным
            Settings.Default.DBServerIP = txtBoxIP.Text;
            Settings.Default.DBServerPort = txtBoxPort.Text;
            Settings.Default.DBName = txtBoxDBName.Text;
            Settings.Default.DBUsername = txtBoxUsr.Text;
            Settings.Default.DBPassword = pwdBoxPwd.Password;
            Settings.Default.IsAdmin = chkBoxAdm.IsChecked.Value;
            //Cохраняем переменные в файл конфигурации
            Settings.Default.Save();
            //Повторно подключаемся к БД
            DBConnect.Connect();

            CancelBt_Click(sender, e);
        }
    }
}
