using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
{
    internal class Throwables
    {
        public static byte[] ClientRequestThrow(BinaryReader binaryReader)
        {
            // player
            var playerIndex = binaryReader.ReadByte();
            // not sure what this is..? maybe the throwable ID?
            var throwableID = binaryReader.ReadInt32();
            // count of throwables
            var throwableCount = binaryReader.ReadInt32();

            // location
            var x = binaryReader.ReadSingle();
            var y = binaryReader.ReadSingle();
            var z = binaryReader.ReadSingle();

            // rotation
            var rotX = binaryReader.ReadSingle();
            var rotY = binaryReader.ReadSingle();
            var rotZ = binaryReader.ReadSingle();

            return SendItemThrowPacket(playerIndex, throwableID, throwableCount, (x, y, z), (rotX, rotY, rotZ));
        }

        public static byte[] SendItemThrowPacket(byte thrower, int throwable, int count, (float x, float y, float z) loc, (float rotX, float rotY, float rotZ) rot)
        {
            byte[] sendByte = new byte[512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // thrower
                    binaryWriterStream.Write(thrower);
                    // network index (?)
                    binaryWriterStream.Write((Int32)0);
                    // throwable id
                    binaryWriterStream.Write(throwable);
                    // count of throwables
                    binaryWriterStream.Write(count);

                    // location
                    binaryWriterStream.Write(loc.x);
                    binaryWriterStream.Write(loc.y);
                    binaryWriterStream.Write(loc.z);

                    // rotation
                    binaryWriterStream.Write(rot.rotX);
                    binaryWriterStream.Write(rot.rotY);
                    binaryWriterStream.Write(rot.rotZ);

                    // projectileSyncWatcher (not sure what this is, but it will Throw if it's true)
                    binaryWriterStream.Write(false);
                }
            }
            return sendByte;
        }
    }
}
