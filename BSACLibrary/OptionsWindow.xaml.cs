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
            //Повторно подключаемся к БД
            DBConnect.Connect();

            CancelBt_Click(sender, e);
        }
    }
}
