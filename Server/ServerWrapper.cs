using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TABG
{
    internal class ServerWrapper
    {
        public ServerInstance Server { get; set; }
        public bool Error { get; set; }

        public ServerWrapper(bool error, ServerInstance? server)
        {
            if(server != null)
            {
                Server = server;
            }
            Error = error;
        }
    }
}
