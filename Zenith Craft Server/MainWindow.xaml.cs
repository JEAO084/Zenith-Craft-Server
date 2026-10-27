using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
// ELIMINADO: 'using System.Threading.Tasks;' eliminado por no ser utilizado.

namespace Zenith_Craft_Server
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            ApplyBackdrop();
            ApplyTheme();
            Instance = this;

            // MODIFICADO: Configuración limpia de la barra de título personalizada y botones transparentes.
            SetTitleBar(titleBar);
            ExtendsContentIntoTitleBar = true;

            if (AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            }

            // MODIFICADO: Uso de Colors.Transparent para simplificar la asignación de colores de los botones de la barra de título.
            // ELIMINADO / REEMPLAZADO: Reemplazado 'Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0)' redundante por 'Colors.Transparent'.
            var transparent = Colors.Transparent;
            AppWindow.TitleBar.ButtonBackgroundColor = transparent;
            AppWindow.TitleBar.ButtonHoverBackgroundColor = transparent;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = transparent;
            AppWindow.TitleBar.ButtonPressedBackgroundColor = transparent;
            AppWindow.TitleBar.ButtonPressedForegroundColor = transparent;
        }

        // MODIFICADO: Alterna entre el modo compacto e imprimible del panel lateral.
        private void titleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            navView.PaneDisplayMode = navView.PaneDisplayMode == NavigationViewPaneDisplayMode.Left
                ? NavigationViewPaneDisplayMode.LeftCompact
                : NavigationViewPaneDisplayMode.Left;
        }

        private static readonly Dictionary<string, Type> PagesByTag = new()
        {
            ["Home"] = typeof(Pages.Home),
            ["Settings"] = typeof(Pages.Settings),
            ["About"] = typeof(Pages.About),
        };

        // MODIFICADO: Manejador de navegación simplificado según el elemento seleccionado en NavigationView.
        private void navView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.IsSettingsSelected)
            {
                if (viewPages.CurrentSourcePageType != typeof(Pages.Settings))
                    viewPages.Navigate(typeof(Pages.Settings));
                return;
            }

            if (args.SelectedItemContainer?.Tag is not string tag)
                return;

            if (PagesByTag.TryGetValue(tag, out var pageType) &&
                viewPages.CurrentSourcePageType != pageType)
            {
                viewPages.Navigate(pageType);
            }
        }

        // MODIFICADO: Aplicación del tipo de fondo (Backdrop) según la configuración del usuario usando 'switch' más legible.
        // REEMPLAZADO: Reemplazado bloque 'if-else if' por expresión 'switch'.
        internal void ApplyBackdrop()
        {
            int backdrop = App.Current.SettingsService.Settings.Backdrop;
            SystemBackdrop = backdrop switch
            {
                0 => new MicaSystemBackdrop(MicaKind.Base),
                1 => new MicaSystemBackdrop(MicaKind.BaseAlt),
                2 => new AcrylicSystemBackdrop(DesktopAcrylicKind.Base),
                3 => new AcrylicSystemBackdrop(DesktopAcrylicKind.Thin),
                4 => new TransparentBackdrop(),
                _ => new MicaSystemBackdrop(MicaKind.Base)
            };
        }

        // MODIFICADO: Aplicación del tema según la configuración del usuario.
        internal void ApplyTheme()
        {
            var theme = App.Current.SettingsService.Settings.Theme;
            var requestedTheme = theme switch
            {
                0 => ElementTheme.Dark,
                1 => ElementTheme.Default,
                2 => ElementTheme.Light,
                _ => ElementTheme.Default,   // "Default" = sigue al sistema
            };
            rootWindows.RequestedTheme = requestedTheme;
        }

        private void serverSelect_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            navView.SelectedItem = navView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(item => (string)item.Tag == "Home");
        }

        // MODIFICADO: Guardado de configuración al cerrar la ventana principal usando SaveSync para asegurar persistencia.
        // REEMPLAZADO: Reemplazado 'SaveAsync().GetAwaiter().GetResult()' por 'SaveSync()' para evitar un bloqueo potencial o terminación sin completar.
        private void MainWindow_Closed(object sender, WindowEventArgs args) => App.Current.SettingsService.SaveSync();
    }
}
