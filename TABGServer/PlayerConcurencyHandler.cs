using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
{
    internal class PlayerConcurencyHandler
    {
        public Dictionary<int, Player> Players = new Dictionary<int, Player>();
        public byte LastID = 0;

        public PlayerConcurencyHandler() { }

        public void AddPlayer(Player player)
        {
            Players[player.Id] = player;
            LastID++;
        }

        public void RemovePlayer(Player player)
        {
            Players.Remove(player.Id);
        }

        public void UpdatePlayerLocation(int playerId, (float X, float Y, float Z) newLocation)
        {
            if (Players.TryGetValue(playerId, out Player? player))
            {
                player.Location = newLocation;
            }
        }
    }
}
