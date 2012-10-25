using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching.Messages
{
    public class RequestIntroducerRegistrationMessage : MessageBase
    {
        public IPEndPoint InternalClientEndPoint { get; set; }

        public RequestIntroducerRegistrationMessage()
            : base(MessageType.RequestIntroducerRegistration)
        {
        }

        public override void WritePayload(IValueWriter writer)
        {
            base.WritePayload(writer);
            writer.WriteBytes(InternalClientEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(InternalClientEndPoint.Port);
        }

        public override void ReadPayload(IValueReader reader)
        {
            base.ReadPayload(reader);
            var endPointAddress = new IPAddress(reader.ReadBytes());
            InternalClientEndPoint = new IPEndPoint(endPointAddress, reader.ReadInt32());
        }
    }
}
