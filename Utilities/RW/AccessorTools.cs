using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW {
	public static class AccessorTools {

		/// <summary>
		/// Returns a reference to the game's instance of <see cref="RainWorld"/>
		/// </summary>
		public static RainWorld RainWorld => Custom.rainWorld;

		/// <summary>
		/// Returns a reference to the game's progression data.
		/// </summary>
		public static PlayerProgression Progression => RainWorld.progression;

		/// <summary>
		/// Returns a reference to the current save state at the time this is referenced.
		/// </summary>
		public static SaveState CurrentSave => Progression.currentSaveState;


	}
}
