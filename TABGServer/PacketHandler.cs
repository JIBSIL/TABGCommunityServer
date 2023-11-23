using ENet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class PacketHandler
    {
        private readonly Peer m_peer;
        private readonly PlayerConncurencyHandler concurrencyHandler;
        private readonly WeaponConcurrencyHandler weaponConcurrencyHandler;

        public PacketHandler(Peer peer, PlayerConncurencyHandler handler, WeaponConcurrencyHandler weaponConcurrencyHandler)
        {
            this.m_peer = peer;
            this.concurrencyHandler = handler;
            this.weaponConcurrencyHandler = weaponConcurrencyHandler;
        }

        public void Handle(ClientEventCode code, byte[] buffer)
        {
            string? stringCode = Enum.GetName(typeof(ClientEventCode), code);
            if ((stringCode != "TABGPing") && (stringCode != "PlayerUpdate")) {
                Console.WriteLine("Handling packet: " + Enum.GetName(typeof(ClientEventCode), code));
            }

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    switch(code)
                    {
                        case ClientEventCode.RoomInit:
                            string roomName = "DecompileServer";
                            byte newIndex = concurrencyHandler.LastID++;

                            byte[] sendByte = new byte[4 + 4 + 4 + 4 + 4 + 4 + (roomName.Length + 4)];

                            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
                            {
                                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                                {
                                    // accepted or not
                                    binaryWriterStream.Write((int)ServerResponse.Accepted);
                                    // gamemode
                                    binaryWriterStream.Write((int)GameMode.BattleRoyale);
                                    // client requires this, but it's useless
                                    binaryWriterStream.Write(1);
                                    // player index
                                    binaryWriterStream.Write(newIndex);
                                    // group index
                                    binaryWriterStream.Write((Byte)0);
                                    // useless
                                    binaryWriterStream.Write(1);
                                    // useless string (but using it to notify server of a custom server)
                                    binaryWriterStream.Write(Encoding.UTF8.GetBytes("CustomServer"));
                                }
                            }

                            Console.WriteLine("Sending request RESPONSE to client!");
                            this.SendMessageToServer(ClientEventCode.RoomInitRequestResponse, sendByte, true);

                            Console.WriteLine("Sending Login RESPONSE to client!");
                            this.SendMessageToServer(ClientEventCode.Login, SendJoinMessageToServer(newIndex), true);

                            this.SendMessageToServer(ClientEventCode.PlayerDead, new PlayerHandler().SendNotification(0, "WELCOME - RUNNING COMMUNITY SERVER V1.TEST"), true);
                            return;
                        case ClientEventCode.ChatMessage:
                            var playerIndex = binaryReader.ReadByte(); // or ReadInt32(), depending on the type of PlayerIndex
                            var messageLength = binaryReader.ReadByte();
                            var messageBytes = binaryReader.ReadBytes(messageLength);
                            var message = Encoding.Unicode.GetString(messageBytes);
                            Console.WriteLine("Player " + playerIndex + ": " + message);

                            var handler = new AdminCommandHandler(message, playerIndex);
                            handler.Handle();

                            // test if there needs to be a packet sent back
                            if(handler.shouldSendPacket)
                            {
                                this.SendMessageToServer(handler.code, handler.packetData, true);
                            }

                            if((handler.notification != null) && (handler.notification != ""))
                            {
                                this.SendMessageToServer(ClientEventCode.PlayerDead, new PlayerHandler().SendNotification(playerIndex, handler.notification), true);
                            }

                            return;
                        case ClientEventCode.RequestItemThrow:
                            this.SendMessageToServer(ClientEventCode.ItemThrown, Throwables.ClientRequestThrow(binaryReader), true);
                            return;
                        case ClientEventCode.RequestItemDrop:
                            this.SendMessageToServer(ClientEventCode.ItemDrop, Droppables.ClientRequestDrop(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case ClientEventCode.RequestWeaponPickUp:
                            this.SendMessageToServer(ClientEventCode.WeaponPickUpAccepted, Droppables.ClientRequestPickUp(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case ClientEventCode.PlayerUpdate:
                            return;
                        default:
                            return;
                    }
                }
            }
        }


        private byte[] SendJoinMessageToServer(byte playerIndex)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write((Byte)playerIndex);
                    // group index
                    binaryWriterStream.Write((Byte)0);
                    // username length
                    binaryWriterStream.Write((Int32)("Tester".Length));
                    // username
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes("Tester"));
                    // is dev
                    binaryWriterStream.Write(true);

                    // set up locations properly
                    Player player = new(playerIndex, 0, "Tester");
                    concurrencyHandler.AddPlayer(player);
                    concurrencyHandler.UpdatePlayerLocation(playerIndex, (0f, 200f, 0f));

                    // x
                    binaryWriterStream.Write(0f);
                    // y
                    binaryWriterStream.Write(200f);
                    // z

                    binaryWriterStream.Write(0f);
                    // rotation
                    binaryWriterStream.Write((float)0);
                    // is dead
                    binaryWriterStream.Write(false);
                    // should update health value
                    binaryWriterStream.Write(false);
                    // health value (not needed)
                    binaryWriterStream.Write((float)100);
                    // something relating to cars? not needed
                    binaryWriterStream.Write(false);
                    // how many players are in the lobby?
                    binaryWriterStream.Write((byte)1);

                    // --- OTHER PLAYERS ---

                    // player index
                    binaryWriterStream.Write((Byte)1);
                    // group index
                    binaryWriterStream.Write((Byte)1);
                    // username length
                    binaryWriterStream.Write((Int32)("Tester".Length));
                    // username
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes("Tester"));
                    // gun
                    binaryWriterStream.Write((Int32)315);
                    // gear data (?)
                    binaryWriterStream.Write((Int32)0);
                    // is dev
                    binaryWriterStream.Write(true);
                    // colour (?)
                    binaryWriterStream.Write((Int32)0);


                    // --- END OTHER PLAYERS ---

                    // --- WEAPONS SECTION ---
                    // number of weapons to spawn, just leave this empty...
                    binaryWriterStream.Write((Int32)0);
                    // --- END WEAPONS SECTION ---

                    // --- CARS SECTION ---
                    // number of cars to spawn, just leave this empty...
                    binaryWriterStream.Write((Int32)0);
                    // --- END CARS SECTION ---

                    // time of day
                    binaryWriterStream.Write((float)0);
                    // seconds before first ring
                    binaryWriterStream.Write((float)1000);
                    // base ring time
                    binaryWriterStream.Write((float)1000);
                    // something ring-related, just set to false to disable
                    binaryWriterStream.Write(0);
                    // lives
                    binaryWriterStream.Write((Int32)2);
                    // kills to win
                    binaryWriterStream.Write((ushort)10);
                    // gamestate
                    binaryWriterStream.Write((Byte)GameState.WaitingForPlayers);
                }
            }
            return sendByte;
        }

        private void SendMessageToServer(ClientEventCode code, byte[] buffer, bool reliable)
        {
            Packet packet = default(Packet);
            byte[] array = new byte[buffer.Length + 1];
            array[0] = (byte)code;
            Array.Copy(buffer, 0, array, 1, buffer.Length);
            packet.Create(array, reliable ? PacketFlags.Reliable : PacketFlags.None);
            this.m_peer.Send(0, ref packet);
        }
    }
}
