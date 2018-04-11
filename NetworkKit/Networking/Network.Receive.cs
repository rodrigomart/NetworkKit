/// DEPENDENCY
using NetworkKit.Containers;


///NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Receive
	/// </summary>
	public partial class Network {
		/// <summary>Received queue</summary>
		private Queue<Package> ReceivedQueue;


		/// <summary>Payload package</summary>
		/// <param name="package">Package</param>
		public bool Payload(out Package package){
			// Package queue
			if(ReceivedQueue == null)
			ReceivedQueue = new Queue<Package>();

			if(ReceivedQueue.Count > 0){
				package = ReceivedQueue.Dequeue();
				return true;
			}

			package = null;
			return false;
		}


		/// <summary>Incoming package</summary>
		/// <param name="package">Package</param>
		internal void Incoming(Package package){
			/// REFUSED package
			if(package.Protocol == Protocol.Refused){
				package.Link.Status = Link.LinkStatus.Unlinked;
				Linktable.Remove(package.Link);
			}


			/// TIMEOUT package
			if(package.Protocol == Protocol.Timeout)
			{Linktable.Remove(package.Link);}


			/// LINKED package
			if(package.Protocol == Protocol.Linked)
			{package.Link.Status = Link.LinkStatus.Linked;}

			/// LINKING package
			if(package.Protocol == Protocol.Linking){
				package.Link.Status = Link.LinkStatus.Waiting;

				/// WAITING package
				var waiting = Package.New();
				waiting.Protocol = Protocol.Waiting;
				package.Link.Send(waiting);

				// Add new link
				Linktable.Add(package.Link);
			}

			/// UNLINKED package
			if(package.Protocol == Protocol.Unlinked){
				package.Link.Status = Link.LinkStatus.Unlinked;

				// Remove link
				Linktable.Remove(package.Link);
			}


			/// WAITING package
			if(package.Protocol == Protocol.Waiting){
				package.Link.Status = Link.LinkStatus.Waiting;

				Package.Recycle(package);
				return;
			}

			// Package queue
			if(ReceivedQueue == null) ReceivedQueue = new Queue<Package>();
			ReceivedQueue.Enqueue(package);
		}
	};
};