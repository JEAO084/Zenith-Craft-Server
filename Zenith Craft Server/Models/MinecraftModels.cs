using System;
using System.Collections.Generic;

namespace Zenith_Craft_Server.Models;

// AGREGADO: Modelo para representar versiones de Minecraft base
public class MinecraftVersion
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "release"; // "release" o "snapshot"
    public string DisplayName => Type == "snapshot" ? $"{Id} (Beta/Snapshot)" : Id;
    public bool IsSnapshot => Type != "release";
}

// AGREGADO: Modelo para representar tipos de loader soportados
public class LoaderInfo
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// AGREGADO: Modelo para representar versiones de loader disponibles
public class LoaderVersionItem
{
    public string Version { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    // Muestra '+ <version>' si es la recomendada, tal como se solicitó
    public string DisplayText => IsRecommended ? $"+ {Version}" : Version;
}

// AGREGADO: Modelo para versiones de Java recomendadas según la versión del servidor
public class JavaVersionItem
{
    public int MajorVersion { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsRecommended { get; set; }
    // Muestra '+ Java <n>' si es la recomendada
    public string DisplayText => IsRecommended ? $"+ {Name}" : Name;
    public string RecommendedReason { get; set; } = string.Empty;
}

// AGREGADO: Modelo para los datos recopilados al crear un servidor
public class ServerCreationConfig
{
    public string ServerName { get; set; } = "Zenith Server";
    public string Motd { get; set; } = "Un servidor de Zenith Craft";
    public string IconPath { get; set; } = string.Empty;
    public string MinecraftVersion { get; set; } = string.Empty;
    public string Loader { get; set; } = string.Empty;
    public string LoaderVersion { get; set; } = string.Empty;
    public int MaxPlayers { get; set; } = 20;
    public int Port { get; set; } = 25565;
    public string Difficulty { get; set; } = "normal";
    public int RamGb { get; set; } = 4;
    public int CpuThreads { get; set; } = 2;
    public string JavaVersion { get; set; } = "Java 21";
}
