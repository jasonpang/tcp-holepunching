using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TcpHolePunching
{
    public class NetworkPeer : NetworkClient
    {
        public Socket PeerSocket { get; private set; }
        public byte[] PeerBuffer { get; private set; }

        public event EventHandler<ConnectionAcceptedEventArgs> OnConnectionAccepted;

        public NetworkPeer() : base()
        {
            PeerSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            PeerBuffer = new byte[1024];
        }

        /// <summary>
        /// Only binds without listening.
        /// </summary>
        public void Bind(EndPoint on)
        {
            Socket.Bind(on);
        }

        public void Listen()
        {
            Socket.Listen(Int32.MaxValue);
            Task_BeginAccepting();
        }

        private void Task_BeginAccepting()
        {
            var task = Task.Factory.FromAsync<Socket>(Socket.BeginAccept, Socket.EndAccept, null);
            task.ContinueWith(nextTask =>
            {
                Task_OnConnectionAccepted(task.Result);
                Task_BeginAccepting(); // Listen for another connection
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void Task_OnConnectionAccepted(Socket socket)
        {
            Console.WriteLine(String.Format("Connection to {0} accepted.", socket.RemoteEndPoint));

            PeerSocket = socket;

            if (OnConnectionAccepted != null)
                OnConnectionAccepted(this, new ConnectionAcceptedEventArgs() { Socket = socket} );
        }
    }
}
