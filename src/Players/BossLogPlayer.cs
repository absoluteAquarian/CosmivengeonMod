﻿using CosmivengeonMod.DataStructures;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Players {
	public class BossLogPlayer : ModPlayer {
		public StaminaBuffCollection BossesKilled;

		public override void Initialize() {
			BossesKilled = new StaminaBuffCollection();
		}

		public override void SaveData(TagCompound tag) {
			tag["bosses"] = BossesKilled.ToTag();
		}

		public override void LoadData(TagCompound tag) {
			//Older versions have the "bosses" tag as a List<int>
			//We need to account for that
			if (tag.TryGet("bosses", out TagCompound bosses))
				BossesKilled = StaminaBuffCollection.FromTag(bosses);
			else
				BossesKilled = new StaminaBuffCollection();
		}
	}
}
