using RWCustom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW {

	/// <summary>
	/// A set of tools relating to the game world.
	/// </summary>
	public static class WorldTools {

		/// <summary>
		/// A reference to the game core object.
		/// </summary>
		/// <exception cref="InvalidOperationException">If Rain World is not currently in a mode describing that of the playable game.</exception>
		public static RainWorldGame Game {
			get {
				if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game) {
					return game;
				}
				throw new InvalidOperationException("Rain World's current process is not the in-game process; getting the camera is not possible at this time.");
			}
		}

		/// <summary>
		/// Provides access to the current room that the player sees through. This is a shortcut to accessing the room property of <see cref="CurrentCamera"/>.
		/// </summary>
		/// <exception cref="InvalidOperationException">If Rain World is not currently in a mode describing that of the playable game.</exception>
		public static Room CurrentRoom => CurrentCamera.room;

		/// <summary>
		/// Provides access to the current camera that the player sees through. This returns camera #0 which may not correspond to player 0 in multiplayer, depending on setup.
		/// </summary>
		/// <exception cref="InvalidOperationException">If Rain World is not currently in a mode describing that of the playable game.</exception>
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
		/// <param name="playerNumber">The index of the player. Must be greater than or equal to 0.</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">If Rain World is not currently in a mode describing that of the playable game.</exception>
		public static Player GetPlayer(int playerNumber = 0) {
			if (playerNumber < 0) throw new ArgumentOutOfRangeException(nameof(playerNumber), "Negative player numbers are invalid.");

			if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game) {
				if (game.Players.Count <= playerNumber) throw new IndexOutOfRangeException("The provided player ordinal is too high.");
				if (game.Players[playerNumber].realizedCreature is Player player) {
					return player;
				}
			}
			throw new InvalidOperationException("Rain World's current process is not the in-game process; getting a player is not possible at this time.");
		}

		/// <summary>
		/// Returns all players.
		/// </summary>
		/// <returns></returns>
		/// <exception cref="IndexOutOfRangeException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static Player[] GetPlayers() {
			if (Custom.rainWorld.processManager.currentMainLoop is RainWorldGame game) {
				return game.Players.Where(ac => ac.realizedCreature is Player).Select(ac => (Player)ac.realizedCreature).ToArray();
			}
			throw new InvalidOperationException("Rain World's current process is not the in-game process; getting a player is not possible at this time.");
		}

		/// <summary>
		/// Allows accessing cameras for other players by their index. This may not be accurate, depending on how multiplayer is used.
		/// </summary>
		/// <param name="playerNumber">The index of the player. Must be greater than or equal to 0.</param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">If Rain World is not currently in a mode describing that of the playable game.</exception>
		/// <exception cref="IndexOutOfRangeException">If the player number is too large.</exception>
		/// <exception cref="ArgumentOutOfRangeException">If the player number is negative.</exception>
		public static RoomCamera GetCamera(int playerNumber = 0) {
			Player target = GetPlayer(playerNumber);
			return target.GetCamera();
		}

		/// <summary>
		/// Returns the camera following this creature, or null if no camera is following this creature.
		/// </summary>
		/// <param name="creature">The creature to get the camera of.</param>
		/// <returns></returns>
		public static RoomCamera GetCamera(this AbstractCreature creature) {
			RoomCamera[] cameras = creature.world.game.cameras;
			for (int i = 0; i < cameras.Length; i++) {
				if (creature.FollowedByCamera(i)) {
					return cameras[i];
				}
			}
			return null;
		}

		/// <inheritdoc cref="GetCamera(AbstractCreature)"/>
		public static RoomCamera GetCamera(this Creature creature) => GetCamera(creature.abstractCreature);
	}
}
