using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching.Messages
{
    public class ResponseIntroducerIntroductionMessage : MessageBase
    {
        /// <summary>
        /// This is the internal end point of the other peer.
        /// </summary>
        public IPEndPoint InternalPeerEndPoint { get; set; }
        /// <summary>
        /// This is the external end point of the other peer.
        /// </summary>
        public IPEndPoint ExternalPeerEndPoint { get; set; }

        public ResponseIntroducerIntroductionMessage()
            : base(MessageType.ResponseIntroducerIntroduction)
        {
        }

        public override void WritePayload(IValueWriter writer)
        {
            base.WritePayload(writer);
            writer.WriteBytes(InternalPeerEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(InternalPeerEndPoint.Port);
            writer.WriteBytes(ExternalPeerEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(ExternalPeerEndPoint.Port);
        }

        public override void ReadPayload(IValueReader reader)
        {
            base.ReadPayload(reader);
            var internalEndPointAddress = new IPAddress(reader.ReadBytes());
            InternalPeerEndPoint = new IPEndPoint(internalEndPointAddress, reader.ReadInt32());
            var externalEndPointAddress = new IPAddress(reader.ReadBytes());
            ExternalPeerEndPoint = new IPEndPoint(externalEndPointAddress, reader.ReadInt32());
        }
    }
}
