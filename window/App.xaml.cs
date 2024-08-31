using System.Windows;

namespace window;
public partial class App : Application
{
    public App(MainWindow window)
    {
        MainWindow = window;
        MainWindow.Visibility = Visibility.Visible;
    }
}
