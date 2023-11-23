using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class PlayerHandler
    {
        public PlayerHandler() { }

        public byte[] KillPlayer(int victim, int killer, string victimName)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write((Byte)victim);
                    // player who killed the player
                    binaryWriterStream.Write((Byte)killer);
                    // spectator value: 255 = don't respawn. 254 = go to boss. other ints = spectate that player
                    binaryWriterStream.Write((Byte)254);
                    // victim length
                    binaryWriterStream.Write((Int32)(victimName.Length));
                    // victim
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes(victimName));
                    // weapon id for killscreen (unused for now)
                    binaryWriterStream.Write((Int32)1);
                    // is ring death (unused for now because ring isn't operational)
                    binaryWriterStream.Write(false);
                }
            }
            return sendByte;
        }

        public byte[] SendNotification(int playerIndex, string notification)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index (set to 255 for invalid player so nobody gets killed)
                    binaryWriterStream.Write((Byte)255);
                    // player who killed the player
                    binaryWriterStream.Write((Byte)playerIndex);
                    // spectator value is unneeded when killing other players
                    binaryWriterStream.Write((Byte)255);

                    // get amount of bytes
                    byte[] message = Encoding.Unicode.GetBytes(notification);

                    // victim length
                    binaryWriterStream.Write((Byte)(message.Length));
                    // victim
                    binaryWriterStream.Write(message);

                    // weapon id for killscreen (unused for now)
                    binaryWriterStream.Write((Int32)1);
                    // is ring death (unused for now because ring isn't operational)
                    binaryWriterStream.Write(false);
                }
            }
            return sendByte;
        }

        public byte[] GiveItem(int itemID, int amount)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // num of items (loops)
                    binaryWriterStream.Write((ushort)1);
                    // item id
                    binaryWriterStream.Write((Int32)itemID);
                    // quantity of item
                    binaryWriterStream.Write((Int32)amount);
                    // Client requires this, but it's redundant
                    binaryWriterStream.Write((Int32)0);
                }
            }
            return sendByte;
        }

        public byte[] GiveGear()
        {
            byte[] sendByte = new byte[768];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // number of loops
                    binaryWriterStream.Write((ushort)14);

                    // start with the ammo
                    for (int i = 0; i < 10; i++)
                    {
                        // item id
                        binaryWriterStream.Write((Int32)i);
                        // quantity of item
                        binaryWriterStream.Write((Int32)999);
                    }

                    // give LP grenade
                    binaryWriterStream.Write((Int32)202);
                    binaryWriterStream.Write((Int32)1);

                    // give vector
                    binaryWriterStream.Write((Int32)315);
                    binaryWriterStream.Write((Int32)999);

                    // give AWP
                    binaryWriterStream.Write((Int32)317);
                    binaryWriterStream.Write((Int32)1);

                    // give deagle
                    binaryWriterStream.Write((Int32)266);
                    binaryWriterStream.Write((Int32)1);

                    // Client requires this, but it's redundant
                    binaryWriterStream.Write((Int32)0);
                }
            }
            return sendByte;
        }
    }

    public byte[] PlayerUpdate()
    {

    }
}
