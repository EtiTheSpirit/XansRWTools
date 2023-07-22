using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XansTools.Utilities.ModInit;

namespace XansTools {
	[BepInPlugin(PLUGIN_ID, PLUGIN_NAME, PLUGIN_VERSION)]
	public class Plugin : BaseUnityPlugin {

		public const string PLUGIN_NAME = "Xan's Tools";
		public const string PLUGIN_ID = "xanstools";
		public const string PLUGIN_VERSION = "1.0.0";

		private void Awake() {
			Log.Initialize(Logger);
			ErrorReporter.Initialize();

			Log.LogMessage($"{PLUGIN_NAME} Loaded, ready for use.");
		}

		
	}
}
