using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class ProwlerFang : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Prowler Fang");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = ItemRarityID.Blue;
			//Placeholder
			item.width = 40;
			item.height = 40;
			item.value = Item.sellPrice(copper: 80);
		}

		public override bool CanUseItem(Player player) => false;
	}
}
