using DevWinUI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Zenith_Craft_Server.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        public Settings()
        {
            InitializeComponent();
            Settings_Loaded();
        }

        private void Settings_Loaded()
        {
            selectBackdrop.SelectedIndex = App.Current.SettingsService.Settings.Backdrop;
            selectTheme.SelectedIndex = App.Current.SettingsService.Settings.Theme;
        }

        private async void selectBackdrop_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Current.SettingsService.Settings.Backdrop = selectBackdrop.SelectedIndex;

            await App.Current.SettingsService.SaveAsync();
            MainWindow.Instance?.ApplyBackdrop();
        }

        private async void selectTheme_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            App.Current.SettingsService.Settings.Theme = selectTheme.SelectedIndex;

            await App.Current.SettingsService.SaveAsync();
            MainWindow.Instance?.ApplyTheme();
        }
    }
}
