using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.VoiceEntities
{
    public sealed class VoiceServerUpdatePayload
    {
        [JsonProperty("token")]
        public string Token { get; set; }

        [JsonProperty("guild_id")]
        public ulong GuildId { get; set; }

        [JsonProperty("endpoint")]
        public string Endpoint { get; set; }
    }
}
