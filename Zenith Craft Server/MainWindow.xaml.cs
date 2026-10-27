using DevWinUI;
using Microsoft.UI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

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
            this.SetTitleBar(titleBar); // Set the custom title bar
            this.ExtendsContentIntoTitleBar = true; // Extend the content into the title bar and hide the default titlebar
            AppWindow.TitleBar.ExtendsContentIntoTitleBar = true;
            if (AppWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
            }
            AppWindow.TitleBar.ButtonBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0);
            AppWindow.TitleBar.ButtonHoverBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0);
            AppWindow.TitleBar.ButtonInactiveBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0);
            AppWindow.TitleBar.ButtonPressedBackgroundColor = Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0);
            AppWindow.TitleBar.ButtonPressedForegroundColor = Microsoft.UI.ColorHelper.FromArgb(0, 0, 0, 0);
        }

        private void titleBar_PaneToggleRequested(TitleBar sender, object args)
        {
            navView.PaneDisplayMode = navView.PaneDisplayMode == NavigationViewPaneDisplayMode.Left ? NavigationViewPaneDisplayMode.LeftCompact : NavigationViewPaneDisplayMode.Left;
        }

        private static readonly Dictionary<string, Type> PagesByTag = new()
        {
            ["Home"] = typeof(Pages.Home),
            ["Settings"] = typeof(Pages.Settings),
            ["About"] = typeof(Pages.About),
        };

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
        internal void ApplyBackdrop()
        {
            int backdrop = App.Current.SettingsService.Settings.Backdrop;
            if (backdrop == 0)
                SystemBackdrop = new MicaSystemBackdrop(MicaKind.Base);
            else if (backdrop == 1)
                SystemBackdrop = new MicaSystemBackdrop(MicaKind.BaseAlt);
            else if (backdrop == 2)
                SystemBackdrop = new AcrylicSystemBackdrop(DesktopAcrylicKind.Base);
            else if (backdrop == 3)
                SystemBackdrop = new AcrylicSystemBackdrop(DesktopAcrylicKind.Thin);
            else if (backdrop == 4)
                SystemBackdrop = new TransparentBackdrop();
        }
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
        private void MainWindow_Closed(object sender, WindowEventArgs args) => App.Current.SettingsService.SaveAsync().GetAwaiter().GetResult();
    }
}
