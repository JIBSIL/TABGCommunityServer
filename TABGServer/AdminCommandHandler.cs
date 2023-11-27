using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class AdminCommandHandler
    {
        private string command;
        public bool shouldSendPacket = false;
        public ClientEventCode code;
        public string? notification;
        public byte[] packetData;
        private byte executor;
        public AdminCommandHandler(string command, byte executor) {
            this.command = command;
            // filler data
            this.packetData = new byte[1];
            this.notification = "";
            this.executor = executor;
        }

        public void Handle(PlayerConcurencyHandler playerConncurencyHandler)
        {
            this.notification = null;
            this.packetData = new byte[1];
            this.shouldSendPacket = false;

            Console.WriteLine("Processing command " + command);

            if (command.StartsWith("/"))
            {
                command = command.Substring(1);
            }
            else
            {
                Console.WriteLine("Debug: command does not start with a /");
                return;
            }

            string[] parts = command.Split(' ');
            
            if (parts.Length == 0)
            {
                // invalid command (just /)
                return;
            }

            switch (parts[0])
            {
                case "kill":
                    // format: /kill VictimID KillerID VictimName
                    // example: /kill 0 0 Tester
                    if(parts.Length != 4)
                    {
                        Console.WriteLine("Ignoring invalid command!");
                        return;
                    }
                    try
                    {
                        int victim = int.Parse(parts[1]);
                        int killer = int.Parse(parts[2]);
                        string victimName = parts[3];

                        this.shouldSendPacket = true;
                        this.packetData = new PlayerHandler().KillPlayer(victim, killer, victimName);
                        this.code = ClientEventCode.PlayerDead;

                        return;

                    } catch(Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                    
                    return;
                case "give":
                    // format: /give ItemID ItemAmount
                    // example: /give 32 1

                    if (parts.Length != 3)
                    {
                        Console.WriteLine("Ignoring invalid command!");
                        return;
                    }

                    try
                    {
                        int itemid = int.Parse(parts[1]);
                        int amount = int.Parse(parts[2]);

                        this.shouldSendPacket = true;
                        this.packetData = new PlayerHandler().GiveItem(itemid, amount);
                        this.code = ClientEventCode.PlayerLootRecieved;

                        return;
                    } catch(Exception error)
                    {
                        Console.WriteLine(error.Message);
                    }
                    return;
                case "notification":
                    this.packetData = new PlayerHandler().SendNotification(executor, "WELCOME - RUNNING COMMUNITY SERVER V1.TEST");
                    this.shouldSendPacket = true;
                    this.code = ClientEventCode.PlayerDead;
                    return;
                case "kit":
                    Console.WriteLine("Giving kit to player " + executor);
                    this.packetData = new PlayerHandler().GiveGear();
                    this.shouldSendPacket = true;
                    this.code = ClientEventCode.PlayerLootRecieved;
                    this.notification = "You got the default kit!";
                    return;
                case "coords":
                    var executorData = playerConncurencyHandler.Players[executor];
                    var loc = executorData.Location;
                    string notif = "COORDS- X: " + loc.X.ToString() + " Y: " + loc.Y.ToString() + " Z: " + loc.Z.ToString();
                    this.packetData = new PlayerHandler().SendNotification(executor, notif);
                    this.code = ClientEventCode.PlayerDead;
                    this.shouldSendPacket = true;
                    return;
                case "broadcast":
                    if (parts.Length != 2)
                    {
                        Console.WriteLine("Ignoring invalid command!");
                        return;
                    }
                    foreach (var item in playerConncurencyHandler.Players)
                    {
                        item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.PlayerDead, new PlayerHandler().SendNotification(item.Value.Id, "ANNOUNCE: " + parts[1])));
                    }
                    return;
                case "revive":
                    this.shouldSendPacket = false;
                    foreach (var item in playerConncurencyHandler.Players)
                    {
                        item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.ReviveState, new PlayerHandler().RevivePlayer(executor)));
                        item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.PlayerHealed, new PlayerHandler().SetPlayerHealth(executor, 100f)));
                    }
                    this.notification = "You were revived by SERVER";
                    return;
                case "heal":
                    float health = 100f;
                    if (parts.Length == 2)
                    {
                        try
                        {
                            health = float.Parse(parts[1]);
                        } catch(Exception error)
                        {
                            Console.WriteLine("Parsing error!" + error.Message);
                            return;
                        }
                    }
                    foreach (var item in playerConncurencyHandler.Players)
                    {
                        item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.PlayerHealed, new PlayerHandler().SetPlayerHealth(executor, health)));
                    }
                    this.notification = "Healed!";
                    return;
                case "state":
                    try
                    {
                        TABGPlayerState playerState = (TABGPlayerState)Byte.Parse(parts[1]);
                        float playerHealth = float.Parse(parts[2]);

                        foreach (var item in playerConncurencyHandler.Players)
                        {
                            item.Value.PendingBroadcastPackets.Add(new Packet(ClientEventCode.PlayerEnteredChunk, new PlayerHandler().SimulateChunkEnter(playerConncurencyHandler, executor, playerState, playerHealth)));
                        }
                        this.notification = "Player state changed!";
                    } catch(Exception e)
                    {
                        Console.WriteLine(e.Message);
                        this.notification = "Player state change ERROR!";
                    }
                    return;
                case "gamestate":
                    if (parts.Length < 2)
                    {
                        Console.WriteLine("Ignoring invalid command!");
                        return;
                    }

                    // broadcast instead of send
                    this.code = ClientEventCode.GameStateChanged;

                    switch (parts[1])
                    {
                        case "waiting":
                            this.packetData = GameHandler.SetWaitingForPlayersState();
                            break;
                        case "started":
                            this.packetData = GameHandler.SetStarted();
                            break;
                        case "countdown":
                            this.packetData = GameHandler.SetCountDown(Int32.Parse(parts[2]));
                            break;
                        case "flying":
                            this.packetData = GameHandler.SetFlying(Byte.Parse(parts[2]));
                            break;
                    }

                    foreach (var item in playerConncurencyHandler.Players)
                    {
                        item.Value.PendingBroadcastPackets.Add(new Packet(this.code, this.packetData));
                    }

                    this.packetData = new PlayerHandler().SendNotification(executor, "GAME STATE CHANGED!");
                    this.code = ClientEventCode.PlayerDead;
                    this.shouldSendPacket = true;
                    return;
                default: return;
            }
        }
    }
}
