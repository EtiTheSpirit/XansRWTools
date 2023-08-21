using HarmonyLib;
using MonoMod.RuntimeDetour;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XansTools.Exceptions;
using XansTools.Utilities.Attributes;
using XansTools.Utilities.General;

namespace XansTools.Utilities {

	/// <summary>
	/// A set of utilities designed to assist in patching the game's assembly.
	/// </summary>
	public partial class AutoPatcher {

		private const BindingFlags COMMON_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		private Harmony _harmony;

		// This technique is less than ideal...
		/// <summary>
		/// To get around an annoying issue, a lookup from types (containing patched members) to the patcher that did those patches is required.
		/// This is because patch methods MUST be static, and so accessing the current patcher requires some lookup using only information that is
		/// available in the harmony patch (one of which is the original method).
		/// </summary>
		private static readonly Dictionary<Type, Dictionary<MethodBase, MethodBase>> _originalsToOverridesByAssembly = new Dictionary<Type, Dictionary<MethodBase, MethodBase>>();


		#region Caches to Methods

#if !REQUIRES_HARMONY_ARGS_PATCH
		private static MethodInfo GenericVoidInjection {
			get {
				if (_genericVoidInjection == null) {
					_genericVoidInjection = typeof(Patcher).GetMethod(nameof(AbstractVoidInjection), BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _genericVoidInjection;
			}
		}
		private static MethodInfo _genericVoidInjection = null;
		private static MethodInfo GenericReturningInjection {
			get {
				if (_genericReturningInjection == null) {
					_genericReturningInjection = typeof(Patcher).GetMethod(nameof(AbstractReturningInjection), BindingFlags.NonPublic | BindingFlags.Static);
				}
				return _genericReturningInjection;
			}
		}
		private static MethodInfo _genericReturningInjection = null; 
#endif

		private static MethodInfo GenericSetterInjection {
			get {
				if (_genericSetterInjection == null) {
					_genericSetterInjection = typeof(AutoPatcher).GetMethod(nameof(AbstractSetterInjection), BindingFlags.NonPublic | BindingFlags.Static);
					if (_genericSetterInjection == null) throw new MissingMethodException($"Failed to locate {nameof(AbstractSetterInjection)}");
				}
				return _genericSetterInjection;
			}
		}
		private static MethodInfo _genericSetterInjection = null;
		private static MethodInfo GenericGetterInjection {
			get {
				if (_genericGetterInjection == null) {
					_genericGetterInjection = typeof(AutoPatcher).GetMethod(nameof(AbstractGetterInjection), BindingFlags.NonPublic | BindingFlags.Static);
					if (_genericGetterInjection == null) throw new MissingMethodException($"Failed to locate {nameof(AbstractGetterInjection)}");
				}
				return _genericGetterInjection;
			}
		}
		private static MethodInfo _genericGetterInjection = null;
		#endregion

#if REQUIRES_HARMONY_ARGS_PATCH
		// Code is generated externally for these below. For the generated code, see Utilities/Patchers/PatcherTargetedVariationMethods.cs
		partial void AdditionalInitializeTask();
		partial void WrapInRedirector(MethodInfo methodInfo, ref HarmonyMethod mtd); // lmfao
																					 // To future people: This odd ref trick is used because C# 7.3 does not support extended partial methods.
																					 // Having a non-void return type (or out parameters) requires accessibility modifiers to be applied.
																					 // ...which requires extended partial methods
																					 // So I had to cheese the shit out of this system with an oddball ref parameter.
																					 // Thankfully, out is literally just ref but with compile-time enforcement to make sure you set the value.

		private HarmonyMethod WrapInRedirector(MethodInfo methodInfo) {
			HarmonyMethod retn = null;
			WrapInRedirector(methodInfo, ref retn);
			return retn;
		}
#endif

		private void PutOriginalToOverride(MethodInfo original, MethodInfo @override) {
			if (!_originalsToOverridesByAssembly.TryGetValue(@override.DeclaringType, out Dictionary<MethodBase, MethodBase> storage)) {
				storage = new Dictionary<MethodBase, MethodBase>();
				_originalsToOverridesByAssembly[@override.DeclaringType] = storage;
			}
			storage[original] = @override;
		}

		[Obsolete("Do not initialize any more.", true)]
		public void Initialize(Harmony harmony) {
			_harmony = harmony;
#if REQUIRES_HARMONY_ARGS_PATCH
			Log.LogWarning("IMPORTANT NOTICE ::: IMPORTANT NOTICE ::: IMPORTANT NOTICE");
			Log.LogWarning("The current version of Rain World does not support __args in Harmony patches. Consequently, this means that a rather unpleasant trick must be used to minimally emulate this feature. Certain patches may fail, and startup might take a little longer.");
			AdditionalInitializeTask();
#endif
			Log.LogMessage("Collecting all shadowed overrides. This might cause a bit of a hitch...");
			Assembly caller = Assembly.GetCallingAssembly();

			IEnumerable<MemberInfo> mbrs = NETExtensions.GetAllMembersWithAttribute<ShadowedOverrideAttribute>(caller);
			foreach (MemberInfo mbr in mbrs) {
				ShadowedOverrideAttribute attr = mbr.GetCustomAttribute<ShadowedOverrideAttribute>();
				string name = mbr.Name;
				Type inheritingType = mbr.DeclaringType;
				Type declaringType = attr.DeclaringType ?? inheritingType.BaseType;
				Log.LogTrace($"Trying to patch {name} (declaring type: {declaringType.FullName}, inheriting type: {inheritingType.FullName})...");

				if (mbr is PropertyInfo) {
					attr.ValidateProperty(inheritingType, name, out PropertyInfo declared, out PropertyInfo inherited, out bool declaredHasGet, out bool declaredHasSet);

					if (declaredHasGet) {
						MethodInfo declaredGet = declared.GetMethod;
						MethodInfo inheritedGet = inherited.GetMethod;
						//_originalsToOverrides[declaredGet] = inheritedGet;
						PutOriginalToOverride(declaredGet, inheritedGet);
						_harmony.Patch(declaredGet, prefix: WrapPropertyInRedirector(inheritingType, inheritedGet));
						Log.LogTrace($"Patched {name}.get of {declaringType.FullName} to redirect to {inheritingType.FullName}.");
					}
					if (declaredHasSet) {
						MethodInfo declaredSet = declared.SetMethod;
						MethodInfo inheritedSet = inherited.SetMethod;
						//_originalsToOverrides[declaredSet] = inheritedSet;
						PutOriginalToOverride(declaredSet, inheritedSet);
						_harmony.Patch(declaredSet, prefix: WrapPropertyInRedirector(inheritingType, inheritedSet));
						Log.LogTrace($"Patched {name}.set of {declaringType.FullName} to redirect to {inheritingType.FullName}.");
					}
				} else if (mbr is MethodInfo) {
					attr.ValidateMethod(inheritingType, name, out MethodInfo declared, out MethodInfo inherited);

					//_originalsToOverrides[declared] = inherited;
					PutOriginalToOverride(declared, inherited);
					_harmony.Patch(declared, prefix: WrapInRedirector(inherited));
					Log.LogTrace($"Patched {declared.FullDescription()} (of {declaringType.FullName}) to redirect to that of {inheritingType.FullName}");
				}
			}
		}

		/// <summary>
		/// Injects into a property to allow changing its behavior on the getter and/or the setter.
		/// </summary>
		/// <typeparam name="TDeclaringType"></typeparam>
		/// <typeparam name="TPropertyType"></typeparam>
		/// <param name="name"></param>
		/// <param name="get"></param>
		/// <param name="set"></param>
		/// <exception cref="MissingMemberException"></exception>
		/// <exception cref="MethodSignatureMismatchException"></exception>
		[Obsolete("Use the version that accepts MethodInfo", true)]
		public void InjectIntoProperty<TDeclaringType>(string name, HarmonyMethod get = null, HarmonyMethod set = null) {
			PropertyInfo declared = typeof(TDeclaringType).GetProperty(name, COMMON_FLAGS);
			if (declared == null) throw new MissingMemberException($"No such property \"{name}\" of type {typeof(TDeclaringType).FullName}");
			if (get == null && set == null) throw new ArgumentNullException($"{nameof(get)}, {nameof(set)}", "Either one of get or set must be passed in. Both cannot be null.");
			bool declaredHasGet = declared.GetMethod != null;
			bool declaredHasSet = declared.SetMethod != null;
			bool replacementGetPresent = get != null;
			bool replacementSetPresent = set != null;
			if (!declaredHasGet && replacementGetPresent) throw new MethodSignatureMismatchException($"Property \"{name}\" (member of type {typeof(TDeclaringType).FullName}) does not have a get method. An override or injection to the getter is not possible.");
			if (!declaredHasSet && replacementSetPresent) throw new MethodSignatureMismatchException($"Property \"{name}\" (member of type {typeof(TDeclaringType).FullName}) does not have a set method. An override or injection to the setter is not possible.");

			if (declaredHasGet && replacementGetPresent) {
				MethodInfo declaredGet = declared.GetMethod;
				_harmony.Patch(declaredGet, prefix: get);
				Log.LogTrace($"Patched {name}.get of {typeof(TDeclaringType).FullName}.");
			}
			if (declaredHasSet && replacementSetPresent) {
				MethodInfo declaredSet = declared.SetMethod;
				_harmony.Patch(declaredSet, prefix: set);
				Log.LogTrace($"Patched {name}.get of {typeof(TDeclaringType).FullName}.");
			}
		}

		/// <summary>
		/// Hooks a getter and/or setter into a property with the provided name. Returns the hooks (get, set), which may be null depending on which of <paramref name="newGet"/> and <paramref name="newSet"/> were provided.
		/// </summary>
		/// <typeparam name="TDeclaringType"></typeparam>
		/// <param name="name"></param>
		/// <param name="newGet"></param>
		/// <param name="newSet"></param>
		/// <returns></returns>
		/// <exception cref="MissingMemberException"></exception>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="MethodSignatureMismatchException"></exception>
		public (Hook, Hook) InjectIntoProperty<TDeclaringType>(string name, MethodInfo newGet = null, MethodInfo newSet = null) {
			PropertyInfo declared = typeof(TDeclaringType).GetProperty(name, COMMON_FLAGS);
			if (declared == null) throw new MissingMemberException($"No such property \"{name}\" of type {typeof(TDeclaringType).FullName}");
			if (newGet == null && newSet == null) throw new ArgumentNullException($"{nameof(newGet)}, {nameof(newSet)}", "Either one of get or set must be passed in. Both cannot be null.");
			bool declaredHasGet = declared.GetMethod != null;
			bool declaredHasSet = declared.SetMethod != null;
			bool replacementGetPresent = newGet != null;
			bool replacementSetPresent = newSet != null;
			if (!declaredHasGet && replacementGetPresent) throw new MethodSignatureMismatchException($"Property \"{name}\" (member of type {typeof(TDeclaringType).FullName}) does not have a get method. An override or injection to the getter is not possible.");
			if (!declaredHasSet && replacementSetPresent) throw new MethodSignatureMismatchException($"Property \"{name}\" (member of type {typeof(TDeclaringType).FullName}) does not have a set method. An override or injection to the setter is not possible.");

			Hook get = null;
			Hook set = null;
			if (declaredHasGet && replacementGetPresent) {
				MethodInfo declaredGet = declared.GetMethod;
				get = new Hook(declaredGet, newGet);
				Log.LogTrace($"Patched {name}.get of {typeof(TDeclaringType).FullName}.");
			}
			if (declaredHasSet && replacementSetPresent) {
				MethodInfo declaredSet = declared.SetMethod;
				set = new Hook(declaredSet, newSet);
				Log.LogTrace($"Patched {name}.get of {typeof(TDeclaringType).FullName}.");
			}
			return (get, set);
		}

		/// <summary>
		/// <strong>This technique is destructive and should only be used when FULL CONTROL of the target property is UNQUESTIONABLY REQUIRED.</strong>
		/// <para/>
		/// Given the declaring type and the inheriting type, this will apply a Harmony patch that causes references to
		/// the getter and/or setter of the named property to redirect to that of the inheriting type, so that a shadowed
		/// property behaves like an override.
		/// </summary>
		/// <typeparam name="TDeclaringType">The type that declares this property or seals it.</typeparam>
		/// <typeparam name="TInheritingType">The type that must shadow (but wants to override) this property.</typeparam>
		/// <param name="name"></param>
		[Obsolete("Decorate with ShadowedOverrideAttribute instead of calling this.", true)]
		public void TurnShadowedPropertyIntoOverride<TDeclaringType, TInheritingType>(string name) where TInheritingType : TDeclaringType {
			PropertyInfo declared = typeof(TDeclaringType).GetProperty(name, COMMON_FLAGS);
			PropertyInfo inherited = typeof(TInheritingType).GetProperty(name, COMMON_FLAGS);
			if (declared == null) throw new MissingMemberException($"No such property \"{name}\" of type {typeof(TDeclaringType).FullName}");
			if (inherited == null) throw new MissingMemberException($"No such property \"{name}\" of type {typeof(TInheritingType).FullName}");
			if (declared.PropertyType != inherited.PropertyType) throw new InvalidOperationException($"The types of the properties \"{name}\" (of types {typeof(TDeclaringType).FullName} and {typeof(TInheritingType).FullName}) do not match.");
			if (inherited.GetMethod?.IsVirtual ?? inherited.SetMethod?.IsVirtual ?? false) throw new NotSupportedException("Due to how the patcher works, shadowed overrides cannot be virtual.");
			bool declaredHasGet = declared.GetMethod != null;
			bool declaredHasSet = declared.SetMethod != null;
			bool inheritedHasGet = inherited.GetMethod != null;
			bool inheritedHasSet = inherited.SetMethod != null;
			if (declaredHasGet != inheritedHasGet) throw new MethodSignatureMismatchException($"Property \"{name}\" (of types {typeof(TDeclaringType).FullName} and {typeof(TInheritingType).FullName}) do not share getters; one has a getter while the other does not.");
			if (declaredHasSet != inheritedHasSet) throw new MethodSignatureMismatchException($"Property \"{name}\" (of types {typeof(TDeclaringType).FullName} and {typeof(TInheritingType).FullName}) do not share setters; one has a setter while the other does not.");

			if (declaredHasGet) {
				MethodInfo declaredGet = declared.GetMethod;
				MethodInfo inheritedGet = inherited.GetMethod;
				// _originalsToOverrides[declaredGet] = inheritedGet;
				PutOriginalToOverride(declaredGet, inheritedGet);
				_harmony.Patch(declaredGet, prefix: WrapPropertyInRedirector(typeof(TInheritingType), inheritedGet));
				Log.LogTrace($"Patched {name}.get of {typeof(TDeclaringType).FullName} to redirect to {typeof(TInheritingType).FullName}.");
			}
			if (declaredHasSet) {
				MethodInfo declaredSet = declared.SetMethod;
				MethodInfo inheritedSet = inherited.SetMethod;
				//_originalsToOverrides[declaredSet] = inheritedSet;
				PutOriginalToOverride(declaredSet, inheritedSet);
				_harmony.Patch(declaredSet, prefix: WrapPropertyInRedirector(typeof(TInheritingType), inheritedSet));
				Log.LogTrace($"Patched {name}.set of {typeof(TDeclaringType).FullName} to redirect to {typeof(TInheritingType).FullName}.");
			}
		}

		/// <summary>
		/// <strong>This technique is destructive and should only be used when FULL CONTROL of the target method is UNQUESTIONABLY REQUIRED.</strong>
		/// <para/>
		/// Given the declaring type and the inheriting type, this will apply a Harmony patch that causes references to 
		/// the named method to redirect to that of the inheriting type, so that a shadowed method behaves like an override.
		/// </summary>
		/// <typeparam name="TDeclaringType">The type that declares this property or seals it.</typeparam>
		/// <typeparam name="TInheritingType">The type that must shadow (but wants to override) this property.</typeparam>
		/// <param name="name"></param>
		[Obsolete("Decorate with ShadowedOverrideAttribute instead of calling this.", true)]
		public void TurnShadowedMethodIntoOverride<TDeclaringType, TInheritingType>(string name) where TInheritingType : TDeclaringType {
			MethodInfo declared = typeof(TDeclaringType).GetMethod(name, COMMON_FLAGS);
			MethodInfo inherited = typeof(TInheritingType).GetMethod(name, COMMON_FLAGS);
			if (declared == null) throw new MissingMethodException($"No such method \"{name}\" of type {typeof(TDeclaringType).FullName}");
			if (inherited == null) throw new MissingMethodException($"No such method \"{name}\" of type {typeof(TInheritingType).FullName}");
			MethodSignatureMismatchException.ThrowIfMismatched(declared, inherited);
			if (inherited.IsVirtual) throw new NotSupportedException("Due to how the patcher works, shadowed overrides cannot be virtual.");

			//_originalsToOverrides[declared] = inherited;
			PutOriginalToOverride(declared, inherited);
			_harmony.Patch(declared, prefix: WrapInRedirector(inherited));
			Log.LogTrace($"Patched {declared.FullDescription()} (of {typeof(TDeclaringType).FullName}) to redirect to that of {typeof(TInheritingType).FullName}");
		}

#if !REQUIRES_HARMONY_ARGS_PATCH
		// This oddball ref technique is used to coerce partial methods into working to cheese the C preprocessor macro system.
		// C# 7.3 doesn't support extended partial methods so things are extremely limited.
		private static void WrapInRedirector<TInheritingType>(MethodInfo inheritedMethod, ref HarmonyMethod mtd) {
			if (inheritedMethod.ReturnType == typeof(void)) {
				mtd = new HarmonyMethod(GenericVoidInjection.MakeGenericMethod(typeof(TInheritingType)));
			} else {
				mtd = new HarmonyMethod(GenericReturningInjection.MakeGenericMethod(inheritedMethod.ReturnType, typeof(TInheritingType)));
			}
		}
#endif
		private static HarmonyMethod WrapPropertyInRedirector(Type inheritingType, MethodInfo inheritedMethod) {
			if (inheritingType is null) throw new ArgumentNullException(nameof(inheritingType), "(checked via reference equality)");
			if (inheritedMethod is null) throw new ArgumentNullException(nameof(inheritedMethod), "(checked via reference equality)");
			if (inheritingType == null) throw new ArgumentNullException(nameof(inheritingType));
			if (inheritedMethod == null) throw new ArgumentNullException(nameof(inheritedMethod));
			if (inheritedMethod.ReturnType == typeof(void) || inheritedMethod.ReturnType == null) {
				return new HarmonyMethod(GenericSetterInjection.MakeGenericMethod(inheritingType));
			} else {
				return new HarmonyMethod(GenericGetterInjection.MakeGenericMethod(inheritedMethod.ReturnType, inheritingType));
			}
		}

#if !REQUIRES_HARMONY_ARGS_PATCH
		private static bool AbstractReturningInjection<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result, object[] __args) {
			if (__instance is TInheritingType) {
				// TODO: How can I access the original => override without a lookup like this? This will get slow.
				if (_originalsToOverrides.TryGetValue(__originalMethod, out MethodBase @override)) {
					__result = (TReturn)@override.Invoke(__instance, __args);
					return false;
				}
				throw new InvalidOperationException($"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method ({__originalMethod.FullDescription()})");
			}
			return true;
		}

		private static bool AbstractVoidInjection<TInheritingType>(object __instance, MethodBase __originalMethod, object[] __args) {
			if (__instance is TInheritingType) {
				// TODO: How can I access the original => override without a lookup like this? This will get slow.
				if (_originalsToOverrides.TryGetValue(__originalMethod, out MethodBase @override)) {
					@override.Invoke(__instance, __args);
					return false;
				}
				throw new InvalidOperationException($"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method ({__originalMethod.FullDescription()})");
			}
			return true;
		}
#endif


		private static bool AbstractGetterInjection<TReturn, TInheritingType>(ref TReturn __result, MethodBase __originalMethod, object __instance) {
			if (__instance is TInheritingType) {
				if (_originalsToOverridesByAssembly.TryGetValue(typeof(TInheritingType), out Dictionary<MethodBase, MethodBase> lookup)) {
					if (lookup.TryGetValue(__originalMethod, out MethodBase @override)) {
						__result = (TReturn)@override.Invoke(__instance, Array.Empty<object>());
						return false;
					}
				}
				throw new InvalidOperationException($"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method ({__originalMethod.FullDescription()})");
			}
			return true;
		}

		private static bool AbstractSetterInjection<TInheritingType>(object value, MethodBase __originalMethod, object __instance) {
			if (__instance is TInheritingType) {
				if (_originalsToOverridesByAssembly.TryGetValue(typeof(TInheritingType), out Dictionary<MethodBase, MethodBase> lookup)) {
					if (lookup.TryGetValue(__originalMethod, out MethodBase @override)) {
						@override.Invoke(__instance, new object[] { value });
						return false;
					}
				}
				throw new InvalidOperationException($"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method ({__originalMethod.FullDescription()})");
			}
			return true;
		}
	}
}
