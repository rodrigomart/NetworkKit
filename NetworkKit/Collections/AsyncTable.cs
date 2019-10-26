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

using System.Collections.Generic;
using System.Collections;
using System;


namespace NetworkKit.Collections {
	/// <summary>Asynchronous table</summary>
	/// <typeparam name="KEY">Key type</typeparam>
	/// <typeparam name="ITEM">Item type</typeparam>
	public sealed class AsyncTable<KEY, ITEM> : IEnumerable<ITEM> {
		/// <summary>Entry</summary>
		private struct Entry {
			/// <summary>Item</summary>
			public ITEM Item;

			/// <summary>Key</summary>
			public KEY  Key;

			/// <summary>Hash</summary>
			public int  Hash;

			/// <summary>Next</summary>
			public int  Next;
		};


		/// <summary>Enumerator</summary>
		public class Enumerator : IEnumerator<ITEM>, IEnumerator {
			/// <summary>Table</summary>
			private readonly AsyncTable<KEY, ITEM> Table;


			/// <summary>Current item</summary>
			private ITEM CurrentItem;

			/// <summary>Index</summary>
			private int Index;


			/// <summary>Get current</summary>
			object IEnumerator.Current {
				get {return CurrentItem;}
			}

			/// <summary>Get current</summary>
			ITEM IEnumerator<ITEM>.Current {
				get {return CurrentItem;}
			}


			/// <summary>Enumerator</summary>
			/// <param name="table">Table</param>
			public Enumerator(AsyncTable<KEY, ITEM> table){
				Table = table;
				Index = -1;
			}


			/// <summary>Dispose</summary>
			void IDisposable.Dispose(){}


			/// <summary>Move next</summary>
			/// <returns>True if move next</returns>
			bool IEnumerator.MoveNext(){
				if(Table.Counter == 0)
				return false;

				if(Index < 0) Index = 0;
				else Index++;

				lock(Table.SyncLock){
					if(Table.Entries.Length <= Index)
					return false;

					// Next valid index
					for(; Index < Table.Entries.Length; Index++){
						if(
							Table.Entries[Index].Hash >= 0 &&
							Table.Entries[Index].Item != null
						){
							// Redeem here to avoid loss of reference
							CurrentItem = Table.Entries[Index].Item;
							return true;
						}
					}

					return false;
				}
			}

			/// <summary>Reset</summary>
			void IEnumerator.Reset()
			{Index = -1;}
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

		/// <summary>Counter</summary>
		private int Counter;


		/// <summary>Is empty</summary>
		public bool IsEmpty {
			get {return ((Counter - FreeKeys) <= 0);}
		}

		/// <summary>Count</summary>
		public int Count {
			get {return (Counter - FreeKeys);}
		}


		/// <summary>Gets or sets</summary>
		/// <returns>Item</returns>
		/// <param name="key">Key</param>
		public ITEM this[KEY key] {
			get {return Getter(key);}
			set {Update(key, value);}
		}


		/// <summary>Table</summary>
		public AsyncTable(){
			// Sync lock
			SyncLock = new object();

			// Entries
			Entries = new Entry[23];
			for(int i = 0; i < Entries.Length; i++)
			Entries[i].Hash = -1;

			// Buckets
			Buckets = new int[23];
			for(int i = 0; i < Buckets.Length; i++)
			Buckets[i] = -1;

			// Free list
			FreeList = -1;
		}


		/// <summary>Clear</summary>
		public void Clear(){
			if(Counter <= 0) return;

			lock(SyncLock){
				// Entries
				for(int i = 0; i < Entries.Length; i++){
					Entries[i].Item = default(ITEM);
					Entries[i].Key  = default(KEY);
					Entries[i].Hash = -1;
				}

				// Buckets
				for(int i = 0; i < Buckets.Length; i++)
				Buckets[i] = -1;

				// Clear 
				FreeKeys =  0;
				FreeList = -1;
				Counter  =  0;
			}
		}


		/// <summary>Getter</summary>
		/// <returns>Item</returns>
		/// <param name="key">Key</param>
		public ITEM Getter(KEY key){
			lock(SyncLock){
				int i = FindIndex(key);

				// Not found
				if(i < 0) return default(ITEM);

				return Entries[i].Item;
			}
		}

		/// <summary>Setter</summary>
		/// <param name="key">Key</param>
		/// <param name="item">Item</param>
		public void Setter(KEY key, ITEM item){
			if(key == null)
			throw new ArgumentNullException(nameof(key));

			lock(SyncLock){
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
					if(Counter == Entries.Length){
						Resize();

						// New bucket index
						bucket = hash % Buckets.Length;
					}

					index = Counter;
					Counter++;
				}

				// Set
				Entries[index].Hash = hash;
				Entries[index].Next = Buckets[bucket];
				Entries[index].Item = item;
				Entries[index].Key  = key;

				// Bucket index
				Buckets[bucket] = index;
			}
		}

		/// <summary>Update</summary>
		/// <param name="key">Key</param>
		/// <param name="item">Item</param>
		public void Update(KEY key, ITEM item){
			lock(SyncLock){
				int i = FindIndex(key);

				// Not found
				if(i < 0){
					Setter(key, item);
					return;
				}

				// Change
				Entries[i].Item = item;
			}
		}

		/// <summary>Remove</summary>
		/// <param name="key">Key</param>
		public void Remove(KEY key){
			if(key == null)
			throw new ArgumentNullException(nameof(key));

			lock(SyncLock){
				// Positive hash
				var hash = key.GetHashCode() & 2147483647;

				// Bucket
				var bucket = hash % Buckets.Length;

				// Last key
				int last = -1;

				// Find entry
				for(int i = Buckets[bucket]; i >= 0; last = i, i = Entries[i].Next){
					if(
						Entries[i].Hash == hash &&
						Entries[i].Key.Equals(key)
					){
						if(last < 0) Buckets[bucket] = Entries[i].Next;
						else Entries[last].Next = Entries[i].Next;

						Entries[i].Hash = -1;
						Entries[i].Next = FreeList;
						Entries[i].Item = default(ITEM);
						Entries[i].Key  = default(KEY);

						FreeList = i;
						FreeKeys++;

						return;
					}
				}
			}
		}


		/// <summary>Contains</summary>
		/// <returns>True if it contains</returns>
		/// <param name="key">Key</param>
		public bool Contains(KEY key){
			lock(SyncLock)
			{return (FindIndex(key) >= 0);}
		}


		/// <summary>Find index </summary>
		/// <returns>Index</returns>
		/// <param name="key">Key</param>
		private int FindIndex(KEY key){
			if(key == null)
			throw new ArgumentNullException(nameof(key));

			// Positive Hash
			var hash = key.GetHashCode() & 2147483647;

			// Bucket
			var bucket = hash % Buckets.Length;

			// Search the entry index
			for(int i = Buckets[bucket]; i >= 0; i = Entries[i].Next){
				if(
					Entries[i].Hash == hash &&
					Entries[i].Key.Equals(key)
				) return i;
			}

			// Not found
			return -1;
		}

		/// <summary>Resize</summary>
		private void Resize(){
			// Calculate the new size
			var size = Helper.NextPrime(Counter + 20);

			// New Entries
			Entry[] newEntries = new Entry[size];
			Array.Copy(Entries, 0, newEntries, 0, Counter);

			// New Buckets
			int[] newBuckets = new int[size];
			for(int i = 0; i < newBuckets.Length; i++)
			newBuckets[i] = -1;

			// Re-indexing
			for(int i = 0; i < Counter; i++){
				if(newEntries[i].Hash >= 0){
					int bucket = newEntries[i].Hash % size;

					newEntries[i].Next = newBuckets[bucket];
					newBuckets[bucket] = i;
				}
			}

			// Storage exchange
			Buckets = newBuckets;
			Entries = newEntries;
		}


		#region IENUMERABLE
		/// <summary>Get enumerator</summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{return new Enumerator(this);}

		/// <summary>Get enumerator</summary>
		/// <returns>Enumerator</returns>
		IEnumerator<ITEM> IEnumerable<ITEM>.GetEnumerator()
		{return new Enumerator(this);}
		#endregion
	};
};