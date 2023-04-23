using CosmivengeonMod.Systems;
using SerousCommonLib.API;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits {
	internal class DesolationModeBossProjectileDebuffsDetour : Edit {
		public override void LoadEdits() {
			On.Terraria.Projectile.StatusPlayer += Projectile_StatusPlayer;
		}

		public override void UnloadEdits() {
			On.Terraria.Projectile.StatusPlayer -= Projectile_StatusPlayer;
		}

		private static void Projectile_StatusPlayer(On.Terraria.Projectile.orig_StatusPlayer orig, Projectile self, int i) {
			if (WorldEvents.desoMode) {
				Player player = Main.player[i];

				if (self.type == ProjectileID.Skull) {
					int num = Main.rand.Next(3);
					if (num == 2)
						num = Main.rand.Next(3);

					switch (num) {
						case 0:
							player.AddBuff(BuffID.Bleeding, Main.rand.Next(60, 180));
							break;
						case 1:
							player.AddBuff(BuffID.BrokenArmor, Main.rand.Next(1 * 60, 2 * 60));
							break;
						case 2:
							player.AddBuff(BuffID.Cursed, Main.rand.Next(60, 90));
							break;
					}
				} else
					orig(self, i);
			} else
				orig(self, i);
		}
	}
}
