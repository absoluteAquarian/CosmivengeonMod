using CosmivengeonMod.Tiles.MusicBoxes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Placeable.MusicBoxes{
	public class FrostbiteBox : ModItem{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Music Box (Frostbite)");
		}

		public override void SetDefaults() {
			Item.useStyle = ItemUseStyleID.Swing;
			Item.useTurn = true;
			Item.useAnimation = 15;
			Item.useTime = 10;
			Item.autoReuse = true;
			Item.consumable = true;
			Item.createTile = ModContent.TileType<FrostbiteBoxTile>();
			Item.width = 24;
			Item.height = 24;
			Item.rare = ItemRarityID.LightRed;
			Item.value = Item.buyPrice(gold: 2);
			Item.accessory = true;
		}
	}
}
