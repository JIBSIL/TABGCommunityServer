using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace TABGCommunityServer
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
                    binaryWriterStream.Write((Int32)ItemIDs.Launch_Pad_Grenade);
                    binaryWriterStream.Write((Int32)1);

                    // give vector
                    binaryWriterStream.Write((Int32)ItemIDs.Vector);
                    binaryWriterStream.Write((Int32)999);

                    // give AWP
                    binaryWriterStream.Write((Int32)ItemIDs.AWP);
                    binaryWriterStream.Write((Int32)1);

                    // give deagle
                    binaryWriterStream.Write((Int32)ItemIDs.Desert_Eagle);
                    binaryWriterStream.Write((Int32)1);

                    // Client requires this, but it's redundant
                    binaryWriterStream.Write((Int32)0);
                }
            }
            return sendByte;
        }

        public UpdatePacket PlayerUpdate(BinaryReader binaryReader, int packetLength, PlayerConcurencyHandler playerConncurencyHandler)
        {
            // player
            var playerIndex = binaryReader.ReadByte();

            // location
            var x = binaryReader.ReadSingle();
            var y = binaryReader.ReadSingle();
            var z = binaryReader.ReadSingle();

            // rotation
            var rotX = binaryReader.ReadSingle();
            var rotY = binaryReader.ReadSingle();

            // "ads" bool (?)
            // OHH THIS MEANS AIM DOWN SIGHTS
            var ads = binaryReader.ReadBoolean();

            // optimizeDirection (?)
            var arrayLen = packetLength - 23;
            var optimizeDirection = binaryReader.ReadBytes(arrayLen);

            // movement flags
            var movementFlags = binaryReader.ReadByte();

            Player player = playerConncurencyHandler.Players[playerIndex];

            player.Id = playerIndex;
            player.Location = (x, y, z);
            player.Rotation = (rotX, rotY);
            player.Ads = ads;
            player.OptimizedDirection = optimizeDirection;
            player.MovementFlags = movementFlags;

            UpdatePacket fullPacket = new UpdatePacket(SendPlayerUpdateResponsePacket(playerConncurencyHandler), player);

            return fullPacket;
        }

        public void PlayerFire(BinaryReader binaryReader, int length, PlayerConcurencyHandler concurrencyHandler)
        {
            // player index
            var playerIndex = binaryReader.ReadByte();
            // firing mode
            var firingMode = binaryReader.ReadByte();
            // ammo type
            var ammoType = binaryReader.ReadInt32();

            // extra data (vectors and more)
            // you gotta be kidding me, did Landfall really just put a flag in rather than passing at least idk like an EMPTY ARRAY?
            // WTF LANDFALL
            int extraDataLength = length - 6;
            byte[] extraData = new byte[0];

            if (extraDataLength != 0)
            {
                extraData = binaryReader.ReadBytes(extraDataLength);
            }

            FiringMode firingModeFlag = (FiringMode)firingMode;
            byte[] sendByte = new byte[1024];
            try
            {
                using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
                {
                    using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                    {

                        // ANOTHER binary stream because of Landfall's bad design
                        // i KNOW this is confusing but the variable scopes require me to write it this way
                        using (MemoryStream memoryStream = new MemoryStream(extraData))
                        {
                            using (BinaryReader extraDataBinaryReader = new BinaryReader(memoryStream))
                            {
                                binaryWriterStream.Write(playerIndex);
                                binaryWriterStream.Write(firingMode);
                                binaryWriterStream.Write(ammoType);

                                // flag is used to signal firing mode
                                if ((firingModeFlag & FiringMode.ContainsDirection) == FiringMode.ContainsDirection)
                                {
                                    // vector direction
                                    float x = extraDataBinaryReader.ReadSingle();
                                    float y = extraDataBinaryReader.ReadSingle();
                                    float z = extraDataBinaryReader.ReadSingle();

                                    binaryWriterStream.Write(x);
                                    binaryWriterStream.Write(y);
                                    binaryWriterStream.Write(z);

                                    // random patch!!!
                                    // THIS CRASHES THE SERVER
                                    // IF YOU USE GUST SPELL!
                                    while (binaryReader.PeekChar() != -1)
                                    {
                                        byte quaternion = binaryReader.ReadByte();
                                        binaryWriterStream.Write(quaternion);
                                        if (!(quaternion >= 4))
                                        {
                                            byte[] quaternionData = extraDataBinaryReader.ReadBytes(6);
                                            binaryWriterStream.Write(quaternionData);
                                        }
                                    }
                                }

                                if ((firingModeFlag & FiringMode.WantsToBeSynced) == FiringMode.WantsToBeSynced)
                                {
                                    var syncIndex = binaryReader.ReadInt32();
                                    binaryWriterStream.Write(syncIndex);
                                }

                                if ((firingModeFlag & FiringMode.UseBulletEffect) == FiringMode.UseBulletEffect)
                                {
                                    var bulletEffectType = binaryReader.ReadByte();
                                    binaryWriterStream.Write(bulletEffectType);
                                }
                            }
                        }
                    }
                }
            } catch(Exception err)
            {
                Console.WriteLine(err);
            }

            foreach (var item in concurrencyHandler.Players)
            {
                if (item.Key == playerIndex)
                {
                    continue;
                }

                // broadcast to ALL players but the shooter
                item.Value.PendingBroadcastPackets.Add(new Packet(EventCode.PlayerFire, sendByte));
            }

            //return sendByte;
        }

        public byte[] PlayerChangedWeapon(BinaryReader binaryReader)
        {
            // player index
            var playerIndex = binaryReader.ReadByte();
            // slot flag
            var slotFlag = binaryReader.ReadByte();

            // UNKNOWN W VALUES
            // w1
            var w1 = binaryReader.ReadInt16();
            // w2
            var w2 = binaryReader.ReadInt16();
            // w3
            var w3 = binaryReader.ReadInt16();
            // w4
            var w4 = binaryReader.ReadInt16();
            // w5
            var w5 = binaryReader.ReadInt16();

            // attachments
            var attachmentsLength = binaryReader.ReadByte();
            short[] attachments = new short[256];
            for (int i = 0; i < attachmentsLength; i++)
            {
                var attachmentID = binaryReader.ReadInt16();
                attachments[i] = (short)attachmentID;
            }

            // is throwable
            var throwable = binaryReader.ReadInt16();

            byte[] sendByte = new byte[2048];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write(playerIndex);
                    binaryWriterStream.Write(slotFlag);
                    binaryWriterStream.Write(w1);
                    binaryWriterStream.Write(w2);
                    binaryWriterStream.Write(w3);
                    binaryWriterStream.Write(w4);
                    binaryWriterStream.Write(w5);
                    binaryWriterStream.Write(attachments.Length);
                    for(int i = 0; i < attachments.Length; i++)
                    {
                        binaryWriterStream.Write(attachments[i]);
                    }
                    binaryWriterStream.Write(throwable);
                }
            }

            return sendByte;
        }

        public byte[] SendPlayerUpdateResponsePacket(PlayerConcurencyHandler playerConcurrencyHandler)
        {
            byte[] sendByte = new byte[2048];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // ms (to get ms since last update)
                    float milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                    binaryWriterStream.Write(milliseconds);

                    // amount of players to loop (unimplemented)
                    binaryWriterStream.Write((byte)playerConcurrencyHandler.Players.Count);

                    foreach(var item in playerConcurrencyHandler.Players)
                    {
                        // player index
                        binaryWriterStream.Write((byte)item.Key);

                        // packet container flags
                        PacketContainerFlags packetContainerFlags = PacketContainerFlags.PlayerPosition | PacketContainerFlags.PlayerRotation | PacketContainerFlags.PlayerDirection;
                        binaryWriterStream.Write((byte)packetContainerFlags);

                        // driving state
                        binaryWriterStream.Write((byte)DrivingState.None);

                        // player position (triggered by packet container flag)
                        var loc = item.Value.Location;
                        binaryWriterStream.Write(loc.X);
                        binaryWriterStream.Write(loc.Y);
                        binaryWriterStream.Write(loc.Z);

                        // player rotation (triggered by flag)
                        var rot = item.Value.Rotation;
                        binaryWriterStream.Write(rot.X);
                        binaryWriterStream.Write(rot.Y);

                        // aim down sights state
                        binaryWriterStream.Write(item.Value.Ads);

                        // optimized direction
                        binaryWriterStream.Write(item.Value.OptimizedDirection);

                        // movement flag bytes
                        binaryWriterStream.Write(item.Value.MovementFlags);
                    }

                    // car stuff - vehicles are disabled so this isn't important
                    binaryWriterStream.Write((byte)0);
                }
            }
            return sendByte;
        }

        public byte[] RequestAirplaneDrop(BinaryReader binaryReader)
        {
            // player index
            var index = binaryReader.ReadByte();

            // location
            var x = binaryReader.ReadSingle();
            var y = binaryReader.ReadSingle();
            var z = binaryReader.ReadSingle();

            //Console.WriteLine($"I, player index {index.ToString()}, am at the location {x}, {y}, {z}");

            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    binaryWriterStream.Write(index);
                    binaryWriterStream.Write(x);
                    binaryWriterStream.Write(y);
                    binaryWriterStream.Write(z);
                    binaryWriterStream.Write(x - 1);
                    binaryWriterStream.Write(y - 100);
                    binaryWriterStream.Write(z - 1);
                }
            }

            return sendByte;
        }

        public byte[] RevivePlayer(byte playerID)
        {
            var sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    binaryWriterStream.Write((byte)ReviveState.Finished);
                    // player being revived
                    binaryWriterStream.Write(playerID);
                    // player who is revivng
                    binaryWriterStream.Write((byte)255);
                }
            }
            return sendByte;
        }

        public byte[] ClientRequestProjectileSyncEvent(PlayerConcurencyHandler playerConcurrencyHandler, BinaryReader binaryReader, int fullLength)
        {
            // "sync index"
            var syncIndex = binaryReader.ReadInt32();
            // removed
            var removed = binaryReader.ReadBoolean();
            // "everyone"
            var everyone = binaryReader.ReadBoolean();
            // include self (?)
            var includeSelf = binaryReader.ReadBoolean();
            // is static (?)
            var isStatic = binaryReader.ReadBoolean();

            // additional data length
            var addDataLength = fullLength - 8;
            // additional data byte
            var additionalData = binaryReader.ReadBytes(addDataLength);

            using (MemoryStream memoryStream = new MemoryStream(additionalData))
            {
                using (BinaryReader extraDataBinaryReader = new BinaryReader(memoryStream))
                {
                    byte syncProjectileDataType = (byte)0;
                    while (binaryReader.PeekChar() != -1)
                    {
                        syncProjectileDataType = extraDataBinaryReader.ReadByte();
                    }
                    byte[] sendByte = new byte[128];
                    using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
                    {
                        using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                        {
                            binaryWriterStream.Write(syncIndex);
                            // removed flag
                            binaryWriterStream.Write(removed);
                            // these don't matter
                            binaryWriterStream.Write(everyone);
                            binaryWriterStream.Write(includeSelf);
                            binaryWriterStream.Write(isStatic);

                            // case
                            switch (syncProjectileDataType)
                            {
                                case 1:
                                    {
                                        var syncedInt = extraDataBinaryReader.ReadInt32();
                                        binaryWriterStream.Write(syncedInt);
                                        break;
                                    }
                                case 3:
                                    {
                                        var x = extraDataBinaryReader.ReadSingle();
                                        var y = extraDataBinaryReader.ReadSingle();
                                        var z = extraDataBinaryReader.ReadSingle();

                                        binaryWriterStream.Write(x);
                                        binaryWriterStream.Write(y);
                                        binaryWriterStream.Write(z);
                                        break;
                                    }
                                case 4:
                                    {
                                        var x = extraDataBinaryReader.ReadSingle();
                                        var y = extraDataBinaryReader.ReadSingle();
                                        var z = extraDataBinaryReader.ReadSingle();

                                        var x2 = extraDataBinaryReader.ReadSingle();
                                        var y2 = extraDataBinaryReader.ReadSingle();
                                        var z2 = extraDataBinaryReader.ReadSingle();

                                        binaryWriterStream.Write(x);
                                        binaryWriterStream.Write(y);
                                        binaryWriterStream.Write(z);
                                        binaryWriterStream.Write(x2);
                                        binaryWriterStream.Write(y2);
                                        binaryWriterStream.Write(z2);
                                        break;
                                    }
                                case 5:
                                    {
                                        // no idea what this does... client requires it tho
                                        float randomFloat = extraDataBinaryReader.ReadSingle();
                                        binaryWriterStream.Write(randomFloat);
                                        break;
                                    }
                                case 6:
                                    {
                                        bool flag2 = extraDataBinaryReader.ReadByte() == 1;
                                        binaryWriterStream.Write(flag2);
                                        break;
                                    }
                                case 7:
                                    {
                                        // two bytes for collission
                                        byte b = extraDataBinaryReader.ReadByte();
                                        byte b2 = extraDataBinaryReader.ReadByte();

                                        binaryWriterStream.Write(b);
                                        binaryWriterStream.Write(b2);
                                        break;
                                    }
                                case 8:
                                    {
                                        // collision bytes PLUS vector
                                        byte b = extraDataBinaryReader.ReadByte();
                                        byte b2 = extraDataBinaryReader.ReadByte();

                                        var x = extraDataBinaryReader.ReadSingle();
                                        var y = extraDataBinaryReader.ReadSingle();
                                        var z = extraDataBinaryReader.ReadSingle();

                                        binaryWriterStream.Write(b);
                                        binaryWriterStream.Write(b2);
                                        binaryWriterStream.Write(x);
                                        binaryWriterStream.Write(y);
                                        binaryWriterStream.Write(z);
                                        break;
                                    }
                                case 9:
                                    {
                                        // just one byte (?)
                                        byte b5 = extraDataBinaryReader.ReadByte();

                                        binaryWriterStream.Write(b5);
                                        break;
                                    }
                                case 10:
                                    {
                                        // one byte and one bool. not sure what this does either
                                        byte b6 = extraDataBinaryReader.ReadByte();
                                        bool flag3 = extraDataBinaryReader.ReadBoolean();

                                        binaryWriterStream.Write(b6);
                                        binaryWriterStream.Write(flag3);
                                        break;
                                    }
                            }
                        }
                    }
                    return sendByte;
                }
            }
        }

        public byte[] RequestBlessingEvent(BinaryReader binaryReader)
        {
            // player index
            var playerIndex = binaryReader.ReadByte();
            // blessing slots
            var slot1 = binaryReader.ReadInt32();
            var slot2 = binaryReader.ReadInt32();
            var slot3 = binaryReader.ReadInt32();

            byte[] sendByte = new byte[256];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    binaryWriterStream.Write(playerIndex);
                    binaryWriterStream.Write(slot1);
                    binaryWriterStream.Write(slot2);
                    binaryWriterStream.Write(slot3);
                }
            }
            return sendByte;
        }

        public void RingDeathEvent(PlayerConcurencyHandler playerConcurencyHandler, BinaryReader binaryReader)
        {
            byte deadUser = binaryReader.ReadByte();

            byte[] killUserBytes = new PlayerHandler().KillPlayer(deadUser, deadUser, "The Wall");
            foreach (var item in playerConcurencyHandler.Players)
            {
                item.Value.PendingBroadcastPackets.Add(new Packet(EventCode.PlayerDead, killUserBytes));
            }
        }

        public byte[] GenerateRespawnPacket(byte playerIndex)
        {
            byte[] respawnBytes = new byte[512];

            using (MemoryStream writerMemoryStream = new MemoryStream(respawnBytes))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // How many players we are respawning?
                    binaryWriterStream.Write(0);

                    // Player Index
                    binaryWriterStream.Write(playerIndex);

                    // New Player Index ( I'm going to keep this the same )
                    binaryWriterStream.Write(playerIndex);

                    // Health
                    binaryWriterStream.Write((Single)100f);

                    // Position X, Y, and Z
                    //Console.WriteLine($"PlayerHandler here, respawning from boss fight. Checking all circle positions!\n {TABGServer.centers.Length} {TABGServer.centers.ToString()}");
                    int[] pos = TABGServer.centers[TABGServer.currentRingIndex - 1];

                    binaryWriterStream.Write(pos[0]);
                    binaryWriterStream.Write(150);
                    binaryWriterStream.Write(pos[1]);

                    // Rotation
                    binaryWriterStream.Write((Single)0);

                    // Curse ID ~ 255 = None
                    binaryWriterStream.Write(255);
                }
            }

            return respawnBytes;
        }

        public byte[] TaseEffectPlayer(byte playerIndex)
        {
            // Tase effect
            byte[] taseEffectBytes = new byte[512];

            using (MemoryStream writerMemoryStream = new MemoryStream(taseEffectBytes))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write(playerIndex);

                    // Weapon Effect
                    binaryWriterStream.Write((byte)WeaponEffect.Tase);

                    // Range Multiplier
                    binaryWriterStream.Write((Single)1);
                }
            }
            return taseEffectBytes;
        }

        public byte[][] BossFightResultEvent(PlayerConcurencyHandler playerConcurencyHandler, BinaryReader binaryReader)
        {
            bool didWin = binaryReader.ReadBoolean();
            byte difficulty = binaryReader.ReadByte();
            Console.WriteLine($"Boss fight Result ~ Win : {didWin}, Difficulty : {difficulty}");
            //if (didWin == false) return new byte[0][];

            byte playerIndex = 0;
            
            return new byte[][] {
                GenerateRespawnPacket(playerIndex),
                TaseEffectPlayer(playerIndex)
            };
        }

        public void PlayerDamagedEvent(PlayerConcurencyHandler playerConcurrencyHandler, BinaryReader binaryReader)
        {
            byte[] sendByte = new byte[256];
            Player playerOutside;
            Player playerOutside2;
            byte attackerOutside;

            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // attacker id
                    var attacker = binaryReader.ReadByte();
                    attackerOutside = attacker;

                    // victim id
                    var victim = binaryReader.ReadByte();
                    Player player = playerConcurrencyHandler.Players[victim];
                    playerOutside = player;

                    Player player2 = playerConcurrencyHandler.Players[attacker];
                    playerOutside2 = player;

                    binaryWriterStream.Write(victim);
                    binaryWriterStream.Write(attacker);

                    // health
                    var health = binaryReader.ReadSingle();
                    binaryWriterStream.Write(health);
                    player.Health = health;

                    if(health <= 0)
                    {
                        byte[] killUserBytes = KillPlayer(victim, attacker, player2.Name);
                        foreach (var item in playerConcurrencyHandler.Players)
                        {
                            item.Value.PendingBroadcastPackets.Add(new Packet(EventCode.PlayerDead, killUserBytes));
                        }
                    }

                    Console.WriteLine("Attacker: " + attacker + ". Victim: " + victim + ". New health value: " + health);

                    // direction
                    var x = binaryReader.ReadSingle();
                    var y = binaryReader.ReadSingle();
                    var z = binaryReader.ReadSingle();
                    binaryWriterStream.Write(x);
                    binaryWriterStream.Write(y);
                    binaryWriterStream.Write(z);

                    // "flag" for force
                    var flag = binaryReader.ReadBoolean();
                    binaryWriterStream.Write((byte)1);
                    if (flag)
                    {
                        var forceX = binaryReader.ReadSingle();
                        var forceY = binaryReader.ReadSingle();
                        var forceZ = binaryReader.ReadSingle();
                        var rigIndex = binaryReader.ReadByte();
                        var forceMode = binaryReader.ReadByte();
                        binaryWriterStream.Write(forceX);
                        binaryWriterStream.Write(forceY);
                        binaryWriterStream.Write(forceZ);
                        binaryWriterStream.Write(rigIndex);
                        binaryWriterStream.Write(forceMode);
                    }
                    else
                    {
                        var returnToSender = binaryReader.ReadBoolean();
                    }
                }
            }

            playerOutside.PendingBroadcastPackets.Add(new Packet(EventCode.PlayerDamaged, sendByte));
            playerOutside2.PendingBroadcastPackets.Add(new Packet(EventCode.PlayerDamaged, sendByte));

            //foreach (var item in playerConcurrencyHandler.Players)
            //{
            //    if (item.Key == attackerOutside)
            //    {
            //        continue;
            //    }

            // broadcast to ALL players but the shooter
            //    item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.PlayerDamaged, sendByte));
            //}

            return;
        }

        public byte[] SetPlayerHealth(byte playerID, float newHealth)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    binaryWriterStream.Write(playerID);
                    binaryWriterStream.Write(newHealth);
                }
            }
            return sendByte;
        }

        public byte[] SimulateChunkEnter(PlayerConcurencyHandler playerConcurrencyHandler, byte playerIndex, TABGPlayerState playerState, float health)
        {
            byte[] sendByte = new byte[1024];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player index
                    binaryWriterStream.Write(playerIndex);
                    // player state (transcended etc)
                    binaryWriterStream.Write((byte)playerState);
                    // health
                    binaryWriterStream.Write(health);
                    // is player downed
                    binaryWriterStream.Write(false);
                    // is skydiving
                    binaryWriterStream.Write(false);
                    // gear data
                    var player = playerConcurrencyHandler.Players[playerIndex];
                    binaryWriterStream.Write((Int32)player.GearData.Length);
                    for(int i = 0; i < player.GearData.Length; i++)
                    {
                        binaryWriterStream.Write(player.GearData[i]);
                    }
                    // equipment (not tracked by concurrency handler yet)
                    binaryWriterStream.Write((byte)0);
                    // attachments (also not tracked)
                    binaryWriterStream.Write((byte)0);
                    // blessings
                    binaryWriterStream.Write((Int32)0);
                    binaryWriterStream.Write((Int32)0);
                    binaryWriterStream.Write((Int32)0);
                    // should spawn car?
                    binaryWriterStream.Write((byte)DrivingState.None);
                }
            }
            return sendByte;
        }

        public byte[] RequestHealthState(PlayerConcurencyHandler playerConcurrencyHandler, BinaryReader binaryReader)
        {

            // player's index
            var playerIndex = binaryReader.ReadByte();
            // new health
            var newHealth = binaryReader.ReadSingle();

            // same packet as heal
            return SetPlayerHealth(playerIndex, newHealth);
        }

        public byte[] BroadcastPlayerJoin(byte playerID, string playerName, int[] gearData)
        {
            byte[] sendByte = new byte[128];
            using (MemoryStream writerMemoryStream = new MemoryStream(sendByte))
            {
                using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                {
                    // player id
                    binaryWriterStream.Write(playerID);

                    // group id
                    binaryWriterStream.Write((byte)0);

                    // username length
                    binaryWriterStream.Write((Int32)(playerName.Length));
                    // username
                    binaryWriterStream.Write(Encoding.UTF8.GetBytes(playerName));

                    // gear data.
                    binaryWriterStream.Write(gearData.Length);
                    foreach (int num in gearData)
                    {
                        binaryWriterStream.Write(num);
                    }

                    // is dev
                    binaryWriterStream.Write(true);
                }
            }
            return sendByte;
        }
    }
}
