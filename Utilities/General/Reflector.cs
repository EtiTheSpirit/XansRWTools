using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities {
	public static class Reflector {

		public const BindingFlags COMMON_ACCESS_BINDING_FLAGS = BindingFlags.Public | BindingFlags.NonPublic;

		/// <summary>
		/// Returns an instance field by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static FieldInfo GetField<TContainer>(string name) {
			return CommonGetField<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Instance);
		}

		/// <summary>
		/// Returns an static field by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static FieldInfo GetStaticField<TContainer>(string name) {
			return CommonGetField<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Static);
		}

		private static FieldInfo CommonGetField<TContainer>(string name, BindingFlags flags) {
			return typeof(TContainer).GetField(name, flags);
		}

		/// <summary>
		/// Returns an instance field by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static PropertyInfo GetProperty<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Instance);
		}

		/// <summary>
		/// Returns an static field by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static PropertyInfo GetStaticProperty<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Static);
		}

		private static PropertyInfo CommonGetProperty<TContainer>(string name, BindingFlags flags) {
			return typeof(TContainer).GetProperty(name, flags);
		}

		/// <summary>
		/// Returns an instance method by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetMethod<TContainer>(string name) {
			return CommonGetMethod<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Instance);
		}

		/// <summary>
		/// Returns an static method by its name, both private and public.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetStaticMethod<TContainer>(string name) {
			return CommonGetMethod<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Static);
		}

		private static MethodInfo CommonGetMethod<TContainer>(string name, BindingFlags flags) {
			return typeof(TContainer).GetMethod(name, flags);
		}

		/// <summary>
		/// Returns the get method of a static property.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetGetter<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Instance).GetMethod;
		}

		/// <summary>
		/// Returns the set method of a static property.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetStaticGetter<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Static).GetMethod;
		}
		/// <summary>
		/// Returns the set method of an instance property.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetSetter<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Instance).SetMethod;
		}

		/// <summary>
		/// Returns the set method of a static property.
		/// </summary>
		/// <typeparam name="TContainer"></typeparam>
		/// <param name="name"></param>
		/// <returns></returns>
		public static MethodInfo GetStaticSetter<TContainer>(string name) {
			return CommonGetProperty<TContainer>(name, COMMON_ACCESS_BINDING_FLAGS | BindingFlags.Static).SetMethod;
		}

	}
}
