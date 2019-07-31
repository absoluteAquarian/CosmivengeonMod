using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using CosmivengeonMod.Projectiles.Weapons;

namespace CosmivengeonMod.Items.Draek{
	public class BoulderChunk : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Boulder Chunk");
			Tooltip.SetDefault("Description to be added.");
		}

		public override void SetDefaults(){
			item.shootSpeed = 9f;
			item.damage = 30;
			item.knockBack = 7f;
			item.useStyle = 1;
			item.useAnimation = 35;
			item.useTime = 35;
			item.width = 40;
			item.height = 40;
			item.maxStack = 999;
			item.rare = 2;

			item.consumable = true;
			item.noUseGraphic = true;
			item.noMelee = true;
			item.autoReuse = true;
			item.thrown = true;

			item.UseSound = SoundID.Item1;
			item.value = Item.sellPrice(0, 0, 2, 75);

			item.shoot = mod.ProjectileType<BoulderChunkProjectile>();
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(mod.ItemType<DraekScales>(), 15);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
