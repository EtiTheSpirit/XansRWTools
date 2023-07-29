using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities {
	public static class Mathematical {

		/// <summary>
		/// The amount of ticks that occur per second in Rain World.
		/// </summary>
		public const int RW_TICKS_PER_SECOND = 40;

		/// <summary>
		/// Rain World's constant delta time, in seconds. Equal to 1/40th of a second.
		/// </summary>
		public const float RW_DELTA_TIME = 1f / RW_TICKS_PER_SECOND;

		#region IsOdd

		// value & 1 will be 1 if the 1 bit is set.
		// This is fine for positive numbers, but if the number is negative, then it is offset by -1 (so that FFFFFFFF is -1 instead of a duplicate of 0)
		// This means that the 1 bit being set on a *negative* value means its actually even instead of odd
		// This creates a truth table:
		// "f" will be (value & 1) (the 1 bit)
		// "s" will be (value >> 31) (the 32 or sign bit)
		// A B | OUT
		// 0 0 | 0
		// f 0 | 1
		// 0 s | 1
		// f s | 0
		// This might look familiar. It is: XOR
		// This rule generally applies to all integer types.

		/// <summary>
		/// Returns whether or not the provided value is odd without using division (modulo) nor branching; all checks are bitwise..
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		public static bool IsOdd(this byte value) {
			return (value & 1) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this sbyte value) {
			return unchecked((value & 1) ^ (value >> (sizeof(sbyte) * 8 - 1))) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this ushort value) {
			return (value & 1) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this short value) {
			return unchecked((value & 1) ^ (value >> (sizeof(short) * 8 - 1))) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this uint value) {
			return (value & 1) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this int value) {
			return unchecked((value & 1) ^ (value >> (sizeof(int) * 8 - 1))) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this ulong value) {
			return (value & 1) == 1;
		}

		/// <inheritdoc cref="IsOdd(byte)"/>
		public static bool IsOdd(this long value) {
			return unchecked((value & 1) ^ (value >> (sizeof(long) * 8 - 1))) == 1;
		}

		#endregion

		/// <summary>
		/// Receives a coordinate in tile space and translates it into world space.
		/// </summary>
		/// <param name="tileCoord"></param>
		/// <returns></returns>
		public static Vector2 TileToWorldCoord(this Vector2 tileCoord) {
			return new Vector2(10f + tileCoord.x * 20f, 10f + tileCoord.y * 20f);
		}

		/// <summary>
		/// Multiplies all components (r,g,b) of this color by its alpha. The result's alpha is always set to 1.0f.<para/>
		/// <c>return float4(color.rgb * color.a, 1.0f)</c>
		/// </summary>
		/// <param name="clrIn"></param>
		/// <returns></returns>
		public static Color AlphaAsIntensity(this Color clrIn) {
			// If only CPUs had vectored instructions.
			return new Color(
				clrIn.r * clrIn.a,
				clrIn.g * clrIn.a,
				clrIn.b * clrIn.a,
				1.0f
			);
		}

		/// <summary>
		/// Converts ticks into seconds.
		/// </summary>
		/// <param name="ticks"></param>
		/// <returns></returns>
		public static float TicksToSeconds(this int ticks) {
			return ticks / 40f;
		}

		/// <summary>
		/// Converts seconds into the nearest tick. Ticks have a resolution of 0.025 seconds.
		/// </summary>
		/// <param name="seconds"></param>
		/// <returns></returns>
		public static int SecondsToTicks(this float seconds) {
			seconds *= 40.0f;
			return Mathf.RoundToInt(seconds);
		}

		/// <summary>
		/// Treats the called value as a tick counter. Returns whether or not the provided time in seconds has passed.
		/// </summary>
		/// <param name="ticks"></param>
		/// <param name="timeInSeconds"></param>
		/// <returns></returns>
		public static bool HasTimeElapsed(this int ticks, float timeInSeconds) {
			return ticks / 40f >= timeInSeconds;
		}

		/// <summary>
		/// Uses square magnitude to determine if the provided vector has a magnitude greater than the distance provided.
		/// </summary>
		/// <param name="vector"></param>
		/// <param name="distance"></param>
		/// <returns></returns>
		public static bool VectorHasMagnitudeGE(this Vector2 vector, float distance) {
			float sqrDistance = distance * distance;
			return vector.sqrMagnitude >= sqrDistance;
		}

	}
}
