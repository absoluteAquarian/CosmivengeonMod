using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class EarthBolt : ModItem{
		public override string Texture => "CosmivengeonMod/Items/Draek/Earth_Bolt";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Earthen Tome");
			Tooltip.SetDefault("Casts a slow-moving bolt of green energy.\nBolts have a chance to poison foes, and can pierce up to 3 enemies at once");
		}

		public override void SetDefaults(){
			item.autoReuse = true;
			item.rare = 2;
			item.mana = 8;
			item.UseSound = SoundID.Item21;
			item.noMelee = true;
			item.useStyle = 5;
			item.damage = 26;
			item.useAnimation = 22;
			item.useTime = 22;
			item.width = 28;
			item.height = 30;
			item.shoot = ModContent.ProjectileType<EarthBoltProjectile>();
			item.scale = 1f;
			item.shootSpeed = 7f;
			item.knockBack = 5f;
			item.magic = true;
			item.value = Item.sellPrice(0, 2, 50, 0);
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ModContent.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
