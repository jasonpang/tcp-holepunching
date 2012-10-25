using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using TcpHolePunching.Messages;

namespace TcpHolePunching
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public IPEndPoint From { get; set; }
        public MessageType MessageType { get; set; }
        public IValueReader MessageReader { get; set; }
    }
}
