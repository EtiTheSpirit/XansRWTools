using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace XansTools.Utilities.RW {

	/// <summary>
	/// Extends sound-related information
	/// </summary>
	public static class SoundExtensions {

		/// <summary>
		/// This delegate can be used to modify <see cref="SoundLoader.SoundData"/>.
		/// </summary>
		/// <param name="data">The sound data to change.</param>
		public delegate void MutateSoundDataDelegate(ref SoundLoader.SoundData data);


		/// <summary>
		/// Play a sound, but have an opportunity to modify the <see cref="SoundLoader.SoundData"/> before it gets sent to the sound system.
		/// </summary>
		/// <param name="this">The virtual microphone to operate on.</param>
		/// <param name="id">The ID of the sound to play.</param>
		/// <param name="mutate">A delegate method used to change the fields of the sound data.</param>
		/// <param name="index">A specific index of sound to play, or -1 to play the event using its settings (a random sound, or all sounds if /PLAYALL was declared in sounds.txt).</param>
		public static SoundLoader.SoundData GetAndMutateSoundData(this VirtualMicrophone @this, SoundID id, MutateSoundDataDelegate mutate, int index = -1) {
			if (@this == null) throw new NullReferenceException($"Extension method {nameof(GetAndMutateSoundData)} called on a null {nameof(VirtualMicrophone)}.");
			if (id == null) throw new ArgumentNullException(nameof(id));
			if (mutate == null) throw new ArgumentNullException(nameof(mutate));
			if (index < -1) throw new ArgumentOutOfRangeException(nameof(index), "Index must be >= -1");

			SoundLoader.SoundData data;
			if (index != -1) {
				data = @this.soundLoader.GetSoundData(id, index);
			} else {
				data = @this.soundLoader.GetSoundData(id);
			}

			mutate(ref data);
			return data;
		}

		/// <summary>
		/// This mimics <see cref="VirtualMicrophone.PlaySound(SoundID, Vector2, float, float)"/> but it forcefully disables the doppler effect.
		/// </summary>
		/// <param name="this">The microphone to play the sound with.</param>
		/// <param name="soundId">The ID of the sound to play.</param>
		/// <param name="pos">The position to play the sound at.</param>
		/// <param name="vol">The volume to multiply the sound's base volume with.</param>
		/// <param name="pitch">The pitch to multiply the sound's base pitch with.</param>
		public static void PlaySoundNoDoppler(this VirtualMicrophone @this, SoundID soundId, Vector2 pos, float vol = 1.0f, float pitch = 1.0f) {
			if (@this.visualize) {
				@this.Log(soundId);
			}
			if (!@this.AllowSound(soundId)) {
				return;
			}
			if (!@this.soundLoader.TriggerPlayAll(soundId)) {
				SoundLoader.SoundData soundData = @this.GetSoundData(soundId, -1);
				soundData.dopplerFac = 0;
				if (@this.SoundClipReady(soundData)) {
					@this.soundObjects.Add(new VirtualMicrophone.StaticPositionSound(@this, soundData, pos, vol, pitch, false));
					return;
				}
			} else {
				for (int i = 0; i < @this.soundLoader.TriggerSamples(soundId); i++) {
					SoundLoader.SoundData soundData = @this.GetSoundData(soundId, i);
					soundData.dopplerFac = 0;
					if (@this.SoundClipReady(soundData)) {
						@this.soundObjects.Add(new VirtualMicrophone.StaticPositionSound(@this, soundData, pos, vol, pitch, false));
					}
				}
			}
		}


		/// <summary>
		/// This mimics <see cref="Room.PlaySound(SoundID, Vector2, float, float)"/> but it forcefully disables the doppler effect.
		/// </summary>
		/// <param name="this"></param>
		/// <param name="id"></param>
		/// <param name="at"></param>
		/// <param name="volume"></param>
		/// <param name="pitch"></param>
		public static void PlaySoundNoDoppler(this Room @this, SoundID soundId, Vector2 pos, float vol = 1.0f, float pitch = 1.0f) {
			for (int i = 0; i < @this.game.cameras.Length; i++) {
				if (@this.game.cameras[i].room == @this) {
					@this.game.cameras[i].virtualMicrophone.PlaySoundNoDoppler(soundId, pos, vol, pitch);
				}
			}
		}

		// Token: 0x060013F7 RID: 5111 RVA: 0x00110740 File Offset: 0x0010E940
		public static void PlaySoundNoDoppler(this VirtualMicrophone @this, SoundID soundId, PositionedSoundEmitter controller, bool loop, float vol, float pitch, bool randomStartPosition) {
			if (@this.visualize) {
				@this.Log(soundId);
			}
			if (!@this.AllowSound(soundId)) {
				return;
			}
			if (!@this.soundLoader.TriggerPlayAll(soundId)) {
				SoundLoader.SoundData soundData = @this.GetSoundData(soundId, -1);
				soundData.dopplerFac = 0;
				if (@this.SoundClipReady(soundData)) {
					if (controller is RectSoundEmitter rectEmitter) {
						@this.soundObjects.Add(new VirtualMicrophone.RectangularSound(@this, soundData, loop, rectEmitter, vol, pitch, randomStartPosition));
					} else {
						@this.soundObjects.Add(new VirtualMicrophone.ObjectSound(@this, soundData, loop, controller, vol, pitch, randomStartPosition));
					}
				}
			} else {
				for (int i = 0; i < @this.soundLoader.TriggerSamples(soundId); i++) {
					SoundLoader.SoundData soundData = @this.GetSoundData(soundId, i);
					soundData.dopplerFac = 0;
					if (@this.SoundClipReady(soundData)) {
						if (controller is RectSoundEmitter rectEmitter) {
							@this.soundObjects.Add(new VirtualMicrophone.RectangularSound(@this, soundData, loop, rectEmitter, vol, pitch, randomStartPosition));
						} else {
							@this.soundObjects.Add(new VirtualMicrophone.ObjectSound(@this, soundData, loop, controller, vol, pitch, randomStartPosition));
						}
					}
				}
			}
		}

		public static void PlaySoundNoDoppler(this Room @this, SoundID soundId, PositionedSoundEmitter emitter, bool loop, float vol = 1.0f, float pitch = 1.0f, bool randomStartPosition = false) {
			@this.AddObject(emitter);
			for (int i = 0; i < @this.game.cameras.Length; i++) {
				if (@this.game.cameras[i].room == @this) {
					@this.game.cameras[i].virtualMicrophone.PlaySoundNoDoppler(soundId, emitter, loop, vol, pitch, randomStartPosition);
					break;
				}
			}
		}

		public static RectSoundEmitter PlayRectSoundNoDoppler(this Room @this, SoundID soundId, FloatRect rect, bool loop, float vol = 1.0f, float pitch = 1.0f) {
			RectSoundEmitter emitter = new RectSoundEmitter(rect, vol, pitch);
			@this.PlaySoundNoDoppler(soundId, emitter, loop, vol, pitch, false);
			return emitter;
		}

	}
}
