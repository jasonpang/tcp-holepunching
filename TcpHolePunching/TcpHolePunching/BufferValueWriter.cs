//
// BufferValueWriter.cs
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
	#if SAFE
	public class BufferValueWriter
	#else
	public unsafe class BufferValueWriter
	#endif
		: IValueWriter
	{
		public BufferValueWriter (byte[] buffer)
		{
			if (buffer == null)
				throw new ArgumentNullException ("buffer");

			this.buffer = buffer;
		}

		public int Length
		{
			get { return this.position; }
			set
			{
				if (value > this.position)
				{
					int additionalCapacity = value - this.position;
					if (this.position + additionalCapacity >= this.buffer.Length)
					{
						int curLength = this.buffer.Length;
						int newLength = curLength * 2;
						while (newLength <= curLength + additionalCapacity)
							newLength *= 2;

						byte[] newbuffer = new byte[newLength];
						Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
						this.buffer = newbuffer;
					}
				}

				this.position = value;
			}
		}

		public byte[] Buffer
		{
			get { return this.buffer; }
		}

		public void WriteByte (byte value)
		{
			if (this.position + 1 >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + 1)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			this.buffer[this.position++] = value;
		}

		public void WriteSByte (sbyte value)
		{
			if (this.position + 1 >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + 1)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			this.buffer[this.position++] = (byte)value;
		}

		public bool WriteBool (bool value)
		{
			if (this.position + 1 >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + 1)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			this.buffer[this.position++] = (byte)((value) ? 1 : 0);

			return value;
		}

		public void WriteBytes (byte[] value)
		{
			if (value == null)
				throw new ArgumentNullException ("value");

			int additionalCapacity = sizeof(int) + value.Length;
			if (this.position + additionalCapacity >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + additionalCapacity)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			Buff.BlockCopy (BitConverter.GetBytes (value.Length), 0, this.buffer, this.position, sizeof(int));
			this.position += sizeof (int);
			Buff.BlockCopy (value, 0, this.buffer, this.position, value.Length);
			this.position += value.Length;
		}

		public void WriteBytes (byte[] value, int offset, int length)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (offset < 0 || offset >= value.Length)
				throw new ArgumentOutOfRangeException ("offset", "offset can not negative or >=data.Length");
			if (length < 0 || offset + length > value.Length)
				throw new ArgumentOutOfRangeException ("length", "length can not be negative or combined with offset longer than the array");

			int additionalCapacity = sizeof(int) + length;
			if (this.position + additionalCapacity >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + additionalCapacity)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			Buff.BlockCopy (BitConverter.GetBytes (length), 0, this.buffer, this.position, sizeof (int));
			this.position += sizeof (int);
			Buff.BlockCopy (value, offset, this.buffer, this.position, length);
			this.position += length;
		}

		public void InsertBytes (int offset, byte[] value, int valueOffset, int length)
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			if (valueOffset < 0 || valueOffset >= value.Length)
				throw new ArgumentOutOfRangeException ("offset", "offset can not negative or >=data.Length");
			if (length < 0 || valueOffset + length > value.Length)
				throw new ArgumentOutOfRangeException ("length", "length can not be negative or combined with offset longer than the array");

			if (this.position + length >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + length)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			if (offset != this.position)
				Buff.BlockCopy (this.buffer, offset, this.buffer, offset + length, this.position - offset);

			Buff.BlockCopy (value, valueOffset, this.buffer, offset, length);
			this.position += length;
		}

		public void WriteInt16 (short value)
		{
			if (this.position + sizeof (short) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (short))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof(short));
			#else
			fixed (byte* ub = this.buffer)
				*((short*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (short);
		}

		public void WriteInt32 (int value)
		{
			if (this.position + sizeof (int) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (int))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (int));
			#else
			fixed (byte* ub = this.buffer)
				*((int*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (int);
		}

		public void WriteInt64 (long value)
		{
			if (this.position + sizeof (long) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (long))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (long));
			#else
			fixed (byte* ub = this.buffer)
				*((long*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (long);
		}

		public void WriteUInt16 (ushort value)
		{
			if (this.position + sizeof (ushort) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (ushort))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (ushort));
			#else
			fixed (byte* ub = this.buffer)
				*((ushort*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (ushort);
		}

		public void WriteUInt32 (uint value)
		{
			if (this.position + sizeof (uint) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (uint))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (uint));
			#else
			fixed (byte* ub = this.buffer)
				*((uint*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (uint);
		}

		public void WriteUInt64 (ulong value)
		{
			if (this.position + sizeof(ulong) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof(ulong))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (ulong));
			#else
			fixed (byte* ub = this.buffer)
				*((ulong*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (ulong);
		}

		public void WriteDecimal (decimal value)
		{
			int[] parts = Decimal.GetBits (value);
			for (int i = 0; i < parts.Length; ++i)
				WriteInt32 (parts[i]);
		}

		public void WriteSingle (float value)
		{
			if (this.position + sizeof(float) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof(float))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (float));
			#else
			fixed (byte* ub = this.buffer)
				*((float*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (float);
		}

		public void WriteDouble (double value)
		{
			if (this.position + sizeof (double) >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + sizeof (double))
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			#if SAFE
			Buff.BlockCopy (BitConverter.GetBytes (value), 0, this.buffer, this.position, sizeof (double));
			#else
			fixed (byte* ub = this.buffer)
				*((double*) (ub + this.position)) = value;
			#endif

			this.position += sizeof (double);
		}

		public void WriteString (Encoding encoding, string value)
		{
			if (encoding == null)
				throw new ArgumentNullException ("encoding");

			if (value == null)
			{
				Write7BitEncodedInt (-1);
				return;
			}

			byte[] data = encoding.GetBytes (value);
			int additionalCapacity = sizeof(int) + data.Length;
			if (this.position + additionalCapacity >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + additionalCapacity)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			Write7BitEncodedInt (data.Length);
			Buff.BlockCopy (data, 0, this.buffer, this.position, data.Length);
			this.position += data.Length;
		}

		public void Flush()
		{
			this.position = 0;
		}

		public void Pad (int count)
		{
			if (this.position + count >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + count)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, this.Length);
				this.buffer = newbuffer;
			}

			this.position += count;
		}

		public byte[] ToArray()
		{
			byte[] value = new byte[Length];
			Buff.BlockCopy (this.buffer, 0, value, 0, Length);

			return value;
		}

		private byte[] buffer;
		private int position;

		private void Write7BitEncodedInt (int value)
		{
			uint v = (uint) value;
			while (v >= 128)
			{
				WriteByte ((byte) (v | 128));
				v >>= 7;
			}

			WriteByte ((byte) v);
		}

		private void EnsureAdditionalCapacity (int additionalCapacity)
		{
			if (this.position + additionalCapacity >= this.buffer.Length)
			{
				int curLength = this.buffer.Length;
				int newLength = curLength * 2;
				while (newLength <= curLength + additionalCapacity)
					newLength *= 2;

				byte[] newbuffer = new byte[newLength];
				Buff.BlockCopy (this.buffer, 0, newbuffer, 0, Length);
				this.buffer = newbuffer;
			}
		}
	}
}