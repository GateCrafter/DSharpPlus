using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using DSharpPlus;
using DSharpPlus.EventArgs;
using DSharpPlus.Net.WebSocket;

using DSharpPlus.Lavalink.LavalinkEntities;

namespace DSharpPlus.Lavalink
{
    public class LavalinkNode
    {
        public PlayerManager Manager;
        public string Host;
        public int Port;
        public string Address;
        public string Region;
        public string Password;
        public bool Ready;
        public bool Connected;
        public WebSocketClient WebSocket;
        public Timer ReconnectTimer;
        public int ReconnectInterval;
        public Stats Stats;

        public event EventHandler OnReady;
        public event EventHandler<ExceptionEventArgs> OnError;
        public event EventHandler OnReconnecting;
        public event EventHandler<StringEventArgs> OnDisconnect;
        public event EventHandler<JsonEventArgs> OnMessage;


        public LavalinkNode(PlayerManager manager, LavalinkNodeOptions options)
        {
            this.Manager = manager;

            this.Host = options.Host;
            this.Port = options.Port;
            this.Address = options.Address;
            this.Region = options.Region;
            this.Password = options.Password;

            this.Ready = false;
            this.WebSocket = null;
            this.ReconnectTimer = null;
            this.ReconnectInterval = options.ReconnectInterval;
            this.Stats = new Stats()
            {
                Players = 0,
                PlayingPlayer = 0
            };

            this.Connect().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task Connect()
        {
            this.WebSocket = new WebSocketClient(null);
            await this.WebSocket.ConnectAsync(new Uri(this.Address), new Dictionary<string, string>() {
                { "User-Id", this.Manager.UserId.ToString() },
                { "Num-Shards", this.Manager.Shards.ToString()},
                { "Authorization", this.Password}
            });

            this.WebSocket.OnConnect += OnConnect;
            this.WebSocket.OnMessage += Message;
            this.WebSocket.OnDisconnect += Close;
            this.WebSocket.OnError += Error;
        }

        private Task OnConnect()
        {
            return Task.Run(() => {
                this.Ready = true;

            if (OnReady != null)
                OnReady(this, System.EventArgs.Empty);
            });
        }

        public bool Send(LavalinkDispatch data)
        {
            if (this.WebSocket == null) return false;
            string payload;
            try
            {
                payload = JsonConvert.SerializeObject(data);
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, new ExceptionEventArgs(ex));
                return false;
            }
            this.WebSocket.SendMessage(payload);
            return true;
        }

        public async Task<bool> Destroy()
        {
            if (this.WebSocket == null) return false;

            await this.WebSocket.DisconnectAsync(new SocketCloseEventArgs(null) { CloseCode = 1000, CloseMessage = "destroy" });
            this.WebSocket.Dispose();
            this.WebSocket = null;
            return true;
        }

        private Task Reconnect()
        {
            return Task.Run(() =>
            {
                this.ReconnectTimer = new Timer(async _ =>
                {
                    this.RemoveAllListener();

                    await this.Connect();
                }, null, this.ReconnectInterval, Timeout.Infinite);
            });

            
        }

        public void RemoveAllListener()
        {
            this.OnDisconnect = null;
            this.OnError = null;
            this.OnMessage = null;
            this.OnReady = null;
        }

        private Task Close(SocketCloseEventArgs e)
        {

            this.Connected = false;
            if (e.CloseCode != 1000 || e.CloseMessage != "destroy") return this.Reconnect();
            return Task.Run(() => {
                this.WebSocket = null;
                if (OnDisconnect != null)
                    OnDisconnect(this, new StringEventArgs(e.CloseMessage));
            });    
        }

        private Task Message(SocketMessageEventArgs e)
        {
            return Task.Run(() => {
                try
            {
                var data = JObject.Parse(e.Message);

                if (OnMessage != null)
                    OnMessage(this, new JsonEventArgs(data));
            }
            catch (Exception ex)
            {
                if (OnError != null)
                    OnError(this, new ExceptionEventArgs(ex));
            }
            });
        }

        private Task Error(SocketErrorEventArgs e)
        {
            return Task.Run(() => {
                if (OnError != null)
                OnError(this, new ExceptionEventArgs(e.Exception));
            });
        }
    }

    public class LavalinkNodeOptions
    {
        public string Host { get; set; }
        public int Port { get; set; } = 80;
        public string Address { get; set; }
        public string Region { get; set; }
        public string Password { get; set; } = "youshallnotpass";
        public int ReconnectInterval { get; set; } = 5000;
    }

    public class Stats
    {
        public int Players { get; set; }
        public int PlayingPlayer { get; set; }
    }


}
