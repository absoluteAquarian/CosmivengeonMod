using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Materials {
	public class FrostCrystal : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frost Crystal");
			Tooltip.SetDefault("A surprisingly durable chunk of ice." +
				"\nIt appears the war’s fallout played into its unnatural properties.");
		}

		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.rare = ItemRarityID.White;
			Item.consumable = false;
			Item.value = Item.sellPrice(0, 0, 1, 75);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
