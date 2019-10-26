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
using System.Net.Sockets;
using System.Net;
using System;


namespace NetworkKit.Networking {
	using Channels;
	using Events;


	/// <summary>Link</summary>
	public sealed partial class Link {
		/// <summary>Status</summary>
		[Flags]
		private enum Status {
			ResolveAddress = 0,

			Connect        = 1,
			Redirect       = 2,
			Close          = 4,

			Wait           = 16,
			Done           = 32,

			All            = Connect | Redirect | Close | Wait | Done,

			Connecting     = Connect  | Wait,
			Connected      = Connect  | Done,
			Redirecting    = Redirect | Connect | Wait,
			Redirected     = Redirect | Connect | Done,
			Closing        = Close    | Wait,
			Closed         = Close    | Done
		};


		/// <summary>Stopwatch</summary>
		private readonly Stopwatch Stopwatch;

		/// <summary>Channel unreliable</summary>
		private readonly IChannel Unreliable;

		/// <summary>Channel reliable</summary>
		private readonly IChannel Reliable;

		/// <summary>Network</summary>
		private readonly Network Network;

		/// <summary>Approval</summary>
		private readonly Content Approval;


		/// <summary>End point</summary>
		private EndPoint _EndPoint;

		/// <summary>Status</summary>
		private Status _Status;


		/// <summary>Private secret</summary>
		private ulong PrivateSecret;

		/// <summary>Private key</summary>
		private ulong PrivateKey;

		/// <summary>Public key</summary>
		private ulong PublicKey;


		/// <summary>Last receipt</summary>
		private uint LastReceiptTime;

		/// <summary>Last ping</summary>
		private uint LastPingTime;


		/// <summary>Latency aggregation</summary>
		private uint[] LatencyPooling;

		/// <summary>Measured latency</summary>
		private uint LatencyMeasured;

		/// <summary>Latency point</summary>
		private int LatencyPoint;


		/// <summary>Link</summary>
		private Link(){
			// Mobile latency average
			LatencyPooling  = new uint[]{15U, 15U, 15U, 15U, 15U};
			LatencyMeasured = 15u;

			// Statistics
			Statistics = new Statistics();

			// Stopwatch
			Stopwatch = new Stopwatch();
			Stopwatch.Start();
		}


		/// <summary>Reset</summary>
		private void Reset(){
			// Clear status
			ClearStatus(Status.Close | Status.Wait | Status.Done);

			// Combine status
			CombineStatus(Status.Connect);

			// Sets the private key and publishes
			PrivateKey = (ulong)DateTime.UtcNow.Ticks;
			PublicKey = (PrivateKey ^ 0x2018E200) % 0x7549F1A399E13FD7;

			// Reset channels
			Unreliable.Reset();
			Reliable.Reset();

			// Content MATCH
			var content = Content.Reliable();
			content.Protocol = Content.MATCH;
			content.WriteUInt64(PublicKey);

			// Send
			Outlet(content);

			// Send approval
			Outlet(Approval);
		}


		/// <summary>Resolve</summary>
		/// <param name="address">Address</param>
		private void Resolve(string address){
			// IP address
			IPAddress resolved = IPAddress.Loopback;

			// Full address split
			string[] splitAddress = (address + ":0").Split(
				new char[]{':'}, StringSplitOptions.RemoveEmptyEntries
			);

			// Parse address
			try {
				if(!IPAddress.TryParse(splitAddress[0], out resolved)){
					// List of IP addresses
					var addrsTask = Dns.GetHostAddressesAsync(splitAddress[0]);

					// Find a valid address
					foreach(IPAddress addr in addrsTask.Result)
					if(addr.AddressFamily == AddressFamily.InterNetwork)
					resolved = addr;
				}

				// Valid connection address
				_EndPoint = new IPEndPoint(
					resolved, ushort.Parse(splitAddress[1])
				);

				// Reset
				Reset();
			}

			// Default return
			catch {
				_EndPoint = new IPEndPoint(
					IPAddress.Any, IPEndPoint.MinPort
				);

				// Change status
				ChangeStatus(Status.Close | Status.Done);

				// Raise event INVALID ADDRESS
				Network.Raise(
					EventType.Failed,
					Failure.InvalidAddress,
					this
				);
			}
		}


		/// <summary>Check status</summary>
		/// <param name="status">Status</param>
		private bool CheckStatus(Status status)
		{return (_Status & status) == status;}

		/// <summary>Changes the status</summary>
		/// <param name="status">Status</param>
		private void ChangeStatus(Status status)
		{_Status = status;}

		/// <summary>Combines the status</summary>
		/// <param name="status">Status</param>
		private void CombineStatus(Status status)
		{_Status |= status;}

		/// <summary>Clears the status</summary>
		/// <param name="status">Status</param>
		private void ClearStatus(Status status)
		{_Status &= ~status;}
	};
};