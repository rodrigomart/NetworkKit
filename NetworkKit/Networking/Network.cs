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

using System.Collections.Generic;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;
using System;


namespace NetworkKit.Networking {
	using Collections;
	using Events;


	/// <summary>Network</summary>
	public sealed partial class Network {
		/// <summary>Links indexed by end point</summary>
		readonly AsyncTable<EndPoint, Link> LinksByEndPoint;


		/// <summary>Socket</summary>
		Socket Socket;

		/// <summary>Task</summary>
		Task Task;


		/// <summary>Running</summary>
		bool Running;


		/// <summary>Statistics</summary>
		public Statistics Statistics {
			private set;
			get;
		}

		/// <summary>Settings</summary>
		public Settings Settings {
			private set;
			get;
		}


		/// <summary>Network</summary>
		public Network():
			this(new Settings())
		{}

		/// <summary>Network</summary>
		/// <param name="settings">Settings</param>
		public Network(Settings settings){
			// Links indexed by end point
			LinksByEndPoint = new AsyncTable<EndPoint, Link>();

			// Event queues
			Events = new AsyncQueue<RaiseEvent>();

			// Statistics
			Statistics = new Statistics();

			// Settings
			Settings = settings;


			/*/ Debug events
			OnApproval   += (link, content) => {Debug.Log("[NETWORK] Approval   {0}",     link.Address);};
			OnLinked     += (link)          => {Debug.Log("[NETWORK] Linked     {0}",     link.Address);};
			OnUnlinked   += (link, reason)  => {Debug.Log("[NETWORK] Unlinked   {0} {1}", link.Address, reason);};
			OnRedirect   += (link)          => {Debug.Log("[NETWORK] Redirect   {0}",     link.Address);};
			OnRedirected += (link)          => {Debug.Log("[NETWORK] Redirected {0}",     link.Address);};
			OnFailed     += (link, failure) => {Debug.Log("[NETWORK] Unlinked   {0} {1}", link.Address, failure);};
			*/
		}


		/// <summary>Initializes on a random port</summary>
		/// <returns>Port number selected</returns>
		public ushort Start(){
			Start(0);

			// Gets the current port number
			var ipEndPoint = Socket.LocalEndPoint as IPEndPoint;
			return ((ushort)ipEndPoint.Port);
		}

		/// <summary>Initializes to a specified port</summary>
		/// <param name="port">Port (1 - 65535)</param>
		public void Start(ushort port){
			if(Running) return;

			// Reset statistics
			Statistics.Reset();

			// Prepare socket IPv4 and binding
			Socket = PrepareSocket(AddressFamily.InterNetwork);
			BindingSocket(Socket, port);

			// Create tasks
			try {
				Task = new Task(Process);
				Task.Start();
			}

			// Exception
			catch (Exception e)
			{throw new NetworkException(e.Message);}
		}

		/// <summary>Stop</summary>
		public void Stop(){
			if(!Running) return;

			// Stop running
			Running = false;

			// Wait task
			Task.Wait();

			// Finalize socket IPv4
			if(Socket != null){
				Socket.Close();
				Socket = null;
			}
		}


		/// <summary>Create a link</summary>
		/// <param name="address">Address</param>
		public Link Link(string address)
		{return Link(address, null);}

		/// <summary>Create a link</summary>
		/// <param name="address">Address</param>
		/// <param name="approval">Approval</param>
		public Link Link(string address, Content approval){
			if(string.IsNullOrEmpty(address))
			throw new ArgumentException("An IPv4 network address was expected");

			if(approval != null && approval.Protocol != Content.RELIABLE)
			throw new NetworkException("A reliable content was expected");

			// Content APPROVAL
			if(approval == null) approval = Content.Reliable();
			approval.SetHeader(Content.APPROVAL);

			// Create a link
			var link = new Link(address, approval, this);

			// Register a link
			LinksByEndPoint.Setter(link, link);

			return link;
		}

		/// <summary>Disconnect all</summary>
		public void UnlinkAll(){
			foreach(Link link in LinksByEndPoint)
			link.Unlink();
		}

		/// <summary>Send to everyone</summary>
		/// <param name="content">Content</param>
		public void SendAll(Content content){
			foreach(Link link in LinksByEndPoint)
			if(!link.Closed) link.Send(content);
		}


		/// <summary>Binds a network address to a link</summary>
		/// <param name="endPoint">Network address</param>
		/// <param name="link">Connection link</param>
		internal void Linked(EndPoint endPoint, Link link)
		{LinksByEndPoint.Setter(endPoint, link);}

		/// <summary>Unlink a network address to a link</summary>
		/// <param name="endPoint">Network address</param>
		internal void Unlinked(EndPoint endPoint)
		{LinksByEndPoint.Remove(endPoint);}


		/// <summary>Inlet</summary>
		/// <param name="socket">Socket</param>
		internal void Inlet(Socket socket){
			try {
				// Address
				var address = IPAddress.Any;
				if(socket.AddressFamily == AddressFamily.InterNetworkV6)
				address = IPAddress.IPv6Any;

				// Network end point
				EndPoint endPoint = new IPEndPoint(address, 0);

				// Do not wait more the 3 seconds
				while(socket.Poll(3000, SelectMode.SelectRead)){
					// Content
					var content = Content.Unreliable();

					// Flow received
					var size = socket.ReceiveFrom(
						content.ByteData,
						SocketFlags.None,
						ref endPoint
					);

					// Sets the number of bits
					content.Unused -= size << 3;
					content.Used = size << 3;

					// Unpacking
					content.Unpacking();

					// Find the link
					var link = LinksByEndPoint.Getter(endPoint);

					// There is no link
					if(link == null){
						// It is not an attempt to match
						if(content.Protocol != Content.MATCH)
						return;

						// Not accessible
						if(Settings.MaxLinks == 0u){
							// Send immediately
							Outlet(Content.New(Content.NOT_ACCESSIBLE), endPoint);
							return;
						}

						// Maximum number of links reached
						if(LinksByEndPoint.Count >= Settings.MaxLinks){
							// Send immediately
							Outlet(Content.New(Content.LIMIT_OF_LINKS), endPoint);
							return;
						}

						// Link to this network address
						link = new Link(endPoint, this);

						// Register
						LinksByEndPoint.Setter(endPoint, link);

						// Statistics
						Statistics.IncreaseLink();
					}

					// Pass the content for the link
					link.Inlet(content);
				}
			}

			// Exception
			catch (Exception e)
			{throw new NetworkException(e.Message, e);}
		}

		/// <summary>Outlet</summary>
		/// <param name="content">Content</param>
		/// <param name="endPoint">End point</param>
		internal int Outlet(Content content, EndPoint endPoint){
			if(Socket == null) return 0;

			if(content  == null) throw new NetworkException("\"content\" can not be null");
			if(endPoint == null) throw new NetworkException("\"endPoint\" can not be null");

			// Packing
			content.Packing();

			try {
				var size = Socket.SendTo(
					content.ByteData, content.LengthBytes,
					SocketFlags.None,
					endPoint
				);

				return size;
			}

			catch (Exception e)
			{throw new NetworkException(e.Message, e);}
		}


		/// <summary>Prepares the socket</summary>
		/// <param name="family">Address family</param>
		private Socket PrepareSocket(AddressFamily family){
			// Create a socket
			try {
				// Socket configured to work with UDP
				var socket = new Socket(family, SocketType.Dgram, ProtocolType.Udp);

				try {socket.IOControl(-1744830452, new byte[]{Convert.ToByte(false)}, null);}
				catch {throw new Exception("Failed to set control code for ignoring ICMP port unreachable");}

				// Blocking mode is required for simultaneous IPv6 support
				socket.Blocking = false;

				// Configuration of the buffers
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, (int)Settings.ReceiveBuffer);
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, (int)Settings.SendBuffer);

				// Reuse address
				socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, Settings.ReuseAddress);

				// TTL
				//socket.Ttl = (short)Settings.TTL;

				// Dont fragment
				socket.DontFragment = true;

				// Socket
				return socket;
			}

			// Exception
			catch (Exception e)
			{throw new NetworkException(e.Message, e);}
		}

		/// <summary>Binding the specified port</summary>
		/// <param name="socket">Socket to bind</param>
		/// <param name="port">Port (1 - 65535)</param>
		private void BindingSocket(Socket socket, ushort port){
			// Address
			var address = IPAddress.Any;
			if(socket.AddressFamily == AddressFamily.InterNetworkV6)
			address = IPAddress.IPv6Any;

			// Binds to specified port
			try {socket.Bind(new IPEndPoint(address, port));}
			catch {throw new NetworkException("Invalid port for binding");}
		}

		/// <summary>Process</summary>
		private void Process(){
			// Puts running
			Running = true;

			// Raise event
			Raise(EventType.Started);

			// Link enumerator
			var linkEnum = ((IEnumerable<Link>)LinksByEndPoint).GetEnumerator();

			// Loop
			while(Running){
				// INLET
				Inlet(Socket);

				// Processing timeouts
				if(linkEnum.MoveNext()){
					var link = linkEnum.Current;
					link.Timeout();

					// If it is closed
					if(link.Closed){
						LinksByEndPoint.Remove(link);

						// Statistics
						Statistics.DecreaseLink();
					}

					// Update traffic
					link.Statistics.UpdateTraffic();
				}

				else {
					linkEnum.Reset();

					// Update traffic
					Statistics.UpdateTraffic();
				}

				// Internal call events
				if(!Settings.UseEvents) Event();
			}


			// Disconnect all
			foreach(Link link in LinksByEndPoint)
			link.Shuting();


			// Wait for disconections
			while(!LinksByEndPoint.IsEmpty){
				// Processing timeouts
				if(linkEnum.MoveNext()){
					var link = linkEnum.Current;
					link.Timeout();

					// If it is closed
					if(link.Closed){
						LinksByEndPoint.Remove(link);

						// Statistics
						Statistics.DecreaseLink();
					}

					// Update trafic
					link.Statistics.UpdateTraffic();
				}
				else {
					linkEnum.Reset();

					// Update traffic
					Statistics.UpdateTraffic();
				}

				// Internal call events
				if(!Settings.UseEvents) Event();
			}

			// Clear links
			LinksByEndPoint.Clear();

			// Raise event
			Raise(EventType.Stopped);

			// Internal call events
			if(!Settings.UseEvents) Event();
		}
	};
};