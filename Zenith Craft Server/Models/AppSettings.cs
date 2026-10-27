// Models/AppSettings.cs
using System.Text.Json.Serialization;

public class AppSettings
{
    public int Backdrop { get; set; } = 0;
    public int Theme { get; set; } = 0;

    public string ServerPath { get; set; } = "";
    public int MaxPlayers { get; set; } = 20;
    public bool AutoStart { get; set; } = false;
    public string LastPage { get; set; } = "Home";

    // Ignora esta propiedad al serializar (útil para campos calculados)
    [JsonIgnore]
    public bool IsValid => MaxPlayers > 0;
}