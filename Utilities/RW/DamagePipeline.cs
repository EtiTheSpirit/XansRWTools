using MonoMod.Cil;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities.RW {
	/// <summary>
	/// A utility class that provides access to different points in the damage pipeline of <see cref="Creature.Violence(BodyChunk, UnityEngine.Vector2?, BodyChunk, PhysicalObject.Appendage.Pos, Creature.DamageType, float, float)"/>.
	/// </summary>
	[Obsolete]
	public static class DamagePipeline {

		internal static void Initialize() {
			Log.LogMessage("Preparing Damage Pipeline...");
			IL.Creature.Violence += InjectViolence;
		}

		private static void InjectViolence(ILContext il) {
			ILCursor cursor = new ILCursor(il);

		}

		#region Events

		/// <summary>
		/// This event fires before all others, and allows mutating the arguments provided to the function.
		/// </summary>
		public static event InterceptIncomingValues OnInterceptArgsOpportunity;

		/// <summary>
		/// This event fires just before calling <see cref="Creature.SetKillTag(AbstractCreature)"/>, and allows intercepting + replacing the creature that will be killed.
		/// </summary>
		public static event SettingKillTag OnSettingKillTag;

		/// <summary>
		/// This event fires when determining the velocity to set the hit chunk to.
		/// </summary>
		public static event DeterminingHitChunkVelocity OnDeterminingHitChunkVelocity;

		/// <summary>
		/// This event fires when getting the base damage resistance of the creature that was hit, and can be used to replace it.
		/// <para/>
		/// This is the <em>base</em> resistance. There are other resistances. See <see cref="OnReplaceDamageBeforeResistanceOpportunity"/>.
		/// </summary>
		public static event InterceptFloatDelegate OnGettingBaseDamageResistance;

		/// <summary>
		/// This event fires when getting the base stun resistance of the creature that was hit, and can be used to replace it.
		/// <para/>
		/// This is the <em>base</em> resistance. There are other resistances. See <see cref="OnReplaceStunBeforeResistanceOpportunity"/>.
		/// </summary>
		public static event InterceptFloatDelegate OnGettingBaseStunResistance;

		/// <summary>
		/// This event fires after evaluating the damage that will be applied, and can be used to directly affect the damage
		/// before per-creature resistances are applied.
		/// </summary>
		public static event InterceptFloatDelegate OnReplaceDamageBeforeResistanceOpportunity;

		/// <summary>
		/// This event fires after evaluating the damage that will be applied, and can be used to directly affect the damage
		/// after per-creature resistances are applied.
		/// </summary>
		public static event InterceptFloatDelegate OnReplaceDamageAfterResistanceOpportunity;

		/// <summary>
		/// This event fires after evaluating the stun ticks that will be applied, and can be used to directly affect the stun ticks
		/// before per-creature resistances are applied.
		/// </summary>
		public static event InterceptFloatDelegate OnReplaceStunBeforeResistanceOpportunity;

		/// <summary>
		/// This event fires after evaluating the stun ticks that will be applied, and can be used to directly affect the stun ticks
		/// after per-creature resistances are applied.
		/// </summary>
		public static event InterceptFloatDelegate OnReplaceStunAfterResistanceOpportunity;

		/// <summary>
		/// This event fires after evaluating amount of ticks of stun that will be applied, and can be used to directly affect the duration.
		/// <para/>
		/// Note that these ticks are cast to <see cref="int"/>.
		/// </summary>
		public static event InterceptFloatDelegate OnReplaceStunTicksOpportunity;

		/// <summary>
		/// This event fires when getting the amount of damage that is guaranteed to oneshot the target, and can be used to modify it.
		/// </summary>
		public static event InterceptFloatDelegate OnGettingInstakillDamageAmount;

		/// <summary>
		/// This event fires when getting whether or not the quick death field is set. This field allows death to occur from damage under the
		/// following conditions:
		/// <list type="number">
		/// <item>The creature's state is or extends <see cref="HealthState"/>, and...</item>
		/// <item>A random value [0-1] is less than -health (this is very strange, but basically if health is in the range of 0 to -1, it has a probability for killing that increases based on how much health is lost), or...</item>
		/// <item>Health is less than -1, or...</item>
		/// <item>Health is less than zero, and a random value [0-1] rolls 0.333 or less (1/3rd chance of death), assuming the second option failed.</item>
		/// </list>
		/// Most creatures have this feature enabled. With it disabled, the creature must be explicitly killed by calling <see cref="Creature.Die"/> directly,
		/// or the creature must take more damage than its instakill threshold, otherwise absolutely nothing happens to it.
		/// </summary>
		public static event InterceptBoolDelegate OnGettingAllowQuickDeath;

		#endregion

		#region Delegates

		/// <summary>
		/// This can be used to change the values of the parameters before any other events get fired.
		/// </summary>
		/// <param name="vanilla"></param>
		/// <param name="current"></param>
		/// <returns></returns>
		public delegate ViolentParameters InterceptIncomingValues(ViolentParameters vanilla, ViolentParameters current);

		/// <summary>
		/// Execuites before setting the kill tag of the creature that Violence was called on. This can be used to "shift the blame" per se.
		/// </summary>
		/// <param name="parameters">The parameters passed into the Violence method.</param>
		/// <param name="vanilla">The creature that the kill tag was going to be set to by vanilla.</param>
		/// <param name="current">The creature that the kill tag is going to be set to, either by vanilla or by another hook.</param>
		/// <returns>The replacement creature to set the kill tag to.</returns>
		public delegate void SettingKillTag(ViolentParameters parameters, in AbstractCreature vanilla, in AbstractCreature current);

		/// <summary>
		/// Executes when determining the velocity to apply to a hit body part.
		/// </summary>
		/// <param name="parameters">The parameters passed into the Violence method.</param>
		/// <param name="currentVelocity">The velocity that will be set. Change this to change the velocity.</param>
		/// <param name="applyClamp">If true, the velocity is clamped at 10 (this is vanilla behavior). If false it can exceed its limit.</param>
		public delegate void DeterminingHitChunkVelocity(ViolentParameters parameters, in Vector2 vanillaVelocity, ref Vector2 currentVelocity, ref bool applyClamp);

		/// <summary>
		/// Intercepts a <see cref="float"/> value before something is done to it. The event that this is a part of will have more information.
		/// </summary>
		/// <param name="parameters">The parameters passed into the Violence method.</param>
		/// <param name="vanilla">The original value that vanilla would use.</param>
		/// <param name="current">The current value, either set by vanilla or by another hook.</param>
		/// <returns>The replacement value</returns>
		public delegate float InterceptFloatDelegate(ViolentParameters parameters, in float vanilla, in float current);

		/// <summary>
		/// Intercepts an <see cref="bool"/> value before something is done to it. The event that this is a part of will have more information.
		/// </summary>
		/// <param name="parameters"></param>
		/// <param name="vanilla"></param>
		/// <param name="current"></param>
		/// <returns></returns>
		public delegate bool InterceptBoolDelegate(ViolentParameters parameters, in bool vanilla, in bool current);

		#endregion

		/// <summary>
		/// A utility struct to keep track of all the parameters used in the Violence method.
		/// </summary>
		public readonly ref struct ViolentParameters {

			/// <summary>
			/// The source of the damage. Remember that this may be a <see cref="Weapon"/>'s body chunk.
			/// </summary>
			public readonly BodyChunk source;

			public readonly Vector2? directionAndMomentum;

			public readonly BodyChunk hitChunk;

			public readonly PhysicalObject.Appendage.Pos hitAppendage;

			public readonly Creature.DamageType damageType;

			public readonly float damage;

			public readonly float stunBonus;

			public ViolentParameters(BodyChunk source, Vector2? directionAndMomentum, BodyChunk hitChunk, PhysicalObject.Appendage.Pos hitAppendage, Creature.DamageType damageType, float damage, float stunBonus) {
				this.source = source;
				this.directionAndMomentum = directionAndMomentum;
				this.hitChunk = hitChunk;
				this.hitAppendage = hitAppendage;
				this.damageType = damageType;
				this.damage = damage;
				this.stunBonus = stunBonus;
			}

		}
	}
}
