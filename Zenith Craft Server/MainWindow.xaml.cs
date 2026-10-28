using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using Zenith_Craft_Server.Models;
using Zenith_Craft_Server.Services;

namespace Zenith_Craft_Server
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private readonly AppSettings Settingz = SettingsService.Load();
        public static MainWindow? Instance { get; private set; }

        public MainWindow()
        {
            InitializeComponent();
            LoadStateBack();
            Instance = this;
            SetTitleBar(titleBar);
            ExtendsContentIntoTitleBar = true;
            AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            var x = Colors.Transparent;
            AppWindow.TitleBar.ButtonBackgroundColor = x;
            AppWindow.TitleBar.ButtonHoverBackgroundColor = x;
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = x;
            AppWindow.TitleBar.ButtonPressedBackgroundColor = x;
            AppWindow.TitleBar.ButtonPressedForegroundColor = x;
            SelectNavigationItem("Home");
        }

        public void LoadStateBack()
        {
            // -- Theme + Backdrop --
            ApplyBackdrop();
            ApplyBackdrop();
            // -- Controls --
            navView.PaneDisplayMode = Settingz.ExPanel;
        }

        private void titleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            navView.PaneDisplayMode = navView.PaneDisplayMode == NavigationViewPaneDisplayMode.Left ? NavigationViewPaneDisplayMode.LeftCompact : NavigationViewPaneDisplayMode.Left;
            Settingz.ExPanel = navView.PaneDisplayMode;
        }

        private static readonly Dictionary<string, Type> PagesByTag = new()
        {
            ["Home"] = typeof(Pages.Home),
            ["Settings"] = typeof(Pages.Settings),
            ["CreateServer"] = typeof(Pages.CreateServer),
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
            SystemBackdrop = Settingz.Backdrop switch
            {
                "Mica" => new MicaSystemBackdrop(MicaKind.Base),
                "MicaAlt" => new MicaSystemBackdrop(MicaKind.BaseAlt),
                "Acrylic" => new AcrylicSystemBackdrop(DesktopAcrylicKind.Base),
                "AcrylicThin" => new AcrylicSystemBackdrop(DesktopAcrylicKind.Thin),
                "Transparent" => new TransparentBackdrop(),
                _ => new AcrylicSystemBackdrop(DesktopAcrylicKind.Base)
            };
        }

        // MODIFICADO: Aplicación del tema según la configuración del usuario.
        internal void ApplyTheme()
        {
            rootWindows.RequestedTheme = Settingz.Theme switch
            {
                "Dark" => ElementTheme.Dark,
                "Default" => ElementTheme.Default,
                "Light" => ElementTheme.Light,
                _ => ElementTheme.Default   // "Default" = sigue al sistema
            };
        }
        public void SelectNavigationItem(string tag)
        {
            var item = navView.MenuItems
                .OfType<NavigationViewItem>()
                .FirstOrDefault(i => (i.Tag as string) == tag);

            if (item is not null)
                navView.SelectedItem = item;
        }

        private void serverSelect_Click(SplitButton sender, SplitButtonClickEventArgs args)
        {
            SelectNavigationItem("Home");
        }
    }
}
