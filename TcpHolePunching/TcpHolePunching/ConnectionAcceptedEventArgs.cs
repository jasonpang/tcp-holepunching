using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpHolePunching
{
    public class ConnectionAcceptedEventArgs : EventArgs
    {
        public Socket Socket { get; set; }
    }
}
