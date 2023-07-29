using RWCustom;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

namespace XansTools.AssetSystems {
	public static class AssetLoader {

		/// <summary>
		/// A cache from bundle name to bundle.
		/// </summary>
		private static readonly Dictionary<string, AssetBundle> _bundlesByName = new Dictionary<string, AssetBundle>();

		/// <summary>
		/// A cache of all shaders in a given bundle.
		/// </summary>
		private static readonly Dictionary<AssetBundle, Dictionary<string, FShader>> _knownShaders = new Dictionary<AssetBundle, Dictionary<string, FShader>>();

		/// <summary>
		/// Given the name of a file marked as "Embedded Resource" in the VS solution, this will load it as a unity <see cref="AssetBundle"/>.
		/// </summary>
		/// <param name="fullyQualifiedPath">The path to the resource. This begins with your namespace, then includes any folders down to the path of your asset. Example: <c>XansCharacter.assets.embedded.shaders</c></param>
		/// <param name="skipCache">If true, the internal cache used to quickly return existing bundles will be skipped, and the cache will be updated with the latest result from the call.</param>
		/// <returns></returns>
		public static AssetBundle LoadAssetBundleFromEmbeddedResource(string fullyQualifiedPath, bool skipCache = false) {
			Log.LogMessage($"Loading embedded asset bundle: {fullyQualifiedPath}");
			if (_bundlesByName.TryGetValue(fullyQualifiedPath, out AssetBundle bundle) && !skipCache) {
				Log.LogWarning($"Something attempted to call {nameof(LoadAssetBundleFromEmbeddedResource)} on a bundle that has already been loaded! Please store the loaded bundle into a variable instead of calling this method multiple times.");
				return bundle;
			}
			using (MemoryStream mstr = new MemoryStream()) {
				Stream str = Assembly.GetCallingAssembly().GetManifestResourceStream(fullyQualifiedPath);
				str.CopyTo(mstr);
				str.Flush();
				str.Close();
				Log.LogTrace("Bundle loaded into memory as byte[], processing with Unity...");
				bundle = AssetBundle.LoadFromMemory(mstr.ToArray());
				Log.LogTrace("Unity has successfully loaded this asset bundle from memory.");
				_bundlesByName[fullyQualifiedPath] = bundle;
				
				Log.LogTrace("Populating fast-access data (Unity Shaders => FShaders)...");
				Dictionary<string, FShader> bindings = LoadAllShadersAsFShader(bundle);
				_knownShaders[bundle] = bindings;
				
				// TODO: Other asset types?
				Log.LogTrace("Done. Bundle is ready for use.");
				return bundle;
			}
		}

		/// <summary>
		/// Operates much like <see cref="Shader.Find"/>, but in this case it loads an instance of <see cref="FShader"/> from a specific
		/// <see cref="AssetBundle"/> rather than searching globally.
		/// </summary>
		/// <remarks>
		/// It is strongly advised to manually store the return value of this method in some static field or property, so that it only needs to be
		/// called once for any given shader.
		/// </remarks>
		/// <param name="bundle">The bundle that was loaded.</param>
		/// <param name="shaderFullName">The name name of the shader as declared by its ShaderLab code (at the very top, where you put <c>Shader "path/to/myshader" { ...</c></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException">If no such shader exists with the provided name.</exception>
		public static FShader FindFShader(this AssetBundle bundle, string shaderFullName) {
			if (_knownShaders.TryGetValue(bundle, out Dictionary<string, FShader> lookup)) {
				if (lookup.TryGetValue(shaderFullName, out FShader shader)) {
					return shader;
				}
				throw new KeyNotFoundException($"Attempted to load a shader with the query \"{shaderFullName}\", but no such shader exists!");
			}

			// Someone loaded the bundle manually.
			// Populate the cache data late, and raise a warning to encourage them to use the intended method of loading.
			Log.LogWarning($"The provided AssetBundle ({bundle.name}) wasn't loaded with {nameof(LoadAssetBundleFromEmbeddedResource)}! The shader lookup needs to be constructed, please wait...");
			_knownShaders[bundle] = LoadAllShadersAsFShader(bundle);
			return FindFShader(bundle, shaderFullName); // Lazy recursive call but it keeps code clean.
		}

		/// <summary>
		/// This operates the same as <see cref="FindFShader(AssetBundle, string)"/> but assists with debugging by reporting unexpected quirks
		/// with the shader code.
		/// </summary>
		/// <param name="bundle"></param>
		/// <param name="shaderFullName"></param>
		/// <returns></returns>
		/// <exception cref="KeyNotFoundException">If no such shader exists with the provided name.</exception>
		public static FShader FindFShaderWithSanityCheck(this AssetBundle bundle, string shaderFullName) {
			FShader result = FindFShader(bundle, shaderFullName);
			Shader unityShader = result.shader;
			string renderType = unityShader.FindPassTagValue("RenderType");
			string queue = unityShader.FindPassTagValue("Queue");
			string projector = unityShader.FindPassTagValue("IgnoreProjector");
			string shadowcast = unityShader.FindPassTagValue("ForceNoShadowCasting");
			string batching = unityShader.FindPassTagValue("DisableBatching");
			string pipeline = unityShader.FindPassTagValue("RenderPipeline");
			if (!string.IsNullOrWhiteSpace(renderType)) Log.LogWarning($"Shader {shaderFullName} declares the RenderType tag! This does not work in Futile. The shader may not work as expected!");
			if (!string.IsNullOrWhiteSpace(queue)) Log.LogWarning($"Shader {shaderFullName} declares the Queue tag! Futile does not have a render queue. To draw this object before or after another, put the object using this shader into the appropriate FContainer.");
			if (!string.IsNullOrWhiteSpace(projector)) Log.LogWarning($"Shader {shaderFullName} declares the IgnoreProjector tag! Futile does not use projectors in any way, so this tag is useless.");
			if (bool.TryParse(shadowcast, out bool forceNoShadowCasting)) {
				if (forceNoShadowCasting) {
					Log.LogWarning($"Shader {shaderFullName} sets the ForceNoShadowCasting tag to true! This does not work in Futile. To prevent casting shadows, draw the shader after lights have drawn.");
				} else {
					Log.LogWarning($"Shader {shaderFullName} sets the ForceNoShadowCasting tag to false! Futile does not use Unity's shadow system, and draws shadows on all objects beneath lights regardless of this tag and the depth buffer.");
				}
			}
			if (bool.TryParse(batching, out bool disableBatching)) {
				if (disableBatching) {
					Log.LogWarning($"Shader {shaderFullName} sets the DisableBatching tag to true! This does nothing, and is completely useless. Consider removing the tag instead.");
				} else {
					Log.LogWarning($"Shader {shaderFullName} sets the DisableBatching tag to false! Futile does not support dynamic batching.");
				}
			} 
			if (!string.IsNullOrWhiteSpace(pipeline)) Log.LogWarning($"Shader {shaderFullName} sets the RenderPipeline tag! Futile always uses the same pipeline regardless of shader settings.");

			// TODO: How do I check ZWrite and Cull?
			// ^ Can't, this is only available to the editor.
			return result;
		}

		public static string FindPassTagValue(this Shader shader, string name, int passIndex = 0) => shader.FindPassTagValue(passIndex, new ShaderTagId(name)).name;

		#region Conversion Methods

		/// <summary>
		/// Loads all <see cref="Shader"/> classes from an <see cref="AssetBundle"/>, and converts each of them to an instance of
		/// <see cref="FShader"/>, assocating the shader's name (as declared in its shaderlab code, at the top, NOT the file name
		/// itself) with the instance of <see cref="FShader"/>.
		/// </summary>
		/// <param name="from"></param>
		/// <returns></returns>
		private static Dictionary<string, FShader> LoadAllShadersAsFShader(AssetBundle from) {
			return from.LoadAllAssets<Shader>().ToDictionary(shader => shader.name, shader => FShader.CreateShader(shader.name, shader));
		}

		#endregion
	}
}