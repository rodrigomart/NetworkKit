/// CONTAINERS IMPLEMENTATION
namespace NetworkKit.Containers {
	/// <summary>
	/// Container Table
	/// </summary>
	public class Table<ITEM> {
		/// <summary>Entry</summary>
		private sealed class Entry {
			/// <summary>Hash Code</summary>
			public int Hash;

			/// <summary>Previous</summary>
			public int Prev;

			/// <summary>Next</summary>
			public int Next;

			/// <summary>Item</summary>
			public ITEM Item;
		};


		/// <summary>IEnumerator</summary>
		public sealed class IEnumerator {
			/// <summary>Container Table</summary>
			private readonly Table<ITEM> Table;

			/// <summary>Index</summary>
			private int Index;

			/// <summary>Item</summary>
			private ITEM Item;


			/// <summary>Current entry</summary>
			public object Current {
				// Redeem without losing reference
				get {return Item;}
			}

			/// <summary>Constructor</summary>
			/// <param name="table">Table</param>
			public IEnumerator(Table<ITEM> table){
				this.Table = table;
				Index = -1;
			}


			/// <summary>Reset</summary>
			public void Reset(){
				lock(this.Table.SyncLock)
				{Index %= this.Table.Entries.Length;}
			}

			/// <summary>Next entry</summary>
			/// <returns>True if not the last</returns>
			public bool MoveNext(){
				lock(this.Table.SyncLock){
					if(this.Table.NumOfEntries <= 0)
					return false;

					// Next index
					if(Index < 0) Index = 0;
					else Index++;

					// Index out of bounds
					if(this.Table.Entries.Length <= Index)
					return false;

					// Next valid index
					for(; Index < this.Table.Entries.Length; Index++)
					if(this.Table.Entries[Index] != null){
						// Redeem here to avoid reference loss
						Item = this.Table.Entries[Index].Item;
						return true;
					}
				}

				return false;
			}
		};


		/// <summary>Synchronization lock</summary>
		private readonly object SyncLock = new object();


		/// <summary>Entries</summary>
		private Entry[] Entries;

		/// <summary>Resize items</summary>
		private bool ResizeItems;

		/// <summary>Number of Entries</summary>
		private int NumOfEntries;


		/// <summary>Queue is resizable</summary>
		public virtual bool Resizable {
			set {
				lock(SyncLock)
				{ResizeItems = value;}
			}
			get {
				var resize = false;
				lock(SyncLock)
				{resize = ResizeItems;}
				return resize;
			}
		}


		/// <summary>Count itens</summary>
		public virtual int Count {
			get {
				var count = 0;
				lock(SyncLock)
				{count = NumOfEntries;}
				return count;
			}
		}


		/// <summary>
		/// Container Queue
		/// </summary>
		public Table() :
			this(23)
		{}

		/// <summary> Containers Queue</summary>
		/// <param name="size">Size</param>
		public Table(int size){
			// Next prime
			size = Helper.NextPrime(size);

			// Entries
			Entries = new Entry[size];
		}


		/// <summary>Get enumerator</summary>
		/// <returns>Enumerator</returns>
		public virtual IEnumerator GetEnumerator()
		{return new IEnumerator(this);}


		/// <summary>Add item</summary>
		/// <param name="item">Item</param>
		public virtual void Add(ITEM item){
			lock(SyncLock){
				// Resize if resizable
				if(
					ResizeItems &&
					NumOfEntries == Entries.Length
				){
					// Next size
					var size = Helper.NextPrime(Entries.Length);

					// New matrix
					Entry[] newEntries = new Entry[size];
					System.Array.Copy(Entries, newEntries, NumOfEntries);

					// Moves pairs by recalculating indexes
					for(int index = 0; index < Entries.Length; index++){
						if(Entries[index] == null) continue;
						InternalAdd(Entries[index].Item);
					}

					Entries = newEntries;
				}

				// Overloaded table
				if(NumOfEntries == Entries.Length)
				throw new System.OverflowException("Table overflow");

				InternalAdd(item);
			}
		}


		/// <summary>Clear</summary>
		public virtual void Clear(){
			if(NumOfEntries <= 0) return;

			lock(SyncLock){
				// Removes all entries
				for(int index = 0; index < Entries.Length; index++)
					if(Entries[index] != null){
						Entries[index].Item = default(ITEM);
						Entries[index] = null;
						NumOfEntries--;
					}
			}
		}


		/// <summary>Find item</summary>
		/// <param name="item">Item</param>
		/// <returns>Item or null</returns>
		public virtual ITEM Find(object item){
			if(NumOfEntries <= 0) return default(ITEM);

			ITEM value = default(ITEM);

			lock(SyncLock){
				// Valid hash
				var hash = item.GetHashCode() & 2147483647;

				// Index
				var index = hash % Entries.Length;

				// There are items in the index
				if(Entries[index] != null){
					// Checks whether or if it belongs to the same index group
					if((Entries[index].Hash % Entries.Length) == index){
						// Interaction between successors
						while(index > -1){
							// Go to the next one if it is not the item
							if(!Entries[index].Item.Equals(item)){
								index = Entries[index].Next;
								continue;
							}

							// Returns the searched item
							value = Entries[index].Item;
							break;
						}
					}
				}
			}

			return value;
		}


		/// <summary>Contains item</summary>
		/// <param name="item">Item</param>
		public virtual bool Contains(object item){
			object obj = Find(item);
			return !(obj == null);
		}


		/// <summary>Remove item</summary>
		/// <param name="item">Item</param>
		public virtual void Remove(ITEM item){
			lock (SyncLock)
			{InternalRemove(item);}
		}


		/// <summary>Internal add item</summary>
		/// <param name="item">Item</param>
		protected void InternalAdd(ITEM item){
			// Next entry
			var next = -1;

			// hash
			var hash = item.GetHashCode() & 2147483647;

			// Index
			var index = hash % Entries.Length;

			// Overlap Entries
			if(Entries[index] != null){
				// Reallocation of Entries
				var reindex = 0;
				while(Entries[reindex] != null)
				reindex++;

				// Overlay another Entries
				if(Entries[index].Hash % Entries.Length != index){
					// Move to a new index
					Entries[reindex] = Entries[index];

					// Change the connection rate
					var prev = Entries[reindex].Prev;
					Entries[prev].Next = reindex;
				}

				// Overlap Entries
				else {
					// Move to a new index
					Entries[reindex] = Entries[index];

					// Change the connection rate
					Entries[reindex].Prev = index;

					next = reindex;
				}
			}

			// Register
			Entries[index] = new Entry();
			Entries[index].Item = item;
			Entries[index].Prev = -1;
			Entries[index].Next = next;
			Entries[index].Hash = hash;

			// Number os Entries
			NumOfEntries++;
		}

		/// <summary>Internal remove item</summary>
		/// <param name="item">item</param>
		protected void InternalRemove(object item){
			// Hash
			var hash = item.GetHashCode() & 2147483647;

			// Index
			var index = hash % Entries.Length;

			// Nonexistent index
			if(Entries[index] == null) return;

			// Index occupied by another entry
			// Usually this entry does not belong to the same index group
			if((Entries[index].Hash % Entries.Length) != index) return;

			// Links of indices
			var prev = Entries[index].Prev;
			var next = Entries[index].Next;

			// Entry with successors
			if(next > -1){
				// Interaction between successors
				while(next > -1){
					// Next interaction
					if(Entries[index].Hash != hash){
						prev = Entries[next].Prev;
						next = Entries[next].Next;
						index = next;
						continue;
					}

					// First index
					if(prev == -1){
						// Remove the entry
						Entries[index].Item = default(ITEM);
						Entries[index] = null;

						// Move the successor
						Entries[index] = Entries[next];
						Entries[index].Prev = -1;

						// Cancels the successor entry
						Entries[next] = null;

						// Change the connection rate
						next = Entries[index].Next;
						if (next > -1) Entries[next].Prev = index;
					}

					// Intermediate entry
					else {
						// Change the connection rate
						Entries[prev].Next = next;
						Entries[next].Prev = prev;

						// Remove the entry
						Entries[index].Item = default(ITEM);
						Entries[index] = null;
					}

					// Number
					NumOfEntries--;

					break;
				}
			}

			// Last installment or only
			else {
				// Different entry
				if(Entries[index].Hash != hash) return;

				// Changes the predecessor's connection
				if(prev > -1) Entries[prev].Next = -1;

				// Remove the entry
				Entries[index].Item = default(ITEM);
				Entries[index] = null;

				// Number
				NumOfEntries--;
			}
		}
	};
};