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

namespace NetworkKit.Stream.Unity {
	/// <summary>
	/// Networking Unity Extension
	/// </summary>
	public static class Extension {
		/// <summary>Read color</summary>
		/// <param name="stream">Bit Stream</param>
		/// <returns>Color</returns>
		public static UnityEngine.Color ReadColor(this BitStream stream){
			var color = stream.ReadUInt32();

			return new UnityEngine.Color(
				((color >> 16) & 0xFF) / 255f,
				((color >> 8)  & 0xFF) / 255f,
				((color >> 0)  & 0xFF) / 255f,
				((color >> 24) & 0xFF) / 255f
			);
		}

		/// <summary>Read quaternion</summary>
		/// <param name="stream">Bit Stream</param>
		/// <returns>Quaternion</returns>
		public static UnityEngine.Quaternion ReadQuaternion(this BitStream stream){
			var quaternion = UnityEngine.Quaternion.identity;
			quaternion.eulerAngles = stream.ReadVector3();

			return quaternion;
		}

		/// <summary>Read vector 2D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <returns>Vector2</returns>
		public static UnityEngine.Vector2 ReadVector2(this BitStream stream){
			return new UnityEngine.Vector2(
				stream.ReadSingle(),
				stream.ReadSingle()
			);
		}

		/// <summary>Read vector 3D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <returns>Vector3</returns>
		public static UnityEngine.Vector3 ReadVector3(this BitStream stream){
			return new UnityEngine.Vector3(
				stream.ReadSingle(),
				stream.ReadSingle(),
				stream.ReadSingle()
			);
		}

		/// <summary>Read vector 4D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <returns>Vector4</returns>
		public static UnityEngine.Vector4 ReadVector4(this BitStream stream){
			return new UnityEngine.Vector4(
				stream.ReadSingle(),
				stream.ReadSingle(),
				stream.ReadSingle(),
				stream.ReadSingle()
			);
		}


		/// <summary>Write color</summary>
		/// <param name="stream">Bit Stream</param>
		/// <param name="value">Value</param>
		public static void WriteColor(this BitStream stream, UnityEngine.Color value){
			var color = 0u;
			color |= ((uint)(value.a * 255)) << 24;
			color |= ((uint)(value.r * 255)) << 16;
			color |= ((uint)(value.g * 255)) << 8;
			color |= ((uint)(value.b * 255)) << 0;
			stream.WriteUInt32(color);
		}

		/// <summary>Write quaternion</summary>
		/// <param name="stream">Bit Stream</param>
		/// <param name="value">Value</param>
		public static void WriteQuaternion(this BitStream stream, UnityEngine.Quaternion value)
		{stream.WriteVector3(value.eulerAngles);}

		/// <summary>Write vector 2D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <param name="value">Value</param>
		public static void WriteVector2(this BitStream stream, UnityEngine.Vector2 value){
			stream.WriteSingle(value.x);
			stream.WriteSingle(value.y);
		}

		/// <summary>Write vector 3D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <param name="value">Value</param>
		public static void WriteVector3(this BitStream stream, UnityEngine.Vector3 value){
			stream.WriteSingle(value.x);
			stream.WriteSingle(value.y);
			stream.WriteSingle(value.z);
		}

		/// <summary>Write vector 4D</summary>
		/// <param name="stream">Bit Stream</param>
		/// <param name="value">Value</param>
		public static void WriteVector4(this BitStream stream, UnityEngine.Vector4 value){
			stream.WriteSingle(value.x);
			stream.WriteSingle(value.y);
			stream.WriteSingle(value.z);
			stream.WriteSingle(value.w);
		}
	};
};