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

using System.Runtime.Serialization;
using System;


namespace NetworkKit.Networking {
	/// <summary>
	/// Network Exception
	/// </summary>
	[Serializable]
	public class NetworkException : Exception {
		/// <summary>
		/// Network Exception
		/// </summary>
		public NetworkException(){}

		/// <summary>Network Exception</summary>
		/// <param name="exception">Exception</param>
		public NetworkException(string exception) :
			base(exception)
		{}

		/// <summary>Network Exception</summary>
		/// <param name="exception">Exception</param>
		/// <param name="inner">Inner</param>
		public NetworkException(string exception, Exception inner) :
			base(exception, inner)
		{}


		/// <summary>Network Exception</summary>
		/// <param name="info">Information</param>
		/// <param name="context">Context</param>
		protected NetworkException(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{}
	};
};