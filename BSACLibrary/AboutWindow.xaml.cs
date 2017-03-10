using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
        private void OnNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            { 
                //Запустим почтовый клиент
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
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            e.Handled = true;
        }
    }
}
