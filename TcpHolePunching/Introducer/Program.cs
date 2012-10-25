using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TcpHolePunching;
using TcpHolePunching.Messages;

namespace Introducer
{
    class Program
    {
        private static NetworkIntroducer Introducer { get; set; }

        static void Main(string[] args)
        {
            Console.Title = "Introducer - TCP Hole Punching Proof of Concept";

            Introducer = new NetworkIntroducer();
            Introducer.OnConnectionAccepted += Introducer_OnConnectionAccepted;
            Introducer.OnMessageSent += Introducer_OnMessageSent;
            Introducer.OnMessageReceived += Introducer_OnMessageReceived;

            Introducer.Listen(new IPEndPoint(IPAddress.Any, 1618));
            Console.WriteLine(String.Format("Listening for clients on {0}...", Introducer.Socket.LocalEndPoint));

            Console.ReadLine();
        }

        static void Introducer_OnConnectionAccepted(object sender, ConnectionAcceptedEventArgs e)
        {
        }

        static void Introducer_OnMessageSent(object sender, MessageSentEventArgs e)
        {
        }

        static void Introducer_OnMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            switch (e.MessageType)
            {
                case MessageType.RequestIntroducerRegistration:
                    {
                        var message = new RequestIntroducerRegistrationMessage();
                        message.ReadPayload(e.MessageReader);

                        // A client wants to register
                        // Get his internal endpoint
                        var internalEndPoint = message.InternalClientEndPoint;
                        // Get his external endpoint
                        var externalEndPoint = e.From;

                        Introducer.Registrants.Add(new Registrant()
                                                       {
                                                           InternalEndPoint = internalEndPoint,
                                                           ExternalEndPoint = externalEndPoint
                                                       });

                        Introducer.Send(e.From, new ResponseIntroducerRegistrationMessage()
                                                    {
                                                        RegisteredEndPoint = e.From
                                                    });
                    }
                    break;
                case MessageType.RequestIntroducerIntroduction:
                    {
                        var message = new RequestIntroducerIntroductionMessage();
                        message.ReadPayload(e.MessageReader);

                        // A client, A, wants to be introduced to another peer, B
                        var bExternalEndPoint = message.ExternalPeerEndPoint;

                        // Get this peer's registration
                        var b =
                            Introducer.Registrants.First(
                                registrant => registrant.ExternalEndPoint.Equals(message.ExternalPeerEndPoint));

                        var a = new Registrant()
                                    {InternalEndPoint = message.InternalOwnEndPoint, ExternalEndPoint = e.From};

                        Introducer.Send(a.ExternalEndPoint, new ResponseIntroducerIntroductionMessage()
                                                                {
                                                                    InternalPeerEndPoint = b.InternalEndPoint,
                                                                    ExternalPeerEndPoint = b.ExternalEndPoint,
                                                                });

                        Introducer.Send(b.ExternalEndPoint, new ResponseIntroducerIntroductionMessage()
                                                                {
                                                                    InternalPeerEndPoint = a.InternalEndPoint,
                                                                    ExternalPeerEndPoint = a.ExternalEndPoint,
                                                                });
                    }
                    break;
            }
        }
    }
}
