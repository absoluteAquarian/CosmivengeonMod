using CosmivengeonMod.Players;
using SerousCommonLib.API;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits {
	internal class BeetleBuffsOverrideDetour : Edit {
		public override void LoadEdits() {
			On.Terraria.Player.UpdateArmorSets += Player_UpdateArmorSets;
		}

		public override void UnloadEdits() {
			On.Terraria.Player.UpdateArmorSets -= Player_UpdateArmorSets;
		}

		private static void Player_UpdateArmorSets(On.Terraria.Player.orig_UpdateArmorSets orig, Terraria.Player self, int i) {
			var mp = self.GetModPlayer<DicePlayer>();

			if (mp.beetleBuffsTimer > 0) {
				// Force the game to thinking the player has the beetle set equipped...
				self.beetleOffense = true;
				self.beetleDefense = true;
				self.beetleOrbs = 3;

				int num2 = 400;
				int num3 = 1200;
				int num4 = 3600;

				self.beetleCounter = num2 + num3 + num4 + num3;

				self.AddBuff(BuffID.BeetleEndurance3, 5, quiet: false);
				self.AddBuff(BuffID.BeetleMight3, 5, quiet: false);
			}

			orig(self, i);
		}
	}
}
