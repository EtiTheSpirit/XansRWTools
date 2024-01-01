using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XansTools.RemixCfg;
using XansTools.Utilities.ModInit;
using XansTools.Utilities.RW.DataPersistence;
using XansTools.Utilities.RW.Debugging;
using XansTools.Utilities.RW.FutileTools;
using XansTools.Utilities.RW.Shaders;

namespace XansTools {
	[BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
	public class XansToolsMain : BaseUnityPlugin {

		public const string PLUGIN_NAME = "Xan's Tools";
		public const string PLUGIN_ID = "xanstools";
		public const string PLUGIN_VERSION = "1.0.0";
		private RemixConfigScreen _cfgScr;

		internal static ErrorReporter Reporter { get; private set; }

		public static Harmony Harmony { get; private set; }

		private void Awake() {
			Log.Initialize(Logger);
			_cfgScr = RemixConfigScreen.BIE_Initialize();
			Reporter = new ErrorReporter(this);
			Harmony = new Harmony(PLUGIN_NAME);

			ErrorReporter.Initialize();
			FutileSettings.Initialize();
			RuntimePaletteDriver.Initialize();
			ShaderOptimizations.Initialize();
			SaveDataAccessor.Initialize();

			On.RainWorld.OnModsInit += OnModsInitializing;

			Log.LogMessage($"{PLUGIN_NAME} Loaded, ready for use.");
		}

		private void OnModsInitializing(On.RainWorld.orig_OnModsInit originalMethod, RainWorld @this) {
			originalMethod(@this);
			try {
				MachineConnector.SetRegisteredOI(PLUGIN_ID, _cfgScr);
			} catch (Exception exc) {
				Log.LogFatal(exc);
				Reporter.DeferredReportModInitError(exc, $"Registering the Remix config menu to {PLUGIN_NAME}");
				throw;
			}
		}
	}
}
