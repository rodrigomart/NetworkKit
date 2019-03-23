// MIT License
//
// Copyright (c) 2018 Rodrigo Martins 
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

using System.Threading;
using System.Diagnostics;
using System.Net.Sockets;
using System.Net;
using System;


namespace NetworkKit.Networking {
	using Collections;


	/// <summary>
	/// Network core
	/// </summary>
	public class Network {
		/// <summary>Table of links</summary>
		private readonly AsyncTable<EndPoint, Link> _Linktable;

		/// <summary>Event queue</summary>
		private readonly AsyncQueue<RaiseEvent> _Events;

		/// <summary>Stopwatch</summary>
		private readonly Stopwatch _Stopwatch;

		/// <summary>Settings</summary>
		private readonly Settings _Settings;


		/// <summary>Thread</summary>
		private Thread _Thread;

		/// <summary>Socket</summary>
		private Socket _Socket;


		/// <summary>Execution</summary>
		private bool Running;


		/// <summary>Occurs when it fails</summary>
		public event FailureHandler OnFailed;

		/// <summary>Occurs when unlinked</summary>
		public event ReasonHandler OnUnlinked;


		/// <summary>Occurs when you have payload</summary>
		public event PayloadHandler OnPayload;


		/// <summary>Occurs when it is approved</summary>
		public event PayloadHandler OnApproval;


		/// <summary>Occurs when redirected</summary>
		public event LinkHandler OnRedirected;

		/// <summary>Occurs when in redirection</summary>
		public event LinkHandler OnRedirect;

		/// <summary>Occurs when linked</summary>
		public event LinkHandler OnLinked;


		/// <summary>Occurs when started</summary>
		public event StateHandler OnStarted;

		/// <summary>Occurs when stopped</summary>
		public event StateHandler OnStopped;


		/// <summary>Settings</summary>
		public Settings Settings {
			get {return _Settings;}
		}


		/// <summary>
		/// Releases for garbage collection
		/// </summary>
		~Network(){Stop();}


		/// <summary>
		/// Network core
		/// </summary>
		public Network() :
			this(new Settings())
		{}


		/// <summary>Network core</summary>
		/// <param name="settings">Settings</param>
		public Network(Settings settings){
			// Table of links
			_Linktable = new AsyncTable<EndPoint, Link>();

			// Event queue
			_Events = new AsyncQueue<RaiseEvent>();

			// Stopwatch
			_Stopwatch = new Stopwatch();

			// Settings
			_Settings = settings;
		}


		/// <summary>Initializes on a random port</summary>
		/// <returns>Port number selected</returns>
		public ushort Start(){
			Start(0);

			// Gets the current port number
			var ipEndPoint = _Socket.LocalEndPoint as IPEndPoint;
			return ((ushort)ipEndPoint.Port);
		}

		/// <summary>Initializes to a specified port</summary>
		/// <param name="port">Port (1 - 65535)</param>
		public void Start(ushort port){
			// If you are running
			if(Running) return;

			// Create a socket and bind
			try {
				// Socket configured to work with UDP
				_Socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

				try {_Socket.IOControl(-1744830452, new byte[]{Convert.ToByte(false)}, null);}
				catch {throw new Exception("Failed to set control code for ignoring ICMP port unreachable");}

				// Configuration of the buffers
				_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveBuffer, _Settings.ReceiveBuffer);
				_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.SendBuffer, _Settings.SendBuffer);

				// Reuse address
				_Socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);

				// Disables the lock
				_Socket.Blocking = false;

				// Binds to specified port
				try {_Socket.Bind(new IPEndPoint(IPAddress.Any, port));}
				catch {throw new Exception("Invalid port for binding");}
			}
			catch (Exception e)
			{throw new NetworkException(e.Message);}

			// Create a thread and start
			try {
				// Processing thread
				_Thread = new Thread(Process);
				_Thread.IsBackground = true;
				_Thread.Start();
			}
			catch (Exception e)
			{throw new NetworkException(e.Message);}

			// Start the stopwatch
			_Stopwatch.Start();

			// Raise an event
			Raise(EventType.Started);
		}

		/// <summary>Stop</summary>
		public void Stop(){
			// If it is not running
			if(!Running) return;

			// End execution
			Running = false;

			// Finalize thread
			_Thread.Join();

			// Finalize socket
			_Socket.Close();

			// Stop the stopwatch
			_Stopwatch.Stop();

			// Clear events
			_Events.Clear();
			
			// Raise an event
			Raise(EventType.Stopped);
			
			// Shoot the events
			Step();
		}


		/// <summary>Create a connection link</summary>
		/// <param name="address">Connection address</param>
		public void Link(string address)
		{Link(address, null);}

		/// <summary>Create a connection link</summary>
		/// <param name="address">Connection address</param>
		/// <param name="approval">Approval payload</param>
		public void Link(string address, Payload approval){
			if(string.IsNullOrEmpty(address))
			throw new ArgumentException("An IPv4 network address was expected");

			if(approval != null && approval.Protocol != Payload.RELIABLE)
			throw new NetworkException("A reliable payload was expected");

			// Approval payload
			if(approval == null) approval = Payload.Reliable();
			approval.SetHeader(Payload.APPROVAL);


			// Network address
			EndPoint endPoint = GetEndPoint(address);

			// Create a link
			var link = new Link(this, endPoint);
			link.Send(approval);

			// Create a link
			Linked(endPoint, link);
		}

		/// <summary>Disconnect all</summary>
		public void UnlinkAll(){
			foreach(Link link in _Linktable)
			link.Unlink();

			_Linktable.Clear();
		}

		/// <summary>Send to everyone</summary>
		/// <param name="payload">Payload</param>
		public void SendAll(Payload payload){
			foreach(Link link in _Linktable){
				if(link.Status == LinkStatus.Linked){
					// Copy to send
					var clone = Payload.New();
					clone.Copy(payload);

					link.Send(clone);
				}
			}

			// Auto recycling
			payload.Recycle();
		}


		/// <summary>Process events</summary>
		public void Step(){
			RaiseEvent raiseEvent;
			while(_Events.TryDequeue(out raiseEvent)){
				// Raise start event
				if(raiseEvent.EventType == EventType.Started)
				OnStarted?.Invoke();

				// Raise stop event
				if(raiseEvent.EventType == EventType.Stopped)
				OnStopped?.Invoke();

				// Raise link event
				if(raiseEvent.EventType == EventType.Linked)
				OnLinked?.Invoke(raiseEvent.Link);

				// Raise redirection event
				if(raiseEvent.EventType == EventType.Redirect)
				OnRedirect?.Invoke(raiseEvent.Link);

				// Raise post redirection event
				if(raiseEvent.EventType == EventType.Redirected)
				OnRedirected?.Invoke(raiseEvent.Link);

				// Raise approval event
				if(raiseEvent.EventType == EventType.Approval)
				OnApproval?.Invoke(raiseEvent.Link, raiseEvent.Payload);

				// Raise payload event
				if(raiseEvent.EventType == EventType.Payload)
				OnPayload?.Invoke(raiseEvent.Link, raiseEvent.Payload);

				// Raise unlink event
				if(raiseEvent.EventType == EventType.Unlinked)
				OnUnlinked?.Invoke(raiseEvent.Link, raiseEvent.Reason);

				// Raise failure event
				if(raiseEvent.EventType == EventType.Failed)
				OnFailed?.Invoke(raiseEvent.Link, raiseEvent.Failure);
			}
		}

		/// <summary>
		/// Network time.
		/// Time is given in milliseconds counting from Start.
		/// </summary>
		[Obsolete("This method will be removed shortly")]
		public uint Time()
		{return (uint)_Stopwatch.ElapsedMilliseconds;}


		/// <summary>Binds a network address to a link</summary>
		/// <param name="endPoint">Network address</param>
		/// <param name="link">Connection link</param>
		internal void Linked(EndPoint endPoint, Link link)
		{_Linktable.Add(endPoint, link);}

		/// <summary>Unlink a network address to a link</summary>
		/// <param name="endPoint">Network address</param>
		internal void Unlinked(EndPoint endPoint)
		{_Linktable.Remove(endPoint);}


		/// <summary>Inlet</summary>
		[Obsolete("This method will be removed shortly")]
		internal void Inlet(){
			try {
				// Network address
				EndPoint endPoint = new IPEndPoint(IPAddress.Any, 0);

				// Do not wait more than 10 milliseconds
				if(_Socket.Poll(10000, SelectMode.SelectRead)){
					// Payload
					var payload = Payload.New();

					// Flow received
					var size = _Socket.ReceiveFrom(
						payload.ByteData,
						SocketFlags.None,
						ref endPoint
					) << 3;

					// Sets the number of bits
					payload.Unused -= size;
					payload.Used += size;

					// Unpack
					payload.Unpacking();


					// Find the linked link
					var link = _Linktable.Find(endPoint);

					// There is no link
					if(link == null){
						// It is not an attempt to match
						if(payload.Protocol != Payload.MATCH){
							payload.Recycle();
							return;
						}

						// Not accessible
						if(_Settings.MaxLinks == 0u){
							Outlet(endPoint, Payload.Create(Payload.NOT_ACCESSIBLE));
							return;
						}

						// Maximum number of links reached
						if(_Linktable.Count >= _Settings.MaxLinks){
							Outlet(endPoint, Payload.Create(Payload.LIMIT_OF_LINKS));
							return;
						}

						// Link to this network address
						link = new Link(this, endPoint);

						// Binding a link
						Linked(endPoint, link);
					}

					// Pass the payload for the link
					link.Inlet(payload);
				}
			}

			catch (Exception e)
			{throw new NetworkException(e.Message);}
		}

		/// <summary>Outlet</summary>
		/// <param name="endPoint">Network address</param>
		/// <param name="stream">Data stream</param>
		[Obsolete("This method will be modified shortly")]
		internal void Outlet(EndPoint endPoint, BitStream stream){
			if(_Socket == null) return;

			if(endPoint == null) throw new NetworkException("\"endPoint\" can not be null");
			if(stream == null) throw new NetworkException("\"stream\" can not be null");

			try {
				var size = _Socket.SendTo(
					stream.ByteData, stream.LengthBytes,
					SocketFlags.None,
					endPoint
				);
			}

			catch (Exception e)
			{throw new NetworkException(e.Message);}
		}


		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		internal void Raise(EventType type){
			var raiseEvent = default(RaiseEvent);
			raiseEvent.EventType = type;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		internal void Raise(EventType type, Link link){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.EventType = type;
			raiseEvent.EndPoint = link.EndPoint;
			raiseEvent.Link = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		/// <param name="reason">Razão</param>
		internal void Raise(EventType type, Link link, Reason reason){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.Reason = reason;
			raiseEvent.EventType = type;
			raiseEvent.EndPoint = link.EndPoint;
			raiseEvent.Link = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		/// <param name="failure">Falha</param>
		internal void Raise(EventType type, Link link, Failure failure){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.Failure = failure;
			raiseEvent.EventType = type;
			raiseEvent.EndPoint = link.EndPoint;
			raiseEvent.Link = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		/// <param name="payload">Payload</param>
		internal void Raise(EventType type, Link link, Payload payload){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.EventType = type;
			raiseEvent.EndPoint = link.EndPoint;
			raiseEvent.Payload = payload;
			raiseEvent.Link = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="endPoint">Network address</param>
		/// <param name="payload">Payload</param>
		internal void Raise(EventType type, EndPoint endPoint, Payload payload){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.EventType = type;
			raiseEvent.EndPoint = endPoint;
			raiseEvent.Payload = payload;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="raiseEvent">Evento</param>
		internal void Raise(RaiseEvent raiseEvent)
		{_Events.Enqueue(raiseEvent);}


		/// <summary>Network processing</summary>
		private void Process(){
			// Puts running
			Running = true;

			// Link enumerator
			var linkEnum = _Linktable.GetEnumerator();

			while(Running){
				// INLET
				Inlet();

				// Processing timeouts
				if(linkEnum.MoveNext()){
					var link = linkEnum.Current as Link;
					link.Timeout(Time());
				}
				else linkEnum.Reset();

				// Use events in steps
				if(!_Settings.UseStepEvents) Step();
			}
		}


		/// <summary>Gets the end point</summary>
		/// <param name="address">Connection address</param>
		/// <returns>Network address</returns>
		[Obsolete("This method will be removed shortly")]
		internal static EndPoint GetEndPoint(string address){
			// IP Address
			IPAddress resolved = IPAddress.Loopback;

			// Full address split
			string[] splitAddress = (address + ":0").Split(
				new char[]{':'}, StringSplitOptions.RemoveEmptyEntries
			);

			// Parse address
			try {
				if(!IPAddress.TryParse(splitAddress[0], out resolved)){
					// List of IP addresses
					IPAddress[] addrs = Dns.GetHostAddresses(splitAddress[0]);

					// Find a valid address
					foreach(IPAddress addr in addrs)
					if(addr.AddressFamily == AddressFamily.InterNetwork)
					resolved = addr;
				}

				// Valid connection address
				return new IPEndPoint(
					resolved, ushort.Parse(splitAddress[1])
				);
			}

			// Default return
			catch {return new IPEndPoint(IPAddress.Any, IPEndPoint.MinPort);}
		}
	};
};
