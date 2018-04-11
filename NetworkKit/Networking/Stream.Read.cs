/// DEPENDENCIES
using UnityEngine;
using System.Text;
using System;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Stream Read
	/// </summary>
	public partial class Stream {
		/// <summary>Read pointer in bits</summary>
		public int ReadPoint {
			internal set;
			get;
		}


		/// <summary>Read bool</summary>
		/// <returns>Bool</returns>
		public virtual bool ReadBool()
		{return (GetByte(1) == 1);}

		/// <summary>Read byte</summary>
		/// <returns>Byte</returns>
		public virtual byte ReadByte()
		{return GetByte(8);}

		/// <summary>Read int 16 signed</summary>
		/// <returns>Int 16 signed</returns>
		public virtual Int16 ReadInt16(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			return bytes.s16;
		}

		/// <summary>Read int 32 signed</summary>
		/// <returns>Int 32 signed</returns>
		public virtual Int32 ReadInt32(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			return bytes.s32;
		}

		/// <summary>Read int 64 signed</summary>
		/// <returns>Int 64 signed</returns>
		public virtual Int64 ReadInt64(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			bytes.x4 = GetByte(8);
			bytes.x5 = GetByte(8);
			bytes.x6 = GetByte(8);
			bytes.x7 = GetByte(8);
			return bytes.s64;
		}

		/// <summary>Read int 16 unsigned</summary>
		/// <returns>Int 16 unsigned</returns>
		public virtual UInt16 ReadUInt16(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			return bytes.u16;
		}

		/// <summary>Read int 32 unsigned</summary>
		/// <returns>Int 32 unsigned</returns>
		public virtual UInt32 ReadUInt32(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			return bytes.u32;
		}

		/// <summary>Read int 64 unsigned</summary>
		/// <returns>Int 64 unsigned</returns>
		public virtual UInt64 ReadUInt64(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			bytes.x4 = GetByte(8);
			bytes.x5 = GetByte(8);
			bytes.x6 = GetByte(8);
			bytes.x7 = GetByte(8);
			return bytes.u64;
		}

		/// <summary>Read single</summary>
		/// <returns>Single</returns>
		public virtual Single ReadSingle(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			return bytes.f32;
		}

		/// <summary>Read double</summary>
		/// <returns>Double</returns>
		public virtual Double ReadDouble(){
			ByteConverter bytes = default(ByteConverter);
			bytes.x0 = GetByte(8);
			bytes.x1 = GetByte(8);
			bytes.x2 = GetByte(8);
			bytes.x3 = GetByte(8);
			bytes.x4 = GetByte(8);
			bytes.x5 = GetByte(8);
			bytes.x6 = GetByte(8);
			bytes.x7 = GetByte(8);
			return bytes.f64;
		}

		/// <summary>Reads a set of bytes</summary>
		/// <returns>Byte array</returns>
		public virtual byte[] ReadBytes(){
			// Array size
			var size = ReadInt32();
			byte[] value = new byte[size];

			// Array empty
			if(size <= 0) return value;

			var point = ReadPoint >> 3;
			var bitsUsed = ReadPoint % 8;

			if(bitsUsed == 0)
			Buffer.BlockCopy(ByteData, point, value, 0, size);

			else {
				var bitsUnused = 8 - bitsUsed;

				for(var i = 0; i < size; ++i){
					var first = ByteData[point] >> bitsUsed;

					point += 1;

					var second = ByteData[point] & (0xff >> bitsUnused);
					value[i] = (byte)(first | (second << bitsUnused));
				}
			}

			ReadPoint += size << 3;
			return value;
		}

		/// <summary>Read string</summary>
		/// <returns>String</returns>
		public virtual string ReadString(){
			byte[] value = ReadBytes();

			if(value.Length <= 0) return "";
			return Encoding.UTF8.GetString(value);
		}

		/// <summary>Read color</summary>
		/// <returns>Color</returns>
		public virtual Color ReadColor(){
			var color = ReadUInt32();

			return new Color(
				((color >> 16) & 0xFF) / 255f,
				((color >>  8) & 0xFF) / 255f,
				((color >>  0) & 0xFF) / 255f,
				((color >> 24) & 0xFF) / 255f
			);
		}

		/// <summary>Read quaternion</summary>
		/// <returns>Quaternion</returns>
		public virtual Quaternion ReadQuaternion(){
			return new Quaternion(
				ReadSingle(),
				ReadSingle(),
				ReadSingle(),
				ReadSingle()
			);
		}

		/// <summary>Read vector 2D</summary>
		/// <returns>Vector2</returns>
		public virtual Vector2 ReadVector2(){
			return new Vector2(
				ReadSingle(),
				ReadSingle()
			);
		}

		/// <summary>Read vector 3D</summary>
		/// <returns>Vector3</returns>
		public virtual Vector3 ReadVector3(){
			return new Vector3(
				ReadSingle(),
				ReadSingle(),
				ReadSingle()
			);
		}

		/// <summary>Read vector 4D</summary>
		/// <returns>Vector4</returns>
		public virtual Vector4 ReadVector4(){
			return new Vector4(
				ReadSingle(),
				ReadSingle(),
				ReadSingle(),
				ReadSingle()
			);
		}

		/// <summary>Gets a byte</summary>
		/// <param name="bits">Bits</param>
		/// <returns>Byte</returns>
		private byte GetByte(int bits){
			if(bits <= 0) return 0;

			byte value;
			var point = ReadPoint >> 3;
			var bitsUsed = ReadPoint % 8;

			if(bitsUsed == 0 && bits == 8)
			{value = ByteData[point];}

			else {
				var rest = bits - (8 - bitsUsed);
				var first = ByteData[point] >> bitsUsed;

				if(rest < 1)
				{value = (byte)(first & (0xff >> (8 - bits)));}

				else {
					var second = ByteData[point + 1] & (0xff >> (8 - rest));
					value = (byte)(first | (second << (bits - rest)));
				}
			}

			ReadPoint += bits;
			return value;
		}
	};
};