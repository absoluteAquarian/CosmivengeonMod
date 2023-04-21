using CosmivengeonMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Crystalice {
	[AutoloadEquip(EquipType.Body)]
	public class CrystaliceBreastplate : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Coat");
		}

		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 20;
			Item.defense = 4;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 16, copper: 75);
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 4);
			recipe.AddIngredient(ItemID.IceBlock, 20);
			recipe.AddIngredient(ItemID.SnowBlock, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
