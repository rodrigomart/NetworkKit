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

using System.Runtime.InteropServices;
using System.Globalization;


namespace NetworkKit.Stream {
	/// <summary>Half</summary>
	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public struct Half {
		/// <summary>Number of decimal digits of precision</summary>
		public const int PrecisionDigits = 3;

		/// <summary>Number of bits in the mantissa</summary>
		public const int MantissaBits = 11;


		/// <summary>Maximum decimal exponent</summary>
		public const int MaximumDecimalExponent = 4;

		/// <summary>Maximum binary exponent</summary>
		public const int MaximumBinaryExponent = 15;


		/// <summary>Minimum decimal exponent</summary>
		public const int MinimumDecimalExponent = -4;

		/// <summary>Minimum binary exponent</summary>
		public const int MinimumBinaryExponent = -14;


		/// <summary>Additional rounding</summary>
		public const int AdditionRounding = 1;

		/// <summary>Exponent radix</summary>
		public const int ExponentRadix = 2;


		/// <summary>Value</summary>
		readonly ushort Value;


		/// <summary>Half</summary>
		/// <param name="value">Floating value</param>
		public Half(float value)
		{Value = HalfUtils.Pack(value);}

		/// <summary>Half</summary>
		/// <param name="value">Value</param>
		public Half(ushort value)
		{Value = value;}


		/// <summary>Hash function</summary>
		/// <returns>Hash code</returns>
		public override int GetHashCode(){
			var num = Value.GetHashCode();
			return ((num * 3) / 2) ^ num;
		}

		/// <summary>Determines whether the Half is equal</summary>
		/// <param name="obj">Half to compare<param>
		/// <returns>True if equal</returns>
		public override bool Equals(object obj){
			if(obj == null) return false;

			if(obj.GetType() != GetType())
			return false;

			var half = (Half)obj;
			return (half.Value == Value);
		}

		/// <summary>Returns the representation of a Half</summary>
		/// <returns>String</returns>
		public override string ToString(){
			float num = this;
			return num.ToString(CultureInfo.CurrentCulture);
		}


		////////////////////////////////////////////////////////////////////////////////////////////////////////////////
		/// STATIC IMPLEMENTATION
		////////////////////////////////////////////////////////////////////////////////////////////////////////////////


		/// <summary>Smallest such that 1.0 + epsilon != 1.0</summary>
		public static readonly float Epsilon = 0.0004887581f;


		/// <summary>Maximum value of the number</summary>
		public static readonly float MaxValue = 65504f;

		/// <summary>Minimum value of the number</summary>
		public static readonly float MinValue = 6.103516E-05f;


		/// <summary>A Half whose value is 0.0f</summary>
		public static readonly Half Zero = new Half(0f);

		/// <summary>A Half whose value is 1.0f</summary>
		public static readonly Half One = new Half(1f);


		/// <summary>Converts an array of half precision values into full precision values</summary>
		/// <param name = "values">The values to be converted</param>
		/// <returns>An array of converted values</returns>
		public static float[] ConvertToFloat(Half[] values){
			float[] results = new float[values.Length];

			for(int i = 0; i < results.Length; i++)
			results[i] = HalfUtils.Unpack(values[i].Value);
			
			return results;
		}

		/// <summary>Converts an array of full precision values into half precision values</summary>
		/// <param name="values">The values to be converted</param>
		/// <returns>An array of converted values</returns>
		public static Half[] ConvertToHalf(float[] values){
			Half[] results = new Half[values.Length];

			for(int i = 0; i < results.Length; i++)
			results[i] = new Half(values[i]);
			
			return results;
		}


		/// <summary>Performs an explicit conversion from Single to Half</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The converted value</returns>
		public static explicit operator Half(float value)
		{return new Half(value);}


		/// <summary>Performs an implicit conversion from Half to Single</summary>
		/// <param name="value">The value to be converted</param>
		/// <returns>The converted value</returns>
		public static implicit operator float(Half value)
		{return HalfUtils.Unpack(value.Value);}


		/// <summary>Compares for equality</summary>
		/// <param name="one">Half one</param>
		/// <param name="two">Half two</param>
		/// <returns>True if equality</returns>
		public static bool operator == (Half one, Half two)
		{return one.Equals(two);}

		/// <summary>Compares for inequality</summary>
		/// <param name="one">Half one</param>
		/// <param name="two">Half two</param>
		/// <returns>True if inequality</returns>
		public static bool operator != (Half one, Half two)
		{return !one.Equals(two);}
	};
};