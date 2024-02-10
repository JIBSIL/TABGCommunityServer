using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class ServerConcurrencyHandler
    {
        public Dictionary<ushort, ServerInstance> Servers = new();
        public int TotalServers = 0;

        private ushort startAt = 9000;
        public int MaxServers = 100;

        public ServerConcurrencyHandler() {}

        public ServerWrapper CreateServer(int players, GameMode gamemode, bool autostart)
        {
            ServerWrapper wrapper = new(true, null);

            if(TotalServers++ > MaxServers)
            {
                UnityEngine.Debug.Log("Max server count exceeded! Consider raising maxServers");
                wrapper.Error = true;
                return wrapper;
            }

            // port finder
            bool hasFoundPort = false;
            ushort port = this.startAt;
            while(!hasFoundPort)
            {
                if (!Servers.ContainsKey(port)) {
                    hasFoundPort = true;
                } else
                {
                    var nextPort = port++;
                    if (nextPort > 65534)
                    {
                        UnityEngine.Debug.Log("No more ports are available!");
                        wrapper.Error = true;

                        return wrapper;
                    }
                    
                }
            }

            ServerInstance serverInstance = new ServerInstance(port);
            // run in background
            UnityEngine.Debug.Log($"Starting server on port {port}");
            _ = StartServer(serverInstance, players, gamemode, autostart);
            Servers[port] = serverInstance;
            TotalServers++;

            wrapper.Server = serverInstance;
            wrapper.Error = false;

            return wrapper;
        }

        private static async Task StartServer(ServerInstance instance, int players, GameMode gamemode, bool autostart)
        {
            // return control back to main thread immediately
            await Task.Yield();
            UnityEngine.Debug.Log("Provisioning server...");
            instance.Start(players, gamemode, autostart);
        }

        public void KillServer(ushort port)
        {
            TotalServers--;
            Servers[port].Kill();
            UnityEngine.Debug.Log("Server on port " + port + "stopped!");
        }
    }
}
