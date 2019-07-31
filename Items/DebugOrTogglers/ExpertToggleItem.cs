using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class ExpertToggleItem : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Expert Mode Toggler");
			Tooltip.SetDefault("Wait is \"toggler\" even a word?\nNevermind.\nAnyway, this item toggles expert mode.");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = 5;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.MenuTick, 0);
			item.consumable = false;
		}

		public override bool CanUseItem(Player player){
			if(!CosmivengeonMod.debug_canUseExpertModeToggle){
				Main.NewText("Sorry, but you can't use this item.", Color.LightGray);
				return false;
			}
			return true;
		}

		public override bool UseItem(Player player){
			if(Main.expertMode)
				Main.NewText("[EXPERT MODE DISABLED]", Color.LightGray);
			else
				Main.NewText("[EXPERT MODE ENABLED]", Color.LightGray);
			
			Main.expertMode = !Main.expertMode;
			return true;
		}
	}
}
