using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OptionInterface;

namespace XansTools.RemixCfg {
	public static class Configuration {

		/// <summary>
		/// If true, highly detailed logs will be made.
		/// </summary>
		public static bool TraceLogging => _traceLogging.Value; // Default to true until configs load.

		/// <summary>
		/// If true, palettes will be reloaded from disk upon changing a room, such that the palette(s) used in the room that is being
		/// entered will be refreshed.
		/// </summary>
		public static bool RuntimePaletteReloading => _runtimePaletteReloading.Value;

		/// <summary>
		/// If true, the system will abuse how the game loads files via <see cref="AssetManager"/> by deleting palettes from the mergedmods folder.
		/// </summary>
		public static bool DestructivePaletteReloading => _destructivePaletteReloading.Value;

		#region Backing Fields

		#region Mod Meta
		private static Configurable<bool> _traceLogging;

		#endregion

		#region Debug
		private static Configurable<bool> _runtimePaletteReloading;
		private static Configurable<bool> _destructivePaletteReloading;
		#endregion

		#endregion

		#region Config Helpers

		internal static bool Initialized { get; private set; }

		private static ConfigHolder _config;

		private static string _currentSection = "No Section";

		private static List<string> _orderedCategories = new List<string>();
		private static IReadOnlyList<string> _orderedCategoriesCache = null;
		private static Dictionary<string, List<ConfigurableBase>> _allConfigs = new Dictionary<string, List<ConfigurableBase>>();
		private static IReadOnlyDictionary<string, IReadOnlyList<ConfigurableBase>> _allConfigsCache = null;
		private static Dictionary<string, string> _categoryDescriptions = new Dictionary<string, string>();

		private static void CreateConfig<T>(ref Configurable<T> field, T defaultValue, string name, string description, bool requiresRestart = false) {
			//field = _config.Bind(new ConfigDefinition(_currentSection, name), defaultValue, new ConfigDescription(description));
			string sanitizedName = new string(name.Where(c => char.IsLetterOrDigit(c) || c == '_').ToArray());
			field = _config.Bind(sanitizedName, defaultValue, new ConfigurableInfo(description, autoTab: _currentSection, tags: new object[] { name, requiresRestart }));
			if (_traceLogging != null) {
				Log.LogTrace($"Registered config entry: {name}");
			}
			_allConfigsCache = null;
			_orderedCategoriesCache = null;
			if (!_allConfigs.TryGetValue(_currentSection, out List<ConfigurableBase> entries)) {
				_orderedCategories.Add(_currentSection);
				entries = new List<ConfigurableBase>();
				_allConfigs[_currentSection] = entries;
			}

			entries.Add(field);
		}

		/// <summary>
		/// Returns a lookup from section => configs in that section. This can be used for Remix.
		/// </summary>
		/// <returns></returns>
		internal static void GetAllConfigs(out IReadOnlyDictionary<string, IReadOnlyList<ConfigurableBase>> lookup, out IReadOnlyList<string> categories) {
			if (_allConfigsCache == null || _orderedCategoriesCache == null) {
				Dictionary<string, IReadOnlyList<ConfigurableBase>> allCfgsRO = new Dictionary<string, IReadOnlyList<ConfigurableBase>>();
				foreach (KeyValuePair<string, List<ConfigurableBase>> entry in _allConfigs) {
					allCfgsRO[entry.Key] = entry.Value.AsReadOnly();
				}
				_allConfigsCache = allCfgsRO;
				_orderedCategoriesCache = _orderedCategories.AsReadOnly();
			}
			lookup = _allConfigsCache;
			categories = _orderedCategoriesCache;
		}

		private static void SetCurrentSectionDescription(string description) => _categoryDescriptions[_currentSection] = description;

		public static string GetCategoryDescription(string cat) {
			if (_categoryDescriptions.TryGetValue(cat, out string categoryDesc)) return categoryDesc;
			return "It seems Xan forgot to put a description on this category.";
		}

		#endregion

		/// <summary>
		/// This should not be called by you. It is called by the remix config screen class of this mod.
		/// </summary>
		/// <param name="cfg"></param>
		/// <exception cref="InvalidOperationException"></exception>
		internal static void Initialize(ConfigHolder cfg) {
			if (Initialized) throw new InvalidOperationException("Configurations have already been initialized!");
			Log.LogMessage("Initializing configuration file.");
			_config = cfg;

			_currentSection = "Mod Meta";
			SetCurrentSectionDescription("Settings that relate to the mod's internal behavior. These settings do not affect gameplay.");

			CreateConfig(ref _traceLogging, false, "Trace Logging", description:
$@"If enabled, logs will be highly detailed. This can negatively impact performance!
You should activate this if you are trying to find bugs in the mod.",
			false);
			Log.LogTrace("TRACE LOGGING IS ENABLED. The logs will be cluttered with information only useful when debugging, and trace entries will incur a performance cost. You have been warned!");

			_currentSection = "Debugging";
			SetCurrentSectionDescription("Settings that make tracking behavior and implementing features easier.");

			CreateConfig(ref _runtimePaletteReloading, false, "Runtime Palette Reloading", description:
$@"If enabled, going to another room will load its palette from disk and mutate 
the cached palette texture. This will cause a hitch upon changing rooms and may
also cause memory leaks and other unpredictable behavior. Only enable if needed!",
			false);

			CreateConfig(ref _destructivePaletteReloading, false, "Destructive Palette Reloading", description:
$@"[Only works when {_runtimePaletteReloading.info.Tags[0]} is enabled.]
If true, the game will delete modded palettes from the mergedmods directory. 
This allows it to load directly from the respective mod's own palettes folder,
but might also break other things that expect the merged option to exist.",
			false);

			Initialized = true;
		}
	}
}
