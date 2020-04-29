using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class CalamityModeChecker : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Calamity Mod Checker");
			Tooltip.SetDefault("This item can be used to check the state of" +
				"\nCalamity's two custom modes:  [c/ff0000:Revengeange] and [c/ac00ff:Death]." +
				"\nLeft click to print [c/ff0000:Revengeance]'s state to the chat." +
				"\nRight click to print [c/ac00ff:Death]'s state to the chat.");
		}

		public override void SetDefaults(){
			item.width = 40;
			item.height = 40;
			item.maxStack = 1;
			item.rare = ItemRarityID.Pink;
			item.useAnimation = 15;
			item.useTime = 15;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.MenuTick, 0);
			item.consumable = false;
		}

		public override bool AltFunctionUse(Player player) => true;

		public override bool CanUseItem(Player player){
			if(!CosmivengeonMod.debug_canUseCalamityChecker){
				Main.NewText("Sorry, but you can't use this item.", Color.LightGray);
				return false;
			}
			return true;
		}

		public override bool UseItem(Player player){
			if(!CosmivengeonMod.CalamityActive)
				Main.NewText("Calamity is not enabled.", Color.Red);
			else{
				bool state;
				string name;
				if(player.altFunctionUse == 2){		//Right click
					state = (bool)CosmivengeonMod.CalamityInstance.Call("Difficulty", "Death");
					name = "[c/ac00ff:Death]";
				}else{
					state = (bool)CosmivengeonMod.CalamityInstance.Call("Difficulty", "Rev");
					name = "[c/ff0000:Revengeance]";
				}
				Main.NewText($"{name}: {state}");
			}

			return true;
		}
	}
}
