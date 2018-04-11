/// DEPENDENCIES
using UnityEngine;
using System.Text;
using System;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Stream Write
	/// </summary>
	public partial class Stream {
		/// <summary>Write pointer in bits</summary>
		public int WritePoint {
			internal set;
			get;
		}


		/// <summary>Write bool</summary>
		/// <param name="value">Value</param>
		public virtual void Write(bool value){
			if(value) SetByte(1, 1);
			else SetByte(0, 1);
		}

		/// <summary>Write byte</summary>
		/// <param name="value">Value</param>
		public virtual void Write(byte value)
		{SetByte(value, 8);}

		/// <summary>Write int 16 signed</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Int16 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
		}

		/// <summary>Write int 32 signed</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Int32 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
		}

		/// <summary>Write int 64 signed</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Int64 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
			SetByte(bytes.x4, 8);
			SetByte(bytes.x5, 8);
			SetByte(bytes.x6, 8);
			SetByte(bytes.x7, 8);
		}

		/// <summary>Write int 16 unsigned</summary>
		/// <param name="value">Value</param>
		public virtual void Write(UInt16 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
		}

		/// <summary>Write int 32 unsigned</summary>
		/// <param name="value">Value</param>
		public virtual void Write(UInt32 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
		}

		/// <summary>Write int 64 unsigned</summary>
		/// <param name="value">Value</param>
		public virtual void Write(UInt64 value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
			SetByte(bytes.x4, 8);
			SetByte(bytes.x5, 8);
			SetByte(bytes.x6, 8);
			SetByte(bytes.x7, 8);
		}

		/// <summary>Write single</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Single value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
		}

		/// <summary>Write double</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Double value){
			ByteConverter bytes = value;
			SetByte(bytes.x0, 8);
			SetByte(bytes.x1, 8);
			SetByte(bytes.x2, 8);
			SetByte(bytes.x3, 8);
			SetByte(bytes.x4, 8);
			SetByte(bytes.x5, 8);
			SetByte(bytes.x6, 8);
			SetByte(bytes.x7, 8);
		}

		/// <summary>Write a set of bytes</summary>
		/// <param name="value">Value</param>
		public virtual void Write(byte[] value){
			// Array size
			Write(value.Length);

			// Array empty
			if(value.Length == 0) return;

			var point = WritePoint >> 3;
			var bitsUsed = WritePoint % 8;
			var bitsFree = 8 - bitsUsed;

			if(bitsUsed == 0)
			Buffer.BlockCopy(value, 0, ByteData, point, value.Length);

			else {
				for(var i = 0; i < value.Length; ++i){
					ByteData[point] &= (byte)(0xff     >> bitsFree);
					ByteData[point] |= (byte)(value[i] << bitsUsed);

					point += 1;

					ByteData[point] &= (byte)(0xff     << bitsUsed);
					ByteData[point] |= (byte)(value[i] >> bitsFree);
				}
			}

			WritePoint += value.Length << 3;
			Unused -= value.Length << 3;
			Used += value.Length << 3;
		}

		/// <summary>Write string</summary>
		/// <param name="value">Value</param>
		public virtual void Write(string value){
			if(string.IsNullOrEmpty(value))
			Write(new byte[0]);

			else {
				// Resize the string to the limit
				if(value.Length > int.MaxValue)
				value = value.Substring(0, int.MaxValue);
				Write(Encoding.UTF8.GetBytes(value));
			}
		}

		/// <summary>Write color</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Color value){
			var color = 0u;
			color |= ((uint)(value.a * 255)) << 24;
			color |= ((uint)(value.r * 255)) << 16;
			color |= ((uint)(value.g * 255)) <<  8;
			color |= ((uint)(value.b * 255)) <<  0;
			Write(color);
		}

		/// <summary>Write quaternion</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Quaternion value){
			Write(value.x);
			Write(value.y);
			Write(value.z);
			Write(value.w);
		}

		/// <summary>Write vector 2D</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Vector2 value){
			Write(value.x);
			Write(value.y);
		}

		/// <summary>Write vector 3D</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Vector3 value){
			Write(value.x);
			Write(value.y);
			Write(value.z);
		}

		/// <summary>Write vector 4D</summary>
		/// <param name="value">Value</param>
		public virtual void Write(Vector4 value){
			Write(value.x);
			Write(value.y);
			Write(value.z);
			Write(value.w);
		}

		/// <summary>Defines a byte</summary>
		/// <param name="value">Value</param>
		/// <param name="bits">Bits</param>
		private void SetByte(byte value, int bits){
			if(bits <= 0) return;

			// Resize if necessary
			Growing(bits);

			value = (byte)(value & (0xff >> (8 - bits)));

			var point = WritePoint >> 3;
			var bitsUsed = WritePoint & 0x7;
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

			WritePoint += bits;
			Unused -= bits;
			Used += bits;
		}
	};
};