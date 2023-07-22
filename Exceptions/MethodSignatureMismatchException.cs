using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Exceptions {

	/// <summary>
	/// This exception is raised when two methods do not share signatures.
	/// </summary>
	public class MethodSignatureMismatchException : InvalidOperationException {

		public MethodSignatureMismatchException() : base("The signatures of the provided methods are not identical.") { }

		public MethodSignatureMismatchException(string message) : base(message) { }

		public static void ThrowIfMismatched(MethodInfo left, MethodInfo right, bool matchNames = true) {
			if (left.ReturnType != right.ReturnType) throw new MethodSignatureMismatchException();
			if (left.Name != right.Name && matchNames) throw new MethodSignatureMismatchException(); 
			ParameterInfo[] leftParams = left.GetParameters();
			ParameterInfo[] rightParams = right.GetParameters();
			if (leftParams.Length != rightParams.Length) throw new MethodSignatureMismatchException();
			for (int i = 0; i < leftParams.Length; i++) {
				ParameterInfo leftParam = leftParams[i];
				ParameterInfo rightParam = rightParams[i];
				if (leftParam.ParameterType != rightParam.ParameterType) throw new MethodSignatureMismatchException();
				if (leftParam.IsIn != rightParam.IsIn) throw new MethodSignatureMismatchException();
				if (leftParam.IsOut != rightParam.IsOut) throw new MethodSignatureMismatchException();
				if (leftParam.IsOptional != rightParam.IsOptional) throw new MethodSignatureMismatchException();
				if (leftParam.IsLcid != rightParam.IsLcid) throw new MethodSignatureMismatchException();
			}
		}

	}
}
