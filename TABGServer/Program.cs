namespace TABGCommunityServer;

using System;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Timers;
using System.Text;
using System.Threading.Channels;
using ENet;

class TABGServer
{
    static Stopwatch stopwatch = new Stopwatch();

    static PlayerConcurencyHandler manager;
    static WeaponConcurrencyHandler weaponConcurrencyHandler;


    // Player/Server Data
    public static int minPlayersStart = 16;
    public static bool startGame = false;


    // Ring Data
    public static int maxRingSize = 1600;
    public static int currentRingIndex = 1;

    public static int secondsBeforeBaseRing = 10;
    public static int baseRingTime = 120; // 2 minutes

    public static int ringCount = 4;
    public static int[] ringTime = new int[] { 60, 60, 60, 60 };
    //public static int[] ringTime = new int[] { 120, 120, 120, 120 };
    public static int[] ringSpeed = new int[] { 1, 1, 1, 1 };

    public static int[] radii = new int[] { maxRingSize, maxRingSize / 2, maxRingSize / 4, maxRingSize / 8, maxRingSize / 16 };
    public static int[][] centers = new int[][] { new int[] { 0, 0 } };

    // Timer Values
    public static int countdownTimerValue = 10;
    public static int maxBusDuration = 60;

    // Boolean Check Values
    static bool flyingTimerSet = false;
    static bool sentFirstRing = false;
    static bool ringSet = false;

    // data to ensure everyone gets packets

    // List of packets, index is their ID
    static List< Tuple<EventCode, byte[], bool> > packetList = new();
    // Index is Peer ID, Sublist is list of every packet ID recieved
    static List<List<int>> peerPacketsRecievedList = new();


    static GameState currentGameState = GameState.WaitingForPlayers;

    static void handleFrame(Event netEvent)
    {
        while(centers.Length < ringCount)
        {
            int[] oldCenter = centers[ centers.Length-1 ];
            int oldRadius = radii[centers.Length-1 ];
            int newRadius = radii[centers.Length ];

            int[] c = getCenterOfNewRing(oldRadius, newRadius, oldCenter);
            //Console.WriteLine($"Center Old: {oldCenter[0]}, {oldCenter[1]} - oldRad: {oldRadius}, newRad: {newRadius} ~ newCenter: {c[0]}, {c[1]}");
            Array.Resize(ref centers, centers.Length+1);
            centers[centers.Length - 1] = c;
        }


        long deltaTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Restart();

        // i'd rather die than reverse this because im lazy
        if (manager.Players.Count >= minPlayersStart || startGame) {}
        else { return; }

        switch (currentGameState)
        {
            case GameState.WaitingForPlayers:
                currentGameState = GameState.CountDown;
                setGamestate(manager, currentGameState, countdownTimerValue);

                Timer countdownTimer = new Timer(countdownTimerValue * 1000);
                countdownTimer.Elapsed += (sender, e) =>
                {
                    currentGameState = GameState.Flying;
                    setGamestate(manager, currentGameState, 0);
                    countdownTimer.Stop();
                };
                countdownTimer.Enabled = true;
                break;
            case GameState.CountDown:
                break;
            case GameState.Flying:
                if (flyingTimerSet == true) break;

                Timer flyingTimer = new Timer(maxBusDuration * 1000);
                flyingTimer.Elapsed += (sender, e) =>
                {
                    currentGameState = GameState.Started;
                    setGamestate(manager, currentGameState, 0);

                    byte[] dropEveryonePacket = new byte[128];
                    using (MemoryStream writerMemoryStream = new MemoryStream(dropEveryonePacket))
                    {
                        using (BinaryWriter binaryWriterStream = new BinaryWriter(writerMemoryStream))
                        {
                            binaryWriterStream.Write(0);
                            binaryWriterStream.Write(0);
                            binaryWriterStream.Write(0);
                        }
                    }
                    //new PacketHandler(netEvent.Peer, manager, weaponConcurrencyHandler).SendMessageToServer(EventCode.AllDrop, dropEveryonePacket, true);
                    SendMessageToServer(EventCode.AllDrop, dropEveryonePacket, true);

                    flyingTimer.Stop();
                };
                flyingTimer.Enabled = true;

                flyingTimerSet = true;
                break;
            case GameState.Started:
                if (sentFirstRing == true) break;

                Timer firstRingTimer = new Timer(secondsBeforeBaseRing * 1000);
                firstRingTimer.Elapsed += (sender, e) =>
                {
                    bool oldRingStatus = ringSet ? true : false;
                    nextRingUpdate(netEvent.Peer, manager, weaponConcurrencyHandler, false);

                    if (oldRingStatus == false)
                    {
                        Console.WriteLine($"Set ring, will start moving in {firstRingTimer.Interval / 1000} seconds");
                        return;
                    }
                    else { firstRingTimer.Stop(); }

                    if (currentRingIndex > ringCount)
                    {
                        Console.WriteLine("Completed all rings, now exiting ring timer loop thing");
                    }
                    else
                    {
                        Console.WriteLine($"Now on ring index {currentRingIndex}, setting an Interval of {ringTime[currentRingIndex - 1]} ");
                        firstRingTimer.Interval = (ringTime[currentRingIndex - 1]) * 1000;
                        firstRingTimer.Start();
                    }
                };
                firstRingTimer.Enabled = true;
                sentFirstRing = true;
                break;
        }
        // end of func.
    }

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

        manager = new();
        weaponConcurrencyHandler = new();

        stopwatch.Start();
        while (!Console.KeyAvailable)
        {
            if (!oneTimeClientStart)
            {
                //Task.Run(TABGEmulationClient);
                oneTimeClientStart = true;
                Console.WriteLine($"Server Started on port {port}!");
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
                        handleFrame(netEvent);
                        Console.WriteLine(netEvent.Peer.ID);


                        byte[] array = new byte[netEvent.Packet.Length];
                        netEvent.Packet.CopyTo(array);
                        
                        var code = (EventCode)array[0];
                        var buffer = new byte[array.Length - 1];

                        Array.Copy(array, 1, buffer, 0, buffer.Length);

                        new PacketHandler(netEvent.Peer, manager, weaponConcurrencyHandler).Handle(code, buffer);

                        netEvent.Packet.Dispose();
                        break;

                    default:
                        Console.WriteLine("Client (ID: " + netEvent.Peer.ID + " ~ IP: " + netEvent.Peer.IP + ") Sent Event: " + netEvent.Type.ToString());
                        break;
                }
            }
        }

        server.Flush();

    }

    /*
    // List of packets, index is their ID
    static List<byte[]> packetList = new();
    // Index is Peer ID, Sublist is list of every packet ID recieved
    static List<List<int>> peerPacketsRecievedList = new();
    */

    public static void checkUnsetPacketsForClient(Peer peer)
    {
        uint peerID = peer.ID;
        List<int> recievedPackets = peerPacketsRecievedList[(int)peerID];

        for(int packetIndex=0; packetIndex<packetList.Count; packetIndex++)
        {
            if (packetList[packetIndex] == null) continue;
            if (recievedPackets.IndexOf(packetIndex) == -1) continue;

            EventCode eventCode = packetList[packetIndex].Item1;
            byte[] packetData = packetList[packetIndex].Item2;
            bool reliable = packetList[packetIndex].Item3;

            // Sent Packet to client & mark packet sent
            new PacketHandler(peer, manager, weaponConcurrencyHandler).SendMessageToServer(eventCode, packetData, reliable);
            peerPacketsRecievedList[(int)peerID].Add(packetIndex);
        }
    }
    public static void SendMessageToServer(EventCode eventCode, byte[] packetData, bool reliable)
    {
        // make sure every client has a list!
        while (peerPacketsRecievedList.Count < manager.Players.Count)
        {
            peerPacketsRecievedList.Add( new() );
        }

        Tuple<EventCode,byte[],bool> tupleData = Tuple.Create(eventCode, packetData, reliable);

        packetList.Add(tupleData);
        int indexOfPacket = packetList.LastIndexOf(tupleData);
        //new PacketHandler(netEvent.Peer, manager, weaponConcurrencyHandler).SendMessageToServer(eventCode, packetData, reliable);
    }

    public static void nextRingUpdate(Peer peer, PlayerConcurencyHandler manager, WeaponConcurrencyHandler weaponConcurrencyHandler, bool doAll)
    {
        if (doAll || ringSet==false)
        {
            // set ring radius
            int[] ringPosition = centers[currentRingIndex - 1];
            byte[] newRingPacket = GameHandler.GenerateRingPacket(1, (byte)currentRingIndex, ringPosition[0], 113, ringPosition[0], radii[currentRingIndex - 1]);
            //new PacketHandler(peer, manager, weaponConcurrencyHandler).SendMessageToServer(EventCode.RingUpdate, newRingPacket, true);
            SendMessageToServer(EventCode.RingUpdate, newRingPacket, true);
        }

        if (doAll || ringSet)
        {
            // start moving the ring
            byte[] startRingPacket = GameHandler.GenerateRingPacket(2, 0, 0, 0, 0, 0);
            //new PacketHandler(peer, manager, weaponConcurrencyHandler).SendMessageToServer(EventCode.RingUpdate, startRingPacket, true);
            SendMessageToServer(EventCode.RingUpdate, startRingPacket, true);
            currentRingIndex++;
        }

        ringSet = !ringSet;
    }

    public static void setGamestate(PlayerConcurencyHandler manager, GameState gamestate, int secondsOrModifier)
    {
        byte[] packetBytes = new byte[512];
        switch (gamestate)
        {
            case GameState.WaitingForPlayers:
                packetBytes = GameHandler.SetWaitingForPlayersState();
                break;
            case GameState.CountDown:
                packetBytes = GameHandler.SetCountDown(secondsOrModifier);
                break;
            case GameState.Flying:
                packetBytes = GameHandler.SetFlying((byte)secondsOrModifier);
                break;
            case GameState.Started:
                packetBytes = GameHandler.SetStarted();
                break;
        }

        foreach (var item in manager.Players)
        {
            item.Value.PendingBroadcastPackets.Add(new Packet(EventCode.GameStateChanged, packetBytes));
        }
        return;
    }

    public static int[] getCenterOfNewRing(int oldRadius, int newRadius, int[] oldPosition) {
        int diff = oldRadius - newRadius;
        int[] newPosition = randomPointInCircle(oldRadius, oldPosition);

        return newPosition;
    }

    public static int[] randomPointInCircle(int radius, int[] position)
    {
        Random rand = new Random();
        int randomRadius = (int)rand.NextInt64(radius);
        int randomAngle = (int)rand.NextInt64(360);

        return new int[] {
            (int)(position[0] + (randomRadius * Math.Cos(randomAngle))),
            (int)(position[1] + (randomRadius * Math.Sin(randomAngle)))
        };
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