// Models/AppSettings.cs
using System.Text.Json.Serialization;

namespace Zenith_Craft_Server.Models
{
    // MODIFICADO: Agregado namespace Zenith_Craft_Server.Models para una estructura de proyecto consistente y limpia.
    public class AppSettings
    {
        public int Backdrop { get; set; } = 0;
        public int Theme { get; set; } = 0;

        public string ServerPath { get; set; } = "";
        public int MaxPlayers { get; set; } = 20;
        public bool AutoStart { get; set; } = false;
        public string LastPage { get; set; } = "Home";

        // MODIFICADO: Ignora esta propiedad al serializar en JSON.
        [JsonIgnore]
        public bool IsValid => MaxPlayers > 0;
    }
}