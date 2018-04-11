/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Link Unreliable
	/// </summary>
	public partial class Link {
		/// <summary>Last unreliable time</summary>
		private uint LastUnreliableTime;


		/// <summary>Unreliables sending</summary>
		/// <param name="package">Package</param>
		private void UnreliableSending(Package package){
			// Reliable package
			if((package.Protocol & Protocol.Reliable) != 0)
			return;

			// Sending
			package.Link = this;
			package.Lifetime = this.Network.Time();
			this.Network.Sending(package);
		}

		/// <summary>Unreliable incoming</summary>
		/// <param name="package">Package</param>
		private void UnreliableIncoming(Package package){
			// Reliable package
			if((package.Protocol & Protocol.Reliable) != 0)
			return;

			// Ignore if shorter than expected
			if(package.Size < 48){
				Package.Recycle(package);
				return;
			}

			// Only the latest package
			if(package.Lifetime <= LastUnreliableTime){
				Package.Recycle(package);
				return;
			}

			// Latest package
			LastUnreliableTime = package.Lifetime;
			this.Network.Incoming(package);
		}

		/// <summary>Unreliable timeouts</summary>
		/// <param name="time">Time</param>
		private void UnreliableTimeouts(uint time){}
	};
};