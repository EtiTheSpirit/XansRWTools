using BepInEx;
using BepInEx.Bootstrap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.ModPresence {

	/// <summary>
	/// Provides tools for getting context of mods.
	/// </summary>
	public static class ModPresenceTool {

		/// <summary>
		/// Returns whether or not a mod with the provided ID is installed. This method caches its result.
		/// </summary>
		/// <param name="modId"></param>
		/// <returns></returns>
		public static bool IsModInstalled(string modId) {
			return Chainloader.PluginInfos.TryGetValue(modId, out _);
		}

	}
}
