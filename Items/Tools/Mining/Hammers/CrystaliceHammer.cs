using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Mining.Hammers {
	public class CrystaliceHammer : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Hammer");
		}

		public override void SetDefaults() {
			//Slightly better than Silver
			Item.CloneDefaults(ItemID.SilverHammer);

			Item.damage++;
			Item.width = 40;
			Item.height = 40;
			Item.useTime -= 2;
			Item.useAnimation -= 3;
			Item.hammer += 3;
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
