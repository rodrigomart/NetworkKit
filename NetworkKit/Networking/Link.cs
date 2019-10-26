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

using System.Net;


namespace NetworkKit.Networking {
	using Events;


	/// <summary>Link</summary>
	public sealed partial class Link {
		/// <summary>Statistics</summary>
		public Statistics Statistics {
			private set;
			get;
		}

		/// <summary>Is redirecting</summary>
		public bool Redirecting {
			get {return CheckStatus(Status.Redirect | Status.Connect);}
		}

		/// <summary>Is closed</summary>
		public bool Closed {
			get {return CheckStatus(Status.Close | Status.Done);}
		}

		/// <summary>Address</summary>
		public string Address {
			get {return _EndPoint.ToString();}
		}

		/// <summary>Latency</summary>
		public uint Latency {
			get {return LatencyMeasured;}
		}


		/// <summary>Accept</summary>
		public void Accept(){
			if(!CheckStatus(Status.Connect | Status.Wait)) return;

			// Change status
			ChangeStatus(Status.Connect | Status.Done);

			// Content ACCEPTED
			var content = Content.Reliable();
			content.Protocol = Content.ACCEPTED;

			// Raise event
			Network.Raise(EventType.Linked, this);

			// Send
			Outlet(content);

			// Statistics
			Network.Statistics.IncreaseLink();
		}

		/// <summary>Deny</summary>
		public void Deny(){
			if(!CheckStatus(Status.Connect | Status.Wait)) return;

			// Content DENIED
			var content = Content.Reliable();
			content.Protocol = Content.DENIED;

			// Send
			Outlet(content);

			// Change status
			ChangeStatus(Status.Close | Status.Wait);
		}


		/// <summary>Redirect to address</summary>
		/// <param name="address">Address</param>
		public void Redirect(string address){
			if(CheckStatus(Status.Close)) return;

			// Content REDIRECT
			var content = Content.Reliable();
			content.Protocol = Content.REDIRECT;

			// Write address
			content.WriteString(address);

			// Send
			Outlet(content);

			// Change status
			ChangeStatus(Status.Redirect | Status.Close | Status.Wait);
		}


		/// <summary>Unlink</summary>
		public void Unlink(){
			if(CheckStatus(Status.Close)) return;

			// Content UNLINK
			var content = Content.Reliable();
			content.Protocol = Content.UNLINK;

			// Outlet
			Outlet(content);

			// Change status
			ChangeStatus(Status.Close | Status.Wait);
		}


		/// <summary>Send content</summary>
		/// <param name="content">Content</param>
		public void Send(Content content){
			if(CheckStatus(Status.Close)) return;

			Outlet(content);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// STATIC IMPLEMENTATION
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////

		/// <summary>Cast to EndPoint</summary>
		/// <param name="link">Link</param>
		public static implicit operator EndPoint(Link link)
		{return link._EndPoint;}
	};
};