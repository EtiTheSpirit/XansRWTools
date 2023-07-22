using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using XansTools.Exceptions;

namespace XansTools.Utilities {

	/// <summary>
	/// This extension class is managed by C, and is not compiled on build (instead, look for (this file)_P.g.cs for the result)
	/// </summary>
	public partial class AutoPatcher {

#ifdef REQUIRES_HARMONY_ARGS_PATCH

		/// <summary>
		/// The amount of harmony patchers, including the 0 arg patcher.
		/// </summary>
		private const int AVAILABLE_HARMONY_ARG_VARIANTS = 12;

		private readonly MethodInfo[] _parameterizedReturningInjections = new MethodInfo[AVAILABLE_HARMONY_ARG_VARIANTS];
		private readonly MethodInfo[] _parameterizedVoidInjections = new MethodInfo[AVAILABLE_HARMONY_ARG_VARIANTS];

		partial void AdditionalInitializeTask() {
			for (int i = 0; i < AVAILABLE_HARMONY_ARG_VARIANTS; i++) {
				string nameVoid = $"AbstractVoidInjection{i}";
				string nameRetn = $"AbstractReturningInjection{i}";
				_parameterizedVoidInjections[i] = typeof(AutoPatcher).GetMethod(nameVoid, BindingFlags.NonPublic | BindingFlags.Static);
				_parameterizedReturningInjections[i] = typeof(AutoPatcher).GetMethod(nameRetn, BindingFlags.NonPublic | BindingFlags.Static);
			}
		}

		partial void WrapInRedirector(MethodInfo inheritedMethod, ref HarmonyMethod mtd) {
			if (inheritedMethod == null) throw new ArgumentNullException("No inherited method.");

			int parameterCount = inheritedMethod.GetParameters().Count(param => !param.IsRetval);
			if (parameterCount > AVAILABLE_HARMONY_ARG_VARIANTS - 1) throw new NotSupportedException($"The fallback harmony method patcher does not support more than {AVAILABLE_HARMONY_ARG_VARIANTS-1} arguments. Sorry.");

			if (inheritedMethod.ReturnType == typeof(void)) {
				mtd = new HarmonyMethod(_parameterizedVoidInjections[parameterCount].MakeGenericMethod(inheritedMethod.DeclaringType));
			} else {
				mtd = new HarmonyMethod(_parameterizedReturningInjections[parameterCount].MakeGenericMethod(inheritedMethod.ReturnType, inheritedMethod.DeclaringType));
			}
		}

		private static object[] paramsof(params object[] array) => array;

		// Do not use this in Harmony; this is not supported in its current version!
		// It is used by the macro.
		private static bool AbstractReturningInjectionAny<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result, params object[] __args) {
			if (__instance is TInheritingType) {
				// TODO: How can I access the original => override without a lookup like this? This will get slow.
				if (_originalsToOverridesByAssembly.TryGetValue(typeof(TInheritingType), out Dictionary<MethodBase, MethodBase> lookup)) {
					if (lookup.TryGetValue(__originalMethod, out MethodBase @override)) {
						__result = (TReturn)@override.Invoke(__instance, __args);
						return false;
					}
					throw new RuntimePatchFailureException(__originalMethod, $"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method.");
				}
				throw new RuntimePatchFailureException(__originalMethod, $"A harmony method replacement patch failed because the lookup of patchers had no entry for this patcher.");
			}
			return true;
		}

		// Do not use this in Harmony; this is not supported in its current version!
		// It is used by the macro.
		private static bool AbstractVoidInjectionAny<TInheritingType>(object __instance, MethodBase __originalMethod, params object[] __args) {
			if (__instance is TInheritingType) {
				// TODO: How can I access the original => override without a lookup like this? This will get slow.
				if (_originalsToOverridesByAssembly.TryGetValue(typeof(TInheritingType), out Dictionary<MethodBase, MethodBase> lookup)) {
					if (lookup.TryGetValue(__originalMethod, out MethodBase @override)) {
						@override.Invoke(__instance, __args);
						return false;
					}
					throw new RuntimePatchFailureException(__originalMethod, $"A harmony method replacement patch failed because the lookup from declared => shadow had no entry for this method.");
				}
				throw new RuntimePatchFailureException(__originalMethod, $"A harmony method replacement patch failed because the lookup of patchers had no entry for this patcher.");
			}
			return true;
		}

	#ifdef DECLARE_ALL_HARMONY_PATCH_METHODS
		// Defined in MacroHack.h
		DECLARE_ALL_HARMONY_PATCH_METHODS;
	#endif
#endif

	}
}

