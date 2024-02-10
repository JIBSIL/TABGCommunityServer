using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class Packet
    {
        public EventCode Type { get; set; }
        public byte[] Data { get; set; }
        public Packet(EventCode type, byte[] data)
        {
            Data = data;
            Type = type;
        }
    }
}
