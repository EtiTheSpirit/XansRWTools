using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.General {

	/// <summary>
	/// Extensions to .NET itself. 
	/// <para/>
	/// This code was taken from my own game that I develop, and adapted to earlier versions of C#. Rest assured, it is under the license of this toolkit.
	/// </summary>
	public static class NETExtensions {

		/// <summary>
		/// Binds Source Type => (Destination Type => Delegate)
		/// </summary>
		private static Dictionary<Type, Dictionary<Type, Delegate>> _reflectiveCastConversionCache = new Dictionary<Type, Dictionary<Type, Delegate>>();
		private static Dictionary<Type, Dictionary<Type, bool>> _attributePresenceCache = new Dictionary<Type, Dictionary<Type, bool>>();
		private static PropertyInfo _hasBeenThrownProp = null;
		private static HashSet<Assembly> _allAssemblies;
		private static IReadOnlyList<Assembly> _allAssembliesCache;
		private static IReadOnlyList<Type> _allTypes = null;
#if NET6_0_OR_GREATER
		private static Dictionary<Type, Delegate> _defaultProviders = new Dictionary<Type, Delegate>();
#endif

		/// <summary>
		/// All primitive types that can be either positive or negative, excluding <see cref="float"/> and <see cref="double"/>, ordered by size.
		/// <para/>
		/// Indices of elements in this list correspond to their counterpart (i.e. <see cref="SIGNED_TYPES"/>[0] is the signed variant of <see cref="UNSIGNED_TYPES"/>[0])
		/// </summary>
		public static readonly IReadOnlyList<Type> SIGNED_TYPES = new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long) };

		/// <summary>
		/// All primitive types that can only be positive, ordered by size.
		/// <para/>
		/// Indices of elements in this list correspond to their counterpart (i.e. <see cref="SIGNED_TYPES"/>[0] is the signed variant of <see cref="UNSIGNED_TYPES"/>[0])
		/// </summary>
		public static readonly IReadOnlyList<Type> UNSIGNED_TYPES = new Type[] { typeof(byte), typeof(ushort), typeof(uint), typeof(ulong) };

		/// <summary>
		/// All signed and unsigned primitive types, ordered by signability then type (<see cref="sbyte"/>, <see cref="short"/>, <see cref="int"/>, <see cref="long"/>, <see cref="byte"/>, <see cref="ushort"/>, <see cref="uint"/>, <see cref="ulong"/>)
		/// </summary>
		public static readonly IReadOnlyList<Type> SIGNABLE_PRIMITIVES = new Type[] { typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(byte), typeof(ushort), typeof(uint), typeof(ulong) };

		/// <summary>
		/// Loads and returns all <see cref="Assembly"/> instances that are referenced, either directly or indirectly, by the currently calling <see cref="Assembly"/>.
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyList<Assembly> GetAllAssemblies() {
			if (_allAssemblies != null) {
				if (_allAssembliesCache != null) return _allAssembliesCache;
				_allAssembliesCache = _allAssemblies.ToList().AsReadOnly();
				return _allAssembliesCache;
			}

			Assembly self = Assembly.GetExecutingAssembly();
			_allAssemblies = new HashSet<Assembly> { self };
			GetAssembliesReferencedBy(self);
			_allAssembliesCache = _allAssemblies.ToList().AsReadOnly();
			return _allAssembliesCache;
		}

		/// <summary>
		/// Returns every type loaded by this assembly. The result is cached after the first call.
		/// </summary>
		/// <returns></returns>
		public static IReadOnlyList<Type> GetAllTypes() {
			if (_allTypes != null) return _allTypes;

			List<Type> result = new List<Type>(512);
			foreach (Assembly asm in GetAllAssemblies()) {
				result.AddRange(asm.GetTypes());
			}
			_allTypes = result.AsReadOnly();
			return _allTypes;
		}

		/// <summary>
		/// This method is extremely expensive! It returns all members of all types (in the provided assembly) that are decorated with the provided attribute type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="asm"></param>
		/// <returns></returns>
		public static IEnumerable<MemberInfo> GetAllMembersWithAttribute<T>(Assembly asm) where T : Attribute {
			foreach (Type type in asm.GetTypes()) {
				IEnumerable<MemberInfo> resultsForType = type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance).Where(t => t.HasAttribute<T>());
				foreach (MemberInfo mbr in resultsForType) {
					yield return mbr;
				}
			}
		}

		/// <summary>
		/// Allows <see cref="GetAllAssemblies"/> and <see cref="GetAllTypes"/> to be called again and return fresh results instead of their cached results.
		/// </summary>
		public static void MarkAllAssembliesDirty() {
			_allAssemblies = null;
			_allAssembliesCache = null;
			_allTypes = null;
		}

		/// <summary>
		/// Returns all assemblies that get referenced by <paramref name="other"/>.
		/// </summary>
		/// <param name="other"></param>
		private static void GetAssembliesReferencedBy(Assembly other) {
			foreach (AssemblyName name in other.GetReferencedAssemblies()) {
				try {
					Assembly asm = Assembly.Load(name);
					if (_allAssemblies.Add(asm)) {
						GetAssembliesReferencedBy(asm);
					}
				} catch { }
			}
		}

		/// <summary>
		/// Returns every type out of every loaded assembly that is decorated with the given attribute type.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static Type[] GetAllTypesWithAttribute(Type t) {
			IReadOnlyList<Type> types = GetAllTypes();
			List<Type> result = new List<Type>();
			foreach (Type type in types) {
				if (type.IsClass && type.GetCustomAttribute(t) != null) {
					result.Add(type);
				}
			}
			return result.ToArray();
		}

		/// <inheritdoc cref="GetAllTypesWithAttribute(Type)"/>
		public static Type[] GetAllTypesWithAttribute<T>() where T : Attribute => GetAllTypesWithAttribute(typeof(T));

		/// <summary>
		/// Returns every type out of every loaded assembly that implements the given interface <paramref name="t"/>.
		/// </summary>
		/// <returns></returns>
		public static Type[] GetAllTypesImplementing(Type t) {
			if (!t.IsInterface) throw new ArgumentException($"The provided generic type {t.FullName} is not an interface.");
			IReadOnlyList<Type> types = GetAllTypes();
			List<Type> result = new List<Type>();
			foreach (Type type in types) {
				if (!type.IsAbstract && type.GetInterface(t.Name) != null) {
					// TODO: t.IsClass?
					result.Add(type);
				}
			}
			return result.ToArray();
		}

		/// <inheritdoc cref="GetAllTypesImplementing(Type)"/>
		public static Type[] GetAllTypesImplementing<T>() => GetAllTypesImplementing(typeof(T));

		/// <summary>
		/// Returns every type out of every loaded assembly that extends the provided type <paramref name="t"/>.
		/// </summary>
		/// <param name="t"></param>
		/// <param name="directly">If true, the type must be directly extending the type <paramref name="t"/> (it does not qualify if its base class or anything higher inherits from <paramref name="t"/>).</param>
		/// <returns></returns>
		/// <exception cref="ArgumentException"></exception>
		public static Type[] GetAllTypesExtending(Type t, bool directly) {
			IReadOnlyList<Type> types = GetAllTypes();
			if (!t.IsClass) throw new ArgumentException($"The provided generic type {t.FullName} is not a class.");
			List<Type> result = new List<Type>();
			foreach (Type type in types) {
				bool qualifies;
				if (directly) {
					qualifies = type.BaseType == t;
				} else {
					qualifies = type != t && type.IsAssignableTo(t);
				}
				if (qualifies) {
					result.Add(type);
				}
			}
			return result.ToArray();
		}

		/// <inheritdoc cref="GetAllTypesExtending(Type, bool)"/>
		public static Type[] GetAllTypesExtending<T>(bool directly) => GetAllTypesExtending(typeof(T), directly);

		/// <summary>
		/// A mnemonic variant of <see cref="Type.IsAssignableFrom(Type)"/>.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="other"></param>
		/// <returns></returns>
		public static bool IsAssignableTo(this Type type, Type other) {
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (other == null) throw new ArgumentNullException(nameof(other));
			return other.IsAssignableFrom(type);
		}

		/// <summary>
		/// Returns <see langword="true"/> if the given <paramref name="value"/> is equal to <c><see langword="default"/>(<typeparamref name="T"/>)</c>. Respects the implementation of <see cref="IEquatable{T}"/>, where applicable.
		/// </summary>
		/// <typeparam name="T">The type to compare to.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <returns><see langword="true"/> if value is equal to <c><see langword="default"/>(<typeparamref name="T"/>)</c>, <see langword="false"/> if not.</returns>
		public static bool IsDefault<T>(this T value) => EqualityComparer<T>.Default.Equals(value, default);

		/// <summary>
		/// Exposes the HasBeenThrown property of <see cref="Exception"/>.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		public static bool HasBeenThrown(this Exception source) {
			if (_hasBeenThrownProp == null) {
				_hasBeenThrownProp = typeof(Exception).GetProperty("HasBeenThrown", BindingFlags.NonPublic | BindingFlags.Instance);
			}
			return (bool)_hasBeenThrownProp.GetValue(source, null);
		}

		/// <summary>
		/// Can be used on an object or a type to get a static member from that instance's type.
		/// This can be called on both a <see cref="Type"/> and an instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instanceOrType">Either an instance of the desired type (via <see langword="new"/>), or the type in and of itself (via <see langword="typeof"/>)</param>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException"></exception>
		public static T GetStatic<T>(this object instanceOrType, string name) {
			Type instanceType;
			if (instanceOrType is Type type) {
				// Passed in a type.
				instanceType = type;
			} else {
				instanceType = instanceOrType.GetType();
			}

			FieldInfo field = instanceType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (field != null) return (T)field.GetValue(null);

			PropertyInfo property = instanceType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (property != null) return (T)property.GetValue(null);

			throw new MissingMemberException($"Failed to find a static property of {instanceType.FullName} with the given name: ", name);
		}

		/// <summary>
		/// Can be used on an object or a type to set a static member on that instance's type.
		/// This can be called on both a <see cref="Type"/> and an instance.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="instanceOrType">Either an instance of the desired type (via <see langword="new"/>), or the type in and of itself (via <see langword="typeof"/>)</param>
		/// <param name="name"></param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException"></exception>
		public static void SetStatic(this object instanceOrType, string name, object value) {
			Type instanceType;
			if (instanceOrType is Type type) {
				// Passed in a type.
				instanceType = type;
			} else {
				instanceType = instanceOrType.GetType();
			}

			FieldInfo field = instanceType.GetField(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (field != null) {
				field.SetValue(null, value);
				return;
			}

			PropertyInfo property = instanceType.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
			if (property != null) {
				property.SetValue(null, value);
				return;
			}

			throw new MissingMemberException($"Failed to find a static property of {instanceType.FullName} with the given name: ", name);
		}

		/// <summary>
		/// Returns the <see langword="default"/> value of the provided <see cref="Type"/>, for use in a context where the <see langword="default"/> operator is not available (such as outside of a generic context).
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public static object Default(this Type type) {
			if (type == null) throw new ArgumentNullException(nameof(type));
			if (type.IsValueType) {
#if NET6_0_OR_GREATER // C# 10 was introduced with .NET 6
				/*
				 * > In C# 10 and later, a structure type (which is a value type) may have an explicit parameterless constructor 
				 * > that may produce a non-default value of the type. Thus, we recommend using the default operator or the default 
				 * > literal to produce the default value of a type.
				 * 
				 * SRC: C# documentation, https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/default-values
				 */
				if (_defaultProviders.TryGetValue(type, out Delegate? @default)) {
					return @default.DynamicInvoke();
				}
				@default = Expression.Lambda(Expression.Convert(Expression.Default(type), typeof(object))).Compile();
				_defaultProviders[type] = @default;
				return @default.DynamicInvoke();
#else
				return Activator.CreateInstance(type);
#endif
			}
			return null;
		}


		/// <summary>
		/// Sourced from <see href="https://stackoverflow.com/questions/1398796/casting-with-reflection">"Casting With Reflection?"</see> --
		/// This casts <paramref name="valueToCast"/> (the <see cref="object"/> this was called on) into an instance of <paramref name="destinationType"/> without using <see cref="Convert.ChangeType(object?, Type)"/>.
		/// <para/>
		/// While marginally slower, this technique is orders of magnitude more capable than its counterpart. Unlike <see cref="Convert.ChangeType(object?, Type)"/>, this method will...
		/// <list type="bullet">
		/// <item>Find and use user-defined or CLR-defined <see langword="implicit"/> and <see langword="explicit"/> casts for the associated types.</item>
		/// <item>Cast enums to and from a numeric type other than its underlying type.</item>
		/// </list>
		/// </summary>
		/// <param name="destinationType"></param>
		/// <param name="valueToCast"></param>
		/// <returns></returns>
		public static object ReflectiveCastInto(this object valueToCast, Type destinationType) {
			if (valueToCast.IsDefault()) return valueToCast;
			if (valueToCast?.GetType() == destinationType) return valueToCast;

			Type srcType = valueToCast.GetType();
			if (!_reflectiveCastConversionCache.TryGetValue(srcType, out Dictionary<Type, Delegate> conversions)) {
				conversions = new Dictionary<Type, Delegate>();
				_reflectiveCastConversionCache[srcType] = conversions;
			}
			if (conversions.TryGetValue(destinationType, out Delegate converter)) {
				return converter.DynamicInvoke(valueToCast);
			}

			ParameterExpression dataParam = Expression.Parameter(typeof(object), "data");
			BlockExpression body = Expression.Block(Expression.Convert(Expression.Convert(dataParam, srcType), destinationType));

			Delegate doConversion = Expression.Lambda(body, dataParam).Compile();
			_reflectiveCastConversionCache[srcType][destinationType] = doConversion;
			return doConversion.DynamicInvoke(valueToCast);
		}

		/// <summary>
		/// Sourced from <see href="https://stackoverflow.com/questions/1398796/casting-with-reflection">"Casting With Reflection?"</see> --
		/// This casts <paramref name="valueToCast"/> (the <see cref="object"/> this was called on) into an instance of <typeparamref name="T"/> without using <see cref="Convert.ChangeType(object?, Type)"/>.
		/// This variant of the method is primarily useful for things like enums, where odd casting rules apply. 
		/// Unless you really, really need this, you should probably just use <c>(<typeparamref name="T"/>)<paramref name="valueToCast"/></c> (which is what this does under the hood anyway, just impolitely bypassing some compile-time checks).
		/// <para/>
		/// While marginally slower, this technique is orders of magnitude more capable than its counterpart. Unlike <see cref="Convert.ChangeType(object?, Type)"/>, this method will...
		/// <list type="bullet">
		/// <item>Find and use user-defined or CLR-defined <see langword="implicit"/> and <see langword="explicit"/> casts for the associated types.</item>
		/// <item>Cast enums to and from a numeric type other than its underlying type.</item>
		/// </list>
		/// </summary>
		/// <param name="valueToCast"></param>
		/// <returns></returns>
		public static T ReflectiveCastInto<T>(this object valueToCast) => (T)valueToCast.ReflectiveCastInto(typeof(T));

		/// <summary>
		/// Returns whether or not the provided <see cref="Type"/> is decorated with the desired <see cref="Attribute"/>. The return value is cached.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="type"></param>
		/// <returns></returns>
		public static bool HasAttribute<T>(this Type type) where T : Attribute {
			if (!_attributePresenceCache.TryGetValue(type, out Dictionary<Type, bool> hasAttrLookup)) {
				hasAttrLookup = new Dictionary<Type, bool>();
				_attributePresenceCache[type] = hasAttrLookup;
			}
			if (hasAttrLookup.TryGetValue(typeof(T), out bool hasAttr)) {
				return hasAttr;
			}

			hasAttr = type.GetCustomAttribute<T>() != null;
			_attributePresenceCache[type][typeof(T)] = hasAttr;
			return hasAttr;
		}
		/// <summary>
		/// Returns whether or not the provided <see cref="Type"/> is decorated with the desired <see cref="Attribute"/>. The return value is cached.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="member"></param>
		/// <returns></returns>
		public static bool HasAttribute<T>(this MemberInfo member) where T : Attribute {
			return member.GetCustomAttribute<T>() != null;
		}

		/// <summary>
		/// Puts the provided <paramref name="value"/> into <paramref name="fld"/>. This value must be a numeric primitive. This technique does not care about the sign.
		/// </summary>
		/// <param name="fld">The field to modify.</param>
		/// <param name="instance">The instance of object that this field is a part of, or null to set statically.</param>
		/// <param name="value">The value to set the field to.</param>
		public static void SetValueCasted(this FieldInfo fld, object instance, object value) {
			fld.SetValue(instance, value.ReflectiveCastInto(fld.FieldType));
		}

		/// <summary>
		/// Puts the provided <paramref name="value"/> into <paramref name="prop"/>. This value must be a numeric primitive. This technique does not care about the sign.
		/// </summary>
		/// <param name="prop">The field to modify.</param>
		/// <param name="instance">The instance of object that this field is a part of, or null to set statically.</param>
		/// <param name="value">The value to set the field to.</param>
		public static void SetValueCasted(this PropertyInfo prop, object instance, object value) {
			prop.SetValue(instance, value.ReflectiveCastInto(prop.PropertyType));
		}

	}
}
