using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class StoneTablet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Stone Tablet");
			Tooltip.SetDefault("Lore to be added");
		}

		public override void SetDefaults(){
			item.maxStack = 1;
			item.rare = 2;
			item.consumable = false;
		}

		public override bool CanUseItem(Player player){
			return false;
		}
	}
}
