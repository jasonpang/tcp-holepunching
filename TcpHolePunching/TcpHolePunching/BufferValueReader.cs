//
// BufferValueReader.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2011-2012 Eric Maupin
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.IO;
using System.Text;
using Buff = System.Buffer;

namespace TcpHolePunching
{
	public class BufferValueReader
		: IValueReader
	{
		private readonly byte[] buffer;
		private readonly int length;

		public BufferValueReader (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			this.buffer = buffer;
			this.length = buffer.Length;
		}

		public BufferValueReader (byte[] buffer, int offset, int length)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			this.buffer = buffer;
			this.position = offset;
			this.length = length;
		}

		/// <summary>
		/// Gets the underlying buffer.
		/// </summary>
		public byte[] Buffer
		{
			get { return this.buffer; }
		}

		/// <summary>
		/// Gets or sets the position of the reader in the buffer.
		/// </summary>
		public int Position
		{
			get { return this.position; }
			set { this.position = value; }
		}

		public bool ReadBool()
		{
			return (this.buffer[this.position++] == 1);
		}

		public byte[] ReadBytes()
		{
			int len = ReadInt32();

			byte[] b = new byte[len];
			Buff.BlockCopy (this.buffer, this.Position, b, 0, len);
			this.Position += len;

			return b;
		}

		public byte[] ReadBytes (int count)
		{
			if (count < 0)
				throw new ArgumentOutOfRangeException ("count", "count must be >= 0");

			byte[] b = new byte[count];
			Buff.BlockCopy (this.buffer, this.position, b, 0, count);
			this.position += count;

			return b;
		}

		public sbyte ReadSByte()
		{
			return (sbyte)this.buffer[this.position++];
		}

		public short ReadInt16()
		{
			short v = BitConverter.ToInt16 (this.buffer, this.position);
			this.position += sizeof (short);
			
			return v;
		}

		public int ReadInt32()
		{
			int v = BitConverter.ToInt32 (this.buffer, this.position);
			this.position += sizeof (int);
			
			return v;
		}

		public long ReadInt64()
		{
			long v = BitConverter.ToInt64 (this.buffer, this.position);
			this.position += sizeof (long);
			
			return v;
		}

		public byte ReadByte()
		{
			return this.buffer[this.position++];
		}

		public ushort ReadUInt16()
		{
			ushort v = BitConverter.ToUInt16 (this.buffer, this.position);
			this.position += sizeof (ushort);
			
			return v;
		}

		public uint ReadUInt32()
		{
			uint v = BitConverter.ToUInt32 (this.buffer, this.position);
			this.position += sizeof (uint);
			
			return v;
		}

		public ulong ReadUInt64()
		{
			ulong v = BitConverter.ToUInt64 (this.buffer, this.position);
			this.position += sizeof (ulong);
			
			return v;
		}

		public decimal ReadDecimal()
		{
			int[] parts = new int[4];
			for (int i = 0; i < parts.Length; ++i)
				parts[i] = ReadInt32 ();

			return new decimal (parts);
		}

		public float ReadSingle()
		{
			float v = BitConverter.ToSingle (this.buffer, this.position);
			this.position += sizeof (float);

			return v;
		}

		public double ReadDouble ()
		{
			double v = BitConverter.ToDouble (this.buffer, this.position);
			this.position += sizeof (double);

			return v;
		}

		public string ReadString (Encoding encoding)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			int len = Read7BitEncodedInt();
			if (len == -1)
				return null;

			string v = encoding.GetString (this.buffer, this.position, len);
			this.Position += len;
			
			return v;
		}

		public void Flush()
		{
		}

		private int position;

		private int Read7BitEncodedInt()
		{
			int count = 0;
			int shift = 0;
			byte b;

			do
			{
				b = ReadByte();

				count |= (b & 127) << shift;
				shift += 7;
			} while ((b & 128) != 0);

			return count;
		}
	}
}