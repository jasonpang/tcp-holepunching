//
// ISerializable.cs
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

namespace TcpHolePunching
{
	/// <summary>
	/// Contract representing a type that can serialize and deserialize itself.
	/// </summary>
	public interface ISerializable
	{
		/// <summary>
		/// Serializes the instance to the <paramref name="writer"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="writer">The <see cref="IValueWriter"/> to serialize with.</param>
		void Serialize (IValueWriter writer);

		/// <summary>
		/// Deserializes the instance from the <paramref name="reader"/>.
		/// </summary>
		/// <param name="context">The serialization context.</param>
		/// <param name="reader">The <see cref="IValueReader"/> to deserialize with.</param>
		void Deserialize (IValueReader reader);
	}
}