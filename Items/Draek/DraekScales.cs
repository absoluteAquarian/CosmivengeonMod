using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CosmivengeonMod.Items.Draek{
	public class DraekScales : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek Scales");
			Tooltip.SetDefault("The serpentine remains of the armored terra worm. They appear to be quite durable.\nMaybe they could be used for something...");
		}

		public override void SetDefaults(){
			item.maxStack = 99;
			item.rare = ItemRarityID.Blue;
			item.consumable = false;
			item.value = Item.sellPrice(0, 0, 15, 0);
		}

		public override bool CanUseItem(Player player){
			return false;
		}
	}
}
