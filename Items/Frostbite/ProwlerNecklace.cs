using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class ProwlerNecklace : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Prowler Necklace");
			Tooltip.SetDefault("Increases armor penetration by 3" +
				"\nEnemies are more likely to target you");
		}

		public override void SetDefaults(){
			item.rare = ItemRarityID.Blue;
			//Placeholder
			item.width = 40;
			item.height = 40;
			item.value = Item.sellPrice(silver: 5);
			item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			player.armorPenetration += 3;
			player.aggro += 300;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.Chain, 2);
			recipe.AddIngredient(ModContent.ItemType<ProwlerFang>(), 8);
			recipe.AddIngredient(ItemID.IceBlock, 20);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
