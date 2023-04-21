using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Materials {
	public class DraekScales : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Draek Scales");
			Tooltip.SetDefault("The serpentine remains of the armored terra worm. They appear to be quite durable.\nMaybe they could be used for something...");
		}

		public override void SetDefaults() {
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
			Item.consumable = false;
			Item.value = Item.sellPrice(0, 0, 15, 0);
		}

		public override bool CanUseItem(Player player) {
			return false;
		}
	}
}
