using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace TcpHolePunching
{
    public static class StringExtensions
    {
        public static IPEndPoint Parse(this String str, int defaultPort = -1)
        {
            if (string.IsNullOrEmpty(str)
                || str.Trim().Length == 0)
            {
                throw new ArgumentException("Endpoint descriptor may not be empty.");
            }

            if (defaultPort != -1 &&
                (defaultPort < IPEndPoint.MinPort
                || defaultPort > IPEndPoint.MaxPort))
            {
                throw new ArgumentException(string.Format("Invalid default port '{0}'", defaultPort));
            }

            string[] values = str.Split(new char[] { ':' });
            IPAddress ipaddy;
            int port = -1;

            //check if we have an IPv6 or ports
            if (values.Length <= 2) // ipv4 or hostname
            {
                if (values.Length == 1)
                    //no port is specified, default
                    port = defaultPort;
                else
                    port = AsPort(values[1]);

                //try to use the address as IPv4, otherwise get hostname
                if (!IPAddress.TryParse(values[0], out ipaddy))
                    ipaddy = IpToHost(values[0]);
            }
            else if (values.Length > 2) //ipv6
            {
                //could [a:b:c]:d
                if (values[0].StartsWith("[") && values[values.Length - 2].EndsWith("]"))
                {
                    string ipaddressstring = string.Join(":", values.Take(values.Length - 1).ToArray());
                    ipaddy = IPAddress.Parse(ipaddressstring);
                    port = AsPort(values[values.Length - 1]);
                }
                else //[a:b:c] or a:b:c
                {
                    ipaddy = IPAddress.Parse(str);
                    port = defaultPort;
                }
            }
            else
            {
                throw new FormatException(string.Format("Invalid endpoint ipaddress '{0}'", str));
            }

            if (port == -1)
                throw new ArgumentException(string.Format("No port specified: '{0}'", str));

            return new IPEndPoint(ipaddy, port);
        }

        private static int AsPort(this String str)
        {
            int port;

            if (!int.TryParse(str, out port)
             || port < IPEndPoint.MinPort
             || port > IPEndPoint.MaxPort)
            {
                throw new FormatException(string.Format("Invalid end point port '{0}'", str));
            }

            return port;
        }

        private static IPAddress IpToHost(this String str)
        {
            var hosts = Dns.GetHostAddresses(str);

            if (hosts == null || hosts.Length == 0)
                throw new ArgumentException(string.Format("Host not found: {0}", str));

            return hosts[0];
        }
    }
}
