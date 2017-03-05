using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : Window
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            //Запустим почтовый клиент
            Process proc = new Process();
            proc.StartInfo.FileName = e.Uri.AbsoluteUri;
            proc.StartInfo.UseShellExecute = true;
            proc.Start();

            e.Handled = true;
        }
    }
}
