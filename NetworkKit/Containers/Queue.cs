/// CONTAINERS IMPLEMENTATION
namespace NetworkKit.Containers {
	/// <summary>
	/// Container Queue
	/// </summary>
	public class Queue<ITEM> {
		/// <summary>Synchronization lock</summary>
		private readonly object SyncLock = new object();


		/// <summary>Items</summary>
		private ITEM[] Items;

		/// <summary>Resize items</summary>
		private bool ResizeItems;

		/// <summary>Enqueue point</summary>
		private int EnqueuePoint;

		/// <summary>Dequeue point</summary>
		private int DequeuePoint;

		/// <summary>Number of items</summary>
		private int NumOfItems;


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
				{count = NumOfItems;}
				return count;
			}
		}


		/// <summary>
		/// Container Queue
		/// </summary>
		public Queue():
			this(23)
		{}

		/// <summary>Containers Queue</summary>
		/// <param name="size">Size</param>
		public Queue(int size){
			size = Helper.NextPrime(size);

			Items = new ITEM[size];
		}


		/// <summary>Clear</summary>
		public virtual void Clear(){
			// Reset pointers
			EnqueuePoint = 0;
			DequeuePoint = 0;

			if(NumOfItems <= 0) return;

			lock(SyncLock)
			{System.Array.Clear(Items, 0, Items.Length);}
		}


		/// <summary>Contains item</summary>
		/// <param name="item">Item</param>
		/// <returns>boolean</returns>
		public virtual bool Contains(object item){
			if(NumOfItems <= 0) return false;

			var value = false;

			lock(SyncLock){
				var i = 0;
				while(i < NumOfItems){
					// Peek point
					var point = (DequeuePoint + i) % Items.Length;

					// Has item
					if(Items[point].Equals(item)){
						value = true;
						break;
					}

					i++;
				}
			}

			return value;
		}


		/// <summary>Enqueue item</summary>
		/// <param name="item">Item</param>
		public virtual void Enqueue(ITEM item){
			lock(SyncLock){
				// Resize if resizable
				if(
					ResizeItems &&
					NumOfItems >= Items.Length
				){
					var size = Helper.NextPrime(Items.Length);

					// Resize and copy items
					ITEM[] newItems = new ITEM[size];
					System.Array.Copy(Items, newItems, NumOfItems);
					Items = newItems;
				}

				// Overloaded queue
				if(NumOfItems >= Items.Length)
				throw new System.OverflowException("Queue overflow");

				// Add item
				Items[EnqueuePoint] = item;

				// Next enqueue point
				EnqueuePoint += 1;
				EnqueuePoint %= Items.Length;

				// Increment num of items
				NumOfItems++;
			}
		}

		/// <summary>Dequeue item</summary>
		/// <returns>Item</returns>
		public virtual ITEM Dequeue(){
			if(NumOfItems <= 0) return default(ITEM);

			ITEM value = default(ITEM);

			lock(SyncLock){
				// Get and remove
				value = Items[DequeuePoint];
				Items[DequeuePoint] = default(ITEM);

				// Next dequeue point
				DequeuePoint += 1;
				DequeuePoint %= Items.Length;

				// Decrement num of items
				NumOfItems--;
			}

			return value;
		}


		/// <summary>Peek item</summary>
		/// <returns>Item</returns>
		public virtual ITEM Peek(){
			if(NumOfItems <= 0) return default(ITEM);

			ITEM value = default(ITEM);

			lock(SyncLock)
			{value = Items[DequeuePoint];}

			return value;
		}
	};
};