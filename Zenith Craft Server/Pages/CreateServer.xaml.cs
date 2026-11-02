using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Zenith_Craft_Server.Models;
using Zenith_Craft_Server.Services;

namespace Zenith_Craft_Server.Pages
{
    /// <summary>
    /// Página completa y funcional para la creación y configuración de servidores de Minecraft.
    /// </summary>
    public sealed partial class CreateServer : Page
    {
        // AGREGADO: Referencia a la configuración y servicios del servidor
        private readonly AppSettings Settingz = SettingsService.Load();
        private readonly MinecraftService _minecraftService = MinecraftService.Instance;

        // AGREGADO: Almacenamiento local de opciones cargadas
        private List<MinecraftVersion> _versions = new();
        private List<LoaderInfo> _loaders = new();
        private List<LoaderVersionItem> _loaderVersions = new();
        private List<JavaVersionItem> _javaVersions = new();
        private string _selectedIconFilePath = string.Empty;

        public CreateServer()
        {
            InitializeComponent();
        }

        // AGREGADO: Inicialización al cargar la página
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeHardwareOptions();
            await LoadMinecraftVersionsAsync();
        }

        // AGREGADO: Inicializa opciones de hardware (Hilos de CPU y RAM) según las especificaciones del sistema
        private void InitializeHardwareOptions()
        {
            // 1. Hilos de CPU: Opciones 1, 2, 4, 6... hasta el total de procesadores lógicos detectados
            int totalCores = Math.Max(1, Environment.ProcessorCount);
            CpuThreadsComboBox.Items.Clear();
            var threadCounts = new SortedSet<int> { 1 };
            for (int i = 2; i <= totalCores; i += 2)
            {
                threadCounts.Add(i);
            }
            if (!threadCounts.Contains(totalCores))
            {
                threadCounts.Add(totalCores);
            }

            foreach (var count in threadCounts)
            {
                var cbi = new ComboBoxItem
                {
                    Content = count.ToString(),
                    Tag = count
                };
                ToolTipService.SetToolTip(cbi, $"{count} hilo(s) del CPU asignados");
                CpuThreadsComboBox.Items.Add(cbi);
            }

            // Seleccionar por defecto un valor balanceado (ej: 2 hilos o la mitad del CPU)
            int defaultThreads = Math.Max(1, totalCores / 2);
            SelectComboBoxItemByTag(CpuThreadsComboBox, defaultThreads);

            // 2. RAM: Opciones en pasos de 2GB (1, 2, 4, 6, 8, 10, 12, 14, 16...)
            if (RamComboBox.Items.Count > 0 && RamComboBox.SelectedIndex < 0)
            {
                SelectComboBoxItemByTag(RamComboBox, 4);
            }
        }

        // AGREGADO: Carga las versiones base de Minecraft considerando el ajuste de Betas/Snapshots
        private async Task LoadMinecraftVersionsAsync()
        {
            try
            {
                SetLoading(true);
                bool includeSnapshots = Settingz.ShowBetaVersions;
                _versions = await _minecraftService.GetMinecraftVersionsAsync(includeSnapshots);

                BaseVersionComboBox.Items.Clear();
                foreach (var version in _versions)
                {
                    BaseVersionComboBox.Items.Add(new ComboBoxItem
                    {
                        Content = version.DisplayName,
                        Tag = version.Id
                    });
                }

                if (BaseVersionComboBox.Items.Count > 0)
                {
                    BaseVersionComboBox.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"Error al cargar versiones de Minecraft: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        // AGREGADO: Al cambiar la versión base de Minecraft, consultar y actualizar Loaders compatibles
        private async void BaseVersionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BaseVersionComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string baseVersion)
                return;

            try
            {
                SetLoading(true);
                _loaders = await _minecraftService.GetCompatibleLoadersAsync(baseVersion);

                LoaderComboBox.Items.Clear();
                foreach (var loader in _loaders)
                {
                    var cbi = new ComboBoxItem
                    {
                        Content = loader.DisplayName,
                        Tag = loader.Id
                    };
                    ToolTipService.SetToolTip(cbi, loader.Description);
                    LoaderComboBox.Items.Add(cbi);
                }

                if (LoaderComboBox.Items.Count > 0)
                {
                    LoaderComboBox.SelectedIndex = 0;
                }
                else
                {
                    LoaderVersionComboBox.Items.Clear();
                }

                // Actualizar recomendaciones de Java para la versión base seleccionada
                UpdateJavaRecommendations(baseVersion, (LoaderComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() ?? "Paper");
            }
            catch (Exception ex)
            {
                ShowStatus($"Error al cargar loaders: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        // AGREGADO: Al cambiar de Loader, cargar las versiones compatibles del loader resaltando la recomendada con '+'
        private async void LoaderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LoaderComboBox.SelectedItem is not ComboBoxItem loaderItem || loaderItem.Tag is not string loaderId)
                return;

            if (BaseVersionComboBox.SelectedItem is not ComboBoxItem baseItem || baseItem.Tag is not string baseVersion)
                return;

            try
            {
                SetLoading(true);
                _loaderVersions = await _minecraftService.GetLoaderVersionsAsync(baseVersion, loaderId);

                LoaderVersionComboBox.Items.Clear();
                int recIndex = 0;
                for (int i = 0; i < _loaderVersions.Count; i++)
                {
                    var lv = _loaderVersions[i];
                    var cbi = new ComboBoxItem
                    {
                        Content = lv.DisplayText,
                        Tag = lv.Version
                    };
                    ToolTipService.SetToolTip(cbi, lv.IsRecommended ? "Versión Recomendada por estabilidad" : $"Versión {lv.Version}");
                    LoaderVersionComboBox.Items.Add(cbi);

                    if (lv.IsRecommended)
                    {
                        recIndex = i;
                    }
                }

                if (LoaderVersionComboBox.Items.Count > 0)
                {
                    LoaderVersionComboBox.SelectedIndex = recIndex;
                }

                // Actualizar versión recomendada de Java
                UpdateJavaRecommendations(baseVersion, loaderId);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error al cargar versiones del loader: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                SetLoading(false);
            }
        }

        // AGREGADO: Actualiza el ComboBox de Java resaltando la versión recomendada con '+'
        private void UpdateJavaRecommendations(string baseVersion, string loader)
        {
            _javaVersions = _minecraftService.GetJavaRecommendations(baseVersion, loader);

            JavaComboBox.Items.Clear();
            int selectedIndex = 0;

            for (int i = 0; i < _javaVersions.Count; i++)
            {
                var jv = _javaVersions[i];
                var cbi = new ComboBoxItem
                {
                    Content = jv.DisplayText,
                    Tag = jv.MajorVersion
                };
                ToolTipService.SetToolTip(cbi, $"{jv.Name}: {jv.RecommendedReason}");
                JavaComboBox.Items.Add(cbi);

                if (jv.IsRecommended)
                {
                    selectedIndex = i;
                }
            }

            if (JavaComboBox.Items.Count > 0)
            {
                JavaComboBox.SelectedIndex = selectedIndex;
            }
        }

        // AGREGADO: Pre-ajustes de recursos (75%, 50%, 25%)
        private void PresetComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PresetComboBox.SelectedItem is not ComboBoxItem item || item.Tag is not string percentStr)
                return;

            if (!int.TryParse(percentStr, out int percentage)) return;

            int totalCores = Math.Max(1, Environment.ProcessorCount);
            int targetCores = Math.Max(1, (int)Math.Round(totalCores * (percentage / 100.0)));

            // Ajustar CPU Threads al más cercano disponible
            SelectComboBoxItemByTag(CpuThreadsComboBox, targetCores);

            // Ajustar RAM basada en porcentaje (estimado estándar: 16GB total -> 12GB (75%), 8GB (50%), 4GB (25%))
            int ramGb = percentage switch
            {
                >= 75 => 12,
                >= 50 => 6,
                _ => 2
            };
            SelectComboBoxItemByTag(RamComboBox, ramGb);
        }

        // AGREGADO: Selección de icono para el servidor
        private async void SelectIconButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var picker = new FileOpenPicker();
                picker.ViewMode = PickerViewMode.Thumbnail;
                picker.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
                picker.FileTypeFilter.Add(".png");
                picker.FileTypeFilter.Add(".jpg");
                picker.FileTypeFilter.Add(".jpeg");

                // Para WinUI 3 en desktop necesitamos inicializar el window handle
                var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindowInstance);
                WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

                var file = await picker.PickSingleFileAsync();
                if (file != null)
                {
                    _selectedIconFilePath = file.Path;
                    var bitmap = new BitmapImage();
                    using (var stream = await file.OpenReadAsync())
                    {
                        await bitmap.SetSourceAsync(stream);
                    }
                    ServerIconPreview.ImageSource = bitmap;
                    ShowStatus("Icono del servidor seleccionado exitosamente.", InfoBarSeverity.Success);
                }
            }
            catch (Exception ex)
            {
                ShowStatus($"No se pudo cargar la imagen: {ex.Message}", InfoBarSeverity.Warning);
            }
        }

        // AGREGADO: Acción principal para crear el servidor
        private async void CreateServerButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var config = BuildServerConfig();
                if (string.IsNullOrWhiteSpace(config.ServerName))
                {
                    ShowStatus("Por favor ingresa un nombre válido para el servidor.", InfoBarSeverity.Warning);
                    return;
                }

                SetLoading(true);
                CreateServerButton.IsEnabled = false;

                // Crear carpeta de datos del servidor y sus archivos de configuración
                var serverDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "ZenithCraftServer",
                    "servers",
                    SanitizeFolderName(config.ServerName));

                Directory.CreateDirectory(serverDir);

                // 1. Guardar metadatos del servidor (zenith-server.json)
                var configPath = Path.Combine(serverDir, "zenith-server.json");
                var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
                await File.WriteAllTextAsync(configPath, json);

                // 2. Generar archivo eula.txt con consentimiento
                var eulaPath = Path.Combine(serverDir, "eula.txt");
                await File.WriteAllTextAsync(eulaPath, "# Por Zenith Craft Server\neula=true\n");

                // 3. Generar server.properties con los parámetros elegidos
                var serverPropsPath = Path.Combine(serverDir, "server.properties");
                var serverPropsContent =
                    $"# Zenith Craft Server Properties\n" +
                    $"motd={config.Motd.Replace("\n", "\\n")}\n" +
                    $"server-port={config.Port}\n" +
                    $"max-players={config.MaxPlayers}\n" +
                    $"difficulty={config.Difficulty}\n" +
                    $"online-mode=true\n" +
                    $"enable-command-block=true\n" +
                    $"view-distance=10\n";
                await File.WriteAllTextAsync(serverPropsPath, serverPropsContent);

                // 4. Generar script de inicio start.bat / start.sh con control de RAM e hilos de CPU
                var startBatPath = Path.Combine(serverDir, "start.bat");
                var startScript =
                    $"@echo off\n" +
                    $"title Zenith Server - {config.ServerName}\n" +
                    $"REM Java flags con optimizaciones Aikar y limite de RAM / hilos\n" +
                    $"java -Xms{config.RamGb}G -Xmx{config.RamGb}G -XX:+UseG1GC -XX:ParallelGCThreads={config.CpuThreads} -XX:ConcGCThreads={Math.Max(1, config.CpuThreads / 2)} -jar server.jar nogui\n" +
                    $"pause\n";
                await File.WriteAllTextAsync(startBatPath, startScript);

                // 5. Copiar icono de servidor a server-icon.png si fue provisto
                if (!string.IsNullOrEmpty(_selectedIconFilePath) && File.Exists(_selectedIconFilePath))
                {
                    try
                    {
                        var destIconPath = Path.Combine(serverDir, "server-icon.png");
                        File.Copy(_selectedIconFilePath, destIconPath, true);
                    }
                    catch { /* silencioso si el formato difiere */ }
                }

                // Indicar que ya no es la primera apertura
                Settingz.FirstOpen = false;

                ShowStatus($"¡Servidor '{config.ServerName}' creado con éxito en {serverDir}! (Minecraft {config.MinecraftVersion} con {config.Loader} {config.LoaderVersion})", InfoBarSeverity.Success);
            }
            catch (Exception ex)
            {
                ShowStatus($"Error al crear el servidor: {ex.Message}", InfoBarSeverity.Error);
            }
            finally
            {
                SetLoading(false);
                CreateServerButton.IsEnabled = true;
            }
        }

        // AGREGADO: Construye el objeto de configuración a partir de los controles de la UI
        private ServerCreationConfig BuildServerConfig()
        {
            var config = new ServerCreationConfig
            {
                ServerName = string.IsNullOrWhiteSpace(NameServerBox.Text) ? "Zenith Server" : NameServerBox.Text.Trim(),
                Motd = string.IsNullOrWhiteSpace(MOTDServerBox.Text) ? "Un servidor de Zenith Craft" : MOTDServerBox.Text.Trim(),
                IconPath = _selectedIconFilePath,
                MaxPlayers = (int)MaxPlayersNumberBox.Value,
                Port = (int)PortNumberBox.Value,
            };

            if (BaseVersionComboBox.SelectedItem is ComboBoxItem baseItem && baseItem.Tag is string baseVer)
                config.MinecraftVersion = baseVer;

            if (LoaderComboBox.SelectedItem is ComboBoxItem loaderItem && loaderItem.Tag is string loader)
                config.Loader = loader;

            if (LoaderVersionComboBox.SelectedItem is ComboBoxItem lvItem && lvItem.Tag is string loaderVer)
                config.LoaderVersion = loaderVer;

            if (DifficultyComboBox.SelectedItem is ComboBoxItem diffItem && diffItem.Tag is string diff)
                config.Difficulty = diff;

            if (RamComboBox.SelectedItem is ComboBoxItem ramItem && int.TryParse(ramItem.Tag?.ToString(), out int ramVal))
                config.RamGb = ramVal;
            else if (int.TryParse(RamComboBox.Text, out int ramCustom))
                config.RamGb = ramCustom;

            if (CpuThreadsComboBox.SelectedItem is ComboBoxItem cpuItem && int.TryParse(cpuItem.Tag?.ToString(), out int cpuVal))
                config.CpuThreads = cpuVal;

            if (JavaComboBox.SelectedItem is ComboBoxItem javaItem)
                config.JavaVersion = javaItem.Content?.ToString() ?? "Java 21";

            return config;
        }

        private static string SanitizeFolderName(string name)
        {
            var invalid = Path.GetInvalidFileNameChars();
            return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
        }

        private void SelectComboBoxItemByTag(ComboBox combo, object targetTag)
        {
            foreach (var item in combo.Items)
            {
                if (item is ComboBoxItem cbi && cbi.Tag?.ToString() == targetTag.ToString())
                {
                    combo.SelectedItem = cbi;
                    return;
                }
            }
            if (combo.Items.Count > 0 && combo.SelectedIndex < 0)
            {
                combo.SelectedIndex = 0;
            }
        }

        private void SetLoading(bool isLoading)
        {
            OperationProgressBar.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
        }

        private void ShowStatus(string message, InfoBarSeverity severity)
        {
            ServerStatusInfoBar.Message = message;
            ServerStatusInfoBar.Severity = severity;
            ServerStatusInfoBar.IsOpen = true;
        }
    }
}
