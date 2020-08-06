using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools{
	public class CrystaliceHammer : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Hammer");
		}

		public override void SetDefaults(){
			//Slightly better than Silver
			item.CloneDefaults(ItemID.SilverHammer);

			item.damage++;
			item.width = 40;
			item.height = 40;
			item.useTime -= 2;
			item.useAnimation -= 3;
			item.hammer += 3;
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
