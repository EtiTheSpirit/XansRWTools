using Mono.Cecil;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Text;

namespace XansTools.Utilities.Cecil {

	/// <summary>
	/// Provides extensions to <see cref="Mono.Cecil.Cil.Instruction"/> that allow matching instructions in a more mnemonic fashion (such as by name).
	/// </summary>
	public static class CecilExtensions {

		#region Extended Matches (nongeneric)

		/// <summary>
		/// Matches a <c>ldfld</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdfld(this Instruction instruction, string fieldName) {
			return instruction.MatchLdfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchLdfld(Instruction, string)"/>
		public static bool MatchLdfld(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchLdfld(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldsfld</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdsfld(this Instruction instruction, string fieldName) {
			return instruction.MatchLdsfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchLdsfld(Instruction, string)"/>
		public static bool MatchLdsfld(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchLdsfld(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>stfld</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchStfld(this Instruction instruction, string fieldName) {
			return instruction.MatchStfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchStfld(Instruction, string)"/>
		public static bool MatchStfld(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchStfld(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>stsfld</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchStsfld(this Instruction instruction, string fieldName) {
			return instruction.MatchStsfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchStsfld(Instruction, string)"/>
		public static bool MatchStsfld(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchStsfld(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldflda</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdflda(this Instruction instruction, string fieldName) {
			return instruction.MatchLdflda(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchLdflda(Instruction, string)"/>
		public static bool MatchLdflda(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchLdflda(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldsflda</c> instruction by its field name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdsflda(this Instruction instruction, string fieldName) {
			return instruction.MatchLdsflda(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <inheritdoc cref="MatchLdsflda(Instruction, string)"/>
		public static bool MatchLdsflda(this Instruction instruction, string fieldName, out FieldReference fld) {
			return instruction.MatchLdsflda(out fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>call</c> instruction by its method name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchCall(this Instruction instruction, string methodName) {
			return instruction.MatchCall(out MethodReference mtd) && mtd.Name == methodName;
		}

		/// <summary>
		/// Matches a <c>callvirt</c> instruction by its method name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCallvirt(this Instruction instruction, string methodName) {
			return instruction.MatchCallvirt(out MethodReference mtd) && mtd.Name == methodName;
		}

		/// <inheritdoc cref="MatchCall(Instruction, string)"/>
		public static bool MatchCall(this Instruction instruction, string methodName, out MethodReference mtd) {
			return instruction.MatchCall(out mtd) && mtd.Name == methodName;
		}

		/// <inheritdoc cref="MatchCallvirt(Instruction, string)"/>
		public static bool MatchCallvirt(this Instruction instruction, string methodName, out MethodReference mtd) {
			return instruction.MatchCallvirt(out mtd) && mtd.Name == methodName;
		}

		/// <summary>
		/// Matches a <c>call</c> or <c>callvirt</c> instruction by its method name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCallOrCallvirt(this Instruction instruction, string methodName) {
			return instruction.MatchCall(methodName) || instruction.MatchCallvirt(methodName);
		}

		/// <summary>
		/// Matches a <c>call</c> or <c>callvirt</c> instruction by its method name without care for the type. Prefers <c>call</c> over <c>callvirt</c>.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCallOrCallvirt(this Instruction instruction, string methodName, out MethodReference mtd) {
			return instruction.MatchCall(methodName, out mtd) || instruction.MatchCallvirt(methodName, out mtd);
		}

		/// <summary>
		/// Matches a method call to a property's getter by its name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static bool MatchGetProp(this Instruction instruction, string propertyName) {
			return instruction.MatchCallOrCallvirt($"get_{propertyName}");
		}

		/// <summary>
		/// Matches a method call to a property's setter by its name without care for the type.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static bool MatchSetProp(this Instruction instruction, string propertyName) {
			return instruction.MatchCallOrCallvirt($"set_{propertyName}");
		}

		/// <inheritdoc cref="MatchGetProp(Instruction, string)"/>
		public static bool MatchGetProp(this Instruction instruction, string propertyName, out MethodReference getter) {
			return instruction.MatchCallOrCallvirt($"get_{propertyName}", out getter);
		}

		/// <inheritdoc cref="MatchSetProp(Instruction, string)"/>
		public static bool MatchSetProp(this Instruction instruction, string propertyName, out MethodReference setter) {
			return instruction.MatchCallOrCallvirt($"set_{propertyName}", out setter);
		}
		#endregion

		#region Extended Matches (generic, not ready)
		/*

		/// <summary>
		/// Matches a <c>ldfld</c> instruction by its field name.
		/// </summary>
		/// <typeparam name="T">The type that the field is declared on.</typeparam>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdfld<T>(this Instruction instruction, string fieldName, bool allowInherited = true) {
			return instruction.MatchLdfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldsfld</c> instruction by its field name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdsfld<T>(this Instruction instruction, string fieldName) {
			return instruction.MatchLdsfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>stfld</c> instruction by its field name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchStfld<T>(this Instruction instruction, string fieldName, bool allowInherited = true) {
			return instruction.MatchStfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>stsfld</c> instruction by its field name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchStsfld<T>(this Instruction instruction, string fieldName) {
			return instruction.MatchStsfld(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldflda</c> instruction by its field name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdflda<T>(this Instruction instruction, string fieldName, bool allowInherited = true) {
			return instruction.MatchLdflda(out FieldReference fld) && fld.Name == fieldName;
		}

		/// <summary>
		/// Matches a <c>ldsflda</c> instruction by its field name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="fieldName"></param>
		/// <returns></returns>
		public static bool MatchLdsflda<T>(this Instruction instruction, string fieldName) {
			return instruction.MatchLdsflda(out FieldReference fld) && fld.Name == fieldName && instruction.oper;
		}

		/// <summary>
		/// Matches a <c>call</c> instruction by its method name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCall<T>(this Instruction instruction, string methodName) {
			return instruction.MatchCall(out MethodReference mtd) && mtd.Name == methodName && mtd.DeclaringType.FullName == typeof(T).FullName;
		}

		/// <summary>
		/// Matches a <c>callvirt</c> instruction by its method name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCallvirt<T>(this Instruction instruction, string methodName) {
			return instruction.MatchCallvirt(out MethodReference mtd) && mtd.Name == methodName && mtd.DeclaringType.FullName == typeof(T).FullName;
		}

		/// <summary>
		/// Matches a <c>call</c> or <c>callvirt</c> instruction by its method name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="methodName"></param>
		/// <returns></returns>
		public static bool MatchCallOrCallvirt<T>(this Instruction instruction, string methodName) {
			return instruction.MatchCall<T>(methodName) || instruction.MatchCallvirt<T>(methodName);
		}

		/// <summary>
		/// Matches a method call to a property's getter by its name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static bool MatchGetProp<T>(this Instruction instruction, string propertyName, bool allowInherited = true) {
			if (allowInherited) {
				return instruction.MatchCallOrCallvirt<T>($"get_{propertyName}");
			} else {
				return instruction.MatchCall<T>($"get_{propertyName}");
			}
		}

		/// <summary>
		/// Matches a method call to a property's setter by its name.
		/// </summary>
		/// <param name="instruction"></param>
		/// <param name="propertyName"></param>
		/// <returns></returns>
		public static bool MatchSetProp<T>(this Instruction instruction, string propertyName, bool allowInherited = true) {
			if (allowInherited) {
				return instruction.MatchCallOrCallvirt<T>($"set_{propertyName}");
			} else {
				return instruction.MatchCall<T>($"set_{propertyName}");
			}
		}

		*/
		#endregion

		#region Utility Replacements

		public static void DumpToLog(this ILCursor cursor, Action<string> log, int startOffset = -5, int numInstructions = 10) {
			cursor.Index += startOffset;
			for (int i = 0; i < numInstructions; i++) {
				log(cursor.Instrs[cursor.Index].ToStringFixed());
				cursor.Index++;
			}
		}

		/// <summary>
		/// A version of <see cref="Instruction.ToString"/> that doesn't shit the bed when it encounters a br* instruction.
		/// </summary>
		/// <param name="instruction"></param>
		/// <returns></returns>
		public static string ToStringFixed(this Instruction instruction) {
			object operand = instruction.Operand;
			OpCode opcode = instruction.OpCode;

			StringBuilder stringBuilder = new StringBuilder();
			AppendLabel(stringBuilder, instruction);
			stringBuilder.Append(':');
			stringBuilder.Append(' ');
			stringBuilder.Append(opcode.Name);
			if (instruction.Operand == null) {
				return stringBuilder.ToString();
			}

			stringBuilder.Append(' ');
			switch (opcode.OperandType) {
				case OperandType.InlineBrTarget:
				case OperandType.ShortInlineBrTarget:
					AppendLabel(stringBuilder, operand);
					break;
				case OperandType.InlineSwitch: {
						Array array = (Array)operand;
						for (int i = 0; i < array.Length; i++) {
							if (i > 0) {
								stringBuilder.Append(',');
							}

							AppendLabel(stringBuilder, array.GetValue(i));
						}

						break;
					}
				case OperandType.InlineString:
					stringBuilder.Append('"');
					stringBuilder.Append(operand);
					stringBuilder.Append('"');
					break;
				default:
					stringBuilder.Append(operand);
					break;
			}

			return stringBuilder.ToString();
		}

		private static void AppendLabel(StringBuilder builder, Instruction instruction) {
			builder.Append("IL_");
			builder.Append(instruction.Offset.ToString("x4"));
		}
		private static void AppendLabel(StringBuilder builder, ILLabel label) => AppendLabel(builder, label.Target);

		private static void AppendLabel(StringBuilder builder, object operand) {
			if (operand is ILLabel label) {
				AppendLabel(builder, label);
			} else if (operand is Instruction instruction) {
				AppendLabel(builder, instruction);
			} else {
				throw new ArgumentException("Unable to get an offset from the provided operand.");
			}
		}

		#endregion
	}
}
