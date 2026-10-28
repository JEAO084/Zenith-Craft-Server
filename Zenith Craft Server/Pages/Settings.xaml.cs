using Microsoft.UI.Xaml.Controls;
using Zenith_Craft_Server.Models;
using Zenith_Craft_Server.Services;
// ELIMINADO: Se eliminaron las directivas 'using' sin uso (DevWinUI, Microsoft.UI.Composition.SystemBackdrops, System, Windows.Foundation, etc.).

namespace Zenith_Craft_Server.Pages
{
    /// <summary>
    /// Página de configuración de temas y personalización visual.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public AppSettings Settingz { get; } = SettingsService.Load();
        public Settings()
        {
            InitializeComponent();
            Settings_Loaded();
        }

        // MODIFICADO: Carga las opciones seleccionadas desde las configuraciones guardadas.
        private void Settings_Loaded()
        {
            SeleccionarItemPorTag(selectBackdrop, Settingz.Backdrop);
            SeleccionarItemPorTag(selectTheme, Settingz.Theme);
        }

        private void SeleccionarItemPorTag(ComboBox comboBox, string valorTag)
        {
            // Recorremos la colección de ítems del ComboBox
            foreach (var item in comboBox.Items)
            {
                // Verificamos si el ítem es del tipo ComboBoxItem y si su Tag coincide
                if (item is ComboBoxItem comboBoxItem && comboBoxItem.Tag?.ToString() == valorTag)
                {
                    // Asignamos el ítem encontrado a la propiedad SelectedItem
                    comboBox.SelectedItem = comboBoxItem;
                    return; // Salimos del método una vez encontrado
                }
            }
        }

        // MODIFICADO: Actualiza el tipo de fondo en la configuración, lo guarda y lo aplica en la ventana principal.
        private async void selectBackdrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = (selectBackdrop.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            Settingz.Backdrop = tag == null ? "Acrylic" : tag;
            MainWindow.Instance?.ApplyBackdrop();
        }

        // MODIFICADO: Actualiza el tema en la configuración, lo guarda y lo aplica en la ventana principal.
        private async void selectTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var tag = (selectTheme.SelectedItem as ComboBoxItem)?.Tag?.ToString();
            Settingz.Theme = tag== null ? "Default" : tag;
            MainWindow.Instance?.ApplyTheme();
        }
    }
}
