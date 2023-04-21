using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Debugging{
	public class ExpertToggleItem : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Expert Mode Toggler");
			Tooltip.SetDefault(CoreMod.Descriptions.DebugItem +
				"\nWait, is \"toggler\" even a word?" +
				"\nNevermind." +
				"\nAnyway, this item toggles expert mode.");
		}

		public override void SetDefaults(){
			Item.width = 40;
			Item.height = 40;
			Item.maxStack = 1;
			Item.rare = ItemRarityID.Pink;
			Item.useAnimation = 45;
			Item.useTime = 45;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.MenuTick, 0);
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player){
			if(!Debug.debug_canUseExpertModeToggle){
				Main.NewText("Sorry, but you can't use this item.", Color.LightGray);
				return false;
			}
			return true;
		}

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */{
			if(Main.expertMode)
				Main.NewText("[EXPERT MODE DISABLED]", Color.LightGray);
			else
				Main.NewText("[EXPERT MODE ENABLED]", Color.LightGray);
			
			Main.expertMode = !Main.expertMode;
			return true;
		}
	}
}
