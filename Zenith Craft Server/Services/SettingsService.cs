using System;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Threading;
using Zenith_Craft_Server.Models;

namespace Zenith_Craft_Server.Services;

public static class SettingsService
{
    private const string AppFolderName = "MiApp";
    private const string SettingsFileName = "settings.json";

    private static readonly string SettingsFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        AppFolderName);

    private static readonly string SettingsFilePath = Path.Combine(SettingsFolder, SettingsFileName);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private static AppSettings? _cachedSettings;

    // Debounce para no escribir en disco en cada tecla/cambio rápido
    private static Timer? _saveTimer;
    private static readonly object _saveLock = new();

    /// <summary>
    /// Carga los ajustes desde disco. Si no existe el archivo o hay error,
    /// devuelve valores por defecto. Registra auto-guardado en cada cambio.
    /// </summary>
    public static AppSettings Load()
    {
        if (_cachedSettings is not null)
            return _cachedSettings;

        AppSettings loaded;
        try
        {
            if (File.Exists(SettingsFilePath))
            {
                var json = File.ReadAllText(SettingsFilePath);
                loaded = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions)
                         ?? new AppSettings();
            }
            else
            {
                loaded = new AppSettings();
            }
        }
        catch
        {
            loaded = new AppSettings();
        }

        // 🔑 Auto-save: cualquier cambio en una propiedad programa un guardado
        loaded.PropertyChanged += OnSettingsChanged;

        _cachedSettings = loaded;
        return loaded;
    }

    /// <summary>
    /// Fuerza un guardado inmediato (normalmente no necesitas llamarlo).
    /// </summary>
    public static void Save(AppSettings settings)
    {
        try
        {
            Directory.CreateDirectory(SettingsFolder);
            var json = JsonSerializer.Serialize(settings, JsonOptions);
            File.WriteAllText(SettingsFilePath, json);
        }
        catch
        {
            // Silencioso: un fallo de escritura no debe romper la app.
        }
    }

    /// <summary>
    /// Programa un guardado con debounce (500 ms tras el último cambio).
    /// </summary>
    private static void OnSettingsChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not AppSettings settings) return;

        lock (_saveLock)
        {
            _saveTimer?.Dispose();
            _saveTimer = new Timer(
                _ => Save(settings),
                null,
                dueTime: 500,
                period: Timeout.Infinite);
        }
    }

    /// <summary>
    /// Ruta lógica del archivo (útil para depurar).
    /// </summary>
    public static string GetSettingsFilePath() => SettingsFilePath;

    /// <summary>
    /// Borra todos los ajustes y restaura valores por defecto.
    /// </summary>
    public static AppSettings Reset()
    {
        try
        {
            if (File.Exists(SettingsFilePath))
                File.Delete(SettingsFilePath);
        }
        catch { /* silencioso */ }

        if (_cachedSettings is not null)
            _cachedSettings.PropertyChanged -= OnSettingsChanged;

        var fresh = new AppSettings();
        fresh.PropertyChanged += OnSettingsChanged;
        _cachedSettings = fresh;
        return fresh;
    }
}