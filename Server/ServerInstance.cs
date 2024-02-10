using ENet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TABGxGUI;

namespace TABG
{
    public class ServerInstance
    {
        public readonly ushort Port = 9700;
        private string Owner;

        private bool killSignal = false;

        public ServerInstance(ushort port, string owner = "") {
            this.Port = port;
            this.Owner = owner;
        }
        public void Start(int players, GameMode gamemode, bool autostart)
        {
            bool oneTimeMatchDisplay = false;
            ushort port = Port;
            ushort maxClients = (ushort) players;
            UnityEngine.Debug.Log("TABG COMMUNITY SERVER v1 STARTED ON PORT " + port);

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

            // TODO: make this loop cleaner so it disconnects people normally when killing rather than timing them out
            while (!killSignal)
            {
                if (!oneTimeClientStart)
                {
                    oneTimeClientStart = true;
                    UnityEngine.Debug.Log("Server Started on port " + port.ToString() + "!");
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
                            UnityEngine.Debug.Log("Client connected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            if(oneTimeMatchDisplay == false)
                            {
                                //TABGxUI.IsInitialized = true;
                                oneTimeMatchDisplay = true;
                            }
                            break;

                        case EventType.Disconnect:
                            UnityEngine.Debug.Log("Client disconnected - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            break;

                        case EventType.Timeout:
                            UnityEngine.Debug.Log("Client timeout - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP);
                            break;

                        case EventType.Receive:
                            //UnityEngine.Debug.Log("Packet received from - ID: " + netEvent.Peer.ID + ", IP: " + netEvent.Peer.IP + ", Channel ID: " + netEvent.ChannelID + ", Data length: " + netEvent.Packet.Length);

                            byte[] array = new byte[netEvent.Packet.Length];
                            netEvent.Packet.CopyTo(array);

                            var code = (EventCode)array[0];
                            var buffer = new byte[array.Length - 1];

                            Array.Copy(array, 1, buffer, 0, buffer.Length);

                            new PacketHandler(netEvent.Peer, manager, weaponConcurrencyHandler, gamemode, autostart).Handle(code, buffer);

                            netEvent.Packet.Dispose();
                            break;
                    }
                }
            }
            server.Flush();
        }

        public void Kill() {
            this.killSignal = true;
        }
    }
}
