using System.Threading;
using System.Windows;

namespace BSACLibrary
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        //Источник http://www.c-sharpcorner.com/UploadFile/f9f215/how-to-restrict-the-application-to-just-one-instance/
        private static Mutex _mutex = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            const string appName = "Картотека периодических изданий";
            bool crNew;

            _mutex = new Mutex(true, appName, out crNew);

            if (crNew == false)
            {
                //Закрываем программу так как ее копия уже была запущена ранее
                Current.Shutdown();
            }

            base.OnStartup(e);
        }
    }
}
