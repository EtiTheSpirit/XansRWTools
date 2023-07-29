using Mono.Cecil.Cil;
using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using XansTools.RemixCfg;
using XansTools.Utilities.Cecil;

namespace XansTools.Utilities.RW.Debugging {
	public static class RuntimePaletteDriver {

		private static bool _hasUpdatedData = false;

		/// <summary>
		/// Binds palette IDs to the full file path to its original file before being copied into mergedmods. 
		/// <para/>
		/// <strong>This does NOT include vanilla palettes!</strong>
		/// </summary>
		private static readonly Dictionary<int, string> _paletteSources = new Dictionary<int, string>();

		internal static void Initialize() {
			On.Room.NowViewed += OnRoomBeingViewed;
			On.Room.Update += OnRoomUpdate;
			On.Room.PausedUpdate += OnRoomPausedUpdate;
		}

		private static void OnRoomPausedUpdate(On.Room.orig_PausedUpdate originalMethod, Room @this) {
			originalMethod(@this);
			if (Configuration.RuntimePaletteReloading) {
				if (!_hasUpdatedData) {
					_hasUpdatedData = true;
					ReloadAllPalettes();
				}
			}
		}

		private static void OnRoomUpdate(On.Room.orig_Update originalMethod, Room @this) {
			originalMethod(@this);
			if (Configuration.RuntimePaletteReloading) {
				if (!_hasUpdatedData) {
					_hasUpdatedData = true;
					ReloadAllPalettes();
				}
			}
		}

		private static void OnRoomBeingViewed(On.Room.orig_NowViewed originalMethod, Room @this) {
			originalMethod(@this);
			if (Configuration.RuntimePaletteReloading) {
				Log.LogTrace("Room has been viewed. Marking data as dirty...");
				_hasUpdatedData = false;
			}
		}

		/// <summary>
		/// Forcefully reloads the palettes in the current room from disk.
		/// </summary>
		public static void ReloadAllPalettes() {
			RoomCamera camera = WorldTools.CurrentCamera;
			if (camera.room == null) {
				Log.LogWarning("Cannot reload palettes for the current camera! Reason: Realized room is null.");
				return;
			}
			if (camera.room.abstractRoom == null) {
				Log.LogWarning("Cannot reload palettes for the current camera! Reason: Abstract room is null.");
				return;
			}
			Log.LogDebug($"Reloading palettes for room {(camera?.room?.abstractRoom?.name ?? "null")}...");
			Log.LogTrace($"Palette A: {camera.paletteA}");
			Log.LogTrace($"Palette B: {camera.paletteB}");

			Log.LogDebug("Locating and updating files for palettes...");
			string pathA = GetOriginalPalette(camera.paletteA);
			string pathB = GetOriginalPalette(camera.paletteB);
			string cachePathA = GetPaletteUsedByGame(camera.paletteA);
			string cachePathB = GetPaletteUsedByGame(camera.paletteB);
			if (pathA != null && cachePathA != null && pathA != cachePathA) {
				Log.LogTrace($"Updating palette A...");
				if (File.Exists(cachePathA) && cachePathA.Contains("mergedmods")) File.Delete(cachePathA);
				if (!Configuration.DestructivePaletteReloading) {
					File.Copy(pathA, cachePathA);
				}
				Log.LogTrace($"Done updating palette A.");
			}
			if (pathB != null && cachePathB != null && pathB != cachePathB) {
				Log.LogTrace($"Updating palette B...");
				if (File.Exists(cachePathB) && cachePathB.Contains("mergedmods")) File.Delete(cachePathB);
				if (!Configuration.DestructivePaletteReloading) {
					File.Copy(pathB, cachePathB);
				}
				Log.LogTrace($"Done updating palette B.");
			}

			camera.LoadPalette(camera.paletteA, ref camera.fadeTexA);
			camera.LoadPalette(camera.paletteB, ref camera.fadeTexB);
			// Above: Calls ApplyEffectColorsToPaletteTexture, do not need to do it again.
			camera.ApplyFade(); // Calls ApplyPalette(), do not do that.
		}

		/// <summary>
		/// Accesses the file location for a given palette. Returns null if no location exists.
		/// </summary>
		/// <param name="palette"></param>
		/// <returns></returns>
		private static string GetOriginalPalette(int palette) {
			if (palette < 80) { // 36
				Log.LogTrace("Skipping vanilla/MSC or invalid palette.");
				return null;
			}
			/*if (ModManager.MSC && palette < 80) {
				Log.LogTrace("Skipping native MSC palette.");
				return null;
			}*/
			if (_paletteSources.TryGetValue(palette, out string src)) return src;

			for (int i = ModManager.ActiveMods.Count - 1; i >= 0; i--) { 
				ModManager.Mod mod = ModManager.ActiveMods[i]; // The vanilla file resolvers count backwards.

				DirectoryInfo palsDir = new	DirectoryInfo(Path.Combine(mod.path, "palettes"));
				if (!palsDir.Exists) continue;
				Log.LogTrace($"Searching {mod.id} for palette #{palette}...");
				FileInfo palFile = new FileInfo(Path.Combine(palsDir.FullName, $"palette{palette}.png"));
				if (!palFile.Exists) palFile = new FileInfo(Path.Combine(palsDir.FullName, $"palette{palette}-1.png"));
				if (!palFile.Exists) continue;
				Log.LogTrace("Found it!");
				_paletteSources[palette] = palFile.FullName;
				return palFile.FullName;
			}
			Log.LogTrace("Didn't find the palette.");
			return null;
		}

		/// <summary>
		/// Accesses the file location for a given palette, but this time for the copy that the game itself is using.
		/// <para/>
		/// This may return a file in a mod directory.
		/// </summary>
		/// <param name="palette"></param>
		/// <returns></returns>
		public static string GetPaletteUsedByGame(int palette) {
			if (palette < 80) {
				Log.LogTrace("Skipping vanilla/MSC or invalid palette.");
				return null;
			}
			string path = AssetManager.ResolveFilePath($"palettes/palette{palette}.png");
			if (!File.Exists(path)) {
				path = AssetManager.ResolveFilePath($"palettes/palette{palette}-1.png");
			}
			if (!File.Exists(path)) return null;
			return path;
		}

	}
}
