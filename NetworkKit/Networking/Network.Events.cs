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
	using Collections;
	using Events;


	/// <summary>Network</summary>
	public sealed partial class Network {
		/// <summary>Events</summary>
		readonly AsyncQueue<RaiseEvent> Events;


		/// <summary>Occurs when on failed</summary>
		public event FailureHandler OnFailed;

		/// <summary>Occurs when on unlinked</summary>
		public event ReasonHandler  OnUnlinked;


		/// <summary>Occurs when on content</summary>
		public event ContentHandler OnContent;

		/// <summary>Occurs when on approval</summary>
		public event ContentHandler OnApproval;


		/// <summary>Occurs when on redirected</summary>
		public event LinkHandler OnRedirected;

		/// <summary>Occurs when on redirect</summary>
		public event LinkHandler OnRedirect;

		/// <summary>Occurs when on linked</summary>
		public event LinkHandler OnLinked;


		/// <summary>Occurs when on stopped</summary>
		public event StateHandler OnStopped;

		/// <summary>Occurs when on started</summary>
		public event StateHandler OnStarted;


		/// <summary>Events</summary>
		public void Event(){
			RaiseEvent raiseEvent;
			while(Events.TryDequeue(out raiseEvent)){
				// Raise start event
				if(raiseEvent.EventType == EventType.Started)
				OnStarted?.Invoke();

				// Raise stop event
				if(raiseEvent.EventType == EventType.Stopped)
				OnStopped?.Invoke();

				// Raise link event
				if(raiseEvent.EventType == EventType.Linked)
				OnLinked?.Invoke(raiseEvent.Link);

				// Raise redirection event
				if(raiseEvent.EventType == EventType.Redirect)
				OnRedirect?.Invoke(raiseEvent.Link);

				// Raise post redirection event
				if(raiseEvent.EventType == EventType.Redirected)
				OnRedirected?.Invoke(raiseEvent.Link);

				// Raise approval event
				if(raiseEvent.EventType == EventType.Approval)
				OnApproval?.Invoke(raiseEvent.Link, raiseEvent.Content);

				// Raise content event
				if(raiseEvent.EventType == EventType.Content)
				OnContent?.Invoke(raiseEvent.Link, raiseEvent.Content);

				// Raise unlink event
				if(raiseEvent.EventType == EventType.Unlinked)
				OnUnlinked?.Invoke(raiseEvent.Link, raiseEvent.Reason);

				// Raise failure event
				if(raiseEvent.EventType == EventType.Failed)
				OnFailed?.Invoke(raiseEvent.Link, raiseEvent.Failure);
			}
		}


		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		internal void Raise(EventType type){
			var raiseEvent = default(RaiseEvent);
			raiseEvent.EventType = type;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		internal void Raise(EventType type, Link link){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.EventType = type;
			raiseEvent.Link      = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="reason">Razão</param>
		/// <param name="link">Link</param>
		internal void Raise(EventType type, Reason reason, Link link){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.Reason    = reason;
			raiseEvent.EventType = type;
			raiseEvent.Link      = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="failure">Falha</param>
		/// <param name="link">Link</param>
		internal void Raise(EventType type, Failure failure, Link link){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.Failure   = failure;
			raiseEvent.EventType = type;
			raiseEvent.Link      = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="type">Event type</param>
		/// <param name="link">Link</param>
		/// <param name="content">Content</param>
		internal void Raise(EventType type, Link link, Content content){
			var raiseEvent = default(RaiseEvent);

			raiseEvent.EventType = type;
			raiseEvent.Content   = content;
			raiseEvent.Link      = link;

			Raise(raiseEvent);
		}

		/// <summary>Raise event</summary>
		/// <param name="raiseEvent">Evento</param>
		internal void Raise(RaiseEvent raiseEvent)
		{Events.Enqueue(raiseEvent);}
	};
};