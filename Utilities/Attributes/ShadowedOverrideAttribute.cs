using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XansTools.Exceptions;

namespace XansTools.Utilities.Attributes {

	/// <summary>
	/// Indicates that a member functions as a "shadowed override". This term refers to a member that shadows that of its superclass,
	/// but which as its IL modified such that it behaves like an override.
	/// <para/>
	/// This is comparable to creating a hook and not calling the original method. As such, <strong>this method is discouraged unless you know that it is acceptable.</strong>
	/// <para/>
	/// Shadowed overrides can <strong>not</strong> call <see langword="base"/>!
	/// </summary>
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
	public class ShadowedOverrideAttribute : Attribute {
		private const BindingFlags COMMON_FLAGS = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

		/// <summary>
		/// The type that declared the original member. This may be null, and if it is, the declaring type is implicitly the direct base type.
		/// </summary>
		public Type DeclaringType { get; }

		/// <summary>
		/// Indicates that a member functions as a "shadowed override". This term refers to a member that shadows that of its superclass,
		/// but which as its IL modified such that it behaves like an override.
		/// <para/>
		/// Shadowed overrides can <strong>not</strong> call <see langword="base"/>!
		/// </summary>
		/// <param name="declaringType">The type that declares the original member. This can be null to implicitly point to the direct base class.</param>
		public ShadowedOverrideAttribute(Type declaringType = null) {
			DeclaringType = declaringType;	
		}

		/// <summary>
		/// Ensures the original and the shadow method have the same signature, then returns references to the two methods.
		/// </summary>
		/// <param name="inheritingType"></param>
		/// <param name="declaredName"></param>
		/// <param name="declared"></param>
		/// <param name="inherited"></param>
		/// <exception cref="MissingMethodException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		public void ValidateMethod(Type inheritingType, string declaredName, out MethodInfo declared, out MethodInfo inherited) {
			Type declaringType = DeclaringType ?? inheritingType.BaseType;
			declared = declaringType.GetMethod(declaredName, COMMON_FLAGS);
			inherited = inheritingType.GetMethod(declaredName, COMMON_FLAGS);
			if (declared == null) throw new MissingMethodException($"No such method \"{declaredName}\" of type {declaringType.FullName}");
			if (inherited == null) throw new MissingMethodException($"No such method \"{declaredName}\" of type {inheritingType.FullName}");
			MethodSignatureMismatchException.ThrowIfMismatched(declared, inherited);
			if (inherited.IsVirtual) throw new NotSupportedException("Due to how the patcher works, shadowed overrides cannot be virtual.");
		}

		/// <summary>
		/// Ensures the original and the shadow properties have the same type and accessors, then returns references to the two properties as well as the presence of get and/or set.
		/// </summary>
		/// <param name="inheritingType"></param>
		/// <param name="declaredName"></param>
		/// <param name="declared"></param>
		/// <param name="inherited"></param>
		/// <param name="declaredHasGet"></param>
		/// <param name="declaredHasSet"></param>
		/// <exception cref="MissingMemberException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		/// <exception cref="NotSupportedException"></exception>
		/// <exception cref="MethodSignatureMismatchException"></exception>
		public void ValidateProperty(Type inheritingType, string declaredName, out PropertyInfo declared, out PropertyInfo inherited, out bool declaredHasGet, out bool declaredHasSet) {
			Type declaringType = DeclaringType ?? inheritingType.BaseType;
			declared = declaringType.GetProperty(declaredName, COMMON_FLAGS);
			inherited = inheritingType.GetProperty(declaredName, COMMON_FLAGS);
			if (declared == null) throw new MissingMemberException($"No such property \"{declaredName}\" of type {declaringType.FullName}");
			if (inherited == null) throw new MissingMemberException($"No such property \"{declaredName}\" of type {inheritingType.FullName}");
			if (declared.PropertyType != inherited.PropertyType) throw new InvalidOperationException($"The types of the properties \"{declaredName}\" (of types {declaringType.FullName} and {inheritingType.FullName}) do not match.");
			if (inherited.GetMethod?.IsVirtual ?? inherited.SetMethod?.IsVirtual ?? false) throw new NotSupportedException("Due to how the patcher works, shadowed overrides cannot be virtual.");
			declaredHasGet = declared.GetMethod != null;
			declaredHasSet = declared.SetMethod != null;
			bool inheritedHasGet = inherited.GetMethod != null;
			bool inheritedHasSet = inherited.SetMethod != null;
			if (declaredHasGet != inheritedHasGet) throw new MethodSignatureMismatchException($"Property \"{declaredName}\" (of types {declaringType.FullName} and {inheritingType.FullName}) do not share getters; one has a getter while the other does not.");
			if (declaredHasSet != inheritedHasSet) throw new MethodSignatureMismatchException($"Property \"{declaredName}\" (of types {declaringType.FullName} and {inheritingType.FullName}) do not share setters; one has a setter while the other does not.");
		}

	}
}
