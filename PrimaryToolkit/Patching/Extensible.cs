using HarmonyLib;
using MonoMod.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using XansTools.Exceptions;

namespace XansTools.PrimaryToolkit.Patching {

	/// <summary>
	/// This incredibly powerful class allows you to create "pseudo-inheritence" of any object type.
	/// <para/>
	/// Its primary use case is on classes like <see cref="Oracle"/> or <see cref="Player"/>, where the vanilla game (and thus, all mods)
	/// kind of stuff all their code into one place and make a huge mess. This works, but if you give any regard to code cleanliness,
	/// especially with respect to hooks, then you should no doubt be concerned at how horrifyingly terrible this is for code maintainability
	/// and cleanliness.
	/// <para/>
	/// This class allows you to create a mock extension of <typeparamref name="TOriginal"/>. Any methods declared in <typeparamref name="TReplacement"/>
	/// that match the names of their counterparts in <typeparamref name="TOriginal"/> will function as redirects, but will only execute iff
	/// <see cref="ShouldRedirect(TOriginal)"/> returns <see langword="true"/>.
	/// More specifically, declared methods should either match the signature of the hook itself, <em>or</em> omit the <c>orig</c> parameter 
	/// (always the first parameter). Doing the former (matching the hook) makes it behave like an ordinary hook that is simply automatically 
	/// connected on your behalf The latter will only run if <see cref="ShouldRedirect(TOriginal)"/> returns <see langword="true"/>.
	/// <para/>
	/// <strong>NOTE:</strong> This only captures declared members, not inherited members!
	/// </summary>
	[Obsolete("This is not ready to be used.", true)]
	public abstract class Extensible<TOriginal, TReplacement> where TOriginal : class where TReplacement : Extensible<TOriginal, TReplacement>, new() {

		private const BindingFlags VALID_HOOK_FLAGS = BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;
		private const BindingFlags VALID_EVENT_FLAGS = BindingFlags.Public | BindingFlags.Static;

		private static readonly ConditionalWeakTable<TOriginal, TReplacement> _bindings = new ConditionalWeakTable<TOriginal, TReplacement>();

		/// <summary>
		/// This is executed whenever a hook to an original method is called, and determines whether or not the code should redirect here.
		/// </summary>
		/// <param name="original"></param>
		/// <returns></returns>
		public abstract bool ShouldRedirect(TOriginal original);

		public static void InitializeExtensible(Type hookClassContainer) {
			try {
				MethodInfo shouldRedirect = typeof(TReplacement).GetMethod(nameof(ShouldRedirect), BindingFlags.Public);
				if (shouldRedirect.IsAbstract) throw new InvalidOperationException($"The {nameof(ShouldRedirect)} method that was matched is abstract! Declaring type: {shouldRedirect.DeclaringType.FullName}");

				MethodInfo getOrCreateCWT = typeof(ConditionalWeakTable<TOriginal, TReplacement>).GetMethod(nameof(ConditionalWeakTable<TOriginal, TReplacement>.GetOrCreateValue));
				if (getOrCreateCWT == null) throw new MissingMethodException($"Failed to find the getter for (or the indexer itself) in ConditionalWeakTable<{typeof(TOriginal).Name}, {typeof(TReplacement).Name}>!");

				FieldInfo bindings = typeof(Extensible<TOriginal, TReplacement>).GetField(nameof(_bindings), BindingFlags.NonPublic | BindingFlags.Static);
				if (bindings == null) throw new MissingMemberException($"Failed to find {nameof(_bindings)} field in Extensible!");

				/*
				ConstructorInfo[] allCtors = typeof(TOriginal).GetConstructors(BindingFlags.Public | BindingFlags.NonPublic);
				foreach (ConstructorInfo ctor in allCtors) {
					XansToolsMain.Harmony.Patch(ctor, prefix: new HarmonyMethod(typeof(Extensible<THook, TOriginal, TReplacement>).GetMethod(nameof(RegisterCWT), VALID_HOOK_FLAGS)));
				}
				*/

				MethodInfo[] hookMethods = typeof(TReplacement).GetMethods(VALID_HOOK_FLAGS);
				foreach (MethodInfo mtd in hookMethods) {
					EventInfo evt = FindMatchingEventDelegate(hookClassContainer, mtd, true, out bool isLiteralHook);
					if (evt != null) {
						MethodInfo invoke = evt.EventHandlerType.GetMethod("Invoke");
						if (isLiteralHook) {
							// Easy
							evt.AddEventHandler(null, mtd.CreateFastDelegate());
							Log.LogTrace($"Added AUTO-HOOK to method: {mtd.Name}");
						} else {
							// oh god oh fuck
							Log.LogTrace($"GENERATING BRANCHED HOOK to method: {mtd.Name} => <>_PROCBRANCH${mtd.Name}");
							Type[] allParams = invoke.GetParameters().Select(param => param.ParameterType).ToArray();
							Type[] allParamsExceptOrig = invoke.GetParameters().Skip(1).Select(param => param.ParameterType).ToArray();
							DynamicMethodDefinition invoker = new DynamicMethodDefinition($"<>_PROCBRANCH${mtd.Name}", invoke.ReturnType, allParams);
							ILGenerator generator = invoker.GetILGenerator();

							// Push all args onto the stack.
							for (int i = 1; i <= allParams.Length; i++) {
								generator.Emit(OpCodes.Ldarg, i);
							}
							generator.Emit(OpCodes.Ldarg_0); // Put 0 onto the stack last, for the end of this method.

							generator.Emit(OpCodes.Ldarg_1);                                            // Load arg1, this is the original object
							generator.Emit(OpCodes.Ldsfld, bindings);									// Load the bindings field
							generator.EmitCall(OpCodes.Call, getOrCreateCWT, null);	// Use arg1 to get the replacement from the CWT.
							generator.Emit(OpCodes.Callvirt, shouldRedirect);                           // Call the shouldRedirect method
							generator.Emit(OpCodes.Brfalse_S, 3);                                   // Jumper
							generator.Emit(OpCodes.Callvirt, mtd);                                      // Call the original method
							generator.Emit(OpCodes.Pop);                                                // And then take orig() off of the stack.
							generator.Emit(OpCodes.Ret);                                                // Exit
							generator.Emit(OpCodes.Calli);                                              // Call original.

							// return (FastReflectionDelegate)Extensions.CreateDelegate(dynamicMethodDefinition.Generate(), typeof(FastReflectionDelegate));
							evt.AddEventHandler(null, invoker.Generate().CreateFastDelegate());
						}
					}
				}
			} catch (Exception err) {
				Log.LogFatal(err);
				throw;
			}
		}

		[Obsolete("This technique is useless", true)]
		private static void RegisterCWT(object __instance) {
			_bindings.GetOrCreateValue((TOriginal)__instance);
		}

		private static EventInfo FindMatchingEventDelegate(Type hook, MethodInfo appliableToThisMethod, bool matchNames, out bool isLiteralHook) {
			if (matchNames) {
				EventInfo eventInfo = hook.GetEvent(appliableToThisMethod.Name, VALID_EVENT_FLAGS);
				isLiteralHook = MethodsEqual(eventInfo.RaiseMethod, appliableToThisMethod, matchNames, false);
				if (isLiteralHook || MethodsEqual(eventInfo.RaiseMethod, appliableToThisMethod, matchNames, true)) {
					return eventInfo;
				}
				isLiteralHook = default;
				return null;
			} else {
				EventInfo[] events = hook.GetEvents();
				foreach (EventInfo eventInfo in events) {
					isLiteralHook = MethodsEqual(eventInfo.RaiseMethod, appliableToThisMethod, matchNames, false);
					if (isLiteralHook || MethodsEqual(eventInfo.RaiseMethod, appliableToThisMethod, matchNames, true)) {
						return eventInfo;
					}
				}
				isLiteralHook = default;
				return null;
			}
		}

		private static bool MethodsEqual(MethodInfo left, MethodInfo right, bool matchNames, bool skipFirstParam) {
			if (left == null && right == null) return true;
			if (left == null || right == null) return false;

			if (left.ReturnType != right.ReturnType) return false;
			if (left.Name != right.Name && matchNames) return false;
			ParameterInfo[] leftParams = left.GetParameters();
			ParameterInfo[] rightParams = right.GetParameters();
			int leftLength = leftParams.Length;
			int rightLength = rightParams.Length;
			if (skipFirstParam) rightLength--;
			int offset = skipFirstParam ? 1 : 0;

			if (leftLength != rightLength) return false;
			for (int i = 0; i < rightLength; i++) {
				ParameterInfo leftParam = leftParams[i + offset];
				ParameterInfo rightParam = rightParams[i];
				if (leftParam.ParameterType != rightParam.ParameterType) return false;
				if (leftParam.IsIn != rightParam.IsIn) return false;
				if (leftParam.IsOut != rightParam.IsOut) return false;
				if (leftParam.IsOptional != rightParam.IsOptional) return false;
				if (leftParam.IsLcid != rightParam.IsLcid) return false;
			}
			return true;
		}

		
		public sealed class ForceUseConditionAttribute { }

		public sealed class ForceSkipConditionAttribute { }
	}
}
