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
		/// Destroys all objects that are present in the provided array. It will also remove them from the room, so this is safe to call on null references.
		/// </summary>
		/// <param name="objects"></param>
		public static void RemoveThenDestroyAll(this IEnumerable<UpdatableAndDeletable> objects) {
			if (objects == null) return;
			foreach (UpdatableAndDeletable obj in objects) {
				obj.room?.RemoveObject(obj);
				obj.Destroy();
			}
		}

		/// <summary>
		/// Removes the provided object from its room then deletes it. Does nothing if the object is null, so this is safe to call on null references.
		/// <para/>
		/// The primary difference between this method and calling <see cref="UpdatableAndDeletable.Destroy"/> is for immediate removal, a bit like
		/// Unity's <see cref="UnityEngine.Object.DestroyImmediate"/> (including with how niche its use cases are).
		/// </summary>
		/// <param name="obj"></param>
		public static void RemoveThenDestroy(this UpdatableAndDeletable obj) {
			if (obj == null) return;
			obj.room?.RemoveObject(obj);
			obj.Destroy();
		}

		/// <summary>
		/// Resets all fields of the object to their default values.
		/// <para/>
		/// <strong>This method is destructive and ignores implementation details.</strong> You will likely break something using this.<br/>
		/// Make ABSOLUTELY SURE that you can use this before you do!
		/// </summary>
		/// <param name="obj"></param>
		[Obsolete("This method was a prototype and may not work as intended for lack of proper testing and implementation.")]
		public static void DestructivelyResetFields(this object obj) {
			Type type = obj.GetType();
			FieldInfo[] flds = type.GetFields(ALL_FLAGS);
			for (int i = 0; i < flds.Length; i++) {
				FieldInfo field = flds[i];
				if (field.IsInitOnly) {
					Log.LogWarning($"Cannot reset field {field} (of type {type.FullName}) - it is readonly.");
				} else {
					field.SetValue(obj, field.GetRawConstantValue());
				}
			}
		}
	}
}
