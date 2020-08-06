using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class CrystaliceAxe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Axe");
		}

		public override void SetDefaults(){
			//Slightly better than Silver
			item.CloneDefaults(ItemID.SilverAxe);

			item.damage++;
			item.width = 40;
			item.height = 40;
			item.useTime -= 3;
			item.useAnimation -= 2;
			item.axe++;
			item.value = Item.sellPrice(silver: 3);
			item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ModContent.ItemType<Frostbite.FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
