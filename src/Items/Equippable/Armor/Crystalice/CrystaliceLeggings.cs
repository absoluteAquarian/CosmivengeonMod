using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Armor.Crystalice {
	[AutoloadEquip(EquipType.Legs)]
	public class CrystaliceLeggings : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Boots");
			Tooltip.SetDefault("Slows movement speed by 15%" +
				"\nAllows for gliding on ice");
		}

		public override void SetDefaults() {
			Item.width = 18;
			Item.height = 12;
			Item.defense = 2;
			Item.rare = ItemRarityID.Blue;
			Item.value = Item.sellPrice(silver: 11, copper: 90);
		}

		public override void UpdateEquip(Player player) {
			player.GetModPlayer<ArmorsPlayer>().crystaliceLegs = true;
			player.iceSkate = true;
		}

		public override void AddRecipes() {
			Recipe recipe = CreateRecipe();
			recipe.AddIngredient(ModContent.ItemType<FrostCrystal>(), 3);
			recipe.AddIngredient(ItemID.IceBlock, 15);
			recipe.AddIngredient(ItemID.SnowBlock, 20);
			recipe.AddTile(TileID.WorkBenches);
			recipe.Register();
		}
	}
}
