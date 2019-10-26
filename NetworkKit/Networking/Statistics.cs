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


namespace NetworkKit.Networking {
	/// <summary>Statistics</summary>
	public sealed class Statistics {
		/// <summary>Stopwatch</summary>
		private readonly Stopwatch Stopwatch;


		/// <summary>Traffic timer</summary>
		private uint TrafficTime;

		/// <summary>Last bytes received per second</summary>
		private uint LastBytesReceivedSec;

		/// <summary>Last bytes sent per second</summary>
		private uint LastBytesSentSec;

		/// <summary>Bytes received per second</summary>
		private uint BytesReceivedSec;

		/// <summary>Bytes sent per second</summary>
		private uint BytesSentSec;


		/// <summary>Traffic received in kbps</summary>
		public float TrafficReceived {
			get {return (LastBytesReceivedSec * 8) / 1000;}
		}

		/// <summary>Traffic sent in kbps</summary>
		public float TrafficSent {
			get {return (LastBytesSentSec * 8) / 1000;}
		}


		/// <summary>Percentage of loss</summary>
		public float LossPercent {
			private set;
			get;
		}


		/// <summary>Reliable packages received</summary>
		public uint ReliableReceived {
			private set;
			get;
		}

		/// <summary>Reliable packages sent</summary>
		public uint ReliableSent {
			private set;
			get;
		}

		/// <summary>Reliable packages loss</summary>
		public uint ReliableLoss {
			private set;
			get;
		}


		/// <summary>Packages received</summary>
		public uint PacketsReceived {
			private set;
			get;
		}

		/// <summary>Packages sent</summary>
		public uint PacketsSent {
			private set;
			get;
		}


		/// <summary>Bytes received</summary>
		public uint BytesReceived {
			private set;
			get;
		}

		/// <summary>Bytes sent</summary>
		public uint BytesSent {
			private set;
			get;
		}


		/// <summary>Number of links</summary>
		public uint NumLinks {
			internal set;
			get;
		}


		/// <summary>Statistics</summary>
		public Statistics(){
			Stopwatch = new Stopwatch();
			Stopwatch.Start();
		}


		/// <summary>Restarts statistics</summary>
		public void Reset(){
			// Stopwatch
			Stopwatch.Restart();

			TrafficTime = 0u;

			LastBytesReceivedSec = 0u;
			LastBytesSentSec = 0u;

			BytesReceivedSec = 0u;
			BytesSentSec = 0u;

			LossPercent = 0f;

			ReliableReceived = 0u;
			ReliableSent = 0u;
			ReliableLoss = 0u;

			PacketsReceived = 0u;
			PacketsSent = 0u;

			BytesReceived = 0u;
			BytesSent = 0u;
		}


		/// <summary>Increase the number of links</summary>
		internal void IncreaseLink()
		{NumLinks++;}

		/// <summary>Decreases the number of links</summary>
		internal void DecreaseLink()
		{if(NumLinks > 0) NumLinks--;}


		/// <summary>Updates to loss statistics</summary>
		internal void PacketLoss()
		{ReliableLoss++;}

		/// <summary>Updates to sent statistics</summary>
		/// <param name="size">Package size in bytes</param>
		/// <param name="reliable">True if reliable</param>
		internal void PacketSent(uint size, bool reliable){
			if(reliable) ReliableSent++;
			PacketsSent++;

			BytesSent += size;
			BytesSentSec += size;
		}

		/// <summary>Updates to receive statistics</summary>
		/// <param name="size">Package size in bytes</param>
		/// <param name="reliable">True if reliable</param>
		internal void PacketReceived(uint size, bool reliable){
			if(reliable) ReliableReceived++;
			PacketsReceived++;

			BytesReceived += size;
			BytesReceivedSec += size;
		}


		/// <summary>Updates to traffic statistics</summary>
		internal void UpdateTraffic(){
			var time = (uint)Stopwatch.ElapsedMilliseconds;

			if(TrafficTime > time) return;
			TrafficTime = time + 1000;

			LastBytesSentSec = BytesSentSec;
			BytesSentSec = 0u;

			LastBytesReceivedSec = BytesReceivedSec;
			BytesReceivedSec = 0u;

			if(ReliableSent > 0)
			LossPercent = (ReliableLoss * 100f) / ReliableSent;
		}
	};
};