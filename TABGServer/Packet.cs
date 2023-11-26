using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class Packet
    {
        public ClientEventCode Type { get; set; }
        public byte[] Data { get; set; }
        public Packet(ClientEventCode type, byte[] data)
        {
            Data = data;
            Type = type;
        }
    }
}
