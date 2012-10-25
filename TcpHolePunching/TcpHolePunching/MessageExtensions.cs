using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TcpHolePunching.Messages;

namespace TcpHolePunching
{
    public static class MessageExtensions
    {
        public static byte[] GetBytes (this MessageBase messageBase)
		{
            var writer = new BufferValueWriter(new byte[1024]);
			messageBase.WritePayload(writer);

            var resizedBuffer = new byte[writer.Length];
            Buffer.BlockCopy(writer.Buffer, 0, resizedBuffer, 0, resizedBuffer.Length);

            return resizedBuffer;
		}
    }
}
