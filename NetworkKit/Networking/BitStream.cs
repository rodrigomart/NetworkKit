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


namespace NetworkKit.Networking {
	/// <summary>
	/// Bit stream
	/// </summary>
	public class BitStream {
		/// <summary>Data de byte</summary>
		internal byte[] ByteData;

		/// <summary>Point of writing</summary>
		internal int WritingPoint;

		/// <summary>Reading point</summary>
		internal int ReadingPoint;

		/// <summary>Unused</summary>
		internal int Unused;

		/// <summary>Used</summary>
		internal int Used;


		/// <summary>Data</summary>
		public virtual byte[] Data {
			get {
				var data = new byte[LengthBytes];
				GetBytes(data, 0, LengthBytes);
				return data;
			}
		}

		/// <summary>Size in bytes</summary>
		public virtual int LengthBytes {
			get {return ((Used + 7) >> 3);}
		}

		/// <summary>Size in bits</summary>
		public virtual int LengthBits {
			get {return Used;}
		}


		/// <summary>
		/// Bit stream
		/// </summary>
		public BitStream()
		{Growing(1024 * 8);}

		/// <summary>Bit stream</summary>
		/// <param name="size">Size in bits</param>
		public BitStream(int size)
		{Growing(size);}

		/// <summary>Bit stream</summary>
		/// <param name="data">Data</param>
		public BitStream(byte[] data)
		{SetBytes(data, 0, data.Length);}

		/// <summary>Bit stream</summary>
		/// <param name="data">Data</param>
		/// <param name="size">Size in bytes</param>
		public BitStream(byte[] data, int size)
		{SetBytes(data, 0, size);}

		/// <summary>Bit stream</summary>
		/// <param name="data">Data</param>
		/// <param name="offset">Displacement</param>
		/// <param name="size">Size in bytes</param>
		public BitStream(byte[] data, int offset, int size)
		{SetBytes(data, offset, size);}


		/// <summary>Sets the data</summary>
		/// <param name="data">Data</param>
		public virtual void Set(byte[] data)
		{SetBytes(data, 0, data.Length);}

		/// <summary>Sets the data</summary>
		/// <param name="data">Data</param>
		/// <param name="size">Size in bytes</param>
		public virtual void Set(byte[] data, int size)
		{SetBytes(data, 0, size);}

		/// <summary>Sets the data</summary>
		/// <param name="data">Data</param>
		/// <param name="offset">Displacement</param>
		/// <param name="size">Size in bytes</param>
		public virtual void Set(byte[] data, int offset, int size)
		{SetBytes(data, offset, size);}


		/// <summary>Can read</summary>
		/// <param name="bits">Number of bits</param>
		/// <returns>True, one can read</returns>
		public virtual bool CanRead(int bits)
		{return ((ReadingPoint + bits) <= Used);}

		/// <summary>Can write</summary>
		/// <param name="bits">Number of bits</param>
		/// <returns>True, one can write</returns>
		public virtual bool CanWrite(int bits)
		{return (Unused >= bits);}


		/// <summary>Clear</summary>
		public virtual void Clear(){
			// Defines the pointers
			ReadingPoint = 0;
			WritingPoint = 0;

			// Sets the size
			Unused = ByteData.Length * 8;
			Used = 0;

			// Cleans the data
			Array.Clear(ByteData, 0, ByteData.Length);
		}


		/// <summary>Copy the stream of bytes</summary>
		/// <param name="stream">Byte stream</param>
		public virtual void Copy(BitStream stream){
			// Resize if necessary
			Growing(stream.Used);

			// Copy stream bytes
			Buffer.BlockCopy(stream.ByteData, 0, ByteData, 0, stream.LengthBytes);

			// Defines the pointers
			ReadingPoint = 0;
			WritingPoint = stream.Used;

			// Sets the size
			Unused -= stream.Used;
			Used = stream.Used;
		}


#pragma warning disable
		/// <summary>Reads a boolean</summary>
		/// <returns>Boolean</returns>
		public virtual bool ReadBool(){
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(1);

			return (value.u8 == 1);
		}

		/// <summary>Reads a character</summary>
		/// <returns>Character</returns>
		public virtual Char ReadChar(){
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			return value.chr;
		}

		/// <summary>Reads a byte</summary>
		/// <returns>Byte</returns>
		public virtual Byte ReadByte(){
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(8);

			return value.u8;
		}

		/// <summary>Reads a signed byte</summary>
		/// <returns>Byte signed</returns>
		public virtual SByte ReadSByte(){
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(8);

			return value.s8;
		}

		/// <summary>Reads an signed 16-bit integer</summary>
		/// <returns>Signed 16-bit integer</returns>
		public virtual Int16 ReadInt16(){
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			value.Exchange16bits();

			return value.s16;
		}

		/// <summary>Reads an signed 32-bit integer</summary>
		/// <returns>Signed 32-bit integer</returns>
		public virtual Int32 ReadInt32(){
			ByteConverter value = default(ByteConverter);
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
			ByteConverter value = default(ByteConverter);
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
			ByteConverter value = default(ByteConverter);
			value.Endian0 = GetByte(8);
			value.Endian1 = GetByte(8);

			value.Exchange16bits();

			return value.u16;
		}

		/// <summary>Reads an unsigned 32-bit integer</summary>
		/// <returns>Unsigned 32-bit integer</returns>
		public virtual UInt32 ReadUInt32(){
			ByteConverter value = default(ByteConverter);
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
			ByteConverter value = default(ByteConverter);
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

		/// <summary>Reads a single precision floating</summary>
		/// <returns>Single Precision Float</returns>
		public virtual Single ReadSingle(){
			ByteConverter value = default(ByteConverter);
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
			ByteConverter value = default(ByteConverter);
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

#pragma warning disable
		/// <summary>Write a boolean</summary>
		/// <param name="value">Boolean</param>
		public virtual void Write(bool value){
			ByteConverter bytes = (value ? 1 : 0);

			SetByte(bytes.Endian0, 1);
		}

		/// <summary>Write a character</summary>
		/// <param name="value">Character</param>
		public virtual void Write(Char value){
			ByteConverter bytes = value;

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Write a unsigned byte</summary>
		/// <param name="value">Byte</param>
		public virtual void Write(Byte value){
			ByteConverter bytes = value;

			SetByte(bytes.Endian0, 8);
		}

		/// <summary>Write a signed byte</summary>
		/// <param name="value">Signed byte</param>
		public virtual void Write(SByte value){
			ByteConverter bytes = value;

			SetByte(bytes.Endian0, 8);
		}

		/// <summary>Writes an signed 16-bit integer</summary>
		/// <param name="value">Signed 16-bit integer</param>
		public virtual void Write(Int16 value){
			ByteConverter bytes = value;

			bytes.Exchange16bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Writes an signed 32-bit integer</summary>
		/// <param name="value">Signed 32-bit integer</param>
		public virtual void Write(Int32 value){
			ByteConverter bytes = value;

			bytes.Exchange32bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Writes an signed 64-bit integer</summary>
		/// <param name="value">Signed 64-bit integer</param>
		public virtual void Write(Int64 value){
			ByteConverter bytes = value;

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
		public virtual void Write(UInt16 value){
			ByteConverter bytes = value;

			bytes.Exchange16bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
		}

		/// <summary>Writes an unsigned 32-bit integer</summary>
		/// <param name="value">Unsigned 32-bit integer</param>
		public virtual void Write(UInt32 value){
			ByteConverter bytes = value;

			bytes.Exchange32bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Writes an unsigned 64-bit integer</summary>
		/// <param name="value">Unsigned 64-bit integer</param>
		public virtual void Write(UInt64 value){
			ByteConverter bytes = value;

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

		/// <summary>Write a single precision floating</summary>
		/// <param name="value">Single Precision Float</param>
		public virtual void Write(Single value){
			ByteConverter bytes = value;

			bytes.Exchange64bits();

			SetByte(bytes.Endian0, 8);
			SetByte(bytes.Endian1, 8);
			SetByte(bytes.Endian2, 8);
			SetByte(bytes.Endian3, 8);
		}

		/// <summary>Write a double-precision floating</summary>
		/// <param name="value">Double Precision Float</param>
		public virtual void Write(Double value){
			ByteConverter bytes = value;

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
		public virtual void Write(String value){
			if(string.IsNullOrEmpty(value))
			Write(0);

			else {
				// Resize the string to the limit
				if(value.Length > int.MaxValue)
				value = value.Substring(0, int.MaxValue);

				Write(Encoding.UTF8.GetBytes(value));
			}
		}

		/// <summary>
		/// Writes an array to bytes.
		/// This method is extremely slow, avoid using it.
		/// </summary>
		/// <param name="value">Byte array</param>
		public virtual void Write(byte[] value){
			Write(value.Length);

			// Vazio
			if(value.Length == 0) return;

			// Resize if necessary
			Growing(value.Length << 3);

			SetBytes(value, 0, value.Length);
		}
#pragma warning restore


		/// <summary>Gets a byte</summary>
		/// <param name="bits">Number of bits</param>
		/// <returns>Byte</returns>
		protected byte GetByte(int bits){
			if(bits <= 0) return 0x0;
			byte value = 0x0;

			var point = ReadingPoint >> 3;
			var bitsUsed = ReadingPoint % 8;

			if(bitsUsed == 0 && bits == 8)
			{value = ByteData[point];}

			else {
				var rest = bits - (8 - bitsUsed);
				var first = ByteData[point] >> bitsUsed;

				if(rest < 1){value = (byte)(first & (255 >> (8 - bits)));}

				else {
					var second = ByteData[point + 1] & (0xff >> (8 - rest));
					value = (byte)(first | (second << (bits - rest)));
				}
			}

			ReadingPoint += bits;
			return value;
		}

		/// <summary>Gets an array of bytes</summary>
		/// <param name="value">Byte array</param>
		/// <param name="offset">Displacement in matrix</param>
		/// <param name="size">Size in bytes</param>
		protected void GetBytes(byte[] value, int offset, int size){
			if(size <= 0) return;

			if(offset > value.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));

			if((offset + size) > value.Length)
			throw new ArgumentOutOfRangeException(nameof(size));

			var point = ReadingPoint >> 3;
			var bitsUsed = ReadingPoint % 8;

			if(bitsUsed == 0)
			Buffer.BlockCopy(ByteData, point, value, offset, size);

			else {
				var bitsUnused = 8 - bitsUsed;

				for(var i = 0; i < size; ++i){
					var first = ByteData[point] >> bitsUsed;

					point += 1;

					var second = ByteData[point] & (255 >> bitsUnused);
					value[offset + i] = (byte)(first | (second << bitsUnused));
				}
			}

			ReadingPoint += size << 3;
		}


		/// <summary>Defines a byte</summary>
		/// <param name="value">Byte value</param>
		/// <param name="bits">Number of bits</param>
		protected void SetByte(byte value, int bits){
			if(bits <= 0) return;

			// Resize if necessary
			Growing(bits);

			value = (byte)(value & (0xff >> (8 - bits)));

			var point = WritingPoint >> 3;
			var bitsUsed = WritingPoint & 0x7;
			var bitsFree = 8 - bitsUsed;
			var bitsLeft = bitsFree - bits;

			if(bitsLeft >= 0){
				var mask = (0xff >> bitsFree) | (0xff << (8 - bitsLeft));
				ByteData[point] = (byte)((ByteData[point] & mask) | (value << bitsUsed));
			}

			else {
				ByteData[point] = (byte)((ByteData[point] & (0xff >> bitsFree)) | (value << bitsUsed));

				point += 1;

				ByteData[point] = (byte)((ByteData[point] & (0xff << (bits - bitsFree))) | (value >> bitsFree));
			}

			WritingPoint += bits;

			// Overwrite
			if(WritingPoint >= Used){
				Unused -= bits;
				Used += bits;
			}
		}

		/// <summary>Defines an array of bytes</summary>
		/// <param name="value">Byte array</param>
		/// <param name="offset">Displacement in matrix</param>
		/// <param name="size">Size in bytes</param>
		protected void SetBytes(byte[] value, int offset, int size){
			if(size <= 0) return;

			// Resize if necessary
			Growing(size << 3);

			if(offset > value.Length)
			throw new ArgumentOutOfRangeException(nameof(offset));

			if((offset + size) > value.Length)
			throw new ArgumentOutOfRangeException(nameof(size));

			var point = WritingPoint >> 3;
			var bitsUsed = WritingPoint % 8;
			var bitsFree = 8 - bitsUsed;

			if (bitsUsed == 0)
				Buffer.BlockCopy(value, offset, ByteData, point, size);

			else {
				for(var i = 0; i < size; ++i){
					ByteData[point] &= (byte)(0xff >> bitsFree);
					ByteData[point] |= (byte)(value[offset + i] << bitsUsed);

					point += 1;

					ByteData[point] &= (byte)(0xff << bitsUsed);
					ByteData[point] |= (byte)(value[offset + i] >> bitsFree);
				}
			}

			WritingPoint += size << 3;

			// Overwrite
			if(WritingPoint >= Used){
				Unused -= size << 3;
				Used += size << 3;
			}
		}


		/// <summary>Resizes byte data</summary>
		/// <param name="size">Required bit size</param>
		protected void Growing(int size){
			// Does not resize
			if(size < Unused) return;

			// Valid size
			var bits = 0;
			while(bits < (size + Used + Unused))
			bits += 2048; // 512 bytes

			// Not started
			if(ByteData == null){
				ByteData = new byte[((bits + 7) >> 3)];

				ReadingPoint = 0;
				WritingPoint = 0;
				Unused = bits;
				Used = 0;
				return;
			}

			// Temporary
			byte[] temp = ByteData;

			// New byte data
			ByteData = new byte[((bits + 7) >> 3)];
			Unused = bits - Used;

			// Copying temporary bytes
			Buffer.BlockCopy(temp, 0, ByteData, 0, LengthBytes);
			temp = null;
		}
	};
};