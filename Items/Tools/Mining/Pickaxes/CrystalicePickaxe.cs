using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Mining.Pickaxes{
	public class CrystalicePickaxe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Pickaxe");
		}

		public override void SetDefaults(){
			//Slightly better than Silver
			item.CloneDefaults(ItemID.SilverPickaxe);

			item.damage++;
			item.width = 38;
			item.height = 38;
			item.useTime--;
			item.useAnimation -= 2;
			item.pick += 2;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
