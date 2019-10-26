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

namespace NetworkKit.Networking.Channels {
	using Collections;
	using Events;


	/// <summary>Channel reliable</summary>
	internal sealed class ChannelReliable : IChannel {
		/// <summary>Attempts</summary>
		private readonly AsyncTable<uint, uint>      Attempts;

		/// <summary>Contents</summary>
		private readonly AsyncTable<uint, Content>   Contents;

		/// <summary>Cache</summary>
		private readonly AsyncTable<ushort, Content> Cached;

		/// <summary>Ack</summary>
		private readonly AsyncTable<ushort, uint>    Acks;


		/// <summary>Network</summary>
		private readonly Network Network;

		/// <summary>Link</summary>
		private readonly Link Link;


		/// <summary>Expected sequence</summary>
		private ushort ExpectedSequence;

		/// <summary>Sequence</summary>
		private ushort Sequence;


		/// <summary>Muted</summary>
		private bool Muted;


		/// <summary>Channel reliable</summary>
		/// <param name="network">Network</param>
		/// <param name="link">Link</param>
		public ChannelReliable(Network network, Link link){
			// Initializes tables
			Attempts = new AsyncTable<uint, uint>();
			Contents = new AsyncTable<uint, Content>();
			Cached = new AsyncTable<ushort, Content>();
			Acks = new AsyncTable<ushort, uint>();

			Network = network;
			Link = link;
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


		/// <summary>Processe content in sequence</summary>
		private void ProcessSequencedContent(){
			// Retrieve the expected sequence content
			if(Cached.Contains(ExpectedSequence)){
				// Delivery and remove from cache
				var content = Cached[ExpectedSequence];
				Cached.Remove(ExpectedSequence);

				// Clear old sequence
				var sequence = (ushort)((ExpectedSequence - 100) + 0xFFFF);
				Acks.Remove((ushort)(sequence % 0xFFFF));

				// Next expected sequence
				NextExpectedSequence();

				if(Link.DoReason(content))   return;
				if(Link.DoFailure(content))  return;
				if(Link.DoMatch(content))    return;
				if(Link.DoRedirect(content)) return;
				if(Link.DoApproval(content)) return;
				if(Link.DoAccepted(content)) return;
				if(Link.DoDenied(content))   return;
				if(Link.DoData(content))     return;

				// Unknown content
				Network.Raise(EventType.Failed, Failure.Unknown, Link);
			}
		}


		/// <summary>Make response</summary>
		/// <param name="content">Content</param>
		private void MakeResponse(Content content){
			// Raise event delivery response
			var response = Content.Reliable();
			response.Protocol  = Content.RESPONSE;
			response.UserFlag  = content.UserFlag;
			response.Timestamp = content.Timestamp;
			response.Sequence  = content.Sequence;
			response.Fragments = content.Fragments;
			response.Fragment  = content.Fragment;

			// Send immediately
			var size = (uint)Network.Outlet(response, Link);

			// Statistics
			Network.Statistics.PacketSent(size, false);
			Link.Statistics.PacketSent(size, false);
		}


		/// <summary>Do FRAGMENT</summary>
		/// <returns>True if FRAGMENT</returns>
		/// <param name="content">Content</param>
		private bool DoFragment(Content content){
			if(content.Protocol != Content.FRAGMENT) return false;

			// Size of the content is within the expected
			if(content.LengthBits >= Content.RELIABLE_HEADER){
				// Content not delivered
				if(!Acks.Contains(content.Sequence)){
					// Merge the received fragment
					if(Cached.Contains(content.Sequence)){
						// Cached content
						var cachedContent = Cached[content.Sequence];

						// Is within expected sequence
						if(content.Fragment == (cachedContent.Fragment + 1)){
							// Make response
							MakeResponse(content);

							// Defragmenter
							cachedContent.Defragmenter(content);

							// Update timestamp
							cachedContent.Timestamp = content.Timestamp;

							// Content is complete
							if(content.Fragment == cachedContent.Fragments)
							Acks.Setter(content.Sequence, content.Timestamp);
						}
					}

					else {
						// Stores the first content of the stream
						if(content.Fragment == 1){
							// Add content
							Cached.Setter(content.Sequence, content);

							// Make response
							MakeResponse(content);

							return true;
						}
					}
				}

				// already delivered
				else MakeResponse(content);
			}

			return true;
		}

		/// <summary>Do RELIABLE</summary>
		/// <param name="content">Content</param>
		/// <returns>True if RELIABLE</returns>
		private bool DoReliable(Content content){
			if((content.Protocol & Content.RELIABLE) == 0) return false;

			// Size of the content is within the expected
			if(content.LengthBits >= Content.RELIABLE_HEADER){
				// Make response
				MakeResponse(content);

				// Content not delivered
				if(!Acks.Contains(content.Sequence)){
					// Add content
					Cached.Setter(content.Sequence, content);

					// Add ACK
					Acks.Setter(content.Sequence, content.Timestamp);

					return true;
				}
			}

			return true;
		}

		/// <summary>Do RESPONSE</summary>
		/// <param name="content">Content</param>
		/// <returns>True if RESPONSE</returns>
		private bool DoResponse(Content content){
			if(content.Protocol != Content.RESPONSE) return false;

			// Measure latency
			Link.MeasureLatency(content.Timestamp);

			// Recycles and removes the content
			if(Contents.Contains(content.Identifier)){
				// Attempts
				Attempts.Remove(content.Identifier);

				// Contents
				Contents.Remove(content.Identifier);
			}

			return true;
		}


		#region ICHANNEL
		/// <summary>ICHANNEL Released</summary>
		bool IChannel.Released()
		{return (Contents.IsEmpty && Cached.IsEmpty);}

		/// <summary>ICHANNEL Muted</summary>
		bool IChannel.Muted()
		{return Muted;}

		/// <summary>ICHANNEL Reset</summary>
		void IChannel.Reset(){
			// Restarts sequences
			ExpectedSequence = 0;
			Sequence = 0;

			// Clean the tables
			Attempts.Clear();
			Contents.Clear();
			Cached.Clear();
			Acks.Clear();

			// Muted
			Muted = false;
		}

		/// <summary>ICHANNEL Inlet</summary>
		/// <param name="content">Content</param>
		void IChannel.Inlet(Content content){
			if((content.Protocol & Content.IDENTIFIABLE) == 0) return;

			// Content size
			var size = (uint)content.LengthBytes;

			// Do not mark confirmation as reliable
			var isAck = content.Protocol == Content.RESPONSE;

			// Statistics
			Network.Statistics.PacketReceived(size, !isAck);
			Link.Statistics.PacketReceived(size, !isAck);

			// Fragment, Reliable or Response
			if(DoFragment(content)) return;
			if(DoReliable(content)) return;
			if(DoResponse(content)) return;
		}

		/// <summary>ICHANNEL Outlet</summary>
		/// <param name="content">Content</param>
		void IChannel.Outlet(Content content){
			if((content.Protocol & Content.IDENTIFIABLE) == 0) return;

			// MTU minis 20 bytes of UDP IPv4 header
			var mtu = Network.Settings.MTU - 20;

			// Sets the sequence
			content.Sequence = Sequence;

			// Fragments in parts compatible with MTU
			if(content.LengthBytes > mtu){
				// Fragments
				var fragments = content.Fragmenter(mtu);
				for(int i = 0; i < fragments.Length; i++){
					// Set the current timestamp and packing
					// Add 10 milliseconds to save connection
					fragments[i].Timestamp = content.Timestamp + ((uint)i * 10);
					fragments[i].Packing();

					// Attempts
					Attempts.Setter(fragments[i].Identifier, 0);

					// Add to the package table
					Contents.Setter(fragments[i].Identifier, fragments[i]);

					// Send immediately
					var size = (uint)Network.Outlet(fragments[i], Link);

					// Statistics
					Network.Statistics.PacketSent(size, true);
					Link.Statistics.PacketSent(size, true);
				}
			}

			// Simple reliable
			else {
				// Copy content
				var contentOrig = content;
				content = Content.New(0);
				content.Copy(contentOrig);

				// Packing
				content.Packing();

				// Attempts
				Attempts.Setter(content.Identifier, 0);

				// Add to the package table
				Contents.Setter(content.Identifier, content);

				// Send immediately
				var size = (uint)Network.Outlet(content, Link);

				// Statistics
				Network.Statistics.PacketSent(size, true);
				Link.Statistics.PacketSent(size, true);
			}

			// Next sequence
			NextSequence();
		}

		/// <summary>ICHANNEL Timeout</summary>
		/// <param name="time">Time</param>
		void IChannel.Timeout(uint time){
			// Process content in sequence
			ProcessSequencedContent();

			// Time to live
			var ttl = Network.Settings.TTL;

			// Resend if they are waiting
			foreach(Content content in Contents)
				if((time - content.Timestamp) > ttl){
					// Number of attempts
					var attempts = Attempts[content.Identifier];

					// Detects if the channel is muted
					Muted = (attempts > 20);

					// Increment attempts
					Attempts[content.Identifier] = attempts + 1;

					// Set the current timestamp and packing
					content.Timestamp = time;
					content.Packing();

					// Send immediately
					var size = (uint)Network.Outlet(content, Link);

					// Statistics
					Network.Statistics.PacketLoss();
					Network.Statistics.PacketSent(size, true);
					Link.Statistics.PacketLoss();
					Link.Statistics.PacketSent(size, true);
				}
		}
		#endregion
	};
};