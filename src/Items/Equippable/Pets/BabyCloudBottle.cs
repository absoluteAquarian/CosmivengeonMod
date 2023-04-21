using CosmivengeonMod.Buffs.Pets;
using CosmivengeonMod.Projectiles.Pets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Pets {
	public class BabyCloudBottle : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Baby Cloud Bottle");
			Tooltip.SetDefault("Summons a baby blizzard cloud.");
		}

		public override void SetDefaults() {
			Item.CloneDefaults(ItemID.ZephyrFish);
			Item.width = 20;
			Item.height = 38;
			Item.scale = 1f;
			Item.shoot = ModContent.ProjectileType<FrostCloudPet>();
			Item.buffType = ModContent.BuffType<FrostCloudPetBuff>();
		}

		public override void UseStyle(Player player, Rectangle heldItemFrame) {
			if (player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(ModContent.BuffType<FrostCloudPetBuff>(), 3600, true);
		}
	}
}
