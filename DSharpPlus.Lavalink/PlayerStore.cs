using System;
using System.Collections;
using System.Collections.Generic;

namespace DSharpPlus.Lavalink
{
    public class PlayerStore : Dictionary<ulong, Player>
    {
        public Player Player;
        public PlayerStore(Player player) : base()
        {
            this.Player = player;
        }

        public Player Add(PlayerOptions playerOptions)
        {
            return this.Add(new Player(playerOptions));
        }

        public Player Add(Player player)
        {
            this.Add(player.GuildId, player);
            return player;
        }
    }
}