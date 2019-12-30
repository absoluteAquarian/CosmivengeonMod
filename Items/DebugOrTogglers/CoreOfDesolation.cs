using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class CoreOfDesolation : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Core of Desolation");
			Tooltip.SetDefault("Activates Desolation Mode." +
				$"\nEnables the \"Stamina\" effect, which can be toggled using \"G\"" +
				"\nStamina increases move and attack speed while active," +
				"\nthough getting Exhausted will cause you to move and attack slower." +
				"\nDesolation Mode unleashes hell upon this world, causing all" +
				"\nenemies to become stronger.");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 1;
			item.rare = 5;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.consumable = false;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			foreach(TooltipLine line in tooltips){
				if(line.text != null && line.text.Length >= 7 && line.text.Substring(0, 7) == "Enables"){
					string hotkey;

					try{
						hotkey = CosmivengeonMod.StaminaHotKey.GetAssignedKeys()[0];
					}catch(Exception){
						hotkey = "<NOT BOUND>";
					}

					line.text = $"Enables the \"Stamina\" effect, which can be toggled using \"{hotkey}\"";
				}
			}
		}

		public override bool CanUseItem(Player player){
			bool calamityRevengeance = (bool?)CosmivengeonMod.CalamityInstance?.Call("Difficulty", "Rev") ?? false;
			bool calamityDeath = (bool?)CosmivengeonMod.CalamityInstance?.Call("Difficulty", "Death") ?? false;

			if(player.GetModPlayer<CosmivengeonPlayer>().stamina.Active)
				return false;
			if(!Main.expertMode)
				Main.NewText("You are not powerful enough to withstand the chaos...", CosmivengeonUtils.TausFavouriteColour);
		//	if(CosmivengeonWorld.desoMode && !CosmivengeonMod.debug_toggleDesoMode)
		//		Main.NewText("Nice try, but the deed has already been done.", CosmivengeonUtils.TausFavouriteColour);

			//Disable Calamity's modes if they are active
			if(Main.expertMode && (calamityRevengeance || calamityDeath)){
				CosmivengeonMod.DeactivateCalamityRevengeance();
				CosmivengeonMod.DeactivateCalamityDeath();
			}

			return Main.expertMode;
		//	return Main.expertMode && (!CosmivengeonWorld.desoMode || CosmivengeonMod.debug_toggleDesoMode);
		}

		public override bool UseItem(Player player){
			Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0), player.Center);

			if(!CosmivengeonWorld.desoMode){
				Main.NewText("An otherworldly chaos has been unleashed...  No turning back now.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = true;
				return true;
			}else{
				Main.NewText("The otherworldy chaos recedes...  For now.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = false;
				return true;
			}

		/*	
			}else if(CosmivengeonMod.debug_toggleDesoMode){
				Main.NewText("Wait, what?  You aren't supposed to be able to toggle that!  Oh well, I guess it can't be helped.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = false;
				return true;
			}

			return false;
		*/
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
