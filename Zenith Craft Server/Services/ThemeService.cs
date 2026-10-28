using DevWinUI;
using Microsoft.UI.Composition.SystemBackdrops;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using Zenith_Craft_Server.Models;
using Zenith_Craft_Server.Pages;

namespace Zenith_Craft_Server.Services;

public static class ThemeService
{
    /// <summary>
    /// Aplica el tema actual al árbol visual y se suscribe a futuros cambios.
    /// </summary>
    public static void Initialize(FrameworkElement root, AppSettings settings)
    {
        ApplyTheme(root, settings.Theme);

        settings.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(AppSettings.Theme))
                ApplyTheme(root, settings.Theme);
            else if (e.PropertyName == nameof(AppSettings.Backdrop))
                ApplyBackdrop(settings.Backdrop);
        };
    }

    private static void ApplyTheme(FrameworkElement root, string theme)
    {
        root.RequestedTheme = theme switch
        {
            "Light" => ElementTheme.Light,
            "Dark" => ElementTheme.Dark,
            _ => ElementTheme.Default
        };
    }
    private static void ApplyBackdrop( string backdrop)
    {
        Window.Current.SystemBackdrop = backdrop switch
        {
            "Mica" => new MicaSystemBackdrop(MicaKind.Base),
            "MicaAlt" => new MicaSystemBackdrop(MicaKind.BaseAlt),
            "Acrylic" => new AcrylicSystemBackdrop(DesktopAcrylicKind.Base),
            "AcrylicThin" => new AcrylicSystemBackdrop(DesktopAcrylicKind.Thin),
            "Transparent" => new TransparentBackdrop(),
            _ => new AcrylicSystemBackdrop(DesktopAcrylicKind.Base)
        };
    }
}