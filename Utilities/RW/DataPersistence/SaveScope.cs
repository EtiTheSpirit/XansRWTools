using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW.DataPersistence {
	/// <summary>
	/// The scope of any associated save data, which determines when/where it is stored and in what context(s).
	/// </summary>
	public enum SaveScope {

		/// <summary>
		/// This data is stored in the world itself, and may be reflected upon by other characters.
		/// </summary>
		Global,

		/// <summary>
		/// This data is stored for the character, persistent between cycles but irrelevant (and inaccessible) to other characters.
		/// </summary>
		Slugcat,

		/// <summary>
		/// This data is stored only for this cycle, and is wiped when the cycle changes (including when sleeping).
		/// </summary>
		Cycle,

		/// <summary>
		/// A value storing the number of save scopes. Do not use this to store data.
		/// </summary>
		Count

	}

}
