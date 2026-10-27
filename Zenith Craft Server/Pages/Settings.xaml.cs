using Microsoft.UI.Xaml.Controls;
// ELIMINADO: Se eliminaron las directivas 'using' sin uso (DevWinUI, Microsoft.UI.Composition.SystemBackdrops, System, Windows.Foundation, etc.).

namespace Zenith_Craft_Server.Pages
{
    /// <summary>
    /// Página de configuración de temas y personalización visual.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            Settings_Loaded();
        }

        // MODIFICADO: Carga las opciones seleccionadas desde las configuraciones guardadas.
        private void Settings_Loaded()
        {
            selectBackdrop.SelectedIndex = App.Current.SettingsService.Settings.Backdrop;
            selectTheme.SelectedIndex = App.Current.SettingsService.Settings.Theme;
        }

        // MODIFICADO: Actualiza el tipo de fondo en la configuración, lo guarda y lo aplica en la ventana principal.
        private async void selectBackdrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectBackdrop.SelectedIndex < 0) return;

            App.Current.SettingsService.Settings.Backdrop = selectBackdrop.SelectedIndex;
            await App.Current.SettingsService.SaveAsync();
            MainWindow.Instance?.ApplyBackdrop();
        }

        // MODIFICADO: Actualiza el tema en la configuración, lo guarda y lo aplica en la ventana principal.
        private async void selectTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (selectTheme.SelectedIndex < 0) return;

            App.Current.SettingsService.Settings.Theme = selectTheme.SelectedIndex;
            await App.Current.SettingsService.SaveAsync();
            MainWindow.Instance?.ApplyTheme();
        }
    }
}
