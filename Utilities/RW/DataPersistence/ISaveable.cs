using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XansTools.Utilities.RW.DataPersistence {
	public interface ISaveable {

		/// <summary>
		/// Writes this object to the provided <see cref="BinaryWriter"/> in the context of the provided <paramref name="scope"/>.
		/// </summary>
		/// <param name="scope">The scope that is being saved. For more information, see <see cref="SaveScope"/></param>
		/// <param name="writer">The destination for the data.</param>
		/// <returns>Some specifically formatted save data (which is defined by the implementor), or null to not save anything.</returns>
		public void SaveToStream(SaveScope scope, BinaryWriter writer);

		/// <summary>
		/// Updates the state of this object such that it reflects upon the provided save string for the provided scope.
		/// </summary>
		/// <param name="saveString"></param>
		public void ReadFromStream(SaveScope scope, BinaryReader reader);

	

	}

	internal static class ISaveableHelper {
		public static byte[] ToByteArray(this ISaveable instance, SaveScope scope) {
			byte[] result;
			using MemoryStream mstr = new MemoryStream(1024);
			using BinaryWriter writer = new BinaryWriter(mstr);
			instance.SaveToStream(scope, writer);
			result = mstr.ToArray();
			return result;
		}

		public static void FromByteArray(this ISaveable instance, SaveScope scope, byte[] data) {
			using MemoryStream mstr = new MemoryStream(data);
			using BinaryReader reader = new BinaryReader(mstr);
			mstr.Position = 0;
			instance.ReadFromStream(scope, reader);
		}
	}
}
