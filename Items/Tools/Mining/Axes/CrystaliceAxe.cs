using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Mining.Axes{
	public class CrystaliceAxe : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Axe");
		}

		public override void SetDefaults(){
			//Slightly better than Silver
			Item.CloneDefaults(ItemID.SilverAxe);

			Item.damage++;
			Item.width = 40;
			Item.height = 40;
			Item.useTime -= 3;
			Item.useAnimation -= 2;
			Item.axe++;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes(){
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
