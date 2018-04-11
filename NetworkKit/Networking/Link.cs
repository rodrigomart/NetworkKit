/// DEPENDENCIES
using System.Net.Sockets;
using System.Net;
using System;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Link
	/// </summary>
	public partial class Link {
		/// <summary>Link status</summary>
		public enum LinkStatus {
			Linking,
			Linked,
			Unlinked,
			Waiting
		};


		/// <summary>Synchronization lock</summary>
		private readonly object SyncLock = new object();


		/// <summary>End Point</summary>
		private readonly EndPoint EndPoint;

		/// <summary>Network</summary>
		private readonly Network Network;


		/// <summary>Last sending time</summary>
		private uint LastSendingTime;

		/// <summary>Last received time</summary>
		private uint LastReceivedTime;

		/// <summary>Status</summary>
		public LinkStatus Status
		{internal set; get;}

		/// <summary>Latency</summary>
		public uint Latency
		{internal set; get;}


		/// <summary>Hashing</summary>
		/// <returns>Int</returns>
		public override int GetHashCode()
		{return this.EndPoint.GetHashCode();}

		/// <summary>Object compared</summary>
		/// <returns>Bool</returns>
		public override bool Equals(object obj)
		{return this.EndPoint.Equals(obj);}

		/// <summary>To string</summary>
		/// <returns>String</returns>
		public override string ToString()
		{return this.EndPoint.ToString();}


		/// <summary>Approved</summary>
		public void Approved(){
			Status = LinkStatus.Linked;

			// Approve and send linked
			this.Network.Approval(this);
		}

		/// <summary>Denied</summary>
		public void Refused(){
			Status = LinkStatus.Unlinked;

			// Deny and remove from table
			this.Network.Refused(this);
		}


		/// <summary>Send package</summary>
		/// <param name="package">Package</param>
		/// <param name="recycle">Recycle</param>
		public void Send(Package package, bool recycle = true){
			ReliableSending(package);
			UnreliableSending(package);

			// Auto recycling
			if(recycle) Package.Recycle(package);
		}


		/// <summary>
		/// Network Link
		/// </summary>
		internal Link()
		{Status = LinkStatus.Linking;}

		/// <summary>Network Link</summary>
		/// <param name="endPoint">End point</param>
		/// <param name="network">Network</param>
		internal Link(EndPoint endPoint, Network network) :
			this()
		{
			LastSendingTime = network.Time();
			LastReceivedTime = network.Time();

			this.EndPoint = endPoint;
			this.Network = network;
		}

		/// <summary>Network Link</summary>
		/// <param name="address">Address</param>
		/// <param name="network">Network</param>
		internal Link(string address, Network network) :
			this()
		{
			LastSendingTime = network.Time();
			LastReceivedTime = network.Time();

			this.EndPoint = StringToEndPoint(address);
			this.Network = network;
		}


		/// <summary>Get end point</summary>
		/// <returns>End point</returns>
		internal EndPoint GetEndPoint()
		{return this.EndPoint;}


		/// <summary>Incoming</summary>
		/// <param name="package">Package</param>
		internal void Incoming(Package package){
			// Last receive time
			LastReceivedTime = this.Network.Time();

			/// KEEPALIVE package
			if(package.Protocol == Protocol.Keepalive){
				// Measure latency
				Latency = LastReceivedTime - package.Lifetime;
				Package.Recycle(package);
				return;
			}

			// Incomings
			ReliableIncoming(package);
			UnreliableIncoming(package);
		}

		/// <summary>Timeouts</summary>
		/// <param name="time">Time</param>
		internal void Timeouts(uint time){
			// Downtime
			var downtime = this.Network.Settings.Downtime;
			if(
				Status != LinkStatus.Unlinked &&
				(time - LastReceivedTime) >= downtime
			){
				Status = LinkStatus.Unlinked;

				/// TIMEOUT package
				var timeout = Package.New();
				timeout.Protocol = Protocol.Timeout;
				timeout.Lifetime = time;
				timeout.Link = this;

				this.Network.Incoming(timeout);
				return;
			}

			// Timeouts
			ReliableTimeouts(time);
			UnreliableTimeouts(time);

			// Alive
			if(
				Status == LinkStatus.Linked &&
				(time - LastReceivedTime) >= 1000u &&
				(time - LastSendingTime) >= 333u
			){
				LastSendingTime = time;

				/// ALIVE package
				var alive = Package.New();
				alive.Protocol = Protocol.Alive;
				alive.Lifetime = time;
				alive.Link = this;

				this.Network.Sending(alive);
				Package.Recycle(alive);
			}
		}

		/// <summary>Strings to end point</summary>
		/// <param name="address">Address</param>
		/// <returns>End Point</returns>
		private EndPoint StringToEndPoint(string address){
			// IP address
			IPAddress resolved = IPAddress.Loopback;

			// Split full address
			string[] splitAddress = (address + ":0").Split(
				new char[]{':'}, StringSplitOptions.RemoveEmptyEntries
			);

			// Parse address
			try {
				if(!IPAddress.TryParse(splitAddress[0], out resolved)){
					// IP addresses
					IPAddress[] addrs = Dns.GetHostAddresses(splitAddress[0]);

					// Find valid address
					foreach (IPAddress addr in addrs){
						if(addr.AddressFamily == AddressFamily.InterNetwork)
						{resolved = addr;}
					}
				}

				// Valid connection address
				return new IPEndPoint(
					resolved, ushort.Parse(splitAddress[1])
				);
			}

			catch {
				// Default end point
				return new IPEndPoint(resolved, 0);
			}
		}
	};
};