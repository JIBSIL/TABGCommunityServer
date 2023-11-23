using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class Player
    {
        public byte Id { get; set; }
        public byte Group { get; set; }
        public string Name { get; set; }
        public (float X, float Y, float Z) Location { get; set; }
        public Player(byte id, byte group, string name) 
        {
            Id = id;
            Group = group;
            Name = name;
        }
    }
}
