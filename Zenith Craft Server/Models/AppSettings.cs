using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Zenith_Craft_Server.Models;

public class AppSettings : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    // ----- Apariencia -----

    private string _theme = "Default";
    public string Theme
    {
        get => _theme;
        set => SetProperty(ref _theme, value);
    }
    //-
    private string _backdrop = "Acrylic";
    public string Backdrop
    {
        get => _backdrop;
        set => SetProperty(ref _backdrop, value);
    }

    // ----- Status -----

    private NavigationViewPaneDisplayMode _exPanel = NavigationViewPaneDisplayMode.LeftCompact;
    public NavigationViewPaneDisplayMode ExPanel
    {
        get => _exPanel;
        set => SetProperty(ref _exPanel, value);
    }

    private bool _firstOpen = true;
    public bool FirstOpen
    {
        get => _firstOpen;
        set => SetProperty(ref _firstOpen, value);
    }

    // ----- Helper -----
    private bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        return true;
    }
}