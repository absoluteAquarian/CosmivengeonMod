using System;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures {
	public struct StaminaBuffData {
		public string mod;
		public string key;

		public StaminaBuffData(string mod, string key) {
			this.mod = mod;
			this.key = key;
		}

		public StaminaBuffData(int type) {
			if (type < 0 || type >= NPCLoader.NPCCount)
				throw new ArgumentOutOfRangeException(nameof(type));

			if (type < NPCID.Count) {
				mod = "Terraria";
				key = type.ToString();
			} else {
				var modNPC = NPCLoader.GetNPC(type);
				mod = modNPC.Mod.Name;
				key = modNPC.Name;
			}
		}

		public int GetNPCID() {
			if (mod == "Terraria") {
				if (int.TryParse(key, out int id))
					return id;
			} else if (ModLoader.TryGetMod(mod, out var source) && source.TryFind(key, out ModNPC npc))
				return npc.Type;

			return 0;
		}
	}
}
