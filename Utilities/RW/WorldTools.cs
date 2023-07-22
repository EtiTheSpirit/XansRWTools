using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW {
	public static class WorldTools {

		/// <summary>
		/// Provides access to the current camera that the player(s) see through.
		/// </summary>
		public static RoomCamera CurrentCamera {
			get {
				if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game) {
					return game.cameras[0];
				}
				throw new InvalidOperationException("Rain World's current process is not the in-game process; getting the camera is not possible at this time.");
			}
		}

		/// <summary>
		/// Returns the player with the given ordinal (0 for player 1, 1 for player 2, so on).
		/// </summary>
		/// <param name="ordinal"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public static Player GetPlayer(int ordinal = 0) {
			if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game) {
				if (game.Players[ordinal].realizedCreature is Player player) {
					return player;
				}
			}
			throw new InvalidOperationException("Rain World's current process is not the in-game process; getting a player is not possible at this time.");
		}

	}
}
