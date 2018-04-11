/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network Sending
	/// </summary>
	public partial class Network {
		/// <summary>Send to all</summary>
		/// <param name="package">Package</param>
		/// <param name="recycle">Recycle</param>
		public void Send(Package package, bool recycle = true){
			foreach(Link link in Linktable)
			{link.Send(package, false);}

			// Auto recycling
			if(recycle) Package.Recycle(package);
		}

		/// <summary>Send to link</summary>
		/// <param name="package">Package</param>
		/// <param name="link">Link</param>
		/// <param name="recycle">Recycle</param>
		public void Send(Package package, Link link, bool recycle = true)
		{link.Send(package, recycle);}


		/// <summary>Sending package</summary>
		/// <param name="package">Package</param>
		internal void Sending(Package package){
			//System.Console.WriteLine("Sending " + package);

			this.Socket.SendTo(
				package.Packing(),
				package.Size >> 3, 0,
				package.Link.GetEndPoint()
			);
		}
	};
};