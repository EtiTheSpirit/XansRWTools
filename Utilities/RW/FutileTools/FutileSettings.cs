using System;
using UnityEngine;

namespace XansTools.Utilities.RW.FutileTools {
	public static class FutileSettings {

		/// <summary>
		/// The amount of bits needed on the depth texture 
		/// </summary>
		public const int DEPTH_BUFFER_BITS = 16;
		public const int DEPTH_AND_STENCIL_BUFFER_BITS = 24;

		private static int _highestRequestedBits = 0;

		/// <summary>
		/// Requests that the depth buffer is enabled, setting the depth bit count of the main screen to 16 bits.
		/// <para/>
		/// Note that this will <strong>NOT</strong> disable the stencil buffer. If the bit count is higher, it will remain at its higher value.
		/// </summary>
		public static void RequestDepthBuffer() {
			_highestRequestedBits = Math.Max(_highestRequestedBits, DEPTH_BUFFER_BITS);
			if (Futile.screen != null) {
				RenderTexture rt = Futile.screen.renderTexture;
				if (rt.depth < _highestRequestedBits) {
					rt.depth = _highestRequestedBits;
					Log.LogTrace("Enabled Depth Buffer.");
				}
			}
		}

		/// <summary>
		/// Requests that the stencil buffer is enabled, setting the depth bit count of the main screen to 24 bits.
		/// </summary>
		public static void RequestDepthAndStencilBuffer() {
			_highestRequestedBits = Math.Max(_highestRequestedBits, DEPTH_AND_STENCIL_BUFFER_BITS);
			if (Futile.screen != null) {
				RenderTexture rt = Futile.screen.renderTexture;
				if (rt.depth < _highestRequestedBits) {
					rt.depth = _highestRequestedBits;
					Log.LogTrace("Enabled Depth and Stencil Buffers.");
				}
			}
		}

		internal static void Initialize() {
			Log.LogMessage("Retrofitting a (nonfunctional) Depth Buffer and a (very functional) Stencil Buffer into Futile...");
			On.FScreen.ctor += OnConstructingFScreen;
			On.FScreen.ReinitRenderTexture += OnReinitializeRT;
		}

		private static void OnReinitializeRT(On.FScreen.orig_ReinitRenderTexture originalMethod, FScreen @this, int displayWidth) {
			originalMethod(@this, displayWidth);
			@this.renderTexture.depth = _highestRequestedBits;
		}

		private static void OnConstructingFScreen(On.FScreen.orig_ctor originalCtor, FScreen @this, FutileParams futileParams) {
			originalCtor(@this, futileParams);
			@this.renderTexture.depth = _highestRequestedBits;
		}

	}
}
