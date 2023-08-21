using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Maths {

	/// <summary>
	/// A 3x2 matrix used to store transformations. Consider using this class to do complicated sprite maths that emulate 3D rotation.
	/// <para/>
	/// The format of args is ROW, COLUMN.
	/// </summary>
	[Obsolete("This type is not yet ready.", true)]
	public readonly struct Matrix3x2 {

		// TODO: Use a 3D matrix (3x3, 4x4)? 
		// The goal is to provide transformations, but also the goal is to move sprites, which need to be scaled (i.e. shrink X to emulate yaw).

	}
}
