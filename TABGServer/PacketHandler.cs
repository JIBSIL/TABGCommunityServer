using ENet;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
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

        public void Handle(EventCode code, byte[] buffer)
        {
            string? stringCode = Enum.GetName(typeof(EventCode), code);
            if ((stringCode != "TABGPing") && (stringCode != "PlayerUpdate")) {
                Console.WriteLine("Handling packet: " + Enum.GetName(typeof(EventCode), code));
            }

            using (MemoryStream memoryStream = new MemoryStream(buffer))
            {
                using (BinaryReader binaryReader = new BinaryReader(memoryStream))
                {
                    switch (code)
                    {
                        case EventCode.RoomInit:
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
                                    binaryWriterStream.Write((Byte)newIndex);
                                    // useless
                                    binaryWriterStream.Write(1);
                                    // useless string (but using it to notify server of a custom server)
                                    binaryWriterStream.Write(Encoding.UTF8.GetBytes("CustomServer"));
                                }
                            }

                            Console.WriteLine("Sending request RESPONSE to client!");
                            this.SendMessageToServer(EventCode.RoomInitRequestResponse, sendByte, true);

                            Console.WriteLine("Sending Login RESPONSE to client!");
                            this.SendMessageToServer(EventCode.Login, SendJoinMessageToServer(newIndex, playerName, gearData), true);

                            foreach (var item in concurrencyHandler.Players)
                            {
                                if(item.Key == newIndex)
                                {
                                    continue;
                                }
                                // this has been moved into the natively supported Login event for the main player
                                //this.SendMessageToServer(ClientEventCode.Login, new PlayerHandler().BroadcastPlayerJoin((byte)item.Key, item.Value.Name, item.Value.GearData), true);

                                // broadcast to ALL players
                                item.Value.PendingBroadcastPackets.Add(new Packet(EventCode.Login, SendLoginMessageToServer(newIndex, playerName, gearData)));
                            }

                                this.SendMessageToServer(EventCode.PlayerDead, new PlayerHandler().SendNotification(0, "WELCOME - RUNNING COMMUNITY SERVER V1.TEST"), true);
                            return;
                        case EventCode.ChatMessage:
                            var playerIndex = binaryReader.ReadByte(); // or ReadInt32(), depending on the type of PlayerIndex
                            var messageLength = binaryReader.ReadByte();
                            var messageBytes = binaryReader.ReadBytes(messageLength);
                            var message = Encoding.Unicode.GetString(messageBytes);
                            Console.WriteLine("Player " + playerIndex + ": " + message);

                            var handler = new AdminCommandHandler(message, playerIndex);
                            handler.Handle(concurrencyHandler);

                            // test if there needs to be a packet sent back
                            if (handler.shouldSendPacket)
                            {
                                this.SendMessageToServer(handler.code, handler.packetData, true);
                            }

                            if((handler.notification != null) && (handler.notification != ""))
                            {
                                this.SendMessageToServer(EventCode.PlayerDead, new PlayerHandler().SendNotification(playerIndex, handler.notification), true);
                            }

                            return;
                        case EventCode.RequestItemThrow:
                            this.BroadcastPacket(EventCode.ItemThrown, Throwables.ClientRequestThrow(binaryReader), true);
                            return;
                        case EventCode.RequestItemDrop:
                            this.BroadcastPacket(EventCode.ItemDrop, Droppables.ClientRequestDrop(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case EventCode.RequestWeaponPickUp:
                            this.BroadcastPacket(EventCode.WeaponPickUpAccepted, Droppables.ClientRequestPickUp(binaryReader, weaponConcurrencyHandler), true);
                            return;
                        case EventCode.PlayerUpdate:
                            // this packet is different because it can have an unlimited amount of subpackets
                            UpdatePacket updatePacket = new PlayerHandler().PlayerUpdate(binaryReader, buffer.Length, concurrencyHandler);

                            this.SendMessageToServer(EventCode.PlayerUpdate, updatePacket.Packet, true);

                            // have to do this so enumeration is safe
                            List<Packet> packetList = updatePacket.BroadcastPackets.PendingBroadcastPackets;

                            // also use this packet to send pending broadcast packets
                            foreach (var item in packetList)
                            {
                                this.SendMessageToServer(item.Type, item.Data, true);
                            }

                            updatePacket.BroadcastPackets.PendingBroadcastPackets = new List<Packet>();

                            return;
                        case EventCode.WeaponChange:
                            this.BroadcastPacket(EventCode.WeaponChanged, new PlayerHandler().PlayerChangedWeapon(binaryReader), true);
                            return;
                        case EventCode.PlayerFire:
                            new PlayerHandler().PlayerFire(binaryReader, buffer.Length, concurrencyHandler);
                            return;
                        case EventCode.RequestSyncProjectileEvent:
                            this.BroadcastPacket(EventCode.SyncProjectileEvent, new PlayerHandler().ClientRequestProjectileSyncEvent(concurrencyHandler, binaryReader, buffer.Length), true);
                            return;
                        case EventCode.RequestAirplaneDrop:
                            this.SendMessageToServer(EventCode.PlayerAirplaneDropped, new PlayerHandler().RequestAirplaneDrop(binaryReader), true);
                            return;
                        // damage event breaks damage for some reason
                        case EventCode.DamageEvent:
                            new PlayerHandler().PlayerDamagedEvent(concurrencyHandler, binaryReader);
                            return;
                        case EventCode.RequestBlessing:
                            this.BroadcastPacket(EventCode.BlessingRecieved, new PlayerHandler().RequestBlessingEvent(binaryReader), true);
                            return;
                        case EventCode.RequestHealthState:
                            this.BroadcastPacket(EventCode.PlayerHealthStateChanged, new PlayerHandler().RequestHealthState(concurrencyHandler, binaryReader), true);
                            return;

                        case EventCode.BossFightResult:
                            byte[][] bossFightRespawnBytes = new PlayerHandler().BossFightResultEvent(concurrencyHandler, binaryReader);

                            this.SendMessageToServer(EventCode.PlayerRespawnFromBoss, bossFightRespawnBytes[0], true);
                            //this.SendMessageToServer(EventCode.PlayerEffect, bossFightRespawnBytes[1], true);
                            return;
                        case EventCode.RingDeath:
                            new PlayerHandler().RingDeathEvent(concurrencyHandler, binaryReader);
                            break;

                        default:
                            Console.WriteLine("Unhandled Handle Event Code: " + code);
                            return;
                    }
                }
            }
        }


        private void BroadcastPacket(EventCode eventCode, byte[] playerData, bool unused)
        {
            foreach (var item in concurrencyHandler.Players)
            {
                item.Value.PendingBroadcastPackets.Add(new Packet(eventCode, playerData));
            }
        }

        private byte[] SendJoinMessageToServer(byte playerIndex, string playerName, int[] gearData)
        {
            List<Vector3> itemsSpawnLocations = LoadItemSpawns();
            List<Int32> allowedItemTypes = new List<Int32> { (int)ItemTypes.Spell_Blinding_Light };
            List<ItemIDs> lootPool = new List<ItemIDs> { ItemIDs.The_mad_mechanic };
            foreach(ItemIDs enumValue in Enum.GetValues(typeof(ItemIDs)))
            {
                ItemTypes itemType;
                if (Enum.TryParse(enumValue.ToString(), out itemType) == false) { continue; }
                if (allowedItemTypes.IndexOf((int)itemType) == -1) { continue; }
                
                lootPool.Add(enumValue);
                //Console.WriteLine($"Added {enumValue.ToString()} to the loot pool.");
            }
            lootPool = new List<ItemIDs>();


            byte[] sendByte = new byte[1024 * 512];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write((Byte)playerIndex);
                    // group index
                    binaryWriterStream.Write((Byte)playerIndex);
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
                        binaryWriterStream.Write((Byte)item.Key);

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
                    //float numOfWeapons = 329; // there are 329 items with the index starting at 0, making 328 the last index
                    float numOfWeapons = itemsSpawnLocations.Count / 2;
                    int itemIdOffset = 0;
                    List<Weapon> weapons = new List<Weapon>();
                    
                    List<int> ammoClassIds = new List<int> { 8, 9, 10, 11, 12, 14, 15, 16, 20, 21 };
                    List<int> ammoIds = new List<int> { 6, 9, 2, 3, 11, 1, 7, 12, 4, 8 };

                    Vector3 offset = new Vector3(0, 0, 0);
                    Random rand = new Random();

                    for (int k = 0; k < numOfWeapons; k++)
                    {

                        // Weapon at random spawn location
                        Vector3 randLocation = itemsSpawnLocations[(int)rand.NextInt64(0, itemsSpawnLocations.Count)] + offset;
                        //Vector3 randLocation = itemsSpawnLocations[k] + offset;

                        // Random Weapon ID from loot pool
                        int randItem;
                        if (lootPool.Count == 0) { randItem = (int)rand.NextInt64(0, 329); }
                        else { randItem = (int)lootPool[(int)rand.NextInt64(0, lootPool.Count)]; }

                        // Hellish way to spawn ammo, but ig it'll work for now
                        ItemIDs itemId;
                        ItemTypes itemType;
                        if (Enum.TryParse(randItem.ToString(), out itemId))
                        {
                            if (Enum.TryParse(itemId.ToString(), out itemType))
                            {
                                int index = ammoClassIds.IndexOf((int)(itemType));
                                if (index != -1) {
                                    Weapon newAmmo = new Weapon(ammoIds[index], k + (int)itemIdOffset, (int)rand.NextInt64(10, 30), (randLocation.X, randLocation.Y, randLocation.Z));
                                    weapons.Add(newAmmo);
                                    itemIdOffset += 1;
                                }
                            }
                        }
                        //Console.WriteLine($"{itemId} - {itemType}");

                        Weapon newWeapon = new Weapon(randItem, k + (int)itemIdOffset, 1, (randLocation.X, randLocation.Y, randLocation.Z));
                        weapons.Add(newWeapon);

                        // Weapon newWeapon = new Weapon(k, k*5, k, (xInc, 113, k % squareSize));
                        // ItemId, NetworkId, Amount, Location
                    }

                    // Generate the weapons first

                    // binaryWriterStream.Write((Int32)numOfWeapons);
                    binaryWriterStream.Write((Int32)weapons.Count);

                    for (int k = 0; k < weapons.Count; k++)
                    {
                        Weapon newWeapon = weapons[k];

                        weaponConcurrencyHandler.SpawnWeapon(newWeapon);

                        // Item ID
                        binaryWriterStream.Write((Int32)newWeapon.Type);
                        // Identifier
                        binaryWriterStream.Write((Int32)newWeapon.Id);
                        // Quantity / Amount
                        binaryWriterStream.Write((Int32)newWeapon.Count);

                        // X Y and Z
                        binaryWriterStream.Write((Single)newWeapon.Location.X);
                        binaryWriterStream.Write((Single)newWeapon.Location.Y);
                        binaryWriterStream.Write((Single)newWeapon.Location.Z);
                    }
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
                    binaryWriterStream.Write( TABGServer.secondsBeforeBaseRing );
                    // base ring time
                    binaryWriterStream.Write( TABGServer.baseRingTime );

                    int ringCount = TABGServer.ringCount;
                    // something ring-related, just set to false to disable                    
                    binaryWriterStream.Write((byte)ringCount);
                    for(int ringIndx = 0; ringIndx < ringCount; ringIndx++)
                    {
                        // ring time
                        binaryWriterStream.Write( TABGServer.ringTime[ringIndx] );
                        // ring speed
                        binaryWriterStream.Write( TABGServer.ringSpeed[ringIndx] );
                    }


                    // lives
                    binaryWriterStream.Write((Int32)2);
                    // kills to win
                    binaryWriterStream.Write((ushort)10);
                    // gamestate
                    GameState gameState = GameState.WaitingForPlayers;

                    binaryWriterStream.Write((Byte)gameState);

                    // flying stuff (?)
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(200f);
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(0f);
                    //binaryWriterStream.Write(200f);
                    //binaryWriterStream.Write(0f);

                    if ((gameState == GameState.Flying || gameState == GameState.Started))
                    {
                        // flying stuff (?)
                        binaryWriterStream.Write(false);
                        binaryWriterStream.Write(300f);
                        binaryWriterStream.Write(125f);
                        binaryWriterStream.Write(300f);

                        binaryWriterStream.Write(-200f);
                        binaryWriterStream.Write(125f);
                        binaryWriterStream.Write(-350f);
                    }
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
                    binaryWriterStream.Write((Byte)playerIndex);
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

        public void SendMessageToServer(EventCode code, byte[] buffer, bool reliable)
        {
            ENet.Packet packet = default(ENet.Packet);
            byte[] array = new byte[buffer.Length + 1];
            array[0] = (byte)code;
            Array.Copy(buffer, 0, array, 1, buffer.Length);
            packet.Create(array, reliable ? PacketFlags.Reliable : PacketFlags.None);
            this.m_peer.Send(0, ref packet);
        }

        private List<Vector3> LoadItemSpawns()
        {
            List<Vector3> list = new List<Vector3>();

            //string absPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "./DataFiles/ItemsSpawns.cs");
            string absPath = @"C:\Users\Weaver\Downloads\TABGCommunityServer-master\TABGServer\DataFiles\ItemSpawns.csv";
            string contents = File.ReadAllText(absPath);
            string[] splits = contents.Split("\n");
            foreach(string line in splits)
            {
                if (line.StartsWith("X")) continue;
                string[] values = line.Split(",");
                Vector3 vector = new Vector3(float.Parse(values[0]), float.Parse(values[1]), float.Parse(values[2]));
                list.Add(vector);
            }

            return list;
        }
    }
}
