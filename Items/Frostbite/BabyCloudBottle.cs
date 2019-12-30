using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
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
			item.shoot = ModContent.ProjectileType<Projectiles.Pets.FrostCloudPet>();
			item.buffType = ModContent.BuffType<Buffs.FrostCloudPetBuff>();
		}

		public override void UseStyle(Player player){
			if(player.whoAmI == Main.myPlayer && player.itemTime == 0)
				player.AddBuff(ModContent.BuffType<Buffs.FrostCloudPetBuff>(), 3600, true);
		}
	}
}
