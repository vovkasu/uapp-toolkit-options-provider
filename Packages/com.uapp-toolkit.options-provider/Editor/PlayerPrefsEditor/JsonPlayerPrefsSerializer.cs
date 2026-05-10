using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using Formatting = Unity.Plastic.Newtonsoft.Json.Formatting;

namespace UAppToolKit.Options.Editor.PlayerPrefsTool
{
    /// <summary>
    /// Implements <see cref="IPlayerPrefsSerializer"/> using indented JSON
    /// via Newtonsoft.Json (Unity Plastic / com.unity.nuget.newtonsoft-json).
    /// </summary>
    internal sealed class JsonPlayerPrefsSerializer : IPlayerPrefsSerializer
    {
        // ─── JSON DTO ─────────────────────────────────────────────────────────

        private class PlayerPrefDto
        {
            [JsonProperty("key")]   public string Key   { get; set; }
            [JsonProperty("type")]  public string Type  { get; set; }
            [JsonProperty("value")] public string Value { get; set; }
        }

        // ─── IPlayerPrefsSerializer ───────────────────────────────────────────

        public string Serialize(List<PlayerPrefStore> prefs)
        {
            var dtos = prefs.Select(p => new PlayerPrefDto
            {
                Key   = p.name,
                Type  = p.value.TypeId,
                Value = p.StringValue,
            }).ToList();

            return JsonConvert.SerializeObject(dtos, Formatting.Indented);
        }

        public List<PlayerPrefStore> Deserialize(string data)
        {
            var dtos = JsonConvert.DeserializeObject<List<PlayerPrefDto>>(data)
                       ?? new List<PlayerPrefDto>();

            return dtos
                .Where(d => d.Key != null && d.Type != null && d.Value != null)
                .Select(d => PlayerPrefStore.FromTypeString(d.Key, d.Type, d.Value))
                .ToList();
        }
    }
}

