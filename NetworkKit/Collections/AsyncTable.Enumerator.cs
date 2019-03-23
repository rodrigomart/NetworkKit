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
	/// <summary>
	/// Asynchronous table enumerated
	/// </summary>
	public partial class AsyncTable<KEY, ITEM> : IEnumerable<ITEM> {
		/// <summary>Enumerator</summary>
		public class Enumerator : IEnumerator<ITEM>, IEnumerator {
			/// <summary>Asynchronous Table</summary>
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
			/// <param name="table">Asynchronous Table</param>
			public Enumerator(AsyncTable<KEY, ITEM> table){
				Table = table;
				Index = -1;
			}


			/// <summary>Dispose</summary>
			void IDisposable.Dispose(){}


			/// <summary>Move next</summary>
			/// <returns>True if move next</returns>
			bool IEnumerator.MoveNext(){
				lock(Table.SyncLock){
					if(Table.Keys == 0)
					return false;

					if(Index < 0) Index = 0;
					else Index++;

					if(Table.Entries.Length <= Index)
					return false;

					// Next valid index
					for(; Index < Table.Entries.Length; Index++){
						if(
							Table.Entries[Index].Hash >= 0 &&
							Table.Entries[Index].Item != null
						){
							// Redeem here to avoid loss of reference
							CurrentItem = (ITEM)Table.Entries[Index].Item;
							return true;
						}
					}
				}

				return false;
			}

			/// <summary>Reset</summary>
			void IEnumerator.Reset()
			{Index = -1;}
		};


		/// <summary>Gets or sets</summary>
		/// <param name="key">Key</param>
		public ITEM this[KEY key]{
			set {
				lock(SyncLock){
					int i = InternalFind(key);
					if(i >= 0) Entries[i].Item = value;
				}
			}
			get {
				lock(SyncLock){
					int i = InternalFind(key);
					if(i >= 0) return (ITEM)Entries[i].Item;
					return default(ITEM);
				}
			}
		}


		/// <summary>Get enumerator</summary>
		/// <returns>Enumerator</returns>
		IEnumerator IEnumerable.GetEnumerator()
		{return new Enumerator(this);}

		/// <summary>Get enumerator</summary>
		/// <returns>Enumerator</returns>
		IEnumerator<ITEM> IEnumerable<ITEM>.GetEnumerator()
		{return new Enumerator(this);}
	};
};