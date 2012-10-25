using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching.Messages
{
    public class RequestIntroducerIntroductionMessage : MessageBase
    {
        public IPEndPoint InternalOwnEndPoint { get; set; }
        public IPEndPoint ExternalPeerEndPoint { get; set; }

        public RequestIntroducerIntroductionMessage()
            : base(MessageType.RequestIntroducerIntroduction)
        {
        }

        public override void WritePayload(IValueWriter writer)
        {
            base.WritePayload(writer);
            writer.WriteBytes(InternalOwnEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(InternalOwnEndPoint.Port);
            writer.WriteBytes(ExternalPeerEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(ExternalPeerEndPoint.Port);
        }

        public override void ReadPayload(IValueReader reader)
        {
            base.ReadPayload(reader);
            var internalEndPointAddress = new IPAddress(reader.ReadBytes());
            InternalOwnEndPoint = new IPEndPoint(internalEndPointAddress, reader.ReadInt32());
            var externalEndPointAddress = new IPAddress(reader.ReadBytes());
            ExternalPeerEndPoint = new IPEndPoint(externalEndPointAddress, reader.ReadInt32());
        }
    }
}
