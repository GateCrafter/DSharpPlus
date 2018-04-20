using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DSharpPlus;
using DSharpPlus.EventArgs;

namespace DSharpPlus.Lavalink
{
    public class PlayerManager : PlayerStore
    {
        public DiscordClient Client;
        public Dictionary<string, LavalinkNode> Nodes;
        public ulong UserId;
        public int Shards;
        public PlayerManagerOptions Options;

        public string SessionId;
        public PlayerManager(DiscordClient client, List<LavalinkNodeOptions> nodes, PlayerManagerOptions options)
            : base(options.Player)
        {
            this.Client = client;
            this.Nodes = new Dictionary<string, LavalinkNode>();
            this.UserId = options.UserId ?? client.CurrentUser.Id;
            this.Shards = options.Shards;
            this.Options = options;

            nodes.ForEach(node => this.CreateNode(node));

            this.Client.VoiceStateUpdated += DiscordClient_VoiceStateUpdated;
            this.Client.VoiceServerUpdated += DiscordClient_VoiceServerUpdated;
        }

        public void CreateNode(LavalinkNodeOptions options)
        {
            var node = new LavalinkNode(this, options);

            node.OnError += (sender, e) => this.Client.EventErrorHandler("", e.Error);
            node.OnDisconnect += (sender, e) =>
            {
                if (this.Nodes.Count != 0)
                {
                    Console.WriteLine("[Lavalink] - No available voice nodes.");
                    return;
                }
                Console.WriteLine(e.Message);
            };
            node.OnMessage += (sender, e) => this.OnMessage(e.Data);

            this.Nodes.Add(options.Host, node);
        }

        public bool RemoveNode(string hostId)
        {
            LavalinkNode node = null;
            bool successNode = this.Nodes.TryGetValue(hostId, out node);
            if (!successNode || node == null) return false;
            node.RemoveAllListener();
            return this.Nodes.Remove(hostId);
        }

        public bool OnMessage(JObject data)
        {
            JToken op = null;
            if (data == null || !data.TryGetValue("op", out op))
                return false;
            switch (op.ToString())
            {
                case "event":
                    var player = this[(ulong)data["guildId"]];
                    if (player != null)
                        return player.Event(data);
                    break;
            }
            return false;
        }

        public Player Join(ulong guildId, ulong channelId, string hostId, bool muted = false, bool deafened = false)
        {
            var player = this[guildId];

            var vsd = new VoiceEntities.VoiceDispatch()
            {
                OpCode = 4,
                Payload = new VoiceEntities.VoiceStateUpdatePayload()
                {
                    GuildId = guildId,
                    ChannelId = channelId,
                    Muted = muted,
                    Deafened = deafened,
                }
            };

            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            this.Client._webSocketClient.SendMessage(vsj);

            return this.Add(new PlayerOptions()
            {
                GuildId = guildId,
                Client = this.Client,
                ChannelId = channelId
            });
        }

        public bool Leave(ulong guildId)
        {
            var player = this[guildId];

            var vsd = new VoiceEntities.VoiceDispatch()
            {
                OpCode = 4,
                Payload = new VoiceEntities.VoiceStateUpdatePayload()
                {
                    GuildId = guildId,
                    ChannelId = null,
                    Muted = false,
                    Deafened = false,
                }
            };

            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            this.Client._webSocketClient.SendMessage(vsj);

            player.RemoveAllListener();
            player.Destroy();
            return this.Remove(guildId);
        }

        private Task DiscordClient_VoiceStateUpdated(EventArgs.VoiceStateUpdateEventArgs e)
        {
            return Task.Run(() =>
            {
                this.SessionId = e.SessionId;
            });
        }

        private Task DiscordClient_VoiceServerUpdated(EventArgs.VoiceServerUpdateEventArgs e)
        {
            return Task.Run(() =>
            {
                if (e.Guild == null) return;
                Player player = null;
                bool success = this.TryGetValue(e.Guild.Id, out player);
                if (!success || player == null) return;

                player.Connect(SessionId, new VoiceEntities.VoiceServerUpdatePayload()
                {
                    Endpoint = e.Endpoint,
                    GuildId = e.Guild.Id,
                    Token = e.VoiceToken
                });
            });
        }

        public Player SpawnPlayer(ulong guildId, ulong channelId, string hostId)
        {
            Player player = null;
            bool successPlayer = this.TryGetValue(guildId, out player);
            if (successPlayer && player != null) return player;

            LavalinkNode node = null;
            bool successNode = this.Nodes.TryGetValue(hostId, out node);
            if (!successNode || node == null) throw new Exception($"INVALID_HOST: No available node with {hostId}");
            return this.Add(new PlayerOptions()
            {
                GuildId = guildId,
                Client = this.Client,
                Manager = this,
                Node = node,
                ChannelId = channelId
            });
        }
    }

    public class PlayerManagerOptions
    {
        public ulong? UserId { get; set; }
        public int Shards { get; set; }
        public Player Player { get; set; }
    }
}
