using Zenith_Craft_Server.Services;
using Microsoft.UI.Xaml;
using Zenith_Craft_Server;

namespace Zenith_Craft_Server;

public partial class App : Application
{
    public static Window? MainWindowInstance { get; private set; }

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(LaunchActivatedEventArgs args)
    {
        // 🔑 Calienta el cache y activa el auto-guardado una sola vez
        _ = SettingsService.Load();

        MainWindowInstance = new MainWindow();
        MainWindowInstance.Activate();
    }
}