using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.DataStructures {
	public class StaminaBuffCollection : List<StaminaBuffData> {
		public StaminaBuffCollection() : base() { }

		public StaminaBuffCollection(int capacity) : base(capacity) { }

		public TagCompound ToTag()
			=> new TagCompound() {
				["mods"] = this.Select(sbd => sbd.mod).ToList(),
				["keys"] = this.Select(sbd => sbd.key).ToList()
			};

		public static StaminaBuffCollection FromTag(TagCompound tag) {
			StaminaBuffCollection ret = new StaminaBuffCollection();
			var mods = tag.GetList<string>("mods");
			var keys = tag.GetList<string>("keys");

			//If one is null, then all are null due to data not being there
			if (mods is null || keys is null)
				return ret;

			//Assume length is the same
			for (int i = 0; i < mods.Count; i++)
				ret.Add(new StaminaBuffData(mods[i], keys[i]));

			return ret;
		}

		public void RemoveAll(string mod, string key) {
			for (int i = 0; i < Count; i++) {
				if (this[i].mod == mod && this[i].key == key) {
					RemoveAt(i);
					i--;
				}
			}
		}

		public bool HasKey(string mod, string key) {
			for (int i = 0; i < Count; i++)
				if (this[i].mod == mod && this[i].key == key)
					return true;

			return false;
		}
	}
}
