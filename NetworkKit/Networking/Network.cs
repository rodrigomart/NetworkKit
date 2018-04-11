/// DEPENDENCIES
using NetworkKit.Containers;
using System.Threading;
using System.Net.Sockets;
using System.Net;
using System;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network
	/// </summary>
	public partial class Network {
		/// <summary>Link table</summary>
		private readonly Table<Link> Linktable;

		/// <summary>Network thread</summary>
		private Thread Thread;

		/// <summary>Network socket</summary>
		private Socket Socket;

		/// <summary>Running</summary>
		private bool Running;


		/// <summary>Settings</summary>
		public Settings Settings {
			private set;
			get;
		}


		/// <summary>
		/// Releases Network
		/// </summary>
		~Network(){Stop();}


		/// <summary>
		/// Network
		/// </summary>
		public Network():
			this(new Settings())
		{}

		/// <summary>Network</summary>
		/// <param name="settings">Settings</param>
		public Network(Settings settings){
			this.Linktable = new Table<Link>();
			this.Settings = settings;
		}


		/// <summary>Linking to address</summary>
		/// <param name="address">Address</param>
		/// <param name="approval">Approval</param>
		public void Linking(string address, Package approval = null){
			/// APPROVAL package
			if(approval == null) approval = Package.New();
			approval.Protocol = Protocol.Linking;

			// New network link
			var link = new Link(address, this);
			Linktable.Add(link);

			// Send approval
			link.Send(approval);
		}

		/// <summary>Unlinking</summary>
		/// <param name="link">Link</param>
		public void Unlinking(Link link){
			Linktable.Remove(link);

			/// UNLINKED package
			var unlinked = Package.New();
			unlinked.Protocol = Protocol.Unlinked;
			unlinked.Link = link;

			Sending(unlinked);
			Package.Recycle(unlinked);
		}


		/// <summary>Start</summary>
		public void Start(){Start(0);}

		/// <summary>Start port</summary>
		/// <param name="port">Port</param>
		public void Start(ushort port){
			if(Running) return;

			// Network socket
			this.Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

			try {
				uint IO_RESET = 0x80000000 | 0x18000000 | 12;
				this.Socket.IOControl((int)IO_RESET, new byte[]{Convert.ToByte(false)}, null);
			}
			catch {}

			// Reuse address
			this.Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

			// Receive buffer
			this.Socket.SetSocketOption(
				SocketOptionLevel.Socket,
				SocketOptionName.ReceiveBuffer,
				(int)this.Settings.ReceiveBuffer
			);

			// Sending buffer
			this.Socket.SetSocketOption(
				SocketOptionLevel.Socket,
				SocketOptionName.SendBuffer,
				(int)this.Settings.SendBuffer
			);

			// Socket lock
			this.Socket.Blocking = false;

			// Binds a connection address
			try {this.Socket.Bind(new IPEndPoint(IPAddress.Any, port));}
			catch {throw new Exception("Invalid address for binding");}

			// Network thread
			this.Thread = new Thread(() => {
				// Running
				Running = true;

				// Byte data
				byte[] byteData = new byte[2048];

				// End point address
				EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

				// Network cycle
				while(Running){Thread.Sleep(1000);
					// Reception of date
					while(this.Socket.Poll(0, SelectMode.SelectRead)){
						var size = this.Socket.ReceiveFrom(byteData, ref endPoint);

						// Find in link table
						var link = Linktable.Find(endPoint);

						// No has link
						if(link == null){
							link = new Link(endPoint, this);

							// Overloaded
							if(Linktable.Count >= this.Settings.MaxLinks){
								/// REFUSED package
								var refused = Package.New();
								refused.Protocol = Protocol.Refused;
								refused.Lifetime = Time();
								refused.Link = link;

								Sending(refused);
								Package.Recycle(refused);
								continue;
							}
						}

						// Unpacking
						var package = Package.New();
						package.Unpacking(byteData, size);
						package.Link = link;

						//System.Console.WriteLine("Receive " + package);

						/// ALIVE package
						if(package.Protocol == Protocol.Alive){
							// Rewrite package
							package.Protocol = Protocol.Keepalive;

							Sending(package);
							Package.Recycle(package);
							continue;
						}

						// Incoming
						link.Incoming(package);
					}

					// Timeouts
					foreach(Link link in Linktable)
					{link.Timeouts(Time());}
				}
			});

			// Thread execution
			this.Thread.Name = "Networking";
			this.Thread.IsBackground = true;
			this.Thread.Start();
		}

		/// <summary>Stop</summary>
		public void Stop(){
			if(!Running) return;

			// Stop running
			Running = false;

			// Thread finalize
			this.Thread.Join();

			// Socket finalize
			this.Socket.Close();

			// Clear links
			Linktable.Clear();
		}


		/// <summary>Approval</summary>
		/// <param name="link">Link</param>
		internal void Approval(Link link){
			/// LINKED package
			var linked = Package.New();
			linked.Protocol = Protocol.Linked;

			link.Send(linked);
		}

		/// <summary>Refused</summary>
		/// <param name="link">Link</param>
		internal void Refused(Link link){
			Linktable.Remove(link);

			/// REFUSED package
			var refused = Package.New();
			refused.Protocol = Protocol.Refused;
			refused.Link = link;

			Sending(refused);
			Package.Recycle(refused);
		}
	};
};