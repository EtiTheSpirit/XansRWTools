using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Exceptions {

	/// <summary>
	/// This exception is raised when an IL patch fails.
	/// </summary>
	public class RuntimePatchFailureException : Exception {
		public RuntimePatchFailureException() : base("Failed to patch a method.") { }
		public RuntimePatchFailureException(string methodName) : base($"Failed to patch method \"{methodName}\".") { }
		public RuntimePatchFailureException(string methodName, string message) : base($"Failed to patch method \"{methodName}\":\n{message}") { }
		public RuntimePatchFailureException(string methodName, string message, Exception inner) : base($"Failed to patch method \"{methodName}\":\n{message}", inner) { }
		public RuntimePatchFailureException(string methodName, Exception inner) : base($"Failed to patch method \"{methodName}\".", inner) { }
		public RuntimePatchFailureException(MethodBase methodName) : base($"Failed to patch method: {methodName.FullDescription()}") { }
		public RuntimePatchFailureException(MethodBase methodName, string message) : base($"Failed to patch method: {methodName.FullDescription()}\n{message}") { }
		public RuntimePatchFailureException(MethodBase methodName, string message, Exception inner) : base($"Failed to patch method: {methodName.FullDescription()}\n{message}", inner) { }
		public RuntimePatchFailureException(MethodBase methodName, Exception inner) : base($"Failed to patch method: {methodName.FullDescription()}", inner) { }
		public RuntimePatchFailureException(Exception inner) : base($"Failed to patch a method.", inner) { }
		protected RuntimePatchFailureException(SerializationInfo info, StreamingContext context) : base(info, context) { }
	}
}
