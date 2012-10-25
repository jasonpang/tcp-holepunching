using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching.Messages
{
    public class ResponseIntroducerRegistrationMessage : MessageBase
    {
        public IPEndPoint RegisteredEndPoint { get; set; }

        public ResponseIntroducerRegistrationMessage()
            : base(MessageType.ResponseIntroducerRegistration)
        {
        }

        public override void WritePayload(IValueWriter writer)
        {
            base.WritePayload(writer);
            writer.WriteBytes(RegisteredEndPoint.Address.GetAddressBytes());
            writer.WriteInt32(RegisteredEndPoint.Port);
        }

        public override void ReadPayload(IValueReader reader)
        {
            base.ReadPayload(reader);
            var endPointAddress = new IPAddress(reader.ReadBytes());
            RegisteredEndPoint = new IPEndPoint(endPointAddress, reader.ReadInt32());
        }
    }
}
