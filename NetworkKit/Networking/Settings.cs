/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Settings
	/// </summary>
	public sealed class Settings {
		/// <summary>Receive buffer</summary>
		public uint ReceiveBuffer = 524288u; // (512 Kilobytes)

		/// <summary>Send buffer</summary>
		public uint SendBuffer = 131072u; // (128 Kilobytes)

		/// <summary>Downtime</summary>
		public uint Downtime = 5000u;

		/// <summary>Maximum num of link</summary>
		public uint MaxLinks = 25u;

		/// <summary>MTU size</summary>
		public uint MtuSize = 1024u;
	};
};