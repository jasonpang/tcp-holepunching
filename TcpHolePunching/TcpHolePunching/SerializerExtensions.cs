//
// SerializerExtensions.cs
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
using System.Linq;
using System.Text;
using System.Collections.Generic;

#if NET_4
using System.Collections.Concurrent;
#endif

namespace TcpHolePunching
{
	public static class SerializerExtensions
	{
		public static void WriteUniversalDate (this IValueWriter writer, DateTime date)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			if (date.Kind != DateTimeKind.Utc)
				date = date.ToUniversalTime();

			writer.WriteInt64 (date.Ticks);
		}

		public static DateTime ReadUniversalDate (this IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return new DateTime (reader.ReadInt64(), DateTimeKind.Utc);
		}

		/// <summary>
		/// Writes a date value.
		/// </summary>
		public static void WriteDate (this IValueWriter writer, DateTime date)
		{
			#if !SILVERLIGHT && !WINDOWS_PHONE
			WriteLocalDate (writer, date);
			#else
			writer.WriteInt64 (date.Ticks);
			#endif
		}

		/// <summary>
		/// Reads a date value.
		/// </summary>
		public static DateTime ReadDate (this IValueReader reader)
		{
			#if !SILVERLIGHT && !WINDOWS_PHONE
			return ReadLocalDate (reader).Item2;
			#else
			return new DateTime (reader.ReadInt64(), DateTimeKind.Unspecified);
			#endif
		}

		#if !SILVERLIGHT && !WINDOWS_PHONE
		public static void WriteLocalDate (this IValueWriter writer, DateTime date)
		{
			WriteLocalDate (writer, date, TimeZoneInfo.Local);
		}

		public static void WriteLocalDate (this IValueWriter writer, DateTime date, TimeZoneInfo timeZone)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			writer.WriteString (timeZone.ToSerializedString());
			writer.WriteInt64 (date.ToLocalTime().Ticks);
		}

		public static Tuple<TimeZoneInfo, DateTime> ReadLocalDate (this IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return new Tuple<TimeZoneInfo, DateTime> (TimeZoneInfo.FromSerializedString (reader.ReadString()),
			                                          new DateTime (reader.ReadInt64(), DateTimeKind.Unspecified));
		}
		#endif

		public static void WriteString (this IValueWriter writer, string value)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			writer.WriteString (Encoding.UTF8, value);
		}

		public static string ReadString (this IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return reader.ReadString (Encoding.UTF8);
		}

		public static void WriteEnumerable<T> (this IValueWriter writer, IEnumerable<T> enumerable)
			where T : ISerializable
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (enumerable == null)
				throw new ArgumentNullException ("enumerable");

			T[] elements = enumerable.ToArray();
			writer.WriteInt32 (elements.Length);
			for (int i = 0; i < elements.Length; ++i)
				elements[i].Serialize (writer);
		}

		public static void WriteEnumerable<T> (this IValueWriter writer, ISerializer<T> serializer, IEnumerable<T> enumerable)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (serializer == null)
				throw new ArgumentNullException ("serializer");
			if (enumerable == null)
				throw new ArgumentNullException ("enumerable");

			T[] elements = enumerable.ToArray();
			writer.WriteInt32 (elements.Length);
			for (int i = 0; i < elements.Length; ++i)
				serializer.Serialize (writer, elements[i]);
		}

		public static IEnumerable<T> ReadEnumerable<T> (this IValueReader reader, Func<T> elementFactory)
			where T : ISerializable
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (elementFactory == null)
				throw new ArgumentNullException ("elementFactory");

			int length = reader.ReadInt32();
			T[] elements = new T[length];
			for (int i = 0; i < elements.Length; ++i)
				(elements[i] = elementFactory()).Deserialize (reader);

			return elements;
		}

		public static IEnumerable<T> ReadEnumerable<T> (this IValueReader reader, ISerializer<T> serializer)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			int length = reader.ReadInt32();
			T[] elements = new T[length];
			for (int i = 0; i < elements.Length; ++i)
				elements[i] = serializer.Deserialize (reader);

			return elements;
		}

		public static IEnumerable<T> ReadEnumerable<T> (this IValueReader reader, Func<IValueReader, T> elementFactory)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (elementFactory == null)
				throw new ArgumentNullException ("elementFactory");

			int length = reader.ReadInt32();
			T[] elements = new T[length];
			for (int i = 0; i < elements.Length; ++i)
				elements[i] = elementFactory (reader);

			return elements;
		}
		
		public static void Write (this IValueWriter writer, object element, Type serializeAs)
		{
			Write (writer, element, new FixedSerializer (serializeAs));
		}

		public static void Write (this IValueWriter writer, object element, ISerializer serializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (element == null)
				throw new ArgumentNullException ("element");
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			serializer.Serialize (writer, element);
		}

		public static void Write<T> (this IValueWriter writer, T element)
		{
			Write (writer, element, Serializer<T>.Default);
		}

		public static void Write<T> (this IValueWriter writer, T element, ISerializer<T> serializer)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			serializer.Serialize (writer, element);
		}

		public static T Read<T> (this IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return Read (reader, Serializer<T>.Default);
		}

		public static T Read<T> (this IValueReader reader, ISerializer<T> serializer)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");
			if (serializer == null)
				throw new ArgumentNullException ("serializer");

			return serializer.Deserialize (reader);
		}

		public static object Read (this IValueReader reader)
		{
			return Read<object> (reader);
		}

		public static object Read (this IValueReader reader, Type type)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return ObjectSerializer.GetSerializer (type).Deserialize (reader);
		}
	}
}