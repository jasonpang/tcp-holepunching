using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace TcpHolePunching
{
    public class Client
    {
        /// <summary>
        /// The socket that belongs to the client.
        /// </summary>
        public Socket Socket { get; set; }

        /// <summary>
        /// The buffer in which to receive bytes from the transport.
        /// </summary>
        public byte[] Buffer { get; set; }

        public Client(Socket socket)
        {
            Socket = socket;
            Buffer = new byte[1024];
        }

        public EndPoint RemoteEndPoint
        {
            get 
            {
                if (Socket == null)
                    return null;

                return Socket.RemoteEndPoint;
            }
        }
    }
}
