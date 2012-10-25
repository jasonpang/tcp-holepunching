//
// ISerializer.cs
//
// Author:
//   Eric Maupin <me@ermau.com>
//
// Copyright (c) 2011 Eric Maupin
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

namespace TcpHolePunching
{
	/// <summary>
	/// Contract for a type that serializes another type.
	/// </summary>
	public interface ISerializer
	{
		/// <summary>
		/// Serializes <paramref name="element"/> using <paramref name="writer"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="writer">The writer to use to serialize.</param>
		/// <param name="element">The element to serialize.</param>
		void Serialize (IValueWriter writer, object element);

		/// <summary>
		/// Deserializes an element with <paramref name="reader"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="reader">The reader to use to deserialize.</param>
		/// <returns>The deserialized element.</returns>
		object Deserialize (IValueReader reader);
	}

	/// <summary>
	/// Contract for a type that serializes another type.
	/// </summary>
	/// <typeparam name="T">The type to serialize and deserialize.</typeparam>
	public interface ISerializer<T>
	{
		/// <summary>
		/// Serializes <paramref name="element"/> using <paramref name="writer"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="writer">The writer to use to serialize.</param>
		/// <param name="element">The element to serialize.</param>
		void Serialize (IValueWriter writer, T element);

		/// <summary>
		/// Deserializes an element with <paramref name="reader"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="reader">The reader to use to deserialize.</param>
		/// <returns>The deserialized element.</returns>
		T Deserialize (IValueReader reader);
	}

	public static class Serializer<T>
	{
		public static readonly ISerializer<T> Default = new DefaultSerializer();

		private class DefaultSerializer
			: ISerializer<T>
		{
			public void Serialize (IValueWriter writer, T element)
			{
				Type etype;
				if (element != null)
				{
					etype = element.GetType();
					if (etype.IsValueType && typeof (T) == typeof (object))
						etype = typeof (object);
				}
				else
					etype = typeof (object);

				ObjectSerializer.GetSerializer (etype).Serialize (writer, element);
			}

			public T Deserialize (IValueReader reader)
			{
				return (T)ObjectSerializer.GetSerializer (typeof (T)).Deserialize (reader);
			}
		}
	}

	internal class FixedSerializer
		: ISerializer
	{
		public FixedSerializer (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			this.serializer = ObjectSerializer.GetSerializer (type);
		}

		public void Serialize (IValueWriter writer, object element)
		{
			this.serializer.Serialize (writer, element);
		}

		public object Deserialize (IValueReader reader)
		{
			return this.serializer.Deserialize (reader);
		}

		private readonly ObjectSerializer serializer;
	}
}