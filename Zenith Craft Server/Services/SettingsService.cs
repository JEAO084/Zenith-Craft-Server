using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

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

        // Semáforo para evitar que dos guardados choquen al mismo tiempo (IOException)
        private readonly SemaphoreSlim _saveLock = new(1, 1);

        public AppSettings Settings { get; private set; } = new();
        public string FilePath => _filePath;

        public SettingsService()
        {
            // Ruta: C:\Users\<Usuario>\AppData\Local\Zenith.C.S
            _folderPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Zenith.C.S");

            _filePath = Path.Combine(_folderPath, "settings.json");

            // Crea la carpeta si no existe
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
                    await SaveAsync(); // Crea el archivo con defaults
                    return;
                }

                string json = await File.ReadAllTextAsync(_filePath);
                Settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[Settings] Load failed: {ex.Message}");
                Settings = new AppSettings(); // Fallback seguro
            }
        }

        /// <summary>
        /// Guarda el estado actual en disco de forma asíncrona (seguro para llamar desde UI).
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
        /// Guarda de forma síncrona. Úsalo ÚNICAMENTE en el evento Closed de la ventana
        /// para asegurar que el archivo se escriba antes de que el proceso muera.
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
        /// Borra el archivo y vuelve a valores por defecto.
        /// </summary>
        public async Task ResetAsync()
        {
            Settings = new AppSettings();
            await SaveAsync();
        }
    }
}