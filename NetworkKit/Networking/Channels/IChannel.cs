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
	/// <summary>In/Out Handler Delegate</summary>
	internal delegate void InOutHandler(Content content);


	/// <summary>
	/// Interface for Channel
	/// </summary>
	internal interface IChannel {
		/// <summary>Released</summary>
		bool Released();

		/// <summary>Muted</summary>
		bool Muted();

		/// <summary>Reset</summary>
		void Reset();

		/// <summary>Inlet</summary>
		/// <param name="content">Content</param>
		void Inlet(Content content);

		/// <summary>Outlet</summary>
		/// <param name="content">Content</param>
		void Outlet(Content content);

		/// <summary>Timeout</summary>
		/// <param name="time">Time</param>
		void Timeout(uint time);
	};
};