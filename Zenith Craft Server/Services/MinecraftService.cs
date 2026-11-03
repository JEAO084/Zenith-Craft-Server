using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Zenith_Craft_Server.Models;

namespace Zenith_Craft_Server.Services;

// AGREGADO: Servicio para obtener metadatos y compatibilidad de versiones de Minecraft y Loaders
public class MinecraftService
{
    private static readonly Lazy<MinecraftService> _instance = new(() => new MinecraftService());
    public static MinecraftService Instance => _instance.Value;

    private readonly HttpClient _httpClient;

    // Cachés en memoria para optimizar peticiones
    private List<MinecraftVersion>? _cachedVersions;
    private readonly Dictionary<string, List<string>> _purpurVersionsCache = new();
    private readonly Dictionary<string, List<string>> _paperVersionsCache = new();
    private readonly Dictionary<string, List<string>> _spongeVersionsCache = new();
    private readonly Dictionary<string, List<string>> _spigotVersionsCache = new();
    private Dictionary<string, string>? _forgePromosCache;
    private List<string>? _neoForgeVersionsCache;
    private HashSet<string>? _fabricGameVersions;

    public MinecraftService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("ZenithCraftServer/1.0 (contact@zenithcraft.local)");
        _httpClient.Timeout = TimeSpan.FromSeconds(10);
    }

    /// <summary>
    /// Obtiene la lista de versiones de Minecraft (oficiales de Mojang)
    /// </summary>
    public async Task<List<MinecraftVersion>> GetMinecraftVersionsAsync(bool includeSnapshots)
    {
        if (_cachedVersions == null)
        {
            try
            {
                var url = "https://launchermeta.mojang.com/mc/game/version_manifest_v2.json";
                var json = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                var versionsElement = doc.RootElement.GetProperty("versions");

                var list = new List<MinecraftVersion>();
                foreach (var item in versionsElement.EnumerateArray())
                {
                    var id = item.GetProperty("id").GetString() ?? "";
                    var type = item.GetProperty("type").GetString() ?? "release";
                    list.Add(new MinecraftVersion
                    {
                        Id = id,
                        Type = type
                    });
                }
                _cachedVersions = list;
            }
            catch
            {
                // Fallback offline en caso de no haber conexión
                _cachedVersions = GetOfflineDefaultVersions();
            }
        }

        if (includeSnapshots)
        {
            return _cachedVersions;
        }

        return _cachedVersions.Where(v => !v.IsSnapshot).ToList();
    }

    /// <summary>
    /// Devuelve los loaders compatibles disponibles para la versión base seleccionada.
    /// Fuentes analizadas: Forge, Fabric, NeoForge, Paper, Spigot, Purpur, SpongeForge.
    /// </summary>
    public async Task<List<LoaderInfo>> GetCompatibleLoadersAsync(string baseVersion)
    {
        var loaders = new List<LoaderInfo>();

        // 1. Paper (V3 API)
        if (await IsPaperCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "Paper",
                DisplayName = "Paper",
                Description = "Servidor optimizado de alto rendimiento compatible con plugins de Spigot/Bukkit"
            });
        }

        // 2. Purpur
        if (await IsPurpurCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "Purpur",
                DisplayName = "Purpur",
                Description = "Bifurcación de Paper diseñada para máxima configurabilidad y rendimiento"
            });
        }

        // 3. Fabric
        if (await IsFabricCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "Fabric",
                DisplayName = "Fabric",
                Description = "Loader moderno y ligero para mods y personalización rápida"
            });
        }

        // 4. Forge
        if (await IsForgeCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "Forge",
                DisplayName = "Forge",
                Description = "El loader tradicional y más extenso para mods de Minecraft"
            });
        }

        // 5. NeoForge (Para Minecraft 1.20.2 o superior)
        if (await IsNeoForgeCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "NeoForge",
                DisplayName = "NeoForge",
                Description = "Continuación moderna de Forge para versiones recientes"
            });
        }

        // 6. Spigot
        if (await IsSpigotCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "Spigot",
                DisplayName = "Spigot",
                Description = "Servidor estándar para la ejecución de plugins"
            });
        }

        // 7. SpongeForge
        if (await IsSpongeForgeCompatibleAsync(baseVersion))
        {
            loaders.Add(new LoaderInfo
            {
                Id = "SpongeForge",
                DisplayName = "SpongeForge",
                Description = "Plataforma potente que combina mods de Forge con plugins de Sponge"
            });
        }

        // Si por alguna razón ninguna coincidió (red caída), proveer Paper y Fabric por defecto
        if (loaders.Count == 0)
        {
            loaders.Add(new LoaderInfo { Id = "Paper", DisplayName = "Paper", Description = "Servidor optimizado" });
            loaders.Add(new LoaderInfo { Id = "Fabric", DisplayName = "Fabric", Description = "Loader ligero" });
        }

        return loaders;
    }

    /// <summary>
    /// Obtiene las versiones de loader compatibles para la versión base seleccionada.
    /// Resalta con '+' la versión recomendada.
    /// </summary>
    public async Task<List<LoaderVersionItem>> GetLoaderVersionsAsync(string baseVersion, string loaderId)
    {
        var result = new List<LoaderVersionItem>();

        try
        {
            switch (loaderId.ToLowerInvariant())
            {
                case "fabric":
                    result = await GetFabricLoaderVersionsAsync(baseVersion);
                    break;
                case "paper":
                    result = await GetPaperBuildsAsync(baseVersion);
                    break;
                case "purpur":
                    result = await GetPurpurBuildsAsync(baseVersion);
                    break;
                case "neoforge":
                    result = await GetNeoForgeVersionsAsync(baseVersion);
                    break;
                case "forge":
                    result = await GetForgeVersionsAsync(baseVersion);
                    break;
                case "spigot":
                    result = await GetSpigotVersionsAsync(baseVersion);
                    break;
                case "spongeforge":
                    result = await GetSpongeForgeVersionsAsync(baseVersion);
                    break;
            }
        }
        catch
        {
            // Silencioso, usaremos fallback
        }

        if (result.Count == 0)
        {
            result = GetFallbackLoaderVersions(loaderId, baseVersion);
        }

        // Asegurar que al menos una versión esté marcada como recomendada si hay elementos
        if (result.Count > 0 && !result.Any(v => v.IsRecommended))
        {
            result[0].IsRecommended = true;
        }

        return result;
    }

    /// <summary>
    /// Devuelve las versiones de Java recomendadas según la versión base de Minecraft y el loader.
    /// Marca con '+' la versión de Java recomendada.
    /// </summary>
    public List<JavaVersionItem> GetJavaRecommendations(string baseVersion, string loader)
    {
        int recommendedJava = GetRecommendedJavaMajorVersion(baseVersion);

        var list = new List<JavaVersionItem>
        {
            new JavaVersionItem
            {
                MajorVersion = 8,
                Name = "Java 8",
                IsRecommended = recommendedJava == 8,
                RecommendedReason = "Ideal para Minecraft 1.16.5 y anteriores"
            },
            new JavaVersionItem
            {
                MajorVersion = 16,
                Name = "Java 16",
                IsRecommended = recommendedJava == 16,
                RecommendedReason = "Ideal para Minecraft 1.17"
            },
            new JavaVersionItem
            {
                MajorVersion = 17,
                Name = "Java 17",
                IsRecommended = recommendedJava == 17,
                RecommendedReason = "Ideal para Minecraft 1.18 a 1.20.4"
            },
            new JavaVersionItem
            {
                MajorVersion = 21,
                Name = "Java 21 (LTS)",
                IsRecommended = recommendedJava == 21,
                RecommendedReason = "Requerido para Minecraft 1.20.5+ y óptimo para versiones recientes"
            }
        };

        return list;
    }

    private int GetRecommendedJavaMajorVersion(string baseVersion)
    {
        if (string.IsNullOrWhiteSpace(baseVersion)) return 21;

        // Versiones 26.x o superiores -> Java 21+
        if (baseVersion.StartsWith("26.") || baseVersion.StartsWith("25."))
            return 21;

        // Comprobación de versión semántica para 1.x
        var parts = baseVersion.Split('.');
        if (parts.Length >= 2 && int.TryParse(parts[1], out int minor))
        {
            if (minor >= 21) return 21;
            if (minor == 20)
            {
                if (parts.Length >= 3 && int.TryParse(parts[2], out int patch) && patch >= 5)
                    return 21;
                return 17;
            }
            if (minor >= 18) return 17;
            if (minor == 17) return 16;
            return 8;
        }

        return 21;
    }

    // --- Métodos de verificación y obtención de fuentes oficiales ---

    private async Task<bool> IsPaperCompatibleAsync(string baseVersion)
    {
        try
        {
            var builds = await GetPaperBuildsAsync(baseVersion);
            return builds.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<LoaderVersionItem>> GetPaperBuildsAsync(string baseVersion)
    {
        if (_paperVersionsCache.TryGetValue(baseVersion, out var cached))
        {
            return MapBuildsToItems(cached);
        }

        try
        {
            var url = $"https://fill.papermc.io/v3/projects/paper/versions/{baseVersion}/builds";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var builds = new List<string>();
            foreach (var element in doc.RootElement.EnumerateArray())
            {
                if (element.TryGetProperty("id", out var idProp))
                {
                    builds.Add(idProp.ToString());
                }
            }

            // Invertir para mostrar las más recientes primero
            builds.Reverse();
            _paperVersionsCache[baseVersion] = builds;
            return MapBuildsToItems(builds);
        }
        catch
        {
            return new List<LoaderVersionItem>();
        }
    }

    private async Task<bool> IsPurpurCompatibleAsync(string baseVersion)
    {
        try
        {
            var builds = await GetPurpurBuildsAsync(baseVersion);
            return builds.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<LoaderVersionItem>> GetPurpurBuildsAsync(string baseVersion)
    {
        if (_purpurVersionsCache.TryGetValue(baseVersion, out var cached))
        {
            return MapBuildsToItems(cached);
        }

        try
        {
            var url = $"https://api.purpurmc.org/v2/purpur/{baseVersion}";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var builds = new List<string>();
            if (doc.RootElement.TryGetProperty("builds", out var buildsObj) &&
                buildsObj.TryGetProperty("all", out var allArray))
            {
                foreach (var b in allArray.EnumerateArray())
                {
                    builds.Add(b.GetString() ?? "");
                }
            }

            builds.Reverse();
            _purpurVersionsCache[baseVersion] = builds;
            return MapBuildsToItems(builds);
        }
        catch
        {
            return new List<LoaderVersionItem>();
        }
    }

    private async Task<bool> IsFabricCompatibleAsync(string baseVersion)
    {
        try
        {
            if (_fabricGameVersions == null)
            {
                var url = "https://meta.fabricmc.net/v2/versions/game";
                var json = await _httpClient.GetStringAsync(url);
                using var doc = JsonDocument.Parse(json);
                _fabricGameVersions = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var item in doc.RootElement.EnumerateArray())
                {
                    if (item.TryGetProperty("version", out var v))
                    {
                        var str = v.GetString();
                        if (!string.IsNullOrEmpty(str))
                            _fabricGameVersions.Add(str);
                    }
                }
            }

            return _fabricGameVersions.Contains(baseVersion);
        }
        catch
        {
            return true; // Fallback razonable
        }
    }

    private async Task<List<LoaderVersionItem>> GetFabricLoaderVersionsAsync(string baseVersion)
    {
        try
        {
            var url = $"https://meta.fabricmc.net/v2/versions/loader/{baseVersion}";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var items = new List<LoaderVersionItem>();
            bool isFirst = true;
            foreach (var item in doc.RootElement.EnumerateArray())
            {
                if (item.TryGetProperty("loader", out var loaderObj) &&
                    loaderObj.TryGetProperty("version", out var vProp))
                {
                    var v = vProp.GetString() ?? "";
                    items.Add(new LoaderVersionItem
                    {
                        Version = v,
                        IsRecommended = isFirst
                    });
                    isFirst = false;
                }
            }

            return items;
        }
        catch
        {
            return new List<LoaderVersionItem>();
        }
    }

    private async Task<bool> IsForgeCompatibleAsync(string baseVersion)
    {
        await EnsureForgePromosLoadedAsync();
        if (_forgePromosCache == null) return false;

        return _forgePromosCache.ContainsKey($"{baseVersion}-recommended") ||
               _forgePromosCache.ContainsKey($"{baseVersion}-latest");
    }

    private async Task<List<LoaderVersionItem>> GetForgeVersionsAsync(string baseVersion)
    {
        await EnsureForgePromosLoadedAsync();
        var items = new List<LoaderVersionItem>();

        string? rec = null;
        string? latest = null;

        _forgePromosCache?.TryGetValue($"{baseVersion}-recommended", out rec);
        _forgePromosCache?.TryGetValue($"{baseVersion}-latest", out latest);

        if (!string.IsNullOrEmpty(rec))
        {
            items.Add(new LoaderVersionItem { Version = rec, IsRecommended = true });
        }

        if (!string.IsNullOrEmpty(latest) && latest != rec)
        {
            items.Add(new LoaderVersionItem { Version = latest, IsRecommended = rec == null });
        }

        return items;
    }

    private async Task EnsureForgePromosLoadedAsync()
    {
        if (_forgePromosCache != null) return;
        try
        {
            var url = "https://files.minecraftforge.net/net/minecraftforge/forge/promotions_slim.json";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            _forgePromosCache = new Dictionary<string, string>();
            if (doc.RootElement.TryGetProperty("promos", out var promos))
            {
                foreach (var prop in promos.EnumerateObject())
                {
                    _forgePromosCache[prop.Name] = prop.Value.GetString() ?? "";
                }
            }
        }
        catch
        {
            _forgePromosCache = new Dictionary<string, string>();
        }
    }

    private async Task<bool> IsNeoForgeCompatibleAsync(string baseVersion)
    {
        await EnsureNeoForgeVersionsLoadedAsync();
        if (_neoForgeVersionsCache == null) return false;

        // NeoForge nombra versiones según MC, ej: 20.4.x o 21.1.x o 26.x
        var prefix = GetNeoForgePrefix(baseVersion);
        return _neoForgeVersionsCache.Any(v => v.StartsWith(prefix));
    }

    private async Task<List<LoaderVersionItem>> GetNeoForgeVersionsAsync(string baseVersion)
    {
        await EnsureNeoForgeVersionsLoadedAsync();
        var items = new List<LoaderVersionItem>();
        if (_neoForgeVersionsCache == null) return items;

        var prefix = GetNeoForgePrefix(baseVersion);
        var matching = _neoForgeVersionsCache.Where(v => v.StartsWith(prefix)).Reverse().ToList();

        bool isFirst = true;
        foreach (var v in matching)
        {
            items.Add(new LoaderVersionItem
            {
                Version = v,
                IsRecommended = isFirst
            });
            isFirst = false;
        }

        return items;
    }

    private string GetNeoForgePrefix(string mcVersion)
    {
        if (mcVersion.StartsWith("1."))
        {
            // Ej: "1.20.4" -> "20.4"
            var parts = mcVersion.Split('.');
            if (parts.Length >= 2)
            {
                return parts.Length > 2 ? $"{parts[1]}.{parts[2]}" : $"{parts[1]}.0";
            }
        }
        return mcVersion;
    }

    private async Task EnsureNeoForgeVersionsLoadedAsync()
    {
        if (_neoForgeVersionsCache != null) return;
        try
        {
            var url = "https://maven.neoforged.net/api/maven/versions/releases/net/neoforged/neoforge";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);
            _neoForgeVersionsCache = new List<string>();
            if (doc.RootElement.TryGetProperty("versions", out var array))
            {
                foreach (var item in array.EnumerateArray())
                {
                    var str = item.GetString();
                    if (!string.IsNullOrEmpty(str))
                        _neoForgeVersionsCache.Add(str);
                }
            }
        }
        catch
        {
            _neoForgeVersionsCache = new List<string>();
        }
    }

    private async Task<bool> IsSpigotCompatibleAsync(string baseVersion)
    {
        try
        {
            var versions = await GetSpigotVersionsAsync(baseVersion);
            return versions.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<LoaderVersionItem>> GetSpigotVersionsAsync(string baseVersion)
    {
        if (_spigotVersionsCache.TryGetValue(baseVersion, out var cached))
        {
            return MapBuildsToItems(cached);
        }

        try
        {
            var url = "https://hub.spigotmc.org/versions/";
            var html = await _httpClient.GetStringAsync(url);
            var regex = new Regex(@"href=""([0-9\.]+)\.json""");
            var matches = regex.Matches(html);

            var list = new List<string>();
            foreach (Match m in matches)
            {
                var v = m.Groups[1].Value;
                if (v.Equals(baseVersion, StringComparison.OrdinalIgnoreCase))
                {
                    list.Add(v);
                }
            }

            _spigotVersionsCache[baseVersion] = list;
            return MapBuildsToItems(list);
        }
        catch
        {
            return new List<LoaderVersionItem>();
        }
    }

    private async Task<bool> IsSpongeForgeCompatibleAsync(string baseVersion)
    {
        try
        {
            var list = await GetSpongeForgeVersionsAsync(baseVersion);
            return list.Count > 0;
        }
        catch
        {
            return false;
        }
    }

    private async Task<List<LoaderVersionItem>> GetSpongeForgeVersionsAsync(string baseVersion)
    {
        if (_spongeVersionsCache.TryGetValue(baseVersion, out var cached))
        {
            return MapBuildsToItems(cached);
        }

        try
        {
            var url = $"https://dl-api.spongepowered.org/v2/groups/org.spongepowered/artifacts/spongeforge/versions?tags=minecraft:{baseVersion}";
            var json = await _httpClient.GetStringAsync(url);
            using var doc = JsonDocument.Parse(json);

            var builds = new List<string>();
            if (doc.RootElement.TryGetProperty("artifacts", out var artifacts))
            {
                foreach (var prop in artifacts.EnumerateObject())
                {
                    builds.Add(prop.Name);
                }
            }

            _spongeVersionsCache[baseVersion] = builds;
            return MapBuildsToItems(builds);
        }
        catch
        {
            return new List<LoaderVersionItem>();
        }
    }

    private List<LoaderVersionItem> MapBuildsToItems(List<string> list)
    {
        var result = new List<LoaderVersionItem>();
        bool first = true;
        foreach (var item in list)
        {
            result.Add(new LoaderVersionItem
            {
                Version = item,
                IsRecommended = first
            });
            first = false;
        }
        return result;
    }

    private List<LoaderVersionItem> GetFallbackLoaderVersions(string loader, string baseVersion)
    {
        return new List<LoaderVersionItem>
        {
            new LoaderVersionItem { Version = $"{baseVersion}-Latest", IsRecommended = true },
            new LoaderVersionItem { Version = $"{baseVersion}-Stable", IsRecommended = false }
        };
    }

    private List<MinecraftVersion> GetOfflineDefaultVersions()
    {
        return new List<MinecraftVersion>
        {
            new MinecraftVersion { Id = "1.21.4", Type = "release" },
            new MinecraftVersion { Id = "1.21.3", Type = "release" },
            new MinecraftVersion { Id = "1.21.1", Type = "release" },
            new MinecraftVersion { Id = "1.20.6", Type = "release" },
            new MinecraftVersion { Id = "1.20.4", Type = "release" },
            new MinecraftVersion { Id = "1.20.1", Type = "release" },
            new MinecraftVersion { Id = "1.19.4", Type = "release" },
            new MinecraftVersion { Id = "1.18.2", Type = "release" },
            new MinecraftVersion { Id = "1.16.5", Type = "release" },
            new MinecraftVersion { Id = "1.12.2", Type = "release" },
            new MinecraftVersion { Id = "1.8.9", Type = "release" },
            new MinecraftVersion { Id = "1.7.10", Type = "release" }
        };
    }
}
