using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Materials{
	public class RaechonShell : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rock Plating");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = ItemRarityID.Blue;
			item.width = 24;
			item.height = 18;
			item.value = Item.sellPrice(silver: 2, copper: 35);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
