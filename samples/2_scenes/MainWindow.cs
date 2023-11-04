using Microsoft.UI.Xaml;

namespace sample;

public class ScenesWindow(MainWindowContext context) : MainWindow(context) {
    public override void OnActivated(object sender, RoutedEventArgs e) {
        base.OnActivated(sender, e);

        Title = "Scenes Sample";
    }
}