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

	}
}
