using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Frostbite {
	public class ProwlerNecklace : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Prowler Necklace");
			Tooltip.SetDefault("Increases armor penetration by 3" +
				"\nEnemies are more likely to target you");
		}

		public override void SetDefaults() {
			Item.rare = ItemRarityID.Blue;
			Item.width = 20;
			Item.height = 26;
			Item.value = Item.sellPrice(silver: 5);
			Item.accessory = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetArmorPenetration(DamageClass.Generic) += 3;
			player.aggro += 300;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ItemID.Chain, 2);
			recipe.AddIngredient(ModContent.ItemType<ProwlerFang>(), 8);
			recipe.AddIngredient(ItemID.IceBlock, 20);
			recipe.Register();
		}
	}
}
