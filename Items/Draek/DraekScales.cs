using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class DraekScales : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek Scales");
			Tooltip.SetDefault("Description to be added");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = 1;
			item.consumable = false;
			item.value = Item.sellPrice(0, 0, 15, 0);
		}

		public override bool CanUseItem(Player player){
			return false;
		}
	}
}
