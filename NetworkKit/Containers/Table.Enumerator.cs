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

using System.Collections;


namespace NetworkKit.Containers {
	/// <summary>
	/// Asynchronous table enumerated
	/// </summary>
	public partial class Table<KEY, ITEM> : IEnumerable {
		/// <summary>Enumerator</summary>
		public sealed class Enumerator : IEnumerator {
			/// <summary>Asynchronous table</summary>
			private readonly Table<KEY, ITEM> Table;


			/// <summary>Current item</summary>
			private ITEM CurrentItem;

			/// <summary>Index</summary>
			private int Index;


			/// <summary>Get the current</summary>
			public object Current {
				get {return CurrentItem;}
			}


			/// <summary>Enumerator</summary>
			/// <param name="table">Asynchronous table</param>
			public Enumerator(Table<KEY, ITEM> table){
				Table = table;
				Index = -1;
			}


			/// <summary>Move to the next</summary>
			/// <returns>True if there is a</returns>
			public bool MoveNext(){
				lock(Table.SyncLock){
					if(Table.Keys == 0)
					return false;

					if(Index < 0) Index = 0;
					else Index++;

					if(Table.Entries.Length <= Index)
					return false;

					// Next valid index
					for(; Index < Table.Entries.Length; Index++)
					if(
						Table.Entries[Index].Hash >= 0 &&
						Table.Entries[Index].Key  != null
					){
						// Redeem here to avoid loss of reference
						CurrentItem = Table.Entries[Index].Item;
						return true;
					}
				}

				return false;
			}

			/// <summary>Restore</summary>
			public void Reset()
			{Index = -1;}
		};


		/// <summary>Obtém o enumerador</summary>
		/// <returns>Enumerador</returns>
		public IEnumerator GetEnumerator()
		{return new Enumerator(this);}
	};
};