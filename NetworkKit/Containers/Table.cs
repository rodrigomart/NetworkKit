// MIT License
//
// Copyright (c) 2018 Rodrigo Martins 
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


namespace NetworkKit.Containers {
	/// <summary>
	/// Asynchronous table
	/// </summary>
	public partial class Table<KEY, ITEM> {
		/// <summary>Entry</summary>
		private struct Entry {
			/// <summary>Item</summary>
			public ITEM Item;

			/// <summary>Key</summary>
			public KEY Key;


			/// <summary>Hash</summary>
			public int Hash;

			/// <summary>Next</summary>
			public int Next;
		};


		/// <summary>Sync lock</summary>
		private readonly object SyncLock = new object();


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
		public int Count {
			get {return (Keys - FreeKeys);}
		}


		/// <summary>
		/// Asynchronous table
		/// </summary>
		public Table(){
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


		/// <summary>Add an item to a key</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
		/// <param name="item">Item <typeparamref name="ITEM"/></param>
		public void Add(KEY key, ITEM item){
			lock(SyncLock)
			{InternalAdd(key, item);}
		}

		/// <summary>Clear the table</summary>
		public void Clear(){
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

		/// <summary>Checks if it contains the key</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
		/// <returns>True if it contains the item</returns>
		public bool Contains(KEY key)
		{return InternalFind(key) >= 0;}

		/// <summary>Search for an item by key</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
		/// <returns>Item <typeparamref name="ITEM"/></returns>
		public ITEM Find(KEY key){
			lock(SyncLock){
				int i = InternalFind(key);
				if(i >= 0) return Entries[i].Item;
			}

			return default(ITEM);
		}

		/// <summary>Remove item with key</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
		public void Remove(KEY key){
			lock(SyncLock)
			{InternalRemove(key);}
		}


		/// <summary>Internal Addition</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
		/// <param name="item">Item <typeparamref name="ITEM"/></param>
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
			} else {
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
			Entries[index].Item = (ITEM)item;
			Entries[index].Key  = (KEY)key;

			// Bucket index
			Buckets[bucket] = index;
		}

		/// <summary>Internal Search</summary>
		/// <param name="key">Key <typeparamref name="KEY"/></param>
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
		/// <param name="key">Key <typeparamref name="KEY"/></param>
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
					Entries[i].Item = default(ITEM);
					Entries[i].Key = default(KEY);

					FreeList = i;
					FreeKeys++;

					return;
				}
			}
		}

		/// <summary>Internal resizing</summary>
		private void InternalResize(){
			// Calculate the new size
			var size = (int)(Keys * 0.5f);
			size = Helper.NextPrime(Keys + size);

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