using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using XansTools.Utilities.General;

namespace XansTools.Utilities.RW.DataPersistence {
	public class SaveDataAccessor {
		
		private static readonly Dictionary<string, SaveDataAccessor> _dataAccessors = new Dictionary<string, SaveDataAccessor>();

		private const string GLOBAL = "XT::GLOBAL";
		private const string CHARACTER = "XT::CHARACTER";
		private const string CYCLE = "XT::CYCLE";

		/// <summary>
		/// The name of this save data, which is used to find it in the player's save file.
		/// </summary>
		public string Name { get; }

		internal static void Initialize() {
			On.SaveState.SaveToString += OnSavingToString;
			On.SaveState.LoadGame += OnLoadingGame;
			// On.RainWorldGame.Re
		}

		private static void OnLoadingGame(On.SaveState.orig_LoadGame originalMethod, SaveState @this, string str, RainWorldGame game) {
			originalMethod(@this, str, game);
			XTSaveData.Import(@this);
		}

		private static string OnSavingToString(On.SaveState.orig_SaveToString originalMethod, SaveState @this) {
			XTSaveData.Export(@this);
			return originalMethod(@this);
		}

		/// <summary>
		/// Returns an existing reference to, or creates and stores a new reference to, a 
		/// <see cref="SaveDataAccessor"/> with the provided <paramref name="name"/>.
		/// </summary>
		/// <param name="name"></param>
		/// <returns></returns>
		public static SaveDataAccessor Get(string modid, string name) {
			string realName = $"{modid}:{name}";
			if (!_dataAccessors.TryGetValue(realName, out SaveDataAccessor accessor)) {
				accessor = new SaveDataAccessor(realName);
				_dataAccessors.Add(realName, accessor);
			}
			return accessor;
		}

		private SaveDataAccessor(string name) {
			Name = name;
		}
		
		/// <summary>
		/// Reloads the data in this object from the save file.
		/// </summary>
		public void ResetAndLoad() {
			XTSaveData.Import(AccessorTools.CurrentSave);
		}

		/// <summary>
		/// Sets the current value of the save to the current state of <paramref name="value"/>.
		/// <para/>
		/// <strong>NOTE THAT THIS DOES NOT STORE A REFERENCE TO THE VALUE.</strong> Any changes made to it
		/// that are not provided by calling this method again <strong>will be ignored and forgotten.</strong>
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="key"></param>
		/// <param name="value"></param>
		public void SetAndUpdateValue(SaveScope scope, string key, ISaveable value) {
			if (value != null) {
				XTSaveData.Set(Name, scope, key, value.ToByteArray(scope));
			} else {
				XTSaveData.Set(Name, scope, key, null);
			}
		}

		/// <summary>
		/// Loads a value from persistent data into the provided <see cref="ISaveable"/>, overwriting its current state
		/// with that of the current game save file.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="key"></param>
		/// <param name="into"></param>
		public void LoadValue(SaveScope scope, string key, ISaveable into) {
			XTSaveData.ReadInto(Name, scope, key, into);
		}

		/// <summary>
		/// Only null is allowed! This is used to remove a value.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="key"></param>
		/// <param name="null"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		[Obsolete("Consider using RemoveValue instead.")]
		public void SetValue(SaveScope scope, string key, object @null) {
			if (@null is not null) {
				throw new ArgumentOutOfRangeException(nameof(@null), "Only null is valid for this variant of the function.");
			}
			XTSaveData.Set(Name, scope, key, null);
		}

		/// <summary>
		/// Only null is allowed! This is used to remove a value.
		/// </summary>
		/// <param name="scope"></param>
		/// <param name="key"></param>
		/// <param name="null"></param>
		/// <exception cref="ArgumentOutOfRangeException"></exception>
		public void RemoveValue(SaveScope scope, string key) {
			XTSaveData.Set(Name, scope, key, null);
		}

		// Below: Objective proof that C# would benefit from macros
		// Your copium is down the hall and to the left c:
		// (so is mine)

		public void SetValue(SaveScope scope, string key, bool value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, sbyte value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, byte value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, short value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, ushort value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, int value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, uint value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, long value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, ulong value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, float value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, double value) {
			byte[] data = BitConverter.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, string value) {
			byte[] data = Encoding.UTF8.GetBytes(value);
			XTSaveData.Set(Name, scope, key, data);
		}
		public void SetValue(SaveScope scope, string key, Enum value) {
			SetValue(scope, key, value.ReflectiveCastInto<ulong>());
		}

		public bool TryGetValue<T>(SaveScope scope, string key, out T result) {
			Type type = typeof(T);
			byte[] data = XTSaveData.Get(Name, scope, key);
			result = default;
			if (data == null) return false;

			unchecked {
				if (type == typeof(bool)) {
					result = (T)(object)BitConverter.ToBoolean(data, 0);
				} else if (type == typeof(sbyte)) {
					result = (T)(object)(sbyte)data[0];
				} else if (type == typeof(byte)) {
					result = (T)(object)data[0];
				} else if (type == typeof(short)) {
					result = (T)(object)BitConverter.ToInt16(data, 0);
				} else if (type == typeof(ushort)) {
					result = (T)(object)BitConverter.ToUInt16(data, 0);
				} else if (type == typeof(int)) {
					result = (T)(object)BitConverter.ToInt32(data, 0);
				} else if (type == typeof(uint)) {
					result = (T)(object)BitConverter.ToUInt32(data, 0);
				} else if (type == typeof(long)) {
					result = (T)(object)BitConverter.ToInt64(data, 0);
				} else if (type == typeof(ulong)) {
					result = (T)(object)BitConverter.ToUInt64(data, 0);
				} else if (type == typeof(float)) {
					result = (T)(object)BitConverter.ToSingle(data, 0);
				} else if (type == typeof(double)) {
					result = (T)(object)BitConverter.ToDouble(data, 0);
				} else if (type == typeof(string)) {
					result = (T)(object)Encoding.UTF8.GetString(data);
				} else if (type.IsEnum) {
					ulong dataU64 = BitConverter.ToUInt64(data, 0);
					result = dataU64.ReflectiveCastInto<T>();
				} else {
					throw new NotSupportedException("This type is not a supported primitive. Enums must use their actual type rather than System.Enum");
				}
			}
			return true;
		}


	}
}
