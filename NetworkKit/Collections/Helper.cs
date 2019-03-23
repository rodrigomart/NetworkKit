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


namespace NetworkKit.Collections {
	/// <summary>
	/// Helper
	/// </summary>
	internal static class Helper {
		/// <summary>Next prime</summary>
		/// <param name="num">Number</param>
		/// <returns>Integer</returns>
		/// <exception cref="System.Exception">
		/// A positive integer was expected
		/// </exception>
		public static int NextPrime(int num){
			if(num < 0) throw new System.Exception("Positive integer was expected");

			// Make sure it is an odd number except 2
			if(num % 2 == 0 && num != 2) num++;

			// Primes
			int[] primes = {2,3,5,7,11,13,17,19,23,29,31,37,41,43};

			// Next prime
			var prime = num;
			while(prime < 2147483647){
				// Checks if divisible
				bool divisible = false;
				for(var i = 0; i < primes.Length; i++){
					if(prime == primes[i]) break;

					if((prime % primes[i]) == 0){
						divisible = true;
						break;
					}
				}

				// It is divisible
				if(divisible){
					// Increment
					if(prime <= 2) prime++;
					else prime += 2;
					continue;
				}

				// Valid number
				if(prime > num) break;

				// Increment
				if(prime <= 2) prime++;
				else prime += 2;
			}

			return prime;
		}
	};
};