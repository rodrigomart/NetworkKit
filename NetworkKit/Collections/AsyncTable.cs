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
	/// <summary>
	/// Asynchronous table
	/// </summary>
	public partial class AsyncTable<KEY, ITEM> {
		/// <summary>Entry</summary>
		private struct Entry {
			/// <summary>Item</summary>
			public object Item;

			/// <summary>Key</summary>
			public object Key;


			/// <summary>Hash</summary>
			public int Hash;

			/// <summary>Next</summary>
			public int Next;
		};


		/// <summary>Sync lock</summary>
		private readonly object SyncLock;


		/// <summary>Entries</summary>
		private Entry[] Entries;

		/// <summary>Buckets</summary>
		private int[] Buckets;

		/// <summary>Free keys</summary>
		private int FreeKeys;

		/// <summary>Free list</summary>
		private int FreeList;

		/// <summary>Keys</summary>
		private int Keys;


		/// <summary>Is empty</summary>
		public virtual bool IsEmpty {
			get {return ((Keys - FreeKeys) <= 0);}
		}

		/// <summary>Count</summary>
		public virtual int Count {
			get {return (Keys - FreeKeys);}
		}


		/// <summary>
		/// Asynchronous Table
		/// </summary>
		public AsyncTable(){
			// Sync lock
			SyncLock = new object();

			// Buckets
			Buckets = new int[23];
			for(int i = 0; i < Buckets.Length; i++)
			Buckets[i] = -1;

			// Entries
			Entries = new Entry[23];
			for(int i = 0; i < Entries.Length; i++)
			Entries[i].Hash = -1;

			// Empty list
			FreeList = -1;
		}


		/// <summary>Clear</summary>
		public virtual void Clear(){
			if(Keys <= 0) return;

			lock(SyncLock){
				for(int i = 0; i < Buckets.Length; i++)
				Buckets[i] = -1;

				Array.Clear(Entries, 0, Keys);
				Entries = new Entry[23];
				for(int i = 0; i < Entries.Length; i++)
				Entries[i].Hash = -1;

				FreeKeys = 0;
				FreeList = -1;
				Keys = 0;
			}
		}


		/// <summary>Add item to a key</summary>
		/// <param name="key">Key</param>
		/// <param name="item">Item</param>
		public virtual void Add(KEY key, ITEM item){
			lock(SyncLock)
			{InternalAdd(key, item);}
		}

		/// <summary>Contains item</summary>
		/// <param name="key">Key</param>
		/// <returns>True if it contains the item</returns>
		public virtual bool Contains(KEY key){
			lock(SyncLock)
			{return InternalFind(key) >= 0;}
		}

		/// <summary>Remove item</summary>
		/// <param name="key">Key</param>
		public virtual void Remove(KEY key){
			lock(SyncLock)
			{InternalRemove(key);}
		}


		/// <summary>Internal Addition</summary>
		/// <param name="key">Key</param>
		/// <param name="item">Item</param>
		private void InternalAdd(object key, object item){
			if(key == null) throw new ArgumentNullException(nameof(key));

			// Positive Hash
			var hash = key.GetHashCode() & 2147483647;

			// Bucket
			var bucket = hash % Buckets.Length;

			// Duplicate entry verification
			for(int i = Buckets[bucket]; i >= 0; i = Entries[i].Next){
				if(
					Entries[i].Hash == hash &&
					Entries[i].Key.Equals(key)
				) return;
			}

			// Index
			int index;
			if(FreeKeys > 0){
				index = FreeList;
				FreeList = Entries[index].Next;
				FreeKeys--;
			}
			else {
				// Resizes storage
				if(Keys == Entries.Length){
					InternalResize();

					// New bucket index
					bucket = hash % Buckets.Length;
				}

				index = Keys;
				Keys++;
			}

			// Entry
			Entries[index].Hash = hash;
			Entries[index].Next = Buckets[bucket];
			Entries[index].Item = item;
			Entries[index].Key  = key;

			// Bucket index
			Buckets[bucket] = index;
		}

		/// <summary>Internal Search</summary>
		/// <param name="key">Key</param>
		/// <returns>Index entry</returns>
		private int InternalFind(object key){
			if(key == null) throw new ArgumentNullException(nameof(key));

			// Positive Hash
			var hash = key.GetHashCode() & 2147483647;

			// Bucket
			var bucket = hash % Buckets.Length;

			// Search the entry index
			for(int i = Buckets[bucket]; i >= 0; i = Entries[i].Next)
			if(Entries[i].Hash == hash && Entries[i].Key.Equals(key)) return i;

			// Not found
			return -1;
		}

		/// <summary>Internal removal</summary>
		/// <param name="key">Key</param>
		private void InternalRemove(object key){
			if(key == null) throw new ArgumentNullException(nameof(key));

			// Positive hash
			var hash = key.GetHashCode() & 2147483647;

			// Bucket
			var bucket = hash % Buckets.Length;

			// Last key
			int last = -1;

			// Find entry
			for(int i = Buckets[bucket]; i >= 0; last = i, i = Entries[i].Next){
				if(Entries[i].Hash == hash && Entries[i].Key.Equals(key)){
					if(last < 0) Buckets[bucket] = Entries[i].Next;
					else Entries[last].Next = Entries[i].Next;

					Entries[i].Hash = -1;
					Entries[i].Next = FreeList;
					Entries[i].Item = null;
					Entries[i].Key  = null;

					FreeList = i;
					FreeKeys++;

					return;
				}
			}
		}

		/// <summary>Internal resizing</summary>
		private void InternalResize(){
			// Calculate the new size
			var size = Prime.Next(Keys + 32);

			// New Buckets
			int[] newBuckets = new int[size];
			for(int i = 0; i < newBuckets.Length; i++) newBuckets[i] = -1;

			// New entries
			Entry[] newEntries = new Entry[size];
			Array.Copy(Entries, 0, newEntries, 0, Keys);

			// Re-indexing
			for(int i = 0; i < Keys; i++){
				if(newEntries[i].Hash >= 0){
					int bucket = newEntries[i].Hash % size;

					newEntries[i].Next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}

			// Storage Exchange
			Buckets = newBuckets;
			Entries = newEntries;
		}
	};
};