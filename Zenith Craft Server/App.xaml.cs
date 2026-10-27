using DevWinUI;
// ELIMINADO: 'using Microsoft.UI;' eliminado por no ser utilizado en este archivo.
using Microsoft.UI.Xaml;
using Zenith_Craft_Server.Services;

namespace Zenith_Craft_Server
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        private Window? _window;

        // NUEVO / MODIFICADO: Propiedad ThemeService inicializada con valor por defecto nulo o asignada en OnLaunched.
        public IThemeService? ThemeService { get; set; }
        public static new App Current => (App)Application.Current;
        public SettingsService SettingsService { get; } = new();

        // ELIMINADO: Se eliminó la propiedad pública sin uso 'public MainWindow? MainWindow { get; private set; }' ya que MainWindow utiliza su propio patrón Instance.

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            InitializeComponent();

            // MODIFICADO: Configuración limpia del manejador global de excepciones para la interfaz de usuario.
            this.UnhandledException += (sender, e) =>
            {
                System.Diagnostics.Debug.WriteLine($"Excepción no controlada: {e.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {e.Exception.StackTrace}");
            };
        }

        /// <summary>
        /// Invoked when the application is launched.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override async void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            // MODIFICADO: Carga asíncrona de configuraciones del usuario e inicialización del servicio de temas y ventana principal.
            await SettingsService.LoadAsync();
            ThemeService = new ThemeService();
            _window = new MainWindow();
            _window.Activate();
        }
    }
}