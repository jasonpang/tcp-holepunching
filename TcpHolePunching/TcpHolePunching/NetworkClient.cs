using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using TcpHolePunching.Messages;

namespace TcpHolePunching
{
    /// <summary>
    /// A specialized Socket for the Introducer.
    /// </summary>
    public class NetworkClient
    {
        /// <summary>
        /// Gets the underlying raw socket.
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// The buffer in which to receive bytes from the transport.
        /// </summary>
        public byte[] Buffer { get; set; }

        /// <summary>
        /// Occurs after an accepted client has been registered.
        /// </summary>
        public event EventHandler<ConnectionAcceptedEventArgs> OnConnectionSuccessful;
        public event EventHandler<MessageSentEventArgs> OnMessageSent;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public NetworkClient()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            // Set our special Tcp hole punching socket options
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Buffer = new byte[1024];
        }

        /// <summary>
        /// </summary>
        public void Connect(IPAddress host, int port)
        {
            Task_BeginConnecting(host, port);
        }

        private void Task_BeginConnecting(IPAddress host, int port)
        {
            var task = Task.Factory.FromAsync(Socket.BeginConnect(host, port, null, null), Socket.EndConnect);
            task.ContinueWith(nextTask => Task_OnConnectSuccessful(), TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void Task_OnConnectSuccessful()
        {
            Console.WriteLine(String.Format("Connected to {0}.", Socket.RemoteEndPoint));

            Task_BeginReceive();

            // Invoke the event
            if (OnConnectionSuccessful != null)
                OnConnectionSuccessful(this, new ConnectionAcceptedEventArgs() { Socket = Socket });
        }

        public void Send(MessageBase messageBase)
        {
            // If the registrant exists
            if (Socket.Connected)
            {
                var data = messageBase.GetBytes();
                var task = Task.Factory.FromAsync<Int32>(Socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, Socket), Socket.EndSend);
                task.ContinueWith(nextTask => Task_OnSendCompleted(task.Result, data.Length, Socket.RemoteEndPoint, messageBase.MessageType), TaskContinuationOptions.OnlyOnRanToCompletion);
            }
        }

        private void Task_OnSendCompleted(int numBytesSent, int expectedBytesSent, EndPoint to, MessageType messageType)
        {
            if (numBytesSent != expectedBytesSent)
                Console.WriteLine(String.Format("Warning: Expected to send {0} bytes but actually sent {1}!",
                                                expectedBytesSent, numBytesSent));

            Console.WriteLine(String.Format("Sent a {0} byte {1}Message to {2}.", numBytesSent, messageType, to));

            if (OnMessageSent != null)
                OnMessageSent(this, new MessageSentEventArgs() {Length = numBytesSent, To = to});
        }

        private void Task_BeginReceive()
        {
            var task = Task.Factory.FromAsync<Int32>(Socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, null, null), Socket.EndReceive);
            task.ContinueWith(nextTask =>
            {
                try
                {
                    Task_OnReceiveCompleted(task.Result);
                    Task_BeginReceive(); // Receive more data
                }
                catch (Exception ex)
                {
                    var exceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                    Console.WriteLine(exceptionMessage);
                    ShutdownAndClose();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void Task_OnReceiveCompleted(int numBytesRead)
        {
            // Build back our MessageReader
            var reader = new BufferValueReader(Buffer);
            var message = new Message();
            message.ReadPayload(reader);
            reader.Position = 0;

            Console.WriteLine(String.Format("Received a {0} byte {1}Message from {2}.", numBytesRead, message.MessageType, Socket.RemoteEndPoint));

            if (OnMessageReceived != null)
                OnMessageReceived(this, new MessageReceivedEventArgs() { From = (IPEndPoint) Socket.RemoteEndPoint, MessageReader = reader, MessageType = message.MessageType });
        }

        /// <summary>
        /// Only disconnects without shutting down.
        /// </summary>
        public void Disconnect()
        {
            Socket.Disconnect(true);
        }

        /// <summary>
        /// Explicitly stops sending and receiving, and then closes the socket.
        /// </summary>
        public void ShutdownAndClose()
        {
            Console.WriteLine("Shutting down socket...");

            try
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
            }
            catch
            {
            }
        }
    }
}
