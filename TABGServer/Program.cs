namespace TABG;

using System;
using System.Diagnostics.Tracing;
using System.Text;
using System.Threading.Channels;
using ENet;

class TABGServer
{
    static void Main()
    {
        ushort port = 9701;
        ushort maxClients = 256;
        Console.WriteLine("TABG COMMUNITY SERVER v1 STARTED");

        ENet.Library.Initialize();

        using Host server = new Host();

        Address address = new Address();
        bool oneTimeClientStart = false;

        address.Port = port;
        address.SetIP("0.0.0.0");

        server.Create(address, maxClients);

        Event netEvent;

        PlayerConcurencyHandler manager = new();
        WeaponConcurrencyHandler weaponConcurrencyHandler = new();

        while (!Console.KeyAvailable)
        {
            if (!oneTimeClientStart)
            {
                //Task.Run(TABGEmulationClient);
                oneTimeClientStart = true;
                Console.WriteLine("Server Started!");
            }
            bool polled = false;

            while (!polled)
            {
                if (server.CheckEvents(out netEvent) <= 0)
                {
                    if (server.Service(15, out netEvent) <= 0)
                        break;

                    polled = true;
                }
                switch (netEvent.Type)
                {
                    case EventType.None:
                        break;

                    case EventType.Connect:
                        Console.WriteLine("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        break;

                    case EventType.Disconnect:
                        Console.WriteLine("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        break;

                    case EventType.Timeout:
                        Console.WriteLine("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                        break;

                    case EventType.Receive:
                        //Console.WriteLine("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                        byte[] array = new byte[netEvent.Packet.Length];
                        netEvent.Packet.CopyTo(array);
                        
                        var code = (ClientEventCode)array[0];
                        var buffer = new byte[array.Length - 1];

                        Array.Copy(array, 1, buffer, 0, buffer.Length);

                        new PacketHandler(netEvent.Peer, manager, weaponConcurrencyHandler).Handle(code, buffer);

                        netEvent.Packet.Dispose();
                        break;
                }
            }
        }

        server.Flush();

    }

    public static void TABGEmulationClient()
    {
        ushort port = 9700;
        bool connected = false;

        Thread.Sleep(1000);

        Console.WriteLine("[CLIENT] Started Client!");

        using (Host client = new Host())
        {
            Address address = new Address();

            address.SetHost("127.0.0.1");
            address.Port = port;
            client.Create();

            Peer peer = client.Connect(address);

            Event netEvent;

            while (!Console.KeyAvailable)
            {
                bool polled = false;

                if (connected)
                {
                    //Console.WriteLine("[CLIENT] Sending Packet to server!");
                    //Packet packet = new Packet();
                    //byte[] data = Encoding.ASCII.GetBytes("POLL_NOTIMEOUT");

                    //packet.Create(data, PacketFlags.Reliable);

                    //bool error = peer.Send(0, ref packet);
                    //if (error)
                    //    Console.WriteLine(error);
                        //Console.WriteLine("[CLIENT] FAILED sending packet!");
                    //else
                    //    Console.WriteLine("[CLIENT] Succeeded sending NOTIMEOUT!");
                }

                while (!polled)
                {
                    if (client.CheckEvents(out netEvent) <= 0)
                    {
                        if (client.Service(15, out netEvent) <= 0)
                            break;

                        polled = true;
                    }

                    switch (netEvent.Type)
                    {
                        case EventType.None:
                            break;

                        case EventType.Connect:
                            Console.WriteLine("Client connected to server");
                            connected = true;
                            break;

                        case EventType.Disconnect:
                            Console.WriteLine("Client disconnected from server");
                            break;

                        case EventType.Timeout:
                            Console.WriteLine("Client connection timeout");
                            break;

                        case EventType.Receive:
                            Console.WriteLine("Packet received from server - Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);
                            netEvent.Packet.Dispose();
                            break;
                    }
                }
            }

            client.Flush();
        }
    }
}