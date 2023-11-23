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

        public void Handle()
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
                default: return;
            }
        }
    }
}
