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

using System;


namespace NetworkKit.Collections {
	/// <summary>Asynchronous stack</summary>
	/// <typeparam name="ITEM">Item type</typeparam>
	public sealed class AsyncStack<ITEM> {
		/// <summary>Node</summary>
		private class Node {
			/// <summary>Item</summary>
			public object Item;

			/// <summary>Next</summary>
			public Node Next;
		};


		/// <summary>Sync lock</summary>
		private readonly object SyncLock;


		/// <summary>Head stack</summary>
		private Node Head;

		/// <summary>Number of items</summary>
		private int Items;


		/// <summary>Is empty</summary>
		public bool IsEmpty {
			get {return (Items == 0);}
		}

		/// <summary>Count</summary>
		public int Count {
			get {return Items;}
		}


		/// <summary>
		/// Asynchronous stack
		/// </summary>
		public AsyncStack(){
			// Sync lock
			SyncLock = new object();

			Head = null;
		}


		/// <summary>Clear the stack</summary>
		public void Clear(){
			lock(SyncLock){
				Head = null;
				Items = 0;
			}
		}

		/// <summary>Push to the stack</summary>
		/// <param name="item">Item</param>
		public void Push(ITEM item){
			if(item == null) throw new ArgumentNullException(nameof(item));

			lock(SyncLock){
				var node = new Node();
				node.Item = item;
				node.Next = Head;

				Head = node;

				Items++;
			}
		}

		/// <summary>Remove from stack</summary>
		/// <returns>Item</returns>
		public ITEM Pop(){
			ITEM item;
			TryPop(out item);
			return item;
		}

		/// <summary>Get an item without removing</summary>
		/// <returns>Item</returns>
		public ITEM Peek(){
			ITEM item;
			TryPeek(out item);
			return item;
		}

		/// <summary>Remove from stack</summary>
		/// <param name="item">Item</param>
		/// <returns>True if there is an item</returns>
		public bool TryPop(out ITEM item){
			lock(SyncLock){
				// If there are items in the stack
				if(Head != null){
					item = (ITEM)Head.Item;
					Head = Head.Next;

					Items--;
					return true;
				}

				// Stack is empty
				else item = default(ITEM);
			}

			return false;
		}

		/// <summary>Try to pick up an item without removing</summary>
		/// <param name="item">Item</param>
		/// <returns>True if there is an item</returns>
		public bool TryPeek(out ITEM item){
			lock(SyncLock){
				// If there are items in the stack
				if(Head != null){
					item = (ITEM)Head.Item;
					return true;
				}

				// Stack is empty
				else item = default(ITEM);
			}

			return false;
		}
	};
};