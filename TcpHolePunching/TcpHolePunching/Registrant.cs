using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching
{
    public class Registrant
    {
        public IPEndPoint InternalEndPoint { get; set; }
        public IPEndPoint ExternalEndPoint { get; set; }
    }
}
