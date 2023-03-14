using System.Text.Json.Serialization;

namespace OsuSkinMixer.Statics;

public static partial class Settings
{
    public class SettingsContent
    {
        [JsonPropertyName("last_version")]
        public string LastVersion { get; set; }

        [JsonPropertyName("osu_folder")]
        public string OsuFolder { get; set; }

        [JsonPropertyName("auto-update")]
        public bool AutoUpdate { get; set; }

        [JsonPropertyName("use_compact_skin_selector")]
        public bool UseCompactSkinSelector { get; set; }

        [JsonPropertyName("arrow_button_pressed")]
        public bool ArrowButtonPressed { get; set; }

        [JsonPropertyName("skins_folder")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
        public string SkinsFolder { get; set; }
    }
}