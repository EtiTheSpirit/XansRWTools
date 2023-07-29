using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XansTools.Utilities.Attributes;
using SpriteLeaser = RoomCamera.SpriteLeaser;

namespace XansTools.PrimaryToolkit.Iterator {

	/// <summary>
	/// This class manages and stores graphics related to a custom iterator.
	/// </summary>
	public abstract class CustomIteratorGraphics<T> : OracleGraphics where T : CustomIterator {

		#region Shadowed Overrides and Base Implementation

		[Obsolete("Consider using the replacement property, Iterator.")]
		public new Oracle oracle { get; }

		/// <summary>
		/// A reference to the <see cref="CustomIterator"/> that this graphics module exists for.
		/// </summary>
		public T Iterator { get; }

		/// <summary>
		/// Returns the index for the iterator's leg sprites.
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		/// <param name="side">Which side is this part on? 0 should indicate left, and 1 right.</param>
		/// <param name="part">The part number, where 0 is at the hip (such as the thigh) and as the value increases, it goes down the leg.</param>
		/// <returns></returns>
		[ShadowedOverride]
		public new abstract int FootSprite(int side, int part);

		/// <summary>
		/// Returns the index for the iterator's arm sprites.
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		/// <param name="side">Which side is this part on? 0 should indicate left, and 1 right.</param>
		/// <param name="part">The part number, where 0 is at the shoulder (such as the upper arm) and as the value increases, it goes down the arm.</param>
		/// <returns></returns>
		[ShadowedOverride]
		public new abstract int HandSprite(int side, int part);

		/// <summary>
		/// Returns the index for the iterator's "headphones" (for lack of a better term).
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		/// <param name="side">Which side is this part on? 0 should indicate left, and 1 right.</param>
		/// <param name="part">The part number for the headphones. This is relatively arbitrary but in general, 0 should represent the base and higher numbers should represent the tips or endings of whatever adornments you have chosen to place.</param>
		/// <returns></returns>
		[ShadowedOverride]
		public new abstract int PhoneSprite(int side, int part);

		/// <summary>
		/// Returns the sprites representing the iterator's eyes.
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		/// <param name="eyeIndex">Which side is this part on? 0 should indicate left, and 1 right.</param>
		/// <returns></returns>
		[ShadowedOverride]
		public new abstract int EyeSprite(int eyeIndex);

		/// <summary>
		/// Returns the sprites representing the iterator's head. This is a more generalized variant of <see cref="HeadSprite"/> that accounts for more than one head sprite.
		/// </summary>
		/// <remarks>
		/// For compatibility, 0 should always be the "root" sprite (see <see cref="HeadSprite"/>), and 1 should always be the chin sprite (or whatever is best applicable, see <see cref="ChinSprite"/>)
		/// </remarks>
		/// <param name="part"></param>
		/// <returns></returns>
		public abstract int GetHeadPartSprite(int part);

		/// <summary>
		/// Returns the head sprite. By default, this calls <see cref="GetHeadPartSprite(int)"/> with an argument of <c>0</c>.
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		[ShadowedOverride]
		public new virtual int HeadSprite => GetHeadPartSprite(0);

		/// <summary>
		/// Returns the chin sprite. By default, this calls <see cref="GetHeadPartSprite(int)"/> with an argument of <c>1</c>.
		/// <para/>
		/// <strong>DANGER: This is a Shadowed Override (see <see cref="ShadowedOverrideAttribute"/>)!</strong> Do not call <see langword="base"/>, as it will raise a <see cref="StackOverflowException"/>.
		/// </summary>
		[ShadowedOverride]
		public new virtual int ChinSprite => GetHeadPartSprite(1);

		/// <summary>
		/// Optional. This should represent the background sprite of the symbol on this iterator's head, if they have one.
		/// </summary>
		public virtual int ForeheadSymbolBackgroundSprite { get; }

		/// <summary>
		/// Optional. This should represent the actual sprite of the symbol on this iterator's head, if they have one.
		/// </summary>
		public virtual int ForeheadSymbolSprite { get; }

		[ShadowedOverride, Obsolete("For mnemonic purposes, consider using ForeheadSymbolBackgroundSprite.")]
		private new int MoonThirdEyeSprite => ForeheadSymbolBackgroundSprite;

		[ShadowedOverride, Obsolete("For mnemonic purposes, consider using ForeheadSymbolSprite.")]
		private new int MoonSigilSprite => ForeheadSymbolSprite;

		#endregion

		#region Custom Iterator Properties

		/// <summary>
		/// If true, this iterator will generate sprites for a halo.
		/// </summary>
		public bool HasHalo { get; }

		/// <summary>
		/// If true, this iterator will generate sprites for a gown or robe.
		/// </summary>
		public bool HasGown { get; }

		#endregion

		public CustomIteratorGraphics(T iterator, PhysicalObject ow) : base(ow) {
			Iterator = iterator;
		}

		/// <summary>
		/// This method should construct the iterator's model. Its default implementation calls the abstract utility methods uesd to build iterator body parts.
		/// </summary>
		/// <param name="sLeaser"></param>
		/// <param name="rCam"></param>
		public override void InitiateSprites(SpriteLeaser sLeaser, RoomCamera rCam) {
			List<FSprite> sprites = new List<FSprite>();
			totalSprites = 0;
			int currentBodyPartIndex = 0; // Don't use totalSprites to prevent mistakes.

			BuildIteratorTorso(sprites, rCam, ref currentBodyPartIndex);
			BuildIteratorHead(sprites, rCam, ref currentBodyPartIndex);
			BuildIteratorArm(sprites, rCam, ref currentBodyPartIndex, true);
			BuildIteratorArm(sprites, rCam, ref currentBodyPartIndex, false);
			BuildIteratorLeg(sprites, rCam, ref currentBodyPartIndex, true);
			BuildIteratorLeg(sprites, rCam, ref currentBodyPartIndex, false);

			// TODO: HasHalo
			// TODO: HasGown

			sLeaser.sprites = sprites.ToArray();
		}

		/// <summary>
		/// This utility method calls <see cref="GenericBodyPart"/>'s constructor with the provided radius and friction, using <see cref="Iterator"/>.<see cref="PhysicalObject.firstChunk"/> to attach to.
		/// </summary>
		/// <param name="radius"></param>
		/// <param name="friction"></param>
		/// <returns></returns>
		protected GenericBodyPart NewRadialBodyPart(float radius, float friction) => new GenericBodyPart(this, radius, 0.5f, friction, Iterator.firstChunk);

		/// <summary>
		/// This method should create the sprites for the Iterator's torso.
		/// </summary>
		/// <param name="sprites">The list of sprites. All entries in this list will be provided to the <see cref="SpriteLeaser"/> constructing this model.</param>
		/// <param name="rCam">A reference to the camera viewing this room.</param>
		/// <param name="currentBodyPartIndex">The body part index represents the current index at this point in time. For more information, check the documentation.</param>
		public abstract void BuildIteratorTorso(List<FSprite> sprites, RoomCamera rCam, ref int currentBodyPartIndex);

		/// <summary>
		/// This method should create the sprites for the Iterator's arms.
		/// </summary>
		/// <param name="sprites">The list of sprites. All entries in this list will be provided to the <see cref="SpriteLeaser"/> constructing this model.</param>
		/// <param name="rCam">A reference to the camera viewing this room.</param>
		/// <param name="currentBodyPartIndex">The body part index represents the current index at this point in time. For more information, check the documentation.</param>
		/// <param name="isLeftSide">This is true when the body part is for the left side of the body, and false for the right side.</param>
		public abstract void BuildIteratorArm(List<FSprite> sprites, RoomCamera rCam, ref int currentBodyPartIndex, bool isLeftSide);

		/// <summary>
		/// This method should create the sprites for the Iterator's legs.
		/// </summary>
		/// <param name="sprites">The list of sprites. All entries in this list will be provided to the <see cref="SpriteLeaser"/> constructing this model.</param>
		/// <param name="rCam">A reference to the camera viewing this room.</param>
		/// <param name="currentBodyPartIndex">The body part index represents the current index at this point in time. For more information, check the documentation.</param>
		/// <param name="isLeftSide">This is true when the body part is for the left side of the body, and false for the right side.</param>
		public abstract void BuildIteratorLeg(List<FSprite> sprites, RoomCamera rCam, ref int currentBodyPartIndex, bool isLeftSide);

		/// <summary>
		/// This method should create the sprites for the Iterator's head, face, and head adornments (headphones).
		/// </summary>
		/// <param name="sprites">The list of sprites. All entries in this list will be provided to the <see cref="SpriteLeaser"/> constructing this model.</param>
		/// <param name="rCam">A reference to the camera viewing this room.</param>
		/// <param name="currentBodyPartIndex">The body part index represents the current index at this point in time. For more information, check the documentation.</param>
		/// <param name="isLeftSide">This is true when the body part is for the left side of the body, and false for the right side.</param>
		public abstract void BuildIteratorHead(List<FSprite> sprites, RoomCamera rCam, ref int currentBodyPartIndex);
	}
}
