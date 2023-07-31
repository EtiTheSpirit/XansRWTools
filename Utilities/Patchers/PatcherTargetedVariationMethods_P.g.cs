#line 1 "Utilities\\Patchers\\PatcherTargetedVariationMethods.cs"
#line 1 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"





// LMFAO ^^

// This is a prank. You are being pranked.

// This is a hilariously terrible hack that allows me to use C macros in C#
// Yes, it's terrible, and no, I don't care (that's why I did it anyway).

// Cope, seethe, and mald simultaneously.


	
		
		
	


#line 23 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"
#line 24 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"


	
	
	//////////////////////////////////

	

	
	
	

	

	//////////////////////////////////

	

	
	

	
	

	
	

	
	

	
	

	
	

	
	

	
	

	
	

	
	

	
	

	
	
	
	
	
	
	
	
	
	
	
	
	
	
	

	
	
	
	
	
	
	
	
	
	
	
	

	












	












	

#line 133 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"




// #define CREATE_BYREFS
































#line 171 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"






































































































#line 274 "F:\\Users\\Xan\\source\\repos\\RWFirstMod\\XansTools\\DevTooling\\MacroHack.h"
#line 1 "Utilities\\Patchers\\PatcherTargetedVariationMethods.cs"
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
	[CPreprocessorGenerated]
	public partial class AutoPatcher {



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

	
		// Defined in MacroHack.h
		[CPreprocessorGenerated] private static bool AbstractReturningInjection0<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result ) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result ); [CPreprocessorGenerated] private static bool AbstractReturningInjection1<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0); [CPreprocessorGenerated] private static bool AbstractReturningInjection2<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1); [CPreprocessorGenerated] private static bool AbstractReturningInjection3<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2); [CPreprocessorGenerated] private static bool AbstractReturningInjection4<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3); [CPreprocessorGenerated] private static bool AbstractReturningInjection5<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4); [CPreprocessorGenerated] private static bool AbstractReturningInjection6<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5); [CPreprocessorGenerated] private static bool AbstractReturningInjection7<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5, object __6) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5, __6); [CPreprocessorGenerated] private static bool AbstractReturningInjection8<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5, __6, __7); [CPreprocessorGenerated] private static bool AbstractReturningInjection9<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5, __6, __7, __8); [CPreprocessorGenerated] private static bool AbstractReturningInjection10<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8, object __9) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5, __6, __7, __8, __9); [CPreprocessorGenerated] private static bool AbstractReturningInjection11<TReturn, TInheritingType>(object __instance, MethodBase __originalMethod, ref TReturn __result,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8, object __9, object __10) => AbstractReturningInjectionAny<TReturn, TInheritingType>(__instance, __originalMethod, ref __result,__0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10); [CPreprocessorGenerated] private static bool AbstractVoidInjection0<TInheritingType>(object __instance, MethodBase __originalMethod ) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod ); [CPreprocessorGenerated] private static bool AbstractVoidInjection1<TInheritingType>(object __instance, MethodBase __originalMethod,object __0) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0); [CPreprocessorGenerated] private static bool AbstractVoidInjection2<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1); [CPreprocessorGenerated] private static bool AbstractVoidInjection3<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2); [CPreprocessorGenerated] private static bool AbstractVoidInjection4<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3); [CPreprocessorGenerated] private static bool AbstractVoidInjection5<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4); [CPreprocessorGenerated] private static bool AbstractVoidInjection6<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5); [CPreprocessorGenerated] private static bool AbstractVoidInjection7<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5, object __6) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5, __6); [CPreprocessorGenerated] private static bool AbstractVoidInjection8<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5, __6, __7); [CPreprocessorGenerated] private static bool AbstractVoidInjection9<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5, __6, __7, __8); [CPreprocessorGenerated] private static bool AbstractVoidInjection10<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8, object __9) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5, __6, __7, __8, __9); [CPreprocessorGenerated] private static bool AbstractVoidInjection11<TInheritingType>(object __instance, MethodBase __originalMethod,object __0, object __1, object __2, object __3, object __4, object __5, object __6, object __7, object __8, object __9, object __10) => AbstractVoidInjectionAny<TInheritingType>(__instance, __originalMethod,__0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10);
	#line 90 "Utilities\\Patchers\\PatcherTargetedVariationMethods.cs"
#line 91 "Utilities\\Patchers\\PatcherTargetedVariationMethods.cs"

	}
}

