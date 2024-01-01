using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW.DataPersistence {
	internal static class XTSaveData {

		private static readonly Dictionary<string, Dictionary<SaveScope, Dictionary<string, byte[]>>> _data = new Dictionary<string, Dictionary<SaveScope, Dictionary<string, byte[]>>>();

		public const string DATAKEY = "XANSTOOLSSAVEDATA";
		public const string DATAKEY_WITH_SEPARATOR = "XANSTOOLSSAVEDATA<svB>";

		/// <summary>
		/// Change the value for the provided save file key, with the provided key.
		/// </summary>
		/// <param name="saveFile"></param>
		/// <param name="key"></param>
		/// <param name="bytes"></param>
		public static void Set(string saveFile, SaveScope scope, string key, byte[] bytes) {
			if (!_data.TryGetValue(saveFile, out Dictionary<SaveScope, Dictionary<string, byte[]>> scopes)) {
				scopes = new Dictionary<SaveScope, Dictionary<string, byte[]>>();
				_data.Add(saveFile, scopes);
			}
			if (!scopes.TryGetValue(scope, out Dictionary<string, byte[]> storage)) {
				storage = new Dictionary<string, byte[]>();
				scopes.Add(scope, storage);
			}
			if (bytes != null) {
				storage[key] = bytes;
			} else {
				storage.Remove(key);
			}
		}

		/// <summary>
		/// Returns the byte array for the provided value, or null if no such array exists.
		/// <para/>
		/// To apply data to an <see cref="ISaveable"/>, use <see cref="ReadInto(string, SaveScope, string, ISaveable)"/>.
		/// </summary>
		/// <param name="saveFile"></param>
		/// <param name="scope"></param>
		/// <param name="key"></param>
		/// <returns></returns>
		public static byte[] Get(string saveFile, SaveScope scope, string key) {
			if (!_data.TryGetValue(saveFile, out Dictionary<SaveScope, Dictionary<string, byte[]>> scopes)) return null;
			if (!scopes.TryGetValue(scope, out Dictionary<string, byte[]> storage)) return null;
			if (storage.TryGetValue(key, out byte[] data)) return data;
			
			return null;
		}

		/// <summary>
		/// Change the value for the provided save file key, with the provided key.
		/// </summary>
		/// <param name="saveFile"></param>
		/// <param name="key"></param>
		/// <param name="bytes"></param>
		public static void ReadInto(string saveFile, SaveScope scope, string key, ISaveable saveable) {
			if (!_data.TryGetValue(saveFile, out Dictionary<SaveScope, Dictionary<string, byte[]>> scopes)) {
				return;
			}
			if (!scopes.TryGetValue(scope, out Dictionary<string, byte[]> storage)) {
				return;
			}
			if (storage.TryGetValue(key, out byte[] data)) {
				saveable.FromByteArray(scope, data);
			}
		}

		/// <summary>
		/// Overwrites all stored data with the data provided in the save state's unrecognized strings array.
		/// It will search for an entry prefixed with <see cref="DATAKEY_WITH_SEPARATOR"/>.
		/// </summary>
		/// <param name="b64"></param>
		public static void Import(SaveState save) {
			foreach (string str in save.unrecognizedSaveStrings) {
				if (str.StartsWith(DATAKEY_WITH_SEPARATOR)) {
					Import(str.Substring(DATAKEY_WITH_SEPARATOR.Length));
					break;
				}
			}
		}

		/// <summary>
		/// Overwrites all stored data with the data provided in the base64 string.
		/// </summary>
		/// <param name="b64"></param>
		public static void Import(string b64) {
			_data.Clear();
			using MemoryStream mstr = new MemoryStream(Convert.FromBase64String(b64));
			using BinaryReader reader = new BinaryReader(mstr);
			mstr.Position = 0;
			int scopeBlocks = reader.ReadInt32();
			for (int i = 0; i < scopeBlocks; i++) {
				string accessor = reader.ReadString();
				int scopes = reader.ReadInt32();
				for (SaveScope scope = 0; (int)scope < scopes; scope++) {
					int valueCount = reader.ReadInt32();
					for (int j = 0; j < valueCount; j++) {
						string key = reader.ReadString();
						int length = reader.ReadInt32();
						byte[] value = reader.ReadBytes(length);
						Set(accessor, scope, key, value);
					}
				}
			}
		}

		/// <summary>
		/// Saves all stored data into a key/value pair for Rain World save data composed of a unique key (<see cref="DATAKEY"/>) and a base64 string.
		/// </summary>
		/// <returns></returns>
		public static string Export() {
			byte[] finalResult;
			using MemoryStream mstr = new MemoryStream(16384);
			using BinaryWriter writer = new BinaryWriter(mstr);
			writer.Write(_data.Count);
			foreach (KeyValuePair<string, Dictionary<SaveScope, Dictionary<string, byte[]>>> scopeBlock in _data) {
				writer.Write(scopeBlock.Key);
				writer.Write((int)SaveScope.Count);
				for (int i = 0; i < (int)SaveScope.Count; i++) {
					if (scopeBlock.Value.TryGetValue((SaveScope)i, out Dictionary<string, byte[]> storageBlock)) {
						writer.Write(storageBlock.Count);
						foreach (KeyValuePair<string, byte[]> data in storageBlock) {
							writer.Write(data.Key);
							writer.Write(data.Value.Length);
							writer.Write(data.Value);
						}
					} else {
						writer.Write(0);
					}
				}
			}
			finalResult = mstr.ToArray();
			return $"{DATAKEY_WITH_SEPARATOR}{Convert.ToBase64String(finalResult)}";
		}

		/// <summary>
		/// Writes this save data into the save state's unrecognized strings array.
		/// </summary>
		/// <param name="into"></param>
		public static void Export(SaveState into) {
			int index = 0;
			bool found = false;
			foreach (string str in into.unrecognizedSaveStrings) {
				if (str.StartsWith(DATAKEY_WITH_SEPARATOR)) {
					found = true;
					break;
				}
				index++;
			}
			if (found) {
				into.unrecognizedSaveStrings[index] = Export();
			} else {
				into.unrecognizedSaveStrings.Add(Export());
			}
		}
	}
}
