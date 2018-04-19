using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DSharpPlus;

using DSharpPlus.Lavalink.VoiceEntities;
using DSharpPlus.Lavalink.LavalinkEntities;

namespace DSharpPlus.Lavalink
{
    public class Player
    {
        public PlayerOptions Options;
        public ulong GuildId;
        public DiscordClient Client;
        public PlayerManager Manager;
        public LavalinkNode Node;
        public ulong ChannelId;
        public bool Playing;
        public bool Paused;
        public State State;
        public string Track;
        public DateTime? Timestamp;

        public EventHandler<StringEventArgs> OnDisconnect;
        public EventHandler<JsonEventArgs> OnEnd;
        public EventHandler<ExceptionEventArgs> OnError;
        public EventHandler<StringEventArgs> OnWarn;

        public Player(PlayerOptions options)
        {
            this.Options = options;
            this.GuildId = options.GuildId;
            this.Client = options.Client;
            this.Manager = options.Manager;
            this.Node = options.Node;
            this.ChannelId = options.ChannelId;

            this.Playing = false;
            this.Paused = false;
            this.State = new State()
            {
                Volume = 100
            };

            this.Track = null;
            this.Timestamp = null;
        }

        public Player Connect(string sessionId, VoiceServerUpdatePayload stateEvent)
        {
            this.Node.Send(new LavalinkEntities.LavalinkDispatch()
            {
                Operation = "voiceUpdate",
                GuildId = this.GuildId,
                SessionId = sessionId,
                Event = stateEvent
            });
            return this;
        }

        public void RemoveAllListener()
        {
            OnDisconnect = null;
            OnEnd = null;
            OnError = null;
            OnWarn = null;
        }

        public Player Disconnect(string reason)
        {
            this.Playing = false;
            this.Stop();

            if (OnDisconnect != null)
                OnDisconnect(this, new StringEventArgs(reason));
            return this;
        }

        public Player Play(string track, int? startTime = null, int? endTime = null)
        {
            this.Track = track;
            var payload = new LavalinkDispatch()
            {
                Operation = "play",
                GuildId = this.GuildId,
                Track = track,
                StartTime = startTime,
                EndTime = endTime
            };

            this.Node.Send(payload);
            this.Playing = true;
            this.Timestamp = DateTime.Now;

            return this;
        }

        public Player Stop()
        {
            this.Node.Send(new LavalinkDispatch()
            {
                Operation = "stop",
                GuildId = this.GuildId
            });

            this.Playing = false;
            this.Track = null;

            return this;
        }

        public Player Pause(bool pause = true)
        {
            if ((pause && this.Paused) || (!pause && !this.Paused)) return this;
            this.Node.Send(new LavalinkDispatch()
            {
                Operation = "pause",
                GuildId = this.GuildId,
                Pause = pause
            });
            this.Paused = pause;
            return this;
        }

        public Player Resume()
        {
            return this.Pause(false);
        }

        public Player Volume(int volume)
        {
            this.Node.Send(new LavalinkDispatch()
            {
                Operation = "volume",
                GuildId = this.GuildId,
                Volume = volume
            });
            this.State.Volume = volume;
            return this;
        }

        public Player Seek(int position)
        {
            this.Node.Send(new LavalinkDispatch()
            {
                Operation = "seek",
                GuildId = this.GuildId,
                Position = position
            });
            return this;
        }

        public Player Destroy()
        {
            this.Node.Send(new LavalinkDispatch()
            {
                Operation = "destroy",
                GuildId = this.GuildId
            });
            return this;
        }

        public bool SwitchChannel(ulong channelId, bool reactive = false)
        {
            if (this.ChannelId == channelId) return false;
            this.ChannelId = channelId;
            if (reactive) this.UpdateVoiceState(channelId);
            return true;
        }

        public bool Event(JObject data)
        {
            switch (data["type"].ToString())
            {
                case "TrackEndEvent":
                    if (data["reason"].ToString() != "REPLACED")
                    {
                        this.Playing = false;
                        this.Track = null;
                    }

                    if (OnEnd != null)
                    {
                        OnEnd(this, new JsonEventArgs(data));
                        return true;
                    }
                    return false;
                case "TrackExceptionEvent":
                    if (OnError != null)
                    {
                        OnError(this, new ExceptionEventArgs(new Exception(data.ToString())));
                        return true;
                    }
                    return false;
                case "TrackStuckEvent":
                    this.Stop();

                    if (OnEnd != null)
                    {
                        OnEnd(this, new JsonEventArgs(data));
                        return true;
                    }
                    return false;
                default:
                    if (OnWarn != null)
                    {
                        var type = data.GetValue("type").ToString();
                        OnWarn(this, new StringEventArgs($"Unexpected event type: {type}"));
                        return true;
                    }
                    return false;
            }
        }

        public void UpdateVoiceState(ulong channelId, bool muted = false, bool deafened = false)
        {
            var vsd = new VoiceEntities.VoiceDispatch()
            {
                OpCode = 4,
                Payload = new VoiceEntities.VoiceStateUpdatePayload()
                {
                    GuildId = this.GuildId,
                    ChannelId = channelId,
                    Muted = muted,
                    Deafened = deafened,
                }
            };

            var vsj = JsonConvert.SerializeObject(vsd, Formatting.None);
            this.Client._webSocketClient.SendMessage(vsj);
        }
    }

    public class PlayerOptions
    {
        public ulong GuildId { get; set; }
        public DiscordClient Client { get; set; }
        public PlayerManager Manager { get; set; }
        public LavalinkNode Node { get; set; }
        public ulong ChannelId { get; set; }
    }

    public class State
    {
        public int Volume { get; set; }
    }
}
