#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.General {

	/// <summary>
	/// Utilities for references.
	/// </summary>
	public static class ReferenceExtensions {

		/// <summary>
		/// Returns the value stored within a <see cref="WeakReference{T}"/> as a nullable value for convenience.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="weakReference"></param>
		/// <returns></returns>
		public static T? Get<T>(this WeakReference<T> weakReference) where T : class {
			return weakReference.TryGetTarget(out T target) ? target : null;
		}

		/// <summary>
		/// Returns the value within a <see cref="ConditionalWeakTable{TKey, TValue}"/> as a nullable value for convenience.
		/// </summary>
		/// <typeparam name="TKey"></typeparam>
		/// <typeparam name="TValue"></typeparam>
		/// <param name="cwt"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static TValue? Get<TKey, TValue>(this ConditionalWeakTable<TKey, TValue> cwt, TKey key) where TKey : class where TValue : class {
			return cwt.TryGetValue(key, out TValue target) ? target : null;
		}

	}
}
