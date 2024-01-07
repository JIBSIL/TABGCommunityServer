using System;
using ENet;

string ip = "0.0.0.0";
ushort port = 5000;

using (Host client = new Host())
{
    Address address = new Address();

    address.SetHost(ip);
    address.Port = port;
    try
    {
        client.Create();
    } catch (Exception)
    {
        Console.WriteLine("server is not running!! exiting...");
        return;
    }

    Peer peer = client.Connect(address);

    Event netEvent;

    while (!Console.KeyAvailable)
    {
        bool polled = false;

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