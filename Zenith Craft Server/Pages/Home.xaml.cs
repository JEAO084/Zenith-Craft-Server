using Microsoft.UI.Xaml.Controls;
using Zenith_Craft_Server.Models;
using Zenith_Craft_Server.Services;
// ELIMINADO: Se eliminaron múltiples directivas 'using' sin uso (System, System.IO, Windows.Foundation, Microsoft.UI.Xaml.Media, etc.).

namespace Zenith_Craft_Server.Pages;

/// <summary>
/// Página principal del servidor Zenith Craft.
/// </summary>
public sealed partial class Home : Page
{
    private static MainWindow? Window => (MainWindow?)App.MainWindowInstance;
    private readonly AppSettings Settingz = SettingsService.Load();

    public Home()
    {
        InitializeComponent();
        isFirstOpen.Visibility= Settingz.FirstOpen == true ? Microsoft.UI.Xaml.Visibility.Visible : Microsoft.UI.Xaml.Visibility.Collapsed;
    }

    private void CreateSerber_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
    {
        Window?.SelectNavigationItem("CreateServer");
    }
}
