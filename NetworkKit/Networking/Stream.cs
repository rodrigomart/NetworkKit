/// DEPENDENCIES
using System.Runtime.InteropServices;
using System;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Stream
	/// </summary>
	public partial class Stream {
		/// <summary>
		/// Byte converter
		/// </summary>
		[StructLayout(LayoutKind.Explicit)]
		public struct ByteConverter {
			[FieldOffset(0)] public Int16  s16;
			[FieldOffset(0)] public Int32  s32;
			[FieldOffset(0)] public Int64  s64;
			[FieldOffset(0)] public UInt16 u16;
			[FieldOffset(0)] public UInt32 u32;
			[FieldOffset(0)] public UInt64 u64;
			[FieldOffset(0)] public Single f32;
			[FieldOffset(0)] public Double f64;

			[FieldOffset(0)] public Byte x0;
			[FieldOffset(1)] public Byte x1;
			[FieldOffset(2)] public Byte x2;
			[FieldOffset(3)] public Byte x3;
			[FieldOffset(4)] public Byte x4;
			[FieldOffset(5)] public Byte x5;
			[FieldOffset(6)] public Byte x6;
			[FieldOffset(7)] public Byte x7;


			/// <summary>Is little endian</summary>
			/// <returns>bool</returns>
			public static bool IsLittle(){
				ByteConverter bytes = default(ByteConverter);
				bytes.s32 = 1;

				return (bytes.x0 == 1);
			}


			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(Int16 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.s16 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(Int32 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.s32 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(Int64 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.s64 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(UInt16 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.u16 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(UInt32 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.u32 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(UInt64 val){
				ByteConverter bytes = default(ByteConverter);
				bytes.u64 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(Single val){
				ByteConverter bytes = default(ByteConverter);
				bytes.f32 = val; return bytes;
			}

			/// <summary>Implicity convertion</summary>
			public static implicit operator ByteConverter(Double val){
				ByteConverter bytes = default(ByteConverter);
				bytes.f64 = val; return bytes;
			}
		};


		/// <summary>Byte data</summary>
		protected byte[] ByteData;

		/// <summary>Unused</summary>
		protected int Unused;

		/// <summary>Used</summary>
		protected int Used;


		/// <summary>Byte data</summary>
		public virtual byte[] Data {
			get {return ByteData;}
		}

		/// <summary>Size in bits</summary>
		public virtual int Size {
			get {return Used;}
		}


		/// <summary>Clear</summary>
		public virtual void Clear(){
			Used = 0;
			Unused = ByteData.Length << 3;

			// Clear array
			Array.Clear(ByteData, 0, ByteData.Length);

			// Moves the pointers
			WritePoint = 0;
			ReadPoint = 0;
		}

		/// <summary>Copy stream</summary>
		/// <param name="stream">Stream</param>
		public virtual void Copy(Stream stream){
			// Resize if necessary
			Growing(stream.Used);

			// Copy data
			Buffer.BlockCopy(stream.ByteData, 0, ByteData, 0, stream.Used >> 3);
			Unused -= stream.Unused;
			Used = stream.Used;

			// Moves the pointers
			WritePoint = stream.WritePoint;
			ReadPoint = 0;
		}


		/// <summary>
		/// Network stream
		/// </summary>
		internal Stream(){
			// Resize if necessary
			Growing(2048);
		}

		/// <summary>Network stream</summary>
		/// <param name="size">Size in bits</param>
		internal Stream(int size){
			// Resize if necessary
			Growing(size);
		}

		/// <summary>Network stream</summary>
		/// <param name="data">Data</param>
		internal Stream(byte[] data)
		{Set(data, data.Length);}

		/// <summary>Network stream</summary>
		/// <param name="data">Data</param>
		/// <param name="size">Size in bits</param>
		internal Stream(byte[] data, int size)
		{Set(data, size);}


		/// <summary>Set data</summary>
		/// <param name="value">Value</param>
		protected void Set(byte[] value)
		{Set(value, value.Length);}

		/// <summary>Set data</summary>
		/// <param name="value">Value</param>
		/// <param name="size">Size</param>
		protected void Set(byte[] value, int size){
			// Resize if necessary
			Growing(size << 3);

			// Copy data
			Buffer.BlockCopy(value, 0, ByteData, 0, size);
			Unused -= size << 3;
			Used = size << 3;

			// Moves the pointers
			WritePoint = Used;
			ReadPoint = 0;
		}


		/// <summary>Resizes the byte data</summary>
		/// <param name="size">Required bits size</param>
		private void Growing(int size){
			// Does not resize
			if(size < Unused) return;

			// Valid size
			var bits = 0;
			while(bits < (size+Used+Unused))
			bits += 2048; // 512 bytes

			// Not started
			if(ByteData == null){
				ByteData = new byte[bits >> 3];
				Unused = bits;
				Used = 0;

				// Points
				WritePoint = 0;
				ReadPoint = 0;
				return;
			}

			// Temp
			byte[] temp = ByteData;

			// New byte data
			ByteData = new byte[bits >> 3];
			Unused = bits - Used;

			// Copy data
			Buffer.BlockCopy(temp, 0, ByteData, 0, Used >> 3);
			temp = null;
		}
	};
};