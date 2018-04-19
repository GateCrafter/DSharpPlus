using Newtonsoft.Json;

namespace DSharpPlus.Lavalink.LavalinkEntities
{
    public sealed class LavalinkDispatch
    {
        [JsonProperty("op")]
        public string Operation { get; set; }

        [JsonProperty("guildId")]
        public ulong GuildId { get; set; }

        [JsonProperty("sessionId", NullValueHandling = NullValueHandling.Ignore)]
        public string SessionId { get; set; }

        [JsonProperty("event", NullValueHandling = NullValueHandling.Ignore)]
        public object Event { get; set; }

        [JsonProperty("track", NullValueHandling = NullValueHandling.Ignore)]
        public string Track { get; set; }

        [JsonProperty("startTime", NullValueHandling = NullValueHandling.Ignore)]
        public int? StartTime { get; set; }

        [JsonProperty("endTime", NullValueHandling = NullValueHandling.Ignore)]
        public int? EndTime { get; set; }

        [JsonProperty("pause", NullValueHandling = NullValueHandling.Ignore)]
        public bool? Pause { get; set; }

        [JsonProperty("position", NullValueHandling = NullValueHandling.Ignore)]
        public int? Position { get; set; }

        [JsonProperty("volume", NullValueHandling = NullValueHandling.Ignore)]
        public int? Volume { get; set; }
    }
}
