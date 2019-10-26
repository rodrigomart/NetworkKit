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

using System.Diagnostics;
using System.Net;


namespace NetworkKit.Networking {
	using Channels;
	using Events;


	/// <summary>Link</summary>
	public sealed partial class Link {
		/// <summary>Link</summary>
		/// <param name="address">Address</param>
		/// <param name="network">Network</param>
		internal Link(string address, Content approval, Network network):
			this()
		{
			// Network
			Network = network;

			// Approval
			Approval = approval;

			// Channels
			Unreliable = new ChannelUnreliable(Network, this);
			Reliable = new ChannelReliable(Network, this);

			// Resolve address
			Resolve(address);
		}

		/// <summary>Link</summary>
		/// <param name="endPoint">End point</param>
		/// <param name="network">Network</param>
		internal Link(EndPoint endPoint, Network network):
			this()
		{
			_EndPoint = endPoint;

			// Network
			Network = network;

			// Channels
			Unreliable = new ChannelUnreliable(Network, this);
			Reliable = new ChannelReliable(Network, this);

			// Reset
			Reset();
		}


		/// <summary>Shuting</summary>
		internal void Shuting(){
			// Change status
			ChangeStatus(Status.Close | Status.Wait);

			// Content SHUTING
			var content = Content.Reliable();
			content.Protocol = Content.SHUTDOWN;

			// Send
			Outlet(content);
		}


		/// <summary>Inlet content</summary>
		/// <param name="content">Content</param>
		internal void Inlet(Content content){
			// Last receipt time
			LastReceiptTime = (uint)Stopwatch.ElapsedMilliseconds;

			// Unreliable content
			if((content.Protocol & Content.IDENTIFIABLE) == 0){
				// Channel unreliable
				Unreliable.Inlet(content);
			}

			// Reliable content
			else {
				// Measure latency
				if(content.Protocol == Content.RESPONSE)
				MeasureLatency(content.Timestamp);

				// Channel reliable
				Reliable.Inlet(content);
			}
		}

		/// <summary>Outlet content</summary>
		/// <param name="content">Content</param>
		internal void Outlet(Content content){
			if(content == null) return;

			if(content.Protocol != Content.PONG)
			content.Timestamp = (uint)Stopwatch.ElapsedMilliseconds;

			// Channel unreliable
			Unreliable.Outlet(content);

			// Channel reliable
			Reliable.Outlet(content);
		}

		/// <summary>Timeout</summary>
		internal void Timeout(){
			// Time
			var time = (uint)Stopwatch.ElapsedMilliseconds;

			// Channel unreliable
			Unreliable.Timeout(time);

			// Channel reliable
			Reliable.Timeout(time);

			// Disconnecting
			if(
				(
					Reliable.Muted() ||
					Unreliable.Muted()
				) || (
					CheckStatus(Status.Close | Status.Wait) &&
					Unreliable.Released() &&
					Reliable.Released()
				)
			){
				// Change status
				ChangeStatus(Status.Close | Status.Done);

				if(Reliable.Muted() || Reliable.Muted())
				Network.Raise(EventType.Unlinked, Reason.Muted, this);

				else Network.Raise(EventType.Unlinked, Reason.Unlinked, this);
				return;
			}

			// Settings
			var downtime     = Network.Settings.Downtime;
			var pingInterval = Network.Settings.PingInterval;

			// Downtime
			if((time - LastReceiptTime) > downtime){
				// Timeout
				if(
					CheckStatus(Status.Connect | Status.Wait) ||
					CheckStatus(Status.Connect | Status.Done)
				) Network.Raise(EventType.Unlinked, Reason.Timeout, this);

				// Host not accessible
				else if(CheckStatus(Status.Connect))
				Network.Raise(EventType.Failed, Failure.NotAccessible, this);

				// Change status
				ChangeStatus(Status.Close | Status.Done);
			}

			// Ping
			if(
				CheckStatus(Status.Connect | Status.Done) &&
				(time - LastPingTime) > pingInterval
			){
				// Last ping interval
				LastPingTime = time;

				// Ping
				var content = Content.Unreliable();
				content.Protocol = Content.PING;

				// Set the current timestamp and pack
				content.Timestamp = time;
				content.Packing();

				// Send immediately
				var size = (uint)Network.Outlet(content, this);

				// Statistics
				Statistics.PacketSent(size, false);
				Network.Statistics.PacketSent(size, false);
			}
		}


		/// <summary>Measure the latency</summary>
		/// <param name="time">Time</param>
		internal void MeasureLatency(uint time){
			// Moving average
			LatencyPooling[LatencyPoint] = ((uint)Stopwatch.ElapsedMilliseconds - time);
			LatencyPoint = (LatencyPoint + 1) % LatencyPooling.Length;

			// Average latency
			LatencyMeasured = 0u;
			for(int i = 0; i < LatencyPooling.Length; i++)
			LatencyMeasured += LatencyPooling[i];
			LatencyMeasured /= 5;
		}


		#region DO
		/// <summary>MATCH</summary>
		/// <returns>True if MATCH</returns>
		/// <param name="content">Content</param>
		internal bool DoMatch(Content content){
			if(content.Protocol != Content.MATCH) return false;

			// Ignore if you are not connecting
			if(!CheckStatus(Status.Connect))
			return true;

			// Combine status
			CombineStatus(Status.Wait);

			// Private secret
			var publicKey = content.ReadUInt64();
			PrivateSecret = (publicKey ^ PrivateKey) % 0x7549F1A399E13FD7;

			return true;
		}


		/// <summary>DATA</summary>
		/// <returns>True if DATA</returns>
		/// <param name="content">Content</param>
		internal bool DoData(Content content){
			// Raise event
			Network.Raise(EventType.Content, this, content);

			return true;
		}


		/// <summary>PING</summary>
		/// <returns>True if PING</returns>
		/// <param name="content">content</param>
		internal bool DoPing(Content content){
			if(content.Protocol != Content.PING) return false;

			// Set the header
			content.SetHeader(Content.PONG);
			content.Packing();

			// Send
			Outlet(content);

			return true;
		}

		/// <summary>PONG</summary>
		/// <returns>True if PONG</returns>
		/// <param name="content">Content</param>
		internal bool DoPong(Content content){
			if(content.Protocol != Content.PONG) return false;

			// Measure the latency
			MeasureLatency(content.Timestamp);

			return true;
		}


		/// <summary>REASON</summary>
		/// <returns>True if REASON</returns>
		/// <param name="content">Content</param>
		internal bool DoReason(Content content){
			if(
				content.Protocol != Content.UNLINK &&
				content.Protocol != Content.SHUTDOWN
			) return false;

			// Change status
			ChangeStatus(Status.Close | Status.Done);

			// Raise event UNLINK
			if(content.Protocol == Content.UNLINK)
			Network.Raise(EventType.Unlinked, Reason.Unlinked, this);

			// Raise event SHUTDOWN
			if(content.Protocol == Content.SHUTDOWN)
			Network.Raise(EventType.Unlinked, Reason.Shutdown, this);

			return true;
		}

		/// <summary>FAILURE</summary>
		/// <returns>True if FAILURE</returns>
		/// <param name="content">Content</param>
		internal bool DoFailure(Content content){
			if(
				content.Protocol != Content.LIMIT_OF_LINKS &&
				content.Protocol != Content.NOT_ACCESSIBLE
			) return false;

			// Change status
			ChangeStatus(Status.Close | Status.Done);

			// Raise event LIMIT OF LINKS
			if(content.Protocol == Content.LIMIT_OF_LINKS)
			Network.Raise(EventType.Failed, Failure.LimitOfLinks, this);

			// Raise event NOT ACCESSIBLE
			if(content.Protocol == Content.NOT_ACCESSIBLE)
			Network.Raise(EventType.Failed, Failure.NotAccessible, this);

			return true;
		}


		/// <summary>REDIRECT</summary>
		/// <returns>True if </returns>
		/// <param name="content">Content</param>
		internal bool DoRedirect(Content content){
			if(content.Protocol != Content.REDIRECT) return false;

			// Ignore if no approval or linked
			if(!CheckStatus(Status.Connect))
			return true;

			// Remove current
			Network.Unlinked(this);

			// Change status
			ChangeStatus(Status.Redirect);

			// Resolve address
			Resolve(content.ReadString());

			// Add current
			Network.Linked(this, this);

			// Raise event
			Network.Raise(EventType.Redirect, this);

			return true;
		}


		/// <summary>APPROVAL</summary>
		/// <returns>True if </returns>
		/// <param name="content">Content</param>
		internal bool DoApproval(Content content){
			if(content.Protocol != Content.APPROVAL) return false;

			// Invalide state
			if(!CheckStatus(Status.Connect | Status.Wait))
			return true;

			// Auto approval
			if(!Network.Settings.RequiresApproval){
				// Linked will be called by the method of approval
				Accept();

				return true;
			}

			// Raise event
			Network.Raise(EventType.Approval, this, content);

			return true;
		}

		/// <summary>ACCEPTED</summary>
		/// <returns>True if </returns>
		/// <param name="content">Content</param>
		internal bool DoAccepted(Content content){
			if(content.Protocol != Content.ACCEPTED) return false;

			// Ignore if no approval
			if(!CheckStatus(Status.Connect | Status.Wait))
			return true;

			// Raise event
			if(CheckStatus(Status.Redirect))
			Network.Raise(EventType.Redirected, this);
			
			else Network.Raise(EventType.Linked, this);

			// Change status
			ChangeStatus(Status.Connect | Status.Done);

			// Statistics
			Network.Statistics.IncreaseLink();

			return true;
		}


		/// <summary>DENIED</summary>
		/// <returns>True if DENIED</returns>
		/// <param name="content">Content</param>
		internal bool DoDenied(Content content){
			if(content.Protocol != Content.DENIED) return false;

			// Change status
			ChangeStatus(Status.Close | Status.Done);

			// Raise event
			Network.Raise(
				EventType.Linked,
				Reason.Denied,
				this
			);

			return true;
		}
		#endregion
	};
};