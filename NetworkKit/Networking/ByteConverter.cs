﻿// MIT License
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

using System.Runtime.InteropServices;
using System;


namespace NetworkKit.Networking {
#pragma warning disable
	/// <summary>
	/// Byte conversion
	/// </summary>
	[StructLayout(LayoutKind.Explicit)]
	public struct ByteConverter {
		[FieldOffset(0)] public SByte  s8;
		[FieldOffset(0)] public Int16  s16;
		[FieldOffset(0)] public Int32  s32;
		[FieldOffset(0)] public Int64  s64;

		[FieldOffset(0)] public Byte   u8;
		[FieldOffset(0)] public UInt16 u16;
		[FieldOffset(0)] public UInt32 u32;
		[FieldOffset(0)] public UInt64 u64;

		[FieldOffset(0)] public Char   chr;

		[FieldOffset(0)] public Single f32;
		[FieldOffset(0)] public Double f64;

		[FieldOffset(0)] public Byte Endian0;
		[FieldOffset(1)] public Byte Endian1;
		[FieldOffset(2)] public Byte Endian2;
		[FieldOffset(3)] public Byte Endian3;
		[FieldOffset(4)] public Byte Endian4;
		[FieldOffset(5)] public Byte Endian5;
		[FieldOffset(6)] public Byte Endian6;
		[FieldOffset(7)] public Byte Endian7;


		/// <summary>Apply mask</summary>
		/// <param name="mask">Byte Mask</param>
		public void ApplyMask(UInt64 mask){
			ByteConverter bitMask = mask;
			bitMask.Exchange64bits();

			// Apply mask
			u64 ^= bitMask.u64;
		}

		/// <summary>
		/// 16 bits end exchange.
		/// Converts Big-endian to Little-endian.
		/// </summary>
		public void Exchange16bits(){
			// To Little-endian
			if(!ByteConverter.IsLittle()){
				// Temporary for conversion
				ByteConverter temp = this;

				Endian0 = temp.Endian1;
				Endian1 = temp.Endian0;
			}
		}

		/// <summary>
		/// 32 bits end exchange.
		/// Converts Big-endian to Little-endian.
		/// </summary>
		public void Exchange32bits(){
			// To Little-endian
			if(!ByteConverter.IsLittle()){
				// Temporary for conversion
				ByteConverter temp = this;

				Endian0 = temp.Endian3;
				Endian1 = temp.Endian2;
				Endian2 = temp.Endian1;
				Endian3 = temp.Endian0;
			}
		}

		/// <summary>
		/// 64 bits end exchange.
		/// Converts Big-endian to Little-endian.
		/// </summary>
		public void Exchange64bits(){
			// To Little-endian
			if(!ByteConverter.IsLittle()){
				// Temporary for conversion
				ByteConverter temp = this;

				Endian0 = temp.Endian7;
				Endian1 = temp.Endian6;
				Endian2 = temp.Endian5;
				Endian3 = temp.Endian4;
				Endian4 = temp.Endian3;
				Endian5 = temp.Endian2;
				Endian6 = temp.Endian1;
				Endian7 = temp.Endian0;
			}
		}


		/// <summary>Is little-endian</summary>
		/// <returns>True if little-endian</returns>
		public static bool IsLittle(){
			ByteConverter bytes = default(ByteConverter);
			bytes.s32 = 1;

			return (bytes.Endian0 == 1);
		}


		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Char val){
			ByteConverter bytes = default(ByteConverter);
			bytes.chr = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(SByte val){
			ByteConverter bytes = default(ByteConverter);
			bytes.s8 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Int16 val){
			ByteConverter bytes = default(ByteConverter);
			bytes.s16 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Int32 val){
			ByteConverter bytes = default(ByteConverter);
			bytes.s32 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Int64 val){
			ByteConverter bytes = default(ByteConverter);
			bytes.s64 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Byte val){
			ByteConverter bytes = default(ByteConverter);
			bytes.u8 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(UInt16 val)
		{
			ByteConverter bytes = default(ByteConverter);
			bytes.u16 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(UInt32 val){
			ByteConverter bytes = default(ByteConverter);
			bytes.u32 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(UInt64 val){
			ByteConverter bytes = default(ByteConverter);
			bytes.u64 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Single val){
			ByteConverter bytes = default(ByteConverter);
			bytes.f32 = val; return bytes;
		}

		/// <summary>Implicit conversion</summary>
		public static implicit operator ByteConverter(Double val){
			ByteConverter bytes = default(ByteConverter);
			bytes.f64 = val; return bytes;
		}
	};
#pragma warning restore
};