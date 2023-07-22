using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW.SoundObjects {

	/// <summary>
	/// A variation of <see cref="RectangularDynamicSoundLoop"/> that disables the doppler effect regardless of the original audio source's settings.
	/// </summary>
	public class NoDopplerRectangularDynamicSoundLoop : RectangularDynamicSoundLoop {
		public NoDopplerRectangularDynamicSoundLoop(UpdatableAndDeletable owner, FloatRect rect, Room room) : base(owner, rect, room) { }

		public override void InitSound() {
			emitter = room.PlayRectSoundNoDoppler(sound, rect, true, v, p);
		}
	}
}
