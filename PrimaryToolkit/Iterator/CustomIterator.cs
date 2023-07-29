using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.PrimaryToolkit.Iterator {

	/// <summary>
	/// The base class for custom iterators. Extend this class to make custom iterators.
	/// </summary>
	public abstract class CustomIterator : Oracle {
		protected CustomIterator(AbstractPhysicalObject abstractPhysicalObject, Room room) : base(abstractPhysicalObject, room) {
		}
	}
}
