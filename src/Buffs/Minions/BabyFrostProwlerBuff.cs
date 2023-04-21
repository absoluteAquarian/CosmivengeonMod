using CosmivengeonMod.Players;
using CosmivengeonMod.Projectiles.Summons;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Buffs.Minions {
	public class BabyIceProwlerBuff : ModBuff {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Baby Ice Prowler");
			Description.SetDefault("A baby Ice Prowler will protect you.");
			Main.buffNoSave[Type] = true;
			Main.buffNoTimeDisplay[Type] = true;
		}

		public override void Update(Player player, ref int buffIndex) {
			MinionPlayer modPlayer = player.GetModPlayer<MinionPlayer>();
			if (player.ownedProjectileCounts[ModContent.ProjectileType<BabyIceProwler>()] > 0)
				modPlayer.babyProwler = true;

			if (!modPlayer.babyProwler) {
				player.DelBuff(buffIndex);
				buffIndex--;
			} else
				player.buffTime[buffIndex] = 18000;
		}
	}
}
