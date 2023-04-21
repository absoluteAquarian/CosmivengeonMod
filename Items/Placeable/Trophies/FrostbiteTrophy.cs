using CosmivengeonMod.Tiles.Trophies;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Placeable.Trophies {
	public class FrostbiteTrophy : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frostbite Trophy");
		}

		public override void SetDefaults() {
			Item.width = 30;
			Item.height = 30;
			Item.maxStack = 99;
			Item.useTurn = true;
			Item.autoReuse = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.useStyle = ItemUseStyleID.Swing;
			Item.consumable = true;
			Item.value = Item.buyPrice(gold: 5);
			Item.rare = ItemRarityID.Blue;
			Item.createTile = ModContent.TileType<FrostbiteTrophyTile>();
		}
	}
}
