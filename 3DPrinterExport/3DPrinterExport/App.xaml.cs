using System.Windows;

namespace _3DPrinterExport
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Window mw;
            mw = new _3DPrinterExport.MainWindow(e);
            mw.Show();
        }
    }
}
