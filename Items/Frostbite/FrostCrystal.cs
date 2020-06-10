using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class FrostCrystal : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Crystal");
			Tooltip.SetDefault("A surprisingly durable chunk of ice." +
				"\nIt appears the war’s fallout played into its unnatural properties.");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = ItemRarityID.White;
			item.consumable = false;
			item.value = Item.sellPrice(0, 0, 1, 75);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
