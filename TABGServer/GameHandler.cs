using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
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

            #region CustomRandomBusPath

                    Random rand = new Random();
                    float max = 360;
                    float angleOne = (float)(rand.NextDouble() * max);
                    float angleTwo = (float)(rand.NextDouble() * max);
                    float radius = 1_000;


                    // Circle Points
                    float x1 = MathF.Sin(angleOne) * radius;
                    float y1 = MathF.Cos(angleOne) * radius;

                    float x2 = MathF.Sin(angleTwo) * radius;
                    float y2 = MathF.Cos(angleTwo) * radius;

                    //Console.WriteLine($"{angleOne} - {angleTwo}\n{x1}, {y1}  ~  {x2}, {y2}");

                    binaryWriterStream.Write(x1);
                    binaryWriterStream.Write((float)200f);
                    binaryWriterStream.Write(y1);


                    binaryWriterStream.Write(x2);
                    binaryWriterStream.Write((float)125);
                    binaryWriterStream.Write(y2);
                    

            #endregion CustomRandomBusPath

                    /*
                    // start flying from this X Y Z
                    binaryWriterStream.Write((float)0f);
                    binaryWriterStream.Write((float)200f);
                    binaryWriterStream.Write((float)0f);

                    // "end vector"
                    binaryWriterStream.Write((float)100f);
                    binaryWriterStream.Write((float)125f);
                    binaryWriterStream.Write((float)100f);
                    */

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

        public static byte[] GenerateRingPacket(byte ringStatus, byte newStateOrTimeTravelled, Single X, Single Y, Single Z, Single Size)
        {
            // Ring Update
            // 1 = Set Size, and Position
            // 0 = TimeTraveled (sync time)
            // 2 = Start moving ring

            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // ring status (See Ring Update Line for info)
                    binaryWriterStream.Write(ringStatus);
                    if (ringStatus == 1)
                    {
                        // new state (index according to PhotonServerHandler : 1437)
                        binaryWriterStream.Write(newStateOrTimeTravelled);

                        // X
                        binaryWriterStream.Write(X);

                        // Y
                        binaryWriterStream.Write(Y);

                        // Z
                        binaryWriterStream.Write(Z);

                        // Size
                        binaryWriterStream.Write(Size);
                    }
                    else if (ringStatus == 0)
                    {
                        // time travelled
                        binaryWriterStream.Write(newStateOrTimeTravelled);
                    }
                    // no data is sent, other than the status we already have, when we give the update code "2"
                }
            }
            return sendByte;
        }
    }
}
