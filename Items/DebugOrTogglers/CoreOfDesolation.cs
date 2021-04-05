using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria.DataStructures;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class CoreOfDesolation : HidableTooltip{
		public override string ItemName => "Core of Desolation";

		public override string FlavourText => "Activates Desolation Mode." +
			"\nEnables the \"Stamina\" effect, which can be toggled using \"G\"" +
			"\nStamina increases move and attack speed while active," +
			"\nthough getting Exhausted will cause you to move and attack slower." +
			"\nDesolation Mode unleashes hell upon this world, causing all" +
			"\nenemies to become stronger.";

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 1;
			item.rare = ItemRarityID.Pink;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.consumable = false;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips){
			int customIndex = FindCustomTooltipIndex(tooltips);

			if(customIndex < 0)
				return;

			do{
				TooltipLine customLine = tooltips[customIndex];

				customIndex++;
				if(!customLine.text.Contains("\"G\""))
					continue;

				string hotkey;

				var hotkeys = CosmivengeonMod.StaminaHotKey.GetAssignedKeys();
				if(hotkeys.Count > 0)
					hotkey = hotkeys[0];
				else
					hotkey = "<NOT BOUND>";

				customLine.text = customLine.text.Replace("\"G\"", $"\"{hotkey}\"");
			}while(tooltips[customIndex].Name.StartsWith("CustomTooltip"));
		}

		public override bool CanUseItem(Player player){
			//If the game is in multiplayer, only allow the host to use the item OR if this game is using a dedicated server, prevent the item's use entirely
			if(Main.netMode != NetmodeID.SinglePlayer && !Main.dedServ && CosmivengeonUtils.ClientIsLocalHost(Main.myPlayer)){
				Main.NewText("Only the server host can use this item!", Color.Red);
				return false;
			}else if(Main.dedServ){
				Main.NewText("This item cannot be used on dedicated servers!  Get the server owner to toggle Desolation mode through the server's console.", Color.Red);
				return false;
			}

			bool calamityRevengeance = (bool?)ModReferences.Calamity.Call("Difficulty", "Rev") ?? false;
			bool calamityDeath = (bool?)ModReferences.Calamity.Call("Difficulty", "Death") ?? false;

			if(player.GetModPlayer<CosmivengeonPlayer>().stamina.Active || player.GetModPlayer<CosmivengeonPlayer>().stamina.Exhaustion)
				return false;
			if(!Main.expertMode)
				Main.NewText("You are not powerful enough to withstand the chaos...", CosmivengeonUtils.TausFavouriteColour);
			if(CosmivengeonWorld.desoMode && !CosmivengeonMod.debug_toggleDesoMode)
				Main.NewText("Nice try, but the deed has already been done.", CosmivengeonUtils.TausFavouriteColour);

			//Disable Calamity's modes if they are active
			bool disableModeDisabler = true;
			if(!disableModeDisabler && Main.expertMode && (calamityRevengeance || calamityDeath)){
				CosmivengeonMod.DeactivateCalamityRevengeance();
				CosmivengeonMod.DeactivateCalamityDeath();
			}

			return Main.expertMode && (!CosmivengeonWorld.desoMode || CosmivengeonMod.debug_toggleDesoMode);
		}

		public override bool UseItem(Player player){
			Main.PlaySound(SoundID.ForceRoar, player.Center, 0);

			if(!CosmivengeonWorld.desoMode){
				CosmivengeonUtils.SendMessage("An otherworldly chaos has been unleashed...  No turning back now.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = true;
			}else{
				CosmivengeonUtils.SendMessage("The otherworldy chaos recedes...  For now.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = false;
			}

			if(Main.npc.Any(n => n?.active == true && n.boss))
				player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} was consumed by the chaos."), 9999, 0);

			return true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
