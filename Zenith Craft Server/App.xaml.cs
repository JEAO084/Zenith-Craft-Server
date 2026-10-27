using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Xaml;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Zenith_Craft_Server
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        public IThemeService ThemeService { get; set; }
        public static new App Current => (App)Application.Current;
        public SettingsService SettingsService { get; } = new();

        public MainWindow? MainWindow { get; private set; }

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();
            // Manejador global de excepciones para la UI
            this.UnhandledException += (sender, e) =>
            {
                // Muestra el error real en la ventana de salida de Visual Studio
                System.Diagnostics.Debug.WriteLine($"Excepción no controlada: {e.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}");
                // Opcional: muestra un mensaje al usuario y cierra la app de forma controlada
                // e.Handled = true; 
            };
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            await SettingsService.LoadAsync();
            ThemeService = new ThemeService();
            _window = new MainWindow();
            _window.Activate();
        }
    }
}