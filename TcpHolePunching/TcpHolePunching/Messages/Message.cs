using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpHolePunching.Messages
{
    public class Message : MessageBase
    {
        public Message()
            : base(MessageType.Internal)
        {
        }
    }
}
