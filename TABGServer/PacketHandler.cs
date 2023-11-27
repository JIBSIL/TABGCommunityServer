using ENet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class PacketHandler
    {
        private readonly Peer m_peer;
        private readonly PlayerConcurencyHandler concurrencyHandler;
        private readonly WeaponConcurrencyHandler weaponConcurrencyHandler;

        public PacketHandler(Peer peer, PlayerConcurencyHandler handler, WeaponConcurrencyHandler weaponConcurrencyHandler)
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

                            var playerName = binaryReader.ReadString();
                            var gravestoneText = binaryReader.ReadString();
                            var loginKey = binaryReader.ReadUInt64();
                            var squadHost = binaryReader.ReadBoolean();
                            var squadMembers = binaryReader.ReadByte();
                            var userGearLength = binaryReader.ReadInt32();

                            int[] gearData = new int[userGearLength];

                            for(int i = 0; i < userGearLength; i++)
                            {
                                var userGear = binaryReader.ReadInt32();
                                gearData[i] = (int)userGear;
                            }

                            byte[] sendByte = new byte[4 + 4 + 4 + 4 + 4 + 4 + (roomName.Length + 4)];

                            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
                            {
                                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                                {
                                    // accepted or not
                                    binaryWriterStream.Write((byte)ServerResponse.Accepted);
                                    // gamemode
                                    binaryWriterStream.Write((byte)GameMode.BattleRoyale);
                                    // client requires this, but it's useless
                                    binaryWriterStream.Write((byte)1);
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
                            this.SendMessageToServer(ClientEventCode.Login, SendJoinMessageToServer(newIndex, playerName, gearData), true);

                            foreach (var item in concurrencyHandler.Players)
                            {
                                if(item.Key == newIndex)
                                {
                                    continue;
                                }
                                // this has been moved into the natively supported Login event for the main player
                                //this.SendMessageToServer(ClientEventCode.Login, new PlayerHandler().BroadcastPlayerJoin((byte)item.Key, item.Value.Name, item.Value.GearData), true);

                                // broadcast to ALL players
                                item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.Login, SendLoginMessageToServer(newIndex, playerName, gearData)));
                            }

                                this.SendMessageToServer(ClientEventCode.PlayerDead, new PlayerHandler().SendNotification(0, "WELCOME - RUNNING COMMUNITY SERVER V1.TEST"), true);
                            return;
                        case ClientEventCode.ChatMessage:
                            var playerIndex = binaryReader.ReadByte(); // or ReadInt32(), depending on the type of PlayerIndex
                            var messageLength = binaryReader.ReadByte();
                            var messageBytes = binaryReader.ReadBytes(messageLength);
                            var message = Encoding.Unicode.GetString(messageBytes);
                            Console.WriteLine("Player " + playerIndex + ": " + message);

                            var handler = new AdminCommandHandler(message, playerIndex);
                            handler.Handle(concurrencyHandler);

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
                            this.BroadcastPacket(ClientEventCode.ItemThrown, Throwables.ClientRequestThrow(binaryReader), true);
                            return;
                        case ClientEventCode.RequestItemDrop:
                            this.BroadcastPacket(ClientEventCode.ItemDrop, Droppables.ClientRequestDrop(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case ClientEventCode.RequestWeaponPickUp:
                            this.BroadcastPacket(ClientEventCode.WeaponPickUpAccepted, Droppables.ClientRequestPickUp(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case ClientEventCode.PlayerUpdate:
                            // this packet is different because it can have an unlimited amount of subpackets
                            UpdatePacket updatePacket = new PlayerHandler().PlayerUpdate(binaryReader, buffer.Length, concurrencyHandler);

                            this.SendMessageToServer(ClientEventCode.PlayerUpdate, updatePacket.Packet, true);

                            // have to do this so enumeration is safe
                            List<Packet> packetList = updatePacket.BroadcastPackets.PendingBroadcastPackets;

                            // also use this packet to send pending broadcast packets
                            foreach (var item in packetList)
                            {
                                this.SendMessageToServer(item.Type, item.Data, true);
                            }

                            updatePacket.BroadcastPackets.PendingBroadcastPackets = new List<Packet>();

                            return;
                        case ClientEventCode.WeaponChange:
                            this.BroadcastPacket(ClientEventCode.WeaponChanged, new PlayerHandler().PlayerChangedWeapon(binaryReader), true);
                            return;
                        case ClientEventCode.PlayerFire:
                            new PlayerHandler().PlayerFire(binaryReader, buffer.Length, concurrencyHandler);
                            return;
                        case ClientEventCode.RequestSyncProjectileEvent:
                            this.BroadcastPacket(ClientEventCode.SyncProjectileEvent, new PlayerHandler().ClientRequestProjectileSyncEvent(concurrencyHandler, binaryReader, buffer.Length), true);
                            return;
                        case ClientEventCode.RequestAirplaneDrop:
                            this.BroadcastPacket(ClientEventCode.PlayerAirplaneDropped, new PlayerHandler().RequestAirplaneDrop(binaryReader), true);
                            return;
                        // damage event breaks damage for some reason
                        case ClientEventCode.DamageEvent:
                            new PlayerHandler().PlayerDamagedEvent(concurrencyHandler, binaryReader);
                            return;
                        case ClientEventCode.RequestBlessing:
                            this.BroadcastPacket(ClientEventCode.BlessingRecieved, new PlayerHandler().RequestBlessingEvent(binaryReader), true);
                            return;
                        case ClientEventCode.RequestHealthState:
                            this.BroadcastPacket(ClientEventCode.PlayerHealthStateChanged, new PlayerHandler().RequestHealthState(concurrencyHandler, binaryReader), true);
                            return;
                        default:
                            return;
                    }
                }
            }
        }


        private void BroadcastPacket(ClientEventCode eventCode, byte[] playerData, bool unused)
        {
            foreach (var item in concurrencyHandler.Players)
            {
                item.Value.PendingBroadcastPackets.Add(new Packet(eventCode, playerData));
            }
        }

        private byte[] SendJoinMessageToServer(byte playerIndex, string playerName, int[] gearData)
        {
            byte[] sendByte = new byte[1024];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write((Byte)playerIndex);
                    // group index
                    binaryWriterStream.Write((Byte)0);
                    // username length
                    binaryWriterStream.Write((Int32)(playerName.Length));
                    // username
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes(playerName));
                    // is dev
                    binaryWriterStream.Write(true);

                    // set up locations properly
                    Player player = new(playerIndex, 0, playerName, (0f, 200f, 0f), (0f, 0f), gearData);
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
                    // is downed
                    binaryWriterStream.Write(false);
                    // health value (not needed)
                    binaryWriterStream.Write((float)100);
                    // something relating to cars? not needed
                    binaryWriterStream.Write(false);
                    // how many players are in the lobby?
                    //binaryWriterStream.Write((byte)concurrencyHandler.Players.Count);
                    binaryWriterStream.Write((byte)(concurrencyHandler.Players.Count - 1));

                    // --- OTHER PLAYERS ---

                    foreach (var item in concurrencyHandler.Players)
                    {
                        if (item.Key == playerIndex)
                        {
                            continue;
                        }
                        // player index
                        binaryWriterStream.Write((Byte)item.Key);
                        // group index
                        binaryWriterStream.Write((Byte)0);

                        // convert so bytes can be grabbed
                        var nameBytes = Encoding.UTF8.GetBytes(item.Value.Name);

                        // username length
                        binaryWriterStream.Write((Int32)(nameBytes.Length));
                        // username
                        binaryWriterStream.Write(nameBytes);
                        // gun (this has been disabled for efficiency)
                        binaryWriterStream.Write((Int32)0);

                        // gear data
                        binaryWriterStream.Write((Int32)item.Value.GearData.Length);
                        for(int i = 0; i < item.Value.GearData.Length; i++)
                        {
                            binaryWriterStream.Write((Int32)item.Value.GearData[i]);
                        }

                        // is dev
                        binaryWriterStream.Write(true);
                        // colour (disabled amongus gamemode)
                        binaryWriterStream.Write((Int32)0);
                    }

                    // --- END OTHER PLAYERS ---

                    // --- WEAPONS SECTION ---
                    // number of weapons to spawn, just leave this empty...
                    binaryWriterStream.Write((Int32)0);
                    // --- END WEAPONS SECTION ---

                    // --- CARS SECTION ---

                    // THIS IS CONFUSING AND BROKEN!!!
                    binaryWriterStream.Write((Int32)1);
                    // car id
                    binaryWriterStream.Write((Int32)1);
                    // car index
                    binaryWriterStream.Write((Int32)0);
                    // seats
                    binaryWriterStream.Write((Int32)4);
                    for(int i = 0; i < 4; i++)
                    {
                        binaryWriterStream.Write((Int32)i);
                    }
                    // parts of car
                    binaryWriterStream.Write((byte)4);
                    for (int i = 0; i < 4; i++)
                    {
                        // index
                        binaryWriterStream.Write((byte)i);
                        // health
                        binaryWriterStream.Write(100f);
                        // name
                        binaryWriterStream.Write("Test");
                    }

                    // --- END CARS SECTION ---

                    // time of day
                    binaryWriterStream.Write((float)0);
                    // seconds before first ring
                    binaryWriterStream.Write((float)1000);
                    // base ring time
                    binaryWriterStream.Write((float)1000);
                    // something ring-related, just set to false to disable
                    binaryWriterStream.Write((byte)0);
                    // lives
                    binaryWriterStream.Write((Int32)2);
                    // kills to win
                    binaryWriterStream.Write((ushort)10);
                    // gamestate
                    binaryWriterStream.Write((Byte)GameState.Started);

                    // flying stuff (?)
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(200f);
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(200f);
                    //binaryWriterStream.Write(0f);
                }
            }
            return sendByte;
        }

        private byte[] SendLoginMessageToServer(byte playerIndex, string playerName, int[] gearData)
        {
            byte[] sendByte = new byte[1024];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write((Byte)playerIndex);
                    // group index
                    binaryWriterStream.Write((Byte)0);
                    // username length
                    binaryWriterStream.Write((Int32)(playerName.Length));
                    // username
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes(playerName));

                    // gear data
                    binaryWriterStream.Write((Int32)gearData.Length);
                    for (int i = 0; i < gearData.Length; i++)
                    {
                        binaryWriterStream.Write((Int32)gearData[i]);
                    }

                    // is dev
                    binaryWriterStream.Write(true);
                }
            }
            return sendByte;
        }

        private void SendMessageToServer(ClientEventCode code, byte[] buffer, bool reliable)
        {
            ENet.Packet packet = default(ENet.Packet);
            byte[] array = new byte[buffer.Length + 1];
            array[0] = (byte)code;
            Array.Copy(buffer, 0, array, 1, buffer.Length);
            packet.Create(array, reliable ? PacketFlags.Reliable : PacketFlags.None);
            this.m_peer.Send(0, ref packet);
        }
    }
}
