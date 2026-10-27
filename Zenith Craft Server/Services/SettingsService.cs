using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Zenith_Craft_Server.Models;

namespace Zenith_Craft_Server.Services
{
    public class SettingsService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };

        private readonly string _folderPath;
        private readonly string _filePath;

        // MODIFICADO: Semáforo para prevenir accesos concurrentes al archivo de configuración.
        private readonly SemaphoreSlim _saveLock = new(1, 1);

        public AppSettings Settings { get; private set; } = new();
        public string FilePath => _filePath;

        public SettingsService()
        {
            // MODIFICADO: Ruta de almacenamiento en LocalAppData\Zenith.C.S\settings.json
            _folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Zenith.C.S");

            _filePath = Path.Combine(_folderPath, "settings.json");

            // Asegura la existencia del directorio objetivo
            Directory.CreateDirectory(_folderPath);
        }

        /// <summary>
        /// Carga el archivo si existe; si no, devuelve valores por defecto y crea el archivo.
        /// </summary>
        public async Task LoadAsync()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    Settings = new AppSettings();
                    await SaveAsync();
                    return;
                }

                string json = await File.ReadAllTextAsync(_filePath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Load failed: {ex.Message}");
                Settings = new AppSettings();
            }
        }

        /// <summary>
        /// Guarda el estado actual en disco de forma asíncrona.
        /// </summary>
        public async Task SaveAsync()
        {
            await _saveLock.WaitAsync();
            try
            {
                Directory.CreateDirectory(_folderPath);
                string json = JsonSerializer.Serialize(Settings, JsonOptions);
                await File.WriteAllTextAsync(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Save failed: {ex.Message}");
            }
            finally
            {
                _saveLock.Release();
            }
        }

        /// <summary>
        /// Guarda de forma síncrona (usado al cerrar la aplicación).
        /// </summary>
        public void SaveSync()
        {
            _saveLock.Wait();
            try
            {
                Directory.CreateDirectory(_folderPath);
                string json = JsonSerializer.Serialize(Settings, JsonOptions);
                File.WriteAllText(_filePath, json);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] SaveSync failed: {ex.Message}");
            }
            finally
            {
                _saveLock.Release();
            }
        }

        /// <summary>
        /// Restablece la configuración a sus valores por defecto y guarda los cambios.
        /// </summary>
        public async Task ResetAsync()
        {
            Settings = new AppSettings();
            await SaveAsync();
        }
    }
}