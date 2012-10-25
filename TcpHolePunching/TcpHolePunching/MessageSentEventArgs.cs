using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching
{
    public class MessageSentEventArgs : EventArgs
    {
        public EndPoint To { get; set; }
        public int Length { get; set; }
    }
}
