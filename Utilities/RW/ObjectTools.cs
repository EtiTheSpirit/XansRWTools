using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities.RW {
	public static class ObjectTools {

		/// <summary>
		/// Using the timeStacker variable, this will return the real position of an object via linear interpolation.
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="timeStacker"></param>
		/// <returns></returns>
		public static Vector2 RealPositionOfObject(this PhysicalObject obj, float timeStacker) {
			BodyChunk first = obj.firstChunk;
			return Vector2.Lerp(first.lastPos, first.pos, timeStacker);
		}

		// TODO: Is this useful?
		/// <summary>
		/// Looks at the object responsible for a given <see cref="BodyChunk"/> and attempts to cast it into an instance of <typeparamref name="T"/>.
		/// This behaves much like the <see langword="is"/> keyword in that it returns a boolean and spits out the resulting type.
		/// </summary>
		/// <typeparam name="T">The type of object to cast into.</typeparam>
		/// <param name="chunk">The chunk belonging to the desired object.</param>
		/// <param name="result">The owner of the chunk as <typeparamref name="T"/>, or <see langword="default"/> if the type does not match.</param>
		/// <returns>True if the owner of the provided <see cref="BodyChunk"/> is an instance of <typeparamref name="T"/>, false if not.</returns>
		public static bool TryGetOwnerAs<T>(this BodyChunk chunk, out T result) where T : PhysicalObject {
			if (chunk.owner is T instance) {
				result = instance;
				return true;
			}
			result = default;
			return false;
		}

	}
}
