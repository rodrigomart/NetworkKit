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

using System;


namespace NetworkKit.Stream {
	/// <summary>Bit stream</summary>
	public partial class BitStream {
		/// <summary>Byte data</summary>
		internal byte[] ByteData;

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
		/// Bit Stream
		/// </summary>
		public BitStream()
		{Growing(1024 * 8);}

		/// <summary>Bit Stream</summary>
		/// <param name="size">Size in bits</param>
		public BitStream(int size)
		{Growing(size);}

		/// <summary>Bit Stream</summary>
		/// <param name="data">Data</param>
		public BitStream(byte[] data)
		{SetBytes(data, 0, data.Length);}

		/// <summary>Bit Stream</summary>
		/// <param name="data">Data</param>
		/// <param name="size">Size in bytes</param>
		public BitStream(byte[] data, int size)
		{SetBytes(data, 0, size);}

		/// <summary>Bit Stream</summary>
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
		/// <param name="stream">Bit Stream</param>
		public virtual void Copy(BitStream stream){
			// Resize if necessary
			Growing(stream.Used);

			// Copy stream bytes
			Buffer.BlockCopy(stream.ByteData, 0, ByteData, 0, stream.LengthBytes);

			// Defines the pointers
			ReadingPoint = stream.ReadingPoint;
			WritingPoint = stream.WritingPoint;

			// Sets the size
			Unused -= stream.Used;
			Used = stream.Used;
		}


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
				var rest  = bits - (8 - bitsUsed);
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

			var point    = ReadingPoint >> 3;
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

			var point    = WritingPoint >> 3;
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

			var point    = WritingPoint >> 3;
			var bitsUsed = WritingPoint % 8;
			var bitsFree = 8 - bitsUsed;

			if(bitsUsed == 0)
			Buffer.BlockCopy(value, offset, ByteData, point, size);

			else {
				for(var i = 0; i < size; ++i){
					var val = value[offset + i];

					ByteData[point] &= (byte)(0xff >> bitsFree);
					ByteData[point] |= (byte)(val << bitsUsed);

					point += 1;

					ByteData[point] &= (byte)(0xff << bitsUsed);
					ByteData[point] |= (byte)(val >> bitsFree);
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
			Buffer.BlockCopy(temp, 0, ByteData, 0, temp.Length);
			temp = null;
		}
	};
};