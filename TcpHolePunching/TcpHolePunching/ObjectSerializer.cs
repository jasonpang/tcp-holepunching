//
// ObjectSerializer.cs
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
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

#if NET_4
using System.Collections.Concurrent;
#endif

#if !SILVERLIGHT
using System.Runtime.Serialization.Formatters.Binary;
#endif

#if !SAFE
using System.Reflection.Emit;
#endif

namespace TcpHolePunching
{
	internal class ObjectSerializer
	{
		private readonly Type type;

		public ObjectSerializer (Type type)
		{
			if (type == null)
				throw new ArgumentNullException ("type");

			this.type = type;

			GenerateSerialization();
		}

		public void Serialize (IValueWriter writer, object obj)
		{
			if (writer == null)
				throw new ArgumentNullException ("writer");

			serializer (writer, obj, false);
		}

		public T Deserialize<T> (IValueReader reader)
		{
			if (typeof(T) != this.type)
				throw new ArgumentException ("Type does not match serializer type");

			return (T)Deserialize (reader);
		}

		public object Deserialize (IValueReader reader)
		{
			if (reader == null)
				throw new ArgumentNullException ("reader");

			return deserializer (reader, false);
		}

		private Func<IValueReader, bool, object> deserializer;
		private Action<IValueWriter, object, bool> serializer;

		private void GenerateSerialization()
		{
			deserializer = GetDeserializer (this.type, this);
			serializer = GetSerializer();
		}

		private class SerializationPair
		{
			public readonly Func<IValueReader, bool, object> Deserializer;
			public readonly Action<IValueWriter, object, bool> Serializer;

			public SerializationPair (Func<IValueReader, bool, object> des, Action<IValueWriter, object, bool> ser)
			{
				Deserializer = des;
				Serializer = ser;
			}
		}

		private ConstructorInfo ctor;
		private Dictionary<MemberInfo, SerializationPair> members;
		private bool deserializingConstructor;

		private static Func<IValueReader, bool, object> GetDeserializer (Type t, ObjectSerializer oserializer)
		{
			if (t.IsPrimitive)
			{
				if (t == typeof(bool))
					return (r, sh) => r.ReadBool();
				if (t == typeof(byte))
					return (r, sh) => r.ReadByte();
				if (t == typeof(sbyte))
					return (r, sh) => r.ReadSByte();
				if (t == typeof(short))
					return (r, sh) => r.ReadInt16();
				if (t == typeof(ushort))
					return (r, sh) => r.ReadUInt16();
				if (t == typeof(int))
					return (r, sh) => r.ReadInt32 ();
				if (t == typeof(uint))
					return (r, sh) => r.ReadUInt32();
				if (t == typeof(long))
					return (r, sh) => r.ReadInt64();
				if (t == typeof(ulong))
					return (r, sh) => r.ReadUInt64();
				if (t == typeof(float))
					return (r, sh) => r.ReadSingle();
				if (t == typeof(double))
					return (r, sh) => r.ReadDouble();

				throw new ArgumentOutOfRangeException ("type"); // Shouldn't happen.
			}
			else if (t == typeof(decimal))
				return (r, sh) => r.ReadDecimal ();
			else if (t == typeof(DateTime))
				return (r, sh) => r.ReadDate();
			else if (t == typeof(string))
				return (r, sh) => !r.ReadBool() ? null : r.ReadString (Encoding.UTF8);
			else if (t.IsEnum)
			{
				Type btype = Enum.GetUnderlyingType (t);
				return GetDeserializer (btype, oserializer);
			}
			else if (t.IsArray || t == typeof(Array))
			{
				Type etype = t.GetElementType();

				return (r, sh) =>
				{
					if (!r.ReadBool())
						return null;

					Array a = Array.CreateInstance (etype, r.ReadInt32());
					for (int i = 0; i < a.Length; ++i)
						a.SetValue (r.Read (etype), i);

					return a;
				};
			}
			else
			{
				return (r, skipHeader) =>
				{
					if (!skipHeader)
					{
						ushort objectHeader = r.ReadUInt16();
						if ((objectHeader & 1) == 0)
						    return null;
					}

					object value;

					if (typeof(ISerializable).IsAssignableFrom (t))
					{
						oserializer.LoadCtor (t);
						if (!oserializer.deserializingConstructor)
						{
							value = oserializer.ctor.Invoke (null);
							((ISerializable)value).Deserialize (r);
						}
						else
							value = oserializer.ctor.Invoke (new object[] { r });

						return value;
					}

					#if !SILVERLIGHT
					if (t.GetCustomAttributes (true).OfType<SerializableAttribute>().Any ())
						return oserializer.SerializableDeserializer (r);
					#endif

					throw new ArgumentException ("No serializer found for type " + t);

					//oserializer.LoadMembers (t);

					//value = oserializer.ctor.Invoke (null);

					//foreach (var kvp in oserializer.members)
					//{
					//    object mvalue = kvp.Value.Deserializer (r, false);
					//    if (kvp.Key.MemberType == MemberTypes.Field)
					//        ((FieldInfo)kvp.Key).SetValue (value, mvalue);
					//    else if (kvp.Key.MemberType == MemberTypes.Property)
					//        ((PropertyInfo)kvp.Key).SetValue (value, mvalue, null);
					//}

					//return value;
				};
			}
		}

		private Action<IValueWriter, object, bool> GetSerializer()
		{
			return GetSerializerAction (this.type);
		}

		private Action<IValueWriter, object, bool> GetSerializerAction (Type t)
		{
			if (t.IsPrimitive)
			{
				if (t == typeof (bool))
					return (w, v, sh) => w.WriteBool ((bool)v);
				if (t == typeof (byte))
					return (w, v, sh) => w.WriteByte ((byte)v);
				else if (t == typeof (sbyte))
					return (w, v, sh) => w.WriteSByte ((sbyte)v);
				else if (t == typeof (short))
					return (w, v, sh) => w.WriteInt16 ((short)v);
				else if (t == typeof (ushort))
					return (w, v, sh) => w.WriteUInt16 ((ushort)v);
				else if (t == typeof (int))
					return (w, v, sh) => w.WriteInt32 ((int)v);
				else if (t == typeof (uint))
					return (w, v, sh) => w.WriteUInt32 ((uint)v);
				else if (t == typeof (long))
					return (w, v, sh) => w.WriteInt64 ((long)v);
				else if (t == typeof (ulong))
					return (w, v, sh) => w.WriteUInt64 ((ulong)v);
				else if (t == typeof (float))
					return (w, v, sh) => w.WriteSingle ((float)v);
				else if (t == typeof (double))
					return (w, v, sh) => w.WriteDouble ((double)v);				

				throw new ArgumentOutOfRangeException ("type"); // Shouldn't happen.
			}
			else if (t == typeof (decimal))
				return (w, v, sh) => w.WriteDecimal ((decimal)v);
			else if (t == typeof (DateTime))
				return (w, v, sh) => w.WriteDate ((DateTime)(object)v);
			else if (t == typeof (string))
			{
				return (w, v, sh) =>
				{
					w.WriteBool (v != null);
					if (v != null)
						w.WriteString (Encoding.UTF8, (string)v);
				};
			}
			else if (t.IsEnum)
			{
				Type btype = Enum.GetUnderlyingType (t);
				return GetSerializerAction (btype);
			}
			else if (t.IsArray || t == typeof(Array))
			{
				return (w, v, sh) =>
				{
					if (v == null)
					{
						w.WriteBool (false);
						return;
					}

					w.WriteBool (true);

					Array a = (Array)v;
					w.WriteInt32 (a.Length);

					if (t.IsPrimitive)
					{
						for (int i = 0; i < a.Length; ++i)
							w.Write (a.GetValue (i));
					}
					else
					{
						var etype = t.GetElementType();
						for (int i = 0; i < a.Length; ++i)
							w.Write (a.GetValue (i), etype);
					}
				};
			}
			else
			{
				return (w, v, sh) =>
				{
					if (!sh)
					{
						ushort objectHeader = 0;
						//if (v == null)
						//{
						//    w.WriteBool (false);
						//    return;
						//}

						Type actualType = null;
						if (v != null)
						{
						    actualType = v.GetType();

						    objectHeader <<= 1;
						    objectHeader |= 1;
						}

						w.WriteUInt16 (objectHeader);
						if (v == null)
						    return;

						//w.WriteBool (true);
						//w.WriteString (String.Format ("{0}, {1}", actualType.FullName, actualType.Assembly.GetName().Name));

						if (!t.IsAssignableFrom (actualType))
							throw new ArgumentException();

						if (actualType != t)
						{
							GetSerializer (actualType).serializer (w, v, true);
							return;
						}
					}

					var serializable = (v as ISerializable);
					if (serializable != null)
					{
						serializable.Serialize (w);
						return;
					}

					#if !SILVERLIGHT
					if (t != typeof(object) && t.GetCustomAttributes (true).OfType<SerializableAttribute>().Any ())
					{
						SerializableSerializer (w, v);
						return;
					}
					#endif

					throw new ArgumentException ("No serializer found or specified for type " + t, "value");

					//LoadMembers (t);

					//var props = this.members;

					//foreach (var kvp in props)
					//{
					//    if (kvp.Key.MemberType == MemberTypes.Field)
					//        kvp.Value.Serializer (w, ((FieldInfo)kvp.Key).GetValue (v), false);
					//    else if (kvp.Key.MemberType == MemberTypes.Property)
					//        kvp.Value.Serializer (w, ((PropertyInfo)kvp.Key).GetValue (v, null), false);
					//}
				};
			}
		}

		private void LoadMembers (Type t)
		{
			LoadCtor (t);

			if (this.members != null)
				return;

			this.members = t.GetMembers (BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.GetField | BindingFlags.NonPublic)
							.Where (mi =>
							{
								if (mi.MemberType == MemberTypes.Field)
								{
									var fi = (FieldInfo)mi;
									if (typeof(Delegate).IsAssignableFrom (fi.FieldType.BaseType))
									    return false;

									return !fi.IsInitOnly;
								}
								else if (mi.MemberType == MemberTypes.Property)
								{
									var p = (PropertyInfo)mi;
									return (p.GetSetMethod() != null && p.GetIndexParameters().Length == 0);
								}

								return false;
							})
							.ToDictionary (mi => mi, mi =>
							{
								ObjectSerializer os = null;
								if (mi.MemberType == MemberTypes.Field)
									os = GetSerializerInternal (((FieldInfo)mi).FieldType);
								else if (mi.MemberType == MemberTypes.Property)
									os = GetSerializerInternal (((PropertyInfo)mi).PropertyType);

								return new SerializationPair (os.Deserialize, os.Serialize);
							});
		}

		private void LoadCtor (Type t)
		{
			if (this.ctor != null)
				return;

			this.ctor = t.GetConstructor (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, new[] {typeof (IValueReader) }, null)
						?? t.GetConstructor (BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public, null, Type.EmptyTypes, null);

			if (this.ctor == null)
				throw new ArgumentException ("No empty or (ISerializationContext,IValueReader) constructor found for " + t.Name);

			this.deserializingConstructor = this.ctor.GetParameters().Length == 2;
		}

		#if !SILVERLIGHT
		private object SerializableDeserializer (IValueReader reader)
		{
			bool isNull = false;
			if (this.type.IsClass)
				isNull = reader.ReadBool();

			if (isNull)
				return null;

			byte[] data = reader.ReadBytes();
			using (MemoryStream stream = new MemoryStream (data))
				return new BinaryFormatter().Deserialize (stream, null);
		}
		
		private void SerializableSerializer (IValueWriter writer, object value)
		{
			if (this.type.IsClass)
				writer.WriteBool (value == null);

			using (MemoryStream stream = new MemoryStream())
			{
				new BinaryFormatter().Serialize (stream, value);
				writer.WriteBytes (stream.ToArray());
			}
		}
		#endif

		private void Serialize (IValueWriter writer, object value, bool skipHeader)
		{
			this.serializer (writer, value, skipHeader);
		}

		private object Deserialize (IValueReader reader, bool skipHeader)
		{
			return this.deserializer (reader, skipHeader);
		}

		#if NET_4
		private static readonly ConcurrentDictionary<Type, ObjectSerializer> Serializers = new ConcurrentDictionary<Type, ObjectSerializer>();
		#else
		private static readonly Dictionary<Type, ObjectSerializer> Serializers = new Dictionary<Type, ObjectSerializer> ();
		#endif

		internal ObjectSerializer GetSerializerInternal (Type stype)
		{
			if (this.type == stype)
				return this;

			return GetSerializer (stype);
		}

		private static readonly ObjectSerializer baseSerializer = new ObjectSerializer (typeof(object));

		internal static ObjectSerializer GetSerializer (Type type)
		{
			if (type == typeof(object) || type.IsInterface || type.IsAbstract)
				return baseSerializer;

			ObjectSerializer serializer;
			#if NET_4
			serializer = Serializers.GetOrAdd (type, t => new ObjectSerializer (t));
			#else
			bool exists;
			lock (Serializers)
				exists = Serializers.TryGetValue (type, out serializer);

			if (!exists)
			{
				serializer = new ObjectSerializer (type);
				lock (Serializers)
				{
					if (!Serializers.ContainsKey (type))
						Serializers.Add (type, serializer);
				}
			}
			#endif

			return serializer;
		}
	}
}