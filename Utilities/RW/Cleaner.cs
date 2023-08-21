using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities {

	/// <summary>
	/// Helps to dispose of certain RW objects.
	/// </summary>
	public static class Cleaner {

		private const BindingFlags ALL_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

		/// <summary>
		/// Destroys all objects that are present in the provided list, and then removes them from that list by clearing it.
		/// </summary>
		/// <param name="objects"></param>
		public static void DestroyAllAndClear<T>(this IList<T> objects) where T : UpdatableAndDeletable {
			if (objects == null) return;
			foreach (UpdatableAndDeletable obj in objects) {
				obj.Destroy();
			}
			objects.Clear();
		}

		/// <summary>
		/// Removes the provided object from its room then deletes it. Does nothing if the object is null, so this is safe to call on null references.
		/// <para/>
		/// The primary difference between this method and calling <see cref="UpdatableAndDeletable.Destroy"/> is for immediate removal, a bit like
		/// Unity's <see cref="UnityEngine.Object.DestroyImmediate"/> (including with how niche its use cases are).
		/// </summary>
		/// <param name="obj"></param>
		public static void RemoveThenDestroy<T>(this T obj) where T : UpdatableAndDeletable {
			if (obj == null) return;
			obj.Destroy();
		}

	}
}
