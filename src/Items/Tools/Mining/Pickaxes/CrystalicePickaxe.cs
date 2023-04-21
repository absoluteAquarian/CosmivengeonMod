using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Mining.Pickaxes {
	public class CrystalicePickaxe : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Pickaxe");
		}

		public override void SetDefaults() {
			//Slightly better than Silver
			Item.CloneDefaults(ItemID.SilverPickaxe);

			Item.damage++;
			Item.width = 38;
			Item.height = 38;
			Item.useTime--;
			Item.useAnimation -= 2;
			Item.pick += 2;
			Item.value = Item.sellPrice(silver: 3);
			Item.rare = ItemRarityID.Blue;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 2);
			recipe.AddIngredient(ItemID.SnowBlock, 15);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
