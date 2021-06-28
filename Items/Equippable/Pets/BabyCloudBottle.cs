using CosmivengeonMod.Buffs.Pets;
using CosmivengeonMod.Projectiles.Pets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Pets{
	public class BabyCloudBottle : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Baby Cloud Bottle");
			Tooltip.SetDefault("Summons a baby blizzard cloud.");
		}

		public override void SetDefaults(){
			item.CloneDefaults(ItemID.ZephyrFish);
			item.width = 20;
			item.height = 38;
			item.scale = 1f;
			item.shoot = ModContent.ProjectileType<FrostCloudPet>();
			item.buffType = ModContent.BuffType<FrostCloudPetBuff>();
		}

		public override void UseStyle(Player player){
			if(player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(ModContent.BuffType<FrostCloudPetBuff>(), 3600, true);
		}
	}
}
