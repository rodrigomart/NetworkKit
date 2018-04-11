/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Link Reliable
	/// </summary>
	public partial class Link {
		/// <summary>Secure sending package</summary>
		private Package[] PackageSending;

		/// <summary>Segment acknowledged</summary>
		private ulong SegmentAcknowlendged = 0UL;

		/// <summary>Segment received</summary>
		private ulong SegmentReceived = 0UL;

		/// <summary>Last reliable time</summary>
		private uint LastReliableTime = 0U;


		/// <summary>Reliable sending</summary>
		/// <param name="package">Package</param>
		private void ReliableSending(Package package){
			// Unreliable package
			if((package.Protocol & Protocol.Reliable) == 0)
			return;
			
			// Last sending
			LastSendingTime = this.Network.Time();

			lock(SyncLock){
				// Started send buffer
				if(PackageSending == null)
				PackageSending = new Package[64];

				// Search for unallocated segment
				for(var bit = 0; bit < 64; bit++){
					if(PackageSending[bit] == null){
						// Copy reliable data
						var copyPackage = Package.New();
						copyPackage.Copy(package);

						// Set data
						copyPackage.Sequence = (byte)bit;
						copyPackage.Segment = SegmentAcknowlendged;

						// Adds the send list
						PackageSending[bit] = copyPackage;

						// Sending
						copyPackage.Link = this;
						copyPackage.Lifetime = this.Network.Time();
						this.Network.Sending(copyPackage);
						break;
					}
				}
			}
		}

		/// <summary>Reliable incoming</summary>
		/// <param name="package">Package</param>
		private void ReliableIncoming(Package package){
			// Unreliable package
			if((package.Protocol & Protocol.Reliable) == 0)
			return;

			lock(SyncLock){
				// Started send buffer
				if(PackageSending == null)
				PackageSending = new Package[64];

				// Ignore if shorter than expected
				if(package.Size < 120){
					Package.Recycle(package);
					return;
				}

				// Only the latest package
				if(package.Lifetime <= LastReliableTime){
					Package.Recycle(package);
					return;
				}

				// Latest package
				LastReliableTime = package.Lifetime;

				// Acknowledgment of delivery
				if(package.Protocol == Protocol.ReliableACK){
					// Measure latency
					Latency = LastReceivedTime - package.Lifetime;

					// Segment confirmed
					for(var bit = 0; bit < 64; bit++){
						if(
							PackageSending[bit] != null &&
							(package.Segment & (1UL << bit)) != 0UL
						){
							Package.Recycle(PackageSending[bit]);
							PackageSending[bit] = null;
						}
					}

					// Acknowledgment
					SegmentAcknowlendged = package.Segment;
					Package.Recycle(package);
					return;
				}

				// Segment
				var segment = (1UL << package.Sequence);

				// Ignore invalid sequence
				if(package.Sequence >= 64){
					Package.Recycle(package);
					return;
				}

				// Duplicate Package
				if((SegmentReceived & segment) != 0UL){
					// Reuse before recycling
					/// RELIACBLE ACK package
					package.Protocol = Protocol.ReliableACK;
					package.Segment = SegmentReceived;
					package.Clear();

					this.Network.Sending(package);
					Package.Recycle(package);
					return;
				}

				// Segment confirmed by sender
				// The segment is only released after delivery confirmation
				SegmentReceived &= ~package.Segment;

				// Mark the received sequence
				SegmentReceived |= segment;

				/// RELIABLE ACK package
				var reliableAck = Package.New();
				reliableAck.Protocol = Protocol.ReliableACK;
				reliableAck.Lifetime = package.Lifetime;
				reliableAck.Segment = SegmentReceived;
				reliableAck.Link = this;

				this.Network.Sending(reliableAck);
				Package.Recycle(reliableAck);
			}

			this.Network.Incoming(package);
		}


		/// <summary>Reliable timeouts</summary>
		/// <param name="time">Time</param>
		private void ReliableTimeouts(uint time){
			lock(SyncLock){
				// Started send buffer
				if(PackageSending == null)
				PackageSending = new Package[64];

				// Search for undelivered
				for(int bit = 0; bit < 64; bit++){
					var package = PackageSending[bit];

					// Unconfirmed package
					if(
						package != null &&
						(time - package.Lifetime) >= 500u
					){
						// Last sending
						LastSendingTime = this.Network.Time();

						// Update package
						package.Segment = SegmentAcknowlendged;
						package.Lifetime = time;

						this.Network.Sending(package);
					}
				}
			}
		}
	};
};