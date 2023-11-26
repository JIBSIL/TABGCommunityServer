using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class GameHandler
    {
        public static byte[] SetWaitingForPlayersState()
        {
            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // game state
                    binaryWriterStream.Write((byte)GameState.WaitingForPlayers);
                }
            }
            return sendByte;
        }
        public static byte[] SetCountDown(int countdownFrom)
        {
            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // game state
                    binaryWriterStream.Write((byte)GameState.CountDown);
                    // count down from this int
                    binaryWriterStream.Write((float)countdownFrom);
                }
            }
            return sendByte;
        }
        public static byte[] SetFlying(byte matchModifier)
        {
            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // game state
                    binaryWriterStream.Write((byte)GameState.Flying);
                    // start flying from this X Y Z
                    binaryWriterStream.Write((float)0f);
                    binaryWriterStream.Write((float)200f);
                    binaryWriterStream.Write((float)0f);

                    // "end vector"
                    binaryWriterStream.Write((float)100f);
                    binaryWriterStream.Write((float)125f);
                    binaryWriterStream.Write((float)100f);

                    // time of day
                    binaryWriterStream.Write((float)0f);

                    // match modifier
                    binaryWriterStream.Write(matchModifier);
                }
            }
            return sendByte;
        }

        public static byte[] SetStarted()
        {
            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // game state
                    binaryWriterStream.Write((byte)GameState.Started);
                }
            }
            return sendByte;
        }
    }
}
