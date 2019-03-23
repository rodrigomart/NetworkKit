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

namespace NetworkKit.Networking {
	/// <summary>
	/// Settings
	/// </summary>
	public sealed class Settings {
		/// <summary>
		/// Encrypts the data to be transmitted.
		/// The encryption key is unique for each connection and exchanged through the Diffie-Hellman protocol.
		/// </summary>
		public bool EncryptData = true;

		/// <summary>
		/// Use the events in steps.
		/// When <c>true</c> the <see cref="Network.Step()" /> method must be called to process network events.
		/// </summary>
		public bool UseStepEvents = true;

		/// <summary>
		/// Requires approval.
		/// When <c>true</c> the <see cref="Network.OnApproval" /> event is called for a connection to be approved,
		/// if <c>False</c> all connections will be approved without passing through.
		/// </summary>
		public bool RequiresApproval = true;

		/// <summary>
		/// Input buffer size in bytes.
		/// The default size is 512 Kilobytes.
		/// </summary>
		public uint ReceiveBuffer = 524288u; // (512 Kilobytes)

		/// <summary>
		/// Send buffer size in bytes.
		/// The standard size is 128 Kilobytes.
		/// </summary>
		public uint SendBuffer = 131072u; // (128 Kilobytes)

		/// <summary>
		/// Ping interval in milliseconds.
		/// The time is given in milliseconds and the default is 2.5 seconds.
		/// </summary>
		public uint PingInterval = 2500u;

		/// <summary>
		/// Downtime.
		/// The time is given in milliseconds and the default is 10 seconds.
		/// </summary>
		public uint Downtime = 10000u;

		/// <summary>
		/// Maximum number of links.
		/// Use 0 to prevent it from receiving new connections, the default amount is 25 links.
		/// </summary>
		public uint MaxLinks = 25u;

		/// <summary>
		/// Maximum transmission unit in bytes.
		/// The default size is 1500 bytes.
		/// </summary>
		public uint MTU = 1500u;
	};
};