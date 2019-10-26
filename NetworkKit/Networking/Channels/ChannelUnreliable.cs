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
	/// <summary>Channel unreliable</summary>
	internal sealed class ChannelUnreliable : IChannel {
		/// <summary>Network</summary>
		private readonly Network Network;

		/// <summary>Link</summary>
		private readonly Link Link;


		/// <summary>Last received time</summary>
		private uint LastReceivedTime;


		/// <summary>Channel unreliable</summary>
		/// <param name="network">Network</param>
		/// <param name="link">Link</param>
		public ChannelUnreliable(Network network, Link link){
			Network = network;
			Link = link;
		}


		#region ICHANNEL
		/// <summary>ICHANNEL Released</summary>
		bool IChannel.Released()
		{return true;}

		/// <summary>ICHANNEL Muted</summary>
		bool IChannel.Muted()
		{return false;}


		/// <summary>ICHANNEL Reset</summary>
		void IChannel.Reset()
		{LastReceivedTime = 0U;}

		/// <summary>ICHANNEL Inlet</summary>
		/// <param name="content">Content</param>
		void IChannel.Inlet(Content content){
			if((content.Protocol & Content.IDENTIFIABLE) != 0) return;

			// Payload size
			var size = (uint)content.LengthBytes;

			// Statistics
			Network.Statistics.PacketReceived(size, false);
			Link.Statistics.PacketReceived(size, false);

			// Ping or Pong
			if(Link.DoPing(content))    return;
			if(Link.DoPong(content))    return;
			if(Link.DoFailure(content)) return;

			// Only the latest content
			if(content.Timestamp <= LastReceivedTime)
			return;

			// Last received time
			LastReceivedTime = content.Timestamp;

			// Data
			Link.DoData(content);
		}

		/// <summary>ICHANNEL Outlet</summary>
		/// <param name="content">Content</param>
		void IChannel.Outlet(Content content){
			if((content.Protocol & Content.IDENTIFIABLE) != 0) return;

			// Packing
			content.Packing();

			// Send immediately
			var size = (uint)Network.Outlet(content, Link);

			// Statistics
			Network.Statistics.PacketSent(size, false);
			Link.Statistics.PacketSent(size, false);
		}

		/// <summary>ICHANNEL Timeout</summary>
		/// <param name="time">Time</param>
		void IChannel.Timeout(uint time){}
		#endregion
	};
};