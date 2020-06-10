using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class RaechonShell : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Rock Plating");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = ItemRarityID.Blue;
			//Placeholder
			item.width = 40;
			item.height = 40;
			item.value = Item.sellPrice(silver: 2, copper: 35);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
