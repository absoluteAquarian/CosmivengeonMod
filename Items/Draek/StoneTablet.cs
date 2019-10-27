using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Draek{
	public class StoneTablet : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Stone Tablet");
			Tooltip.SetDefault("Long ago, the ancient earth elemental Oronitus ruled over these lands. One day, his legacy collapsed, and all his relics were cast into obscurity, buried beneath thousands of years of rock and legend.\nMany years later, an innocent serpent stumbled across Oronitus's most prized possession, his sacred jewel, and its mystical powers transformed the snake into the terrifying guardian of the forest, Draek, capable of manipulating the terrain around him into forming arms, servants, and even granting the ability to hover above the ground.\nThe earthen worm held his position fiercely, striking down any challengers with other relics recovered shortly after, until now, where it appears that he has finally met his match...");
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
