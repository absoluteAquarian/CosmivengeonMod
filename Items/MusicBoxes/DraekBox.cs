using Terraria.ModLoader;
using Terraria.ID;
using Terraria;

namespace CosmivengeonMod.Items.MusicBoxes{
	public class DraekBox : ModItem{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Music Box (Draek)");
		}

		public override void SetDefaults() {
			item.useStyle = ItemUseStyleID.SwingThrow;
			item.useTurn = true;
			item.useAnimation = 15;
			item.useTime = 10;
			item.autoReuse = true;
			item.consumable = true;
			item.createTile = ModContent.TileType<Tiles.DraekBox>();
			item.width = 24;
			item.height = 24;
			item.rare = ItemRarityID.LightRed;
			item.value = Item.buyPrice(gold: 2);
			item.accessory = true;
		}
	}
}
