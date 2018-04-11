/// DEPENDENCY
using System.Diagnostics;


/// NETWORKING IMPLEMENTATION
namespace NetworkKit.Networking {
	/// <summary>
	/// Network
	/// </summary>
	public partial class Network {
		/// <summary>Frequency</summary>
		private double Frequency;

		/// <summary>Timestamp</summary>
		private long Timestamp;


		/// <summary>Time</summary>
		/// <returns>Time in milliseconds</returns>
		public uint Time(){
			if(Timestamp == 0){
				// High precision stopwatch
				Frequency = 1.0 / Stopwatch.Frequency;
				Timestamp = Stopwatch.GetTimestamp();
			}

			long diff = (Stopwatch.GetTimestamp() - Timestamp);
			return (uint)(diff * Frequency * 1000.0);
		}
	};
};