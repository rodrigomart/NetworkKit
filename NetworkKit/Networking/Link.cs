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

using System.Net;
using System;


namespace NetworkKit.Networking {
	using Containers;


	/// <summary>
	/// Link Status
	/// </summary>
	public enum LinkStatus {
		Linking,
		Approval,
		Redirect,
		Redirecting,
		Linked,
		Close
	};


	/// <summary>
	/// Connection link
	/// </summary>
	public class Link {
		/// <summary>Payloads</summary>
		private readonly Table<UInt32, Payload> _Payloads;

		/// <summary>Cache</summary>
		private readonly Table<UInt16, Payload> _Cached;

		/// <summary>Ack</summary>
		private readonly Table<UInt16, UInt32>  _Acks;


		/// <summary>Network core</summary>
		private readonly Network _Network;


		/// <summary>Network address</summary>
		private EndPoint _EndPoint;


		/// <summary>Link Status</summary>
		private LinkStatus _Status;


		/// <summary>Expected sequence</summary>
		private UInt16 ExpectedSequence;

		/// <summary>Sequence</summary>
		private UInt16 Sequence;


		/// <summary>Private secret</summary>
		private UInt64 PrivateSecret;

		/// <summary>Private Key</summary>
		private UInt64 PrivateKey;

		/// <summary>Public key</summary>
		private UInt64 PublicKey;


		/// <summary>Last untrusted payload</summary>
		private UInt32 LastUnreliableTime;

		/// <summary>Last receipt</summary>
		private UInt32 LastReceiptTime;

		/// <summary>Last ping</summary>
		private UInt32 LastPingTime;


		/// <summary>Latency aggregation</summary>
		private UInt32[] LatencyPooling;

		/// <summary>Measured latency</summary>
		private Double LatencyMeasured;

		/// <summary>Latency point</summary>
		private Int32 LatencyPoint;


		/// <summary>Redirecting</summary>
		private bool IsRedirect;


		/// <summary>Network address</summary>
		internal EndPoint EndPoint {
			get {return _EndPoint;}
		}


		/// <summary>Link Status</summary>
		public LinkStatus Status {
			get {return _Status;}
		}

		/// <summary>Latency</summary>
		public uint Latency {
			get {return (uint)LatencyMeasured;}
		}


		/// <summary>Accept connection</summary>
		public void Accept(){
			if(_Status != LinkStatus.Approval) return;

			// Change status
			_Status = LinkStatus.Linked;

			// ACCEPTED payload
			Outlet(Payload.Create(Payload.ACCEPTED));
		}

		/// <summary>Deny connection</summary>
		public void Deny(){
			if(_Status != LinkStatus.Approval) return;

			// Change status
			_Status = LinkStatus.Close;

			// Out
			Outlet(Payload.Create(Payload.DENIED));

			// Unlinked
			_Network.Unlinked(_EndPoint);
		}


		/// <summary>Redirects to an address</summary>
		/// <param name="address">Connection address</param>
		public void Redirect(string address){
			// Change status
			_Status = LinkStatus.Redirect;

			// Redirect payload
			var payload = Payload.Create(Payload.REDIRECT);
			payload.Write(address);

			// Out
			Outlet(payload);
		}

		/// <summary>Disconnect link</summary>
		public void Unlink(){
			_Status = LinkStatus.Close;

			// UNLINK Payload
			Outlet(Payload.Create(Payload.UNLINK));

			// Unlink
			_Network.Unlinked(_EndPoint);
		}


		/// <summary>Send payload</summary>
		/// <param name="payload">Payload</param>
		public void Send(Payload payload)
		{Outlet(payload);}


		/// <summary>Connection Link</summary>
		/// <param name="network">Network core</param>
		/// <param name="endPoint">Network address</param>
		internal Link(Network network, EndPoint endPoint){
			_EndPoint = endPoint;
			_Network = network;

			// Mobile latency average
			LatencyPooling = new uint[]{50U, 50U, 50U, 50U, 50U};

			// Sets the private key and publishes
			PrivateKey = (ulong)DateTime.UtcNow.Ticks;
			PublicKey = (PrivateKey ^ 0x2018E200) % 0x7549F1A399E13FD7;

			// Initializes tables
			_Payloads = new Table<uint, Payload>();
			_Cached = new Table<ushort, Payload>();
			_Acks = new Table<ushort, uint>();

			// Restart
			Reset();
		}


		/// <summary>Payload input</summary>
		/// <param name="payload">Payload</param>
		internal void Inlet(Payload payload){
			// Last receipt time
			LastReceiptTime = _Network.Time();

			// Ignore if shorter than expected
			if(payload.LengthBits < Payload.UNRELIABLE_HEADER){
				payload.Recycle();
				return;
			}

			if(DoPing(payload)) return;
			if(DoPong(payload)) return;
			if(DoReason(payload)) return;
			if(DoFailure(payload)) return;
			if(DoFragment(payload)) return;
			if(DoReliable(payload)) return;
			if(DoResponse(payload)) return;
			if(DoUnreliable(payload)) return;

			// Unknown payload
			_Network.Raise(EventType.Failed, this, Failure.Unknown);

			// Auto Recycling
			payload.Recycle();
		}

		/// <summary>Payload output</summary>
		/// <param name="payload">Payload</param>
		internal void Outlet(Payload payload){
			// MTU minus 20 bytes of UDP IPv4 header
			var mtu = _Network.Settings.MTU - 20;

			// Reliable payload
			if((payload.Protocol & Payload.RELIABLE) != 0){
				// Sets the sequence
				payload.Sequence = Sequence;

				// Fragments in parts compatible with MTU
				if(payload.LengthBytes > mtu){
					// Fragments
					var fragments = payload.Fragmenter(mtu);
					for(int i = 0; i < fragments.Length; i++){
						// Set the current timestamp and pack
						fragments[i].Timestamp = _Network.Time();
						fragments[i].Packing();

						// Adds to the package table
						_Payloads.Add(fragments[i].Identifier, fragments[i]);

						// Send immediately
						_Network.Outlet(_EndPoint, fragments[i]);
					}

					// Recycles the original payload
					payload.Recycle();
				}

				// Reliable simple
				else {
					// Set the current timestamp and pack
					payload.Timestamp = _Network.Time();
					payload.Packing();

					// Adds to the package table
					_Payloads.Add(payload.Identifier, payload);

					// Send immediately
					_Network.Outlet(_EndPoint, payload);
				}

				// Next sequence
				NextSequence();
				return;
			}

			// Unreliable payload
			else {
				// Unreliable larger than MTU are ignored
				if(payload.LengthBytes > mtu){
					payload.Recycle();
					return;
				}

				// Set the current timestamp and pack
				if(payload.Protocol != Payload.RESPONSE)
				payload.Timestamp = _Network.Time();
				payload.Packing();

				// Send immediately
				_Network.Outlet(_EndPoint, payload);
			}

			// Auto recycling
			payload.Recycle();
		}

		/// <summary>Timeout</summary>
		/// <param name="time">Tempo</param>
		internal void Timeout(uint time){
			// Process payload in sequence
			ProcessSequencedPayload();

			// Resend if they are waiting
			foreach(Payload payload in _Payloads)
			if((time - payload.Timestamp) > 333){
				// Set the current timestamp and pack
				payload.Timestamp = time;
				payload.Packing();

				// Send immediately
				_Network.Outlet(_EndPoint, payload);
			}

			// Settings
			var downtime = _Network.Settings.Downtime;
			var pingInterval = _Network.Settings.PingInterval;

			// Downtime
			if((time - LastReceiptTime) >= downtime){
				_Network.Unlinked(_EndPoint);

				// Host not accessible
				if(Status == LinkStatus.Linking)
				_Network.Raise(EventType.Failed, this, Failure.NotAccessible);

				// Timeout
				if(Status == LinkStatus.Approval || Status == LinkStatus.Linked)
				_Network.Raise(EventType.Unlinked, this, Reason.Timeout);

				// Change status
				_Status = LinkStatus.Close;
				return;
			}

			// Ping
			if(
				_Status == LinkStatus.Linked &&
				(time - LastPingTime) > pingInterval
			){
				LastPingTime = time;

				// PING Payload
				var payload = Payload.Create(Payload.PING);

				// Set the current timestamp and pack
				payload.Timestamp = time;
				payload.Packing();

				// Send immediately
				_Network.Outlet(_EndPoint, payload);
			}
		}


		/// <summary>Measure the latency</summary>
		/// <param name="time">Time</param>
		internal void MeasureLatency(uint time){
			// Moving average
			LatencyPooling[LatencyPoint] = _Network.Time() - time;
			LatencyPoint = (LatencyPoint + 1) % LatencyPooling.Length;

			// Average latency
			LatencyMeasured = 0.0;
			for(int i = 0; i < LatencyPooling.Length; i++)
			LatencyMeasured += LatencyPooling[i];
			LatencyMeasured /= 5.0;
		}


		/// <summary>Next sequence</summary>
		private void NextSequence(){
			Sequence += 1;
			Sequence %= 0xFFFF;
		}

		/// <summary>Next expected sequence</summary>
		private void NextExpectedSequence(){
			ExpectedSequence += 1;
			ExpectedSequence %= 0xFFFF;
		}

		/// <summary>Process payload in sequence</summary>
		private void ProcessSequencedPayload(){
			// Retrieve the expected sequence payload
			if(_Cached.Contains(ExpectedSequence)){
				// Delivery and remove from cache
				var payload = _Cached.Find(ExpectedSequence);
				_Cached.Remove(ExpectedSequence);

				// Clear old sequence
				var sequence = (ushort)((ExpectedSequence - 100) + 0xFFFF);
				_Acks.Remove((ushort)(sequence % 0xFFFF));

				// Next expected sequence
				NextExpectedSequence();

				if(DoMatch(payload)) return;
				if(DoApproval(payload)) return;
				if(DoRedirect(payload)) return;
				if(DoAccepted(payload)) return;
				if(DoDenied(payload)) return;
				if(DoData(payload)) return;

				// Unknown payload
				_Network.Raise(EventType.Failed, this, Failure.Unknown);

				// Auto Recycling
				payload.Recycle();
			}
		}


		/// <summary>Restarts the connection</summary>
		private void Reset(){
			// Change status
			_Status = LinkStatus.Linking;

			// Clean the tables
			_Payloads.Clear();
			_Cached.Clear();
			_Acks.Clear();

			// Restarts sequences
			ExpectedSequence = 0;
			Sequence = 0;

			// Reset the timers
			var time = _Network.Time();
			LastUnreliableTime = time;
			LastReceiptTime = time;
			LastPingTime = time;


			// Match the pair
			var payload = Payload.Create(Payload.MATCH);
			payload.Write(PublicKey);

			// Out
			Outlet(payload);
		}


		/// <summary>Do PING</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if PING</returns>
		private bool DoPing(Payload payload){
			if(payload.Protocol != Payload.PING) return false;

			// Sets the header
			payload.SetHeader(Payload.PONG);

			// Send immediately
			_Network.Outlet(_EndPoint, payload);

			payload.Recycle();
			return true;
		}

		/// <summary>Do PONG</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if PONG</returns>
		private bool DoPong(Payload payload){
			if(payload.Protocol != Payload.PONG) return false;

			// Measure the latency
			MeasureLatency(payload.Timestamp);

			payload.Recycle();
			return true;
		}

		/// <summary>Do MATCH</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if MATCH</returns>
		private bool DoMatch(Payload payload){
			if(payload.Protocol == Payload.MATCH) return false;

			// Ignore if you are not connecting
			if(_Status != LinkStatus.Linking){
				payload.Recycle();
				return false;
			}

			// Private secret
			var publicKey = payload.ReadUInt64();
			PrivateSecret = (publicKey ^ PrivateKey) % 0x7549F1A399E13FD7;

			// Change status
			_Status = LinkStatus.Approval;

			payload.Recycle();
			return true;
		}

		/// <summary>Do REASON</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if REASON</returns>
		private bool DoReason(Payload payload){
			if(
				payload.Protocol != Payload.UNLINK   &&
				payload.Protocol != Payload.SHUTDOWN
			) return false;

			// Change status
			_Status = LinkStatus.Close;

			// Unlink
			_Network.Unlinked(_EndPoint);

			// Raise event UNLINK
			if(payload.Protocol == Payload.UNLINK)
			_Network.Raise(EventType.Unlinked, this, Reason.Unlinked);

			// Raise event SHUTDOWN
			if(payload.Protocol == Payload.SHUTDOWN)
			_Network.Raise(EventType.Unlinked, this, Reason.Shutdown);

			payload.Recycle();
			return true;
		}

		/// <summary>Do FAILURE</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if FAILURE</returns>
		private bool DoFailure(Payload payload){
			if(
				payload.Protocol != Payload.LIMIT_OF_LINKS &&
				payload.Protocol != Payload.NOT_ACCESSIBLE
			) return false;

			// Change status
			_Status = LinkStatus.Close;

			// Unlink
			_Network.Unlinked(_EndPoint);

			// Raise event LIMIT OF LINKS
			if(payload.Protocol == Payload.LIMIT_OF_LINKS)
			_Network.Raise(EventType.Failed, this, Failure.LimitOfLinks);

			// Raise event NOT ACCESSIBLE
			if(payload.Protocol == Payload.NOT_ACCESSIBLE)
			_Network.Raise(EventType.Failed, this, Failure.NotAccessible);

			payload.Recycle();
			return true;
		}

		/// <summary>Do FRAGMENT</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if FRAGMENT</returns>
		private bool DoFragment(Payload payload){
			if(payload.Protocol != Payload.FRAGMENT) return false;

			// Size of the payload is within the expected
			if(payload.LengthBits >= Payload.RELIABLE_HEADER){
				// Payload not delivered
				if(!_Acks.Contains(payload.Sequence)){
					// Merge the received fragment
					if(_Cached.Contains(payload.Sequence)){
						// Cached payload
						var cachedPayload = _Cached.Find(payload.Sequence);

						// Is within expected sequence
						if(payload.Fragment == (cachedPayload.Fragment + 1)){
							// Raise event delivery response
							_Network.Outlet(_EndPoint, Payload.Create(
								Payload.RESPONSE,
								payload.UserFlag,
								payload.Timestamp,
								payload.Sequence,
								payload.Fragments,
								payload.Fragment
							));

							// Defragmenter
							cachedPayload.Defragmenter(payload);

							// Update timestamp
							cachedPayload.Timestamp = payload.Timestamp;

							// Payload is complete
							if(payload.Fragment == cachedPayload.Fragments)
							_Acks.Add(payload.Sequence, payload.Timestamp);
						}
					}

					else {
						// Stores the first payload of the stream
						if(payload.Fragment == 1){
							// Add payload
							_Cached.Add(payload.Sequence, payload);

							// Raise event delivery response
							_Network.Outlet(_EndPoint, Payload.Create(
								Payload.RESPONSE,
								payload.UserFlag,
								payload.Timestamp,
								payload.Sequence,
								payload.Fragments,
								payload.Fragment
							));
						}
					}
				}

				// already delivered
				else {
					// Raise event delivery response
					_Network.Outlet(_EndPoint, Payload.Create(
						Payload.RESPONSE,
						payload.UserFlag,
						payload.Timestamp,
						payload.Sequence,
						payload.Fragments,
						payload.Fragment
					));
				}
			}

			payload.Recycle();
			return true;
		}

		/// <summary>Do RELIABLE</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if RELIABLE</returns>
		private bool DoReliable(Payload payload){
			if((payload.Protocol & Payload.RELIABLE) != 0) return false;

			// Size of the payload is within the expected
			if(payload.LengthBits >= Payload.RELIABLE_HEADER){
				// Raise event delivery response
				_Network.Outlet(_EndPoint, Payload.Create(
					Payload.RESPONSE,
					payload.UserFlag,
					payload.Timestamp,
					payload.Sequence,
					payload.Fragments,
					payload.Fragment
				));

				// Payload not delivered
				if(!_Acks.Contains(payload.Sequence)){
					// Add payload
					_Cached.Add(payload.Sequence, payload);

					// Add ACK
					_Acks.Add(payload.Sequence, payload.Timestamp);

					return true;
				}
			}

			payload.Recycle();
			return true;
		}

		/// <summary>Do RESPONSE</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if RESPONSE</returns>
		private bool DoResponse(Payload payload){
			if(payload.Protocol != Payload.RESPONSE) return false;

			// Measure the latency
			MeasureLatency(payload.Timestamp);

			// Recycles and removes the payload
			if(_Payloads.Contains(payload.Identifier)){
				_Payloads.Find(payload.Identifier).Recycle();
				_Payloads.Remove(payload.Identifier);
			}

			payload.Recycle();
			return true;
		}

		/// <summary>Do UNRELIABLE</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if UNRELIABLE</returns>
		private bool DoUnreliable(Payload payload){
			if((payload.Protocol & Payload.IDENTIFIABLE) != 0) return false;

			if(Status == LinkStatus.Linked){
				// Only the latest payload
				if(payload.Timestamp <= LastUnreliableTime){
					payload.Recycle();
					return true;
				}

				// Last unreliable time
				LastUnreliableTime = payload.Timestamp;

				// Raise event
				_Network.Raise(EventType.Payload, this, payload);
				return true;
			}

			payload.Recycle();
			return true;
		}

		/// <summary>Do APPROVA</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if APPROVAL</returns>
		private bool DoApproval(Payload payload){
			if(payload.Protocol != Payload.APPROVAL) return false;

			// Auto approval
			if(!_Network.Settings.RequiresApproval){
				// Accept
				Accept();

				// Raise event
				// In automatic approval the OnApprova event is ignored and replaced by OnLinked
				_Network.Raise(EventType.Linked, this);

				payload.Recycle();
				return true;
			}

			// Raise event
			_Network.Raise(EventType.Approval, this, payload);
			return true;
		}

		/// <summary>Do REDIRECT</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if REDIRECT</returns>
		private bool DoRedirect(Payload payload){
			if(payload.Protocol == Payload.REDIRECT) return false;

			// Ignore if no approval or linked
			if(
				_Status != LinkStatus.Approval ||
				_Status != LinkStatus.Linked
			){
				payload.Recycle();
				return true;
			}

			// Is redirect
			IsRedirect = true;

			// Unlink current
			_Network.Unlinked(_EndPoint);

			// Set new address
			_EndPoint = Network.GetEndPoint(payload.ReadString());
			_Network.Linked(_EndPoint, this);

			// Restart
			Reset();

			// Raise event
			_Network.Raise(EventType.Redirect, this);

			payload.Recycle();
			return true;
		}


		/// <summary>Do ACCEPTED</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if ACCEPTED</returns>
		private bool DoAccepted(Payload payload){
			if(payload.Protocol != Payload.ACCEPTED) return false;

			// Ignore if no approval
			if(_Status != LinkStatus.Approval){
				payload.Recycle();
				return true;
			}

			// Chage status
			_Status = LinkStatus.Linked;

			// Raise event
			if(IsRedirect) _Network.Raise(EventType.Redirected, this);
			else _Network.Raise(EventType.Linked, this);

			// Was redirected
			IsRedirect = false;

			payload.Recycle();
			return true;
		}

		/// <summary>Do DENIED</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if DENIED</returns>
		private bool DoDenied(Payload payload){
			if(payload.Protocol != Payload.DENIED) return false;

			// Change status
			_Status = LinkStatus.Close;

			// Unlinked
			_Network.Unlinked(_EndPoint);

			// Raise event
			_Network.Raise(EventType.Unlinked, this, Reason.Denied);

			payload.Recycle();
			return true;
		}

		/// <summary>Do DATA</summary>
		/// <param name="payload">Payload</param>
		/// <returns>True if DATA</returns>
		private bool DoData(Payload payload){
			if((payload.Protocol & 0x0F) != 0) return false;

			if(Status == LinkStatus.Linked){
				// Raise event
				_Network.Raise(EventType.Payload, this, payload);
				return true;
			}

			payload.Recycle();
			return true;
		}
	};
};