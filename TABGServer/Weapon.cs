using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
{
    internal class Weapon
    {
        public int Id { get; set; }
        public int Type { get; set; }
        public int Count { get; set; }
        public (float X, float Y, float Z) Location { get; set; }
        public Weapon(int id, int localIndex, int count, (float x, float y, float z) loc)
        {
            Id = id;
            Type = localIndex;
            Count = count;
            Location = loc;
        }
    }
}
