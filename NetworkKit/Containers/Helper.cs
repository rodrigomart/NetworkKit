/// CONTAINERS IMPLEMENTATION
namespace NetworkKit.Containers {
	/// <summary>
	/// Container Helper
	/// </summary>
	internal static class Helper {
		/// <summary>Next prime</summary>
		/// <param name="num">Number</param>
		/// <returns>Interger</returns>
		public static int NextPrime(int num){
			// Make sure it is an odd number except 2
			if(num % 2 == 0 && num != 2) num++;

			// Primes
			int[] primes = { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29 };

			// Next prime
			var prime = num;
			while(prime < 2147483647){
				// Checks if divisible
				bool divisible = false;
				for(var i = 0; i < primes.Length; i++)
				if(prime % primes[i] == 0 && prime != primes[i]){
					divisible = true;
					break;
				}

				// Is divisible
				if(divisible){
					// Increment
					if (prime <= 2) prime++;
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