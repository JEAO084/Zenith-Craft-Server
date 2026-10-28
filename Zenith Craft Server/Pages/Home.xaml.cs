using Microsoft.UI.Xaml.Controls;
// ELIMINADO: Se eliminaron múltiples directivas 'using' sin uso (System, System.IO, Windows.Foundation, Microsoft.UI.Xaml.Media, etc.).

namespace Zenith_Craft_Server.Pages;

/// <summary>
/// Página principal del servidor Zenith Craft.
/// </summary>
public sealed partial class Home : Page
{
    private static MainWindow? Window => (MainWindow?)App.MainWindowInstance;

    public Home()
    {
        InitializeComponent();
    }

    private void CreateSerber_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Window?.SelectNavigationItem("CreateServer");
    }
}
