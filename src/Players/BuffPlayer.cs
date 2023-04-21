using CosmivengeonMod.Buffs.Harmful;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players {
	public class BuffPlayer : ModPlayer {
		public bool primordialWrath;
		public bool rotting;

		public override void ResetEffects() {
			primordialWrath = false;
			rotting = false;
		}

		public override void UpdateBadLifeRegen() {
			if (rotting) {
				if (Player.lifeRegen > 0) {
					Player.lifeRegen -= 4 * 2;
					if (Player.lifeRegen < 0)
						Player.lifeRegen = 0;
				}

				Player.statDefense -= 10;
				Player.endurance -= 0.05f;
			}
			if (primordialWrath) {
				if (Player.lifeRegen > 0)
					Player.lifeRegen = 0;
				Player.statDefense -= 10;
				Player.endurance -= 0.1f;
				Player.lifeRegen -= 15 * 2;
			}
		}

		public override bool PreKill(double damage, int hitDirection, bool pvp, ref bool playSound, ref bool genGore, ref PlayerDeathReason damageSource) {
			if (Player.HasBuff(ModContent.BuffType<PrimordialWrath>()))
				damageSource = PlayerDeathReason.ByCustomReason($"{Player.name}'s flesh was melted off.");
			return true;
		}
	}
}
