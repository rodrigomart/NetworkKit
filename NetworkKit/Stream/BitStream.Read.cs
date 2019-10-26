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
	/// <summary>Bit Stream Reading</summary>
	public partial class BitStream {
		/// <summary>Reading point</summary>
		internal int ReadingPoint;


		/// <summary>Can read</summary>
		/// <param name="bits">Number of bits</param>
		/// <returns>True, one can read</returns>
		public virtual bool CanRead(int bits)
		{return ((ReadingPoint + bits) <= Used);}


		#pragma warning disable
		/// <summary>Reads a boolean</summary>
		/// <returns>Boolean</returns>
		public virtual bool ReadBool(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(1);

			return (value.u8 == 1);
		}

		/// <summary>Reads a character</summary>
		/// <returns>Character</returns>
		public virtual Char ReadChar(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			return value.chr;
		}

		/// <summary>Reads a byte</summary>
		/// <returns>Byte</returns>
		public virtual Byte ReadByte(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);

			return value.u8;
		}

		/// <summary>Reads a signed byte</summary>
		/// <returns>Byte signed</returns>
		public virtual SByte ReadSByte(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);

			return value.s8;
		}

		/// <summary>Reads an signed 16-bit integer</summary>
		/// <returns>Signed 16-bit integer</returns>
		public virtual Int16 ReadInt16(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			value.Exchange16bits();

			return value.s16;
		}

		/// <summary>Reads an signed 32-bit integer</summary>
		/// <returns>Signed 32-bit integer</returns>
		public virtual Int32 ReadInt32(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);

			value.Exchange32bits();

			return value.s32;
		}

		/// <summary>Reads an signed 64-bit integer</summary>
		/// <returns>Signed 64-bit integer</returns>
		public virtual Int64 ReadInt64(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);
			value.Endian4 = GetByte(8);
			value.Endian5 = GetByte(8);
			value.Endian6 = GetByte(8);
			value.Endian7 = GetByte(8);

			value.Exchange64bits();

			return value.s64;
		}

		/// <summary>Reads an unsigned 16-bit integer</summary>
		/// <returns>Unsigned 16-bit integer</returns>
		public virtual UInt16 ReadUInt16(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			value.Exchange16bits();

			return value.u16;
		}

		/// <summary>Reads an unsigned 32-bit integer</summary>
		/// <returns>Unsigned 32-bit integer</returns>
		public virtual UInt32 ReadUInt32(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);

			value.Exchange32bits();

			return value.u32;
		}

		/// <summary>Reads an unsigned 64-bit integer</summary>
		/// <returns>Unsigned 64-bit integer</returns>
		public virtual UInt64 ReadUInt64(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);
			value.Endian4 = GetByte(8);
			value.Endian5 = GetByte(8);
			value.Endian6 = GetByte(8);
			value.Endian7 = GetByte(8);

			value.Exchange64bits();

			return value.u64;
		}


		/// <summary>Read a half precision floating</summary>
		/// <returns>Single Precision Float</returns>
		public virtual Single ReadHalf(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			value.Exchange16bits();

			return HalfUtils.Unpack(value.u16);
		}


		/// <summary>Reads a single precision floating</summary>
		/// <returns>Single Precision Float</returns>
		public virtual Single ReadSingle(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);

			value.Exchange32bits();

			return value.f32;
		}

		/// <summary>Reads a double-precision floating</summary>
		/// <returns>Double Precision Float</returns>
		public virtual Double ReadDouble(){
			BitConverter value = default(BitConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);
			value.Endian2 = GetByte(8);
			value.Endian3 = GetByte(8);
			value.Endian4 = GetByte(8);
			value.Endian5 = GetByte(8);
			value.Endian6 = GetByte(8);
			value.Endian7 = GetByte(8);

			value.Exchange64bits();

			return value.f64;
		}

		/// <summary>
		/// Reads a string.
		/// This method is extremely slow, avoid using it.
		/// </summary>
		/// <returns>string</returns>
		public virtual String ReadString(){
			byte[] value = ReadBytes();

			if(value.Length <= 0) return "";
			return Encoding.UTF8.GetString(value);
		}

		/// <summary>Reads an array of bytes</summary>
		/// <returns>Byte array</returns>
		public virtual byte[] ReadBytes(){
			var size = ReadInt32();

			byte[] value = new byte[size];
			GetBytes(value, 0, size);

			return value;
		}
		#pragma warning restore
	};
};