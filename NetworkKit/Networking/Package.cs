/// DEPENDENCY
using NetworkKit.Containers;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Package
	/// </summary>
	public class Package : Stream {
		#region RECYCLING
		/// <summary>Recycling queue</summary>
		private static readonly Queue<Package> RecyclingQueue;


		/// <summary>
		/// Network Package
		/// </summary>
		static Package(){
			// Starts the recycling queue
			RecyclingQueue = new Containers.Queue<Package>(2048);
		}


		/// <summary>New recycled stream</summary>
		/// <returns>Network stream</returns>
		public static Package New(){
			// New package
			if(RecyclingQueue.Count <= 0)
			return new Package();

			// Recycled
			Package package = RecyclingQueue.Dequeue();
			package.Recycled = false;
			package.Lifetime = 0U;
			package.Protocol = 0x0000;
			package.Sequence = 0x00;
			package.Segment = 0UL;

			return package;
		}

		/// <summary>Recycle stream</summary>
		public static void Recycle(Package package){
			// It's in recycling
			if(!package.Recycled) return;

			// Recycle
			package.Recycled = true;
			RecyclingQueue.Enqueue(package);
		}
		#endregion


		/// <summary>Trail used</summary>
		private int TrailUsed {get; set;}

		/// <summary>Package recycled</summary>
		private bool Recycled {get; set;}


		/// <summary>Sequence</summary>
		internal byte Sequence {get; set;}

		/// <summary>Segment</summary>
		internal ulong Segment {get; set;}


		/// <summary>Network link</summary>
		public Link Link {internal set; get;}

		/// <summary>Protocol</summary>
		public ushort Protocol {get; set;}

		/// <summary>Lifetime</summary>
		public uint Lifetime {get; set;}


		/// <summary>Byte data</summary>
		public override byte[] Data {
			get {return ByteData;}
		}

		/// <summary>Size in bits</summary>
		public override int Size {
			get {
				if((Protocol & 0x8000) != 0) return (Used + 120);
				else return (Used + 48);
			}
		}


		/// <summary>To string</summary>
		/// <returns>String</returns>
		public override string ToString(){
			return string.Format(
				"Package (Protocol:0x{0:X} Lifetime:{1} Sequence:{2} Segment:{3})",
				Protocol, Lifetime, Sequence, Segment
			);
		}


		/// <summary>Copy package</summary>
		/// <param name="package">Package</param>
		public void Copy(Package package){
			// Parameters
			Protocol = package.Protocol;
			Lifetime = package.Lifetime;
			Sequence = package.Sequence;
			Segment = package.Segment;

			// Copy
			this.Copy((Stream)package);
		}


		/// <summary>Packing to bytes</summary>
		/// <returns>bytes</returns>
		public byte[] Packing(){
			// Last points
			int lastWritePoint = WritePoint;
			int lastUnused = Unused;
			int lastUsed = Used;

			// Moves the writing pointer
			WritePoint = Used;

			// Reliable package
			if((Protocol & 0x8000) != 0){
				Protocol |= 0x8000;

				// Reliability data
				Write(Sequence);
				Write(Segment);
			}

			// Common data
			Write(Protocol);
			Write(Lifetime);

			// Suppress trail
			WritePoint = lastWritePoint;
			Unused = lastUnused;
			Used = lastUsed;

			// Byte data
			return ByteData;
		}


		/// <summary>Unpacking from bytes</summary>
		/// <param name="value">Value</param>
		/// <param name="size">Size in bits</param>
		public void Unpacking(byte[] value, int size){
			// Set byte data
			Set(value, size);

			// Moves the reading pointer
			ReadPoint = Used - 48;

			// Common data
			Protocol = ReadUInt16();
			Lifetime = ReadUInt32();

			// Trail used
			TrailUsed += 48;

			// Reliable
			if((Protocol & 0x8000) != 0){
				// Moves the reading pointer
				ReadPoint = Used - 120;

				// Reliability data
				Sequence = ReadByte();
				Segment = ReadUInt64();

				// Trail used
				TrailUsed += 72;
			}

			// Suppress trail
			Unused += TrailUsed;
			Used -= TrailUsed;

			// Moves the pointers
			WritePoint = Used;
			ReadPoint = 0;
		}
	};
};