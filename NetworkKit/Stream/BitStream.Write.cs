// MIT License
//
// Copyright (c) 2019 Rodrigo Martins 
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all copies or substantial portions
// of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
// LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN
// NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
// SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
// Author:
//    Rodrigo Martins <rodrigo.martins.071090@gmail.com>
//

using System.Text;
using System;


namespace NetworkKit.Stream {
	/// <summary>Bit Stream Writing</summary>
	public partial class BitStream {
		/// <summary>Point of writing</summary>
		internal int WritingPoint;


		/// <summary>Can write</summary>
		/// <param name="bits">Number of bits</param>
		/// <returns>True, one can write</returns>
		public virtual bool CanWrite(int bits)
		{return (Unused >= bits);}


		#pragma warning disable
		/// <summary>Write a boolean</summary>
		/// <param name="value">Boolean</param>
		public virtual void WriteBool(bool value){
			BitConverter bytes = (value ? 1 : 0);

			SetByte(bytes.Endian0, 1);
		}

		/// <summary>Write a character</summary>
		/// <param name="value">Character</param>
		public virtual void WriteChar(Char value){
			BitConverter bytes = value;

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Write a unsigned byte</summary>
		/// <param name="value">Byte</param>
		public virtual void WriteByte(Byte value){
			BitConverter bytes = value;

			SetByte(bytes.Endian0, 8);
		}

		/// <summary>Write a signed byte</summary>
		/// <param name="value">Signed byte</param>
		public virtual void WriteSByte(SByte value){
			BitConverter bytes = value;

			SetByte(bytes.Endian0, 8);
		}

		/// <summary>Writes an signed 16-bit integer</summary>
		/// <param name="value">Signed 16-bit integer</param>
		public virtual void WriteInt16(Int16 value){
			BitConverter bytes = value;

			bytes.Exchange16bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Writes an signed 32-bit integer</summary>
		/// <param name="value">Signed 32-bit integer</param>
		public virtual void WriteInt32(Int32 value){
			BitConverter bytes = value;

			bytes.Exchange32bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Writes an signed 64-bit integer</summary>
		/// <param name="value">Signed 64-bit integer</param>
		public virtual void WriteInt64(Int64 value){
			BitConverter bytes = value;

			bytes.Exchange64bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
			SetByte(bytes.Endian4, 8);
			SetByte(bytes.Endian5, 8);
			SetByte(bytes.Endian6, 8);
			SetByte(bytes.Endian7, 8);
		}

		/// <summary>Writes an unsigned 16-bit integer</summary>
		/// <param name="value">Unsigned 16-bit integer</param>
		public virtual void WriteUInt16(UInt16 value){
			BitConverter bytes = value;

			bytes.Exchange16bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Writes an unsigned 32-bit integer</summary>
		/// <param name="value">Unsigned 32-bit integer</param>
		public virtual void WriteUInt32(UInt32 value){
			BitConverter bytes = value;

			bytes.Exchange32bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Writes an unsigned 64-bit integer</summary>
		/// <param name="value">Unsigned 64-bit integer</param>
		public virtual void WriteUInt64(UInt64 value){
			BitConverter bytes = value;

			bytes.Exchange64bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
			SetByte(bytes.Endian4, 8);
			SetByte(bytes.Endian5, 8);
			SetByte(bytes.Endian6, 8);
			SetByte(bytes.Endian7, 8);
		}


		/// <summary>Writes the half</summary>
		/// <param name="value">Value</param>
		public virtual void WriteHalf(Single value){
			BitConverter bytes = HalfUtils.Pack(value);

			bytes.Exchange16bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}


		/// <summary>Write a single precision floating</summary>
		/// <param name="value">Single Precision Float</param>
		public virtual void WriteSingle(Single value){
			BitConverter bytes = value;

			bytes.Exchange32bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Write a double-precision floating</summary>
		/// <param name="value">Double Precision Float</param>
		public virtual void WriteDouble(Double value){
			BitConverter bytes = value;

			bytes.Exchange64bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
			SetByte(bytes.Endian4, 8);
			SetByte(bytes.Endian5, 8);
			SetByte(bytes.Endian6, 8);
			SetByte(bytes.Endian7, 8);
		}

		/// <summary>
		/// Write a string.
		/// This method is extremely slow, avoid using it.
		/// </summary>
		/// <param name="value">String</param>
		public virtual void WriteString(String value){
			if(string.IsNullOrEmpty(value))
			WriteInt32(0);

			else {
				// Resize the string to the limit
				if(value.Length > int.MaxValue)
				value = value.Substring(0, int.MaxValue);

				WriteBytes(Encoding.UTF8.GetBytes(value));
			}
		}

		/// <summary>
		/// Writes an array to bytes.
		/// This method is extremely slow, avoid using it.
		/// </summary>
		/// <param name="value">Byte array</param>
		public virtual void WriteBytes(byte[] value){
			WriteInt32(value.Length);

			// Vazio
			if(value.Length == 0) return;

			// Resize if necessary
			Growing(value.Length << 3);

			SetBytes(value, 0, value.Length);
		}
		#pragma warning restore
	};
};