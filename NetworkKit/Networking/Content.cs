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

using System.Runtime.InteropServices;
using System;


namespace NetworkKit.Networking {
	using Stream;


	/// <summary>Content</summary>
	public sealed class Content : BitStream {
		#pragma warning disable
		/// <summary>ID Maker</summary>
		[StructLayout(LayoutKind.Explicit)]
		private struct IDMaker {
			/// <summary>Identifier</summary>
			[FieldOffset(0)] public UInt32 Identifier;

			/// <summary>Sequence</summary>
			[FieldOffset(0)] public UInt16 Sequence;

			/// <summary>Fragments</summary>
			[FieldOffset(2)] public Byte Fragments;

			/// <summary>Fragment</summary>
			[FieldOffset(2)] public Byte Fragment;
		};
		#pragma warning restore


		// CONSTANTS
		internal const int UNRELIABLE_HEADER = 48; // Bits
		internal const int RELIABLE_HEADER   = 80; // Bits

		internal const byte PING             = 0x01;
		internal const byte PONG             = 0x02;

		internal const byte LIMIT_OF_LINKS   = 0x03;
		internal const byte NOT_ACCESSIBLE   = 0x04;

		internal const byte MATCH            = 0x41;
		internal const byte APPROVAL         = 0x42;
		internal const byte ACCEPTED         = 0x43;
		internal const byte REDIRECT         = 0x44;
		internal const byte DENIED           = 0x45;

		internal const byte UNLINK           = 0x46;
		internal const byte SHUTDOWN         = 0x47;

		internal const byte NONE             = 0x00;
		internal const byte UNNAMED          = 0x10;
		internal const byte FRAGMENT         = 0x20;
		internal const byte RELIABLE         = 0x40;
		internal const byte RESPONSE         = 0x80;

		internal const byte IDENTIFIABLE     = 0xE0;


		#pragma warning disable
		/// <summary>Protocol</summary>
		internal Byte Protocol;

		/// <summary>User flag</summary>
		internal Byte UserFlag;


		/// <summary>Timestamp</summary>
		internal UInt32 Timestamp;

		/// <summary>Sequence</summary>
		internal UInt16 Sequence;


		/// <summary>Fragments</summary>
		internal Byte Fragments;

		/// <summary>Fragment</summary>
		internal Byte Fragment;
		#pragma warning restore


		/// <summary>Is reliable</summary>
		public bool IsReliable {
			get {return ((Protocol & IDENTIFIABLE) != 0);}
		}


		/// <summary>Identifier</summary>
		public uint Identifier {
			get {
				var idMaker       = default(IDMaker);
				idMaker.Sequence  = Sequence;
				idMaker.Fragments = Fragments;
				idMaker.Fragment  = Fragment;

				return idMaker.Identifier;
			}
		}

		/// <summary>Flag</summary>
		public byte Flag {
			set {UserFlag = value;}
			get {return UserFlag;}
		}


		/// <summary>Hashing</summary>
		/// <returns>Hash</returns>
		public override int GetHashCode()
		{return (int)Identifier;}

		/// <summary>Comparison of objects</summary>
		/// <param name="obj">Object for comparison</param>
		/// <returns>True if the same</returns>
		public override bool Equals(object obj){
			if(
				obj != null &&
				obj is Content
			){
				var content = obj as Content;
				return (Identifier == content.Identifier);
			}

			return false;
		}

		/// <summary>To string</summary>
		/// <returns>String</returns>
		public override string ToString(){
			if((Protocol & IDENTIFIABLE) != 0){
				return string.Format(
					"CONTENT {0} flag:{1} time:{2}ms ID:{3}",
					Protocol.ToString("X2"), UserFlag.ToString("X2"), Timestamp, Identifier
				);
			}

			return string.Format(
				"CONTENT {0} flag:{1} time:{2}ms",
				Protocol.ToString("X2"), UserFlag.ToString("X2"), Timestamp
			);
		}


		/// <summary>Clear</summary>
		public override void Clear(){
			base.Clear();

			// Header size
			var header = ((Protocol & IDENTIFIABLE) != 0)? RELIABLE_HEADER : UNRELIABLE_HEADER ;

			// Moves the writing pointer
			WritingPoint = header;

			// Moves the reading point
			ReadingPoint = header;

			// Size not used
			Unused -= header;

			// Size used
			Used += header;

			// Restarts the header
			UserFlag  = 0x00;
			Timestamp = 0x00000000;
			Sequence  = 0x0000;
			Fragments = 0x00;
			Fragment  = 0x00;
		}


		/// <summary>Copy</summary>
		/// <param name="content">Content</param>
		public void Copy(Content content){
			base.Copy(content);

			// Sets the header
			Protocol  = content.Protocol;
			UserFlag  = content.UserFlag;
			Timestamp = content.Timestamp;
			Sequence  = content.Sequence;
			Fragments = content.Fragments;
			Fragment  = content.Fragment;
		}


		/// <summary>Sets the header</summary>
		/// <param name="protocol">Protocol</param>
		internal void SetHeader(byte protocol){
			// Sets the header
			Protocol = protocol;
		}

		/// <summary>Sets the header</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		internal void SetHeader(
			byte protocol,
			byte userflag,
			uint timestamp
		){
			// Sets the header
			Protocol  = protocol;
			UserFlag  = userflag;
			Timestamp = timestamp;
		}

		/// <summary>Sets the header</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		/// <param name="sequence">Sequence</param>
		/// <param name="fragments">Fragments</param>
		/// <param name="fragment">Fragment</param>
		internal void SetHeader(
			byte protocol,
			byte userflag,
			uint timestamp,
			ushort sequence,
			byte fragments,
			byte fragment
		){
			// Sets the header
			Protocol  = protocol;
			UserFlag  = userflag;
			Timestamp = timestamp;
			Sequence  = sequence;
			Fragments = fragments;
			Fragment  = fragment;
		}


		/// <summary>Fragmenter</summary>
		/// <param name="mtu">Mtu</param>
		internal Content[] Fragmenter(uint mtu){
			// Counting and sizing
			// Scaling divides the package into equal sizes
			var header = RELIABLE_HEADER >> 3;
			var count  = (int)Math.Floor((LengthBytes - header) / (double)mtu);
			var size   = (int)Math.Floor((LengthBytes - header) / (double)count);

			var contents = new Content[count];
			for(int frag = 1; frag <= count; frag++){
				// Create fragmented content
				contents[frag - 1] = new Content(
					FRAGMENT, UserFlag, Timestamp,
					Sequence, (byte)count, (byte)frag
				);

				// Copy point
				var point = size * (frag - 1);

				// Copy the data
				contents[frag - 1].SetBytes(
					ByteData,
					point + header,
					size
				);
			}

			return contents;
		}

		/// <summary>Defragmenter</summary>
		/// <param name="content">Content</param>
		internal void Defragmenter(Content content){
			// Check compatibility
			if(
				Sequence  != content.Sequence ||
				Fragments != content.Fragments
			) return;

			// Resize to the appropriate size
			Growing(content.Used - RELIABLE_HEADER);

			// Change fragment count
			Fragment++;

			// Copy point
			var header = RELIABLE_HEADER >> 3;
			var point  = (content.LengthBytes - header) * (content.Fragment - 1);
			var size   = (content.LengthBytes - header);

			// Copy the data
			SetBytes(
				content.ByteData,
				point + header,
				size
			);
		}


		/// <summary>Packing</summary>
		internal void Packing(){
			// Moves the writing pointer
			WritingPoint = 0;

			// WRITE THE HEADER
			WriteByte(Protocol);
			WriteByte(UserFlag);
			WriteUInt32(Timestamp);

			// IDENTIFIABLE HEAD
			if((Protocol & IDENTIFIABLE) != 0){
				WriteUInt16(Sequence);
				WriteByte(Fragments);
				WriteByte(Fragment);
			}

			// Moves the writing pointer to the end
			WritingPoint = Used;
		}

		/// <summary>Unpacking</summary>
		internal void Unpacking(){
			// Moves the reading pointer
			ReadingPoint = 0;

			// READ THE HEAD
			Protocol  = ReadByte();
			UserFlag  = ReadByte();
			Timestamp = ReadUInt32();

			// IDENTIFIABLE HEAD
			if((Protocol & IDENTIFIABLE) != 0){
				Sequence  = ReadUInt16();
				Fragments = ReadByte();
				Fragment  = ReadByte();
			}
		}


		/// <summary>Content</summary>
		private Content() :
			this(NONE)
		{}

		/// <summary>Content</summary>
		/// <param name="protocol">Protocol</param>
		private Content(
			byte protocol
		) :
			this(
				protocol,
				0x00,
				0x00000000,
				0x0000,
				0x00,
				0x00
			)
		{}

		/// <summary>Content</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		private Content(
			byte protocol,
			byte userflag,
			uint timestamp
		) :
			this(
				protocol,
				userflag,
				timestamp,
				0x0000,
				0x00,
				0x00
			)
		{}

		/// <summary>Content</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		/// <param name="sequence">Sequence</param>
		/// <param name="fragments">Fragments</param>
		/// <param name="fragment">Fragment</param>
		private Content(
			byte   protocol,
			byte   userflag,
			uint   timestamp,
			ushort sequence,
			byte   fragments,
			byte   fragment
		){
			// Sets the header
			Protocol  = protocol;
			UserFlag  = userflag;
			Timestamp = timestamp;
			Sequence  = sequence;
			Fragments = fragments;
			Fragment  = fragment;

			// Header size
			var header = ((Protocol & IDENTIFIABLE) != 0) ? RELIABLE_HEADER : UNRELIABLE_HEADER;

			// Moves the writing pointer
			WritingPoint = header;

			// Moves the reading point
			ReadingPoint = header;

			// Size not used
			Unused -= header;

			// Size used
			Used += header;
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// STATIC IMPLEMENTATION
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		/// <summary>Unreliable</summary>
		/// <returns>Content</returns>
		public static Content Unreliable()
		{return new Content(UNNAMED);}

		/// <summary>Reliable</summary>
		/// <returns>Content</returns>
		public static Content Reliable()
		{return new Content(RELIABLE);}


		/// <summary>New</summary>
		/// <param name="protocol">Protocol</param>
		internal static Content New(
			byte protocol
		){
			return new Content(
				protocol,
				0x00,
				0x00000000,
				0x0000,
				0x00,
				0x00
			);
		}

		/// <summary>New</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		internal static Content New(
			byte protocol,
			byte userflag,
			uint timestamp
		){
			return new Content(
				protocol,
				userflag,
				timestamp,
				0x0000,
				0x00,
				0x00
			);
		}

		/// <summary>New</summary>
		/// <param name="protocol">Protocol</param>
		/// <param name="userflag">User flag</param>
		/// <param name="timestamp">Timestamp</param>
		/// <param name="sequence">Sequence</param>
		/// <param name="fragments">Fragments</param>
		/// <param name="fragment">Fragment</param>
		internal static Content New(
			byte protocol,
			byte userflag,
			uint timestamp,
			ushort sequence,
			byte fragments,
			byte fragment
		){
			return new Content(
				protocol,
				userflag,
				timestamp,
				sequence,
				fragments,
				fragment
			);
		}
	};
};