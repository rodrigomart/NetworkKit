/// NETWORK IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Protocol
	/// </summary>
	public struct Protocol {
		public const ushort Alive        = 0x0000;
		public const ushort Keepalive    = 0xFFFF;

		public const ushort Timeout      = 0x0F00;

		public const ushort Linking      = 0x8100;
		public const ushort Linked       = 0x9100;
		public const ushort Unlinked     = 0xA100;

		public const ushort Waiting      = 0x8200;

		public const ushort Accepted     = 0x1000;
		public const ushort Refused      = 0x2000;

		public const ushort Fragmented   = 0x4000;
		public const ushort Reliable     = 0x8000;

		public const ushort ReliableACK  = 0x9000;


		public const ushort InputSync    = 0x0A;
		public const ushort SyncData     = 0x10;
		public const ushort SyncSpawn    = 0x11;
		public const ushort SyncUnspawn  = 0x12;
		public const ushort SwapServer   = 0xFF;
	};
};