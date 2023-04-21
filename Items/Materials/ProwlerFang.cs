using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Materials{
	public class ProwlerFang : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Prowler Fang");
		}

		public override void SetDefaults(){
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Blue;
			Item.width = 14;
			Item.height = 20;
			Item.value = Item.sellPrice(copper: 80);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
