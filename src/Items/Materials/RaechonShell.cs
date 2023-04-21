using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Materials {
	public class RaechonShell : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Rock Plating");
		}

		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
			Item.width = 24;
			Item.height = 18;
			Item.value = Item.sellPrice(silver: 2, copper: 35);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
