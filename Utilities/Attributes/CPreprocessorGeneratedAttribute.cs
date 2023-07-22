using System;

/// <summary>
/// Should not be used manually. This denotes that a method, member, class, or any other object that supports attributes was generated
/// with the C standard preprocessor.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = true)]
public sealed class CPreprocessorGeneratedAttribute : Attribute {

	public const string NOTE = "This element was generated via the C Preprocessor.";
	
}
