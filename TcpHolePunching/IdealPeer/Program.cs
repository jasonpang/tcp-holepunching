using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TcpHolePunching;
using TcpHolePunching.Messages;

namespace IdealClient
{
    public class Program
    {
        private static NetworkPeer Peer { get; set; }

        static void Main(string[] args)
        {
            Console.Title = "Ideal Client - TCP Hole Punching Proof of Concept";

            Peer = new NetworkPeer();
            Peer.OnConnectionAccepted += Peer_OnConnectionAccepted;
            Peer.OnConnectionSuccessful += PeerOnConnectionSuccessful;
            Peer.OnMessageSent += PeerOnMessageSent;
            Peer.OnMessageReceived += Peer_OnMessageReceived;

            Peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.NoDelay, true);
            Peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, 1);
            Peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

            Console.Write("Bind to which port?: ");
            int portToBind = Int32.Parse(Console.ReadLine());
            Peer.Bind(new IPEndPoint(IPAddress.Any, portToBind));

            Console.Write("Endpoint of your peer: ");

            var introducerEndpoint = Console.ReadLine().Parse();

            Console.WriteLine(String.Format("Connecting to at {0}:{1}...", introducerEndpoint.Address, introducerEndpoint.Port));
            Peer.Connect(introducerEndpoint.Address, introducerEndpoint.Port);

            Console.Write("Press <ENTER> to set socket options back to normal.");
            Console.ReadLine();
            Peer.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.IpTimeToLive, 4);

            Application.Run();
        }

        static void Peer_OnConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
        {
        }

        static void PeerOnConnectionSuccessful(object sender, ConnectionAcceptedEventArgs e)
        {
        }

        static void PeerOnMessageSent(object sender, MessageSentEventArgs e)
        {
        }

        static void Peer_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
        }
    }
}
