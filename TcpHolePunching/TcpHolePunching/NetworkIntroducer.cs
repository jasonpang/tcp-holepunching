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
    public class NetworkIntroducer
    {
        /// <summary>
        /// Gets the underlying raw socket.
        /// </summary>
        public Socket Socket { get; private set; }
        /// <summary>
        /// Gets the list of connected clients.
        /// </summary>
        public List<Client> Clients { get; private set; }
        /// <summary>
        /// Gets the list of connected clients that have registered with RequestIntroductionRegistrationMessage.
        /// </summary>
        public List<Registrant> Registrants { get; private set; } 

        /// <summary>
        /// Occurs after an accepted client has been registered.
        /// </summary>
        public event EventHandler<ConnectionAcceptedEventArgs> OnConnectionAccepted;
        public event EventHandler<MessageSentEventArgs> OnMessageSent;
        public event EventHandler<MessageReceivedEventArgs> OnMessageReceived;

        public NetworkIntroducer()
        {
            Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            Clients = new List<Client>();
            Registrants = new List<Registrant>();
        }

        /// <summary>
        /// Asynchronously accepts a client. Upon accepting the client, the client will automatically be added to
        /// a list of registered clients.
        /// </summary>
        public void Listen(EndPoint on)
        {
            Socket.Bind(on);
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

            // If the registrant was already registered
            if (Clients.FindAll(registrant => registrant.RemoteEndPoint == socket.RemoteEndPoint).Any())
            {
                // Remove the registrant
                Clients.RemoveAll(registrant => registrant.RemoteEndPoint == socket.RemoteEndPoint);
            }

            // Register the registrant
            var newRegistrant = new Client(socket);
            Clients.Add(newRegistrant);

            Task_BeginReceive(newRegistrant);
            
            // Invoke the event
            if (OnConnectionAccepted != null)
                OnConnectionAccepted(this, new ConnectionAcceptedEventArgs() { Socket = socket } );
        }

        public void Send(EndPoint to, MessageBase messageBase)
        {
            var registrant = Clients.Find(r => r.RemoteEndPoint == to);

            // If the registrant exists
            if (registrant != null && registrant.Socket.Connected)
            {
                var data = messageBase.GetBytes();
                var task = Task.Factory.FromAsync<Int32>(registrant.Socket.BeginSend(data, 0, data.Length, SocketFlags.None, null, Socket), registrant.Socket.EndSend);
                task.ContinueWith(nextTask => Task_OnSendCompleted(task.Result, data.Length, registrant.RemoteEndPoint, messageBase.MessageType), TaskContinuationOptions.OnlyOnRanToCompletion);
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

        private void Task_BeginReceive(Client registrant)
        {
            var task = Task.Factory.FromAsync<Int32>(registrant.Socket.BeginReceive(registrant.Buffer, 0, registrant.Buffer.Length, SocketFlags.None, null, null), registrant.Socket.EndReceive);
            task.ContinueWith(nextTask =>
            {
                try
                {
                    Task_OnReceiveCompleted(task.Result, registrant);
                    Task_BeginReceive(registrant); // Receive more data
                }
                catch (Exception ex)
                {
                    var exceptionMessage = (ex.InnerException != null) ? ex.InnerException.Message : ex.Message;
                    Console.WriteLine(exceptionMessage);
                    ShutdownAndClose();
                }
            }, TaskContinuationOptions.OnlyOnRanToCompletion);
        }

        private void Task_OnReceiveCompleted(int numBytesRead, Client registrant)
        {
            // Build back our MessageReader
            var reader = new BufferValueReader(registrant.Buffer);
            var message = new Message();
            message.ReadPayload(reader);
            reader.Position = 0;

            Console.WriteLine(String.Format("Received a {0} byte {1}Message from {2}.", numBytesRead, message.MessageType, registrant.Socket.RemoteEndPoint));

            if (OnMessageReceived != null)
                OnMessageReceived(this, new MessageReceivedEventArgs() { From = (IPEndPoint) registrant.RemoteEndPoint, MessageReader = reader, MessageType = message.MessageType });
        }

        /// <summary>
        /// Explicitly stops sending and receiving, and then closes the socket.
        /// </summary>
        public void ShutdownAndClose()
        {
            Console.WriteLine("Shutting down sockets...");

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
