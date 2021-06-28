using CosmivengeonMod.API;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Difficulty{
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

				var hotkeys = CoreMod.StaminaHotKey.GetAssignedKeys();
				if(hotkeys.Count > 0)
					hotkey = hotkeys[0];
				else
					hotkey = "<NOT BOUND>";

				customLine.text = customLine.text.Replace("\"G\"", $"\"{hotkey}\"");
			}while(tooltips[customIndex].Name.StartsWith("CustomTooltip"));
		}

		public override bool CanUseItem(Player player){
			//If the game is in multiplayer, only allow the host to use the item OR if this game is using a dedicated server, prevent the item's use entirely
			if(Main.netMode != NetmodeID.SinglePlayer && !Main.dedServ && MiscUtils.ClientIsLocalHost(Main.myPlayer)){
				Main.NewText("Only the server host can use this item!", Color.Red);
				return false;
			}else if(Main.dedServ){
				Main.NewText("This item cannot be used on dedicated servers!  Get the server owner to toggle Desolation mode through the server's console.", Color.Red);
				return false;
			}

			bool calamityRevengeance = (bool?)ModReferences.Calamity.Call("Difficulty", "Rev") ?? false;
			bool calamityDeath = (bool?)ModReferences.Calamity.Call("Difficulty", "Death") ?? false;

			if(player.GetModPlayer<StaminaPlayer>().stamina.Active || player.GetModPlayer<StaminaPlayer>().stamina.Exhaustion)
				return false;
			if(!Main.expertMode)
				Main.NewText("You are not powerful enough to withstand the chaos...", MiscUtils.TausFavouriteColour);
			if(WorldEvents.desoMode && !Debug.debug_toggleDesoMode)
				Main.NewText("Nice try, but the deed has already been done.", MiscUtils.TausFavouriteColour);

			//Disable Calamity's modes if they are active
			bool disableModeDisabler = true;
			if(!disableModeDisabler && Main.expertMode && (calamityRevengeance || calamityDeath)){
				CoreMod.DeactivateCalamityRevengeance();
				CoreMod.DeactivateCalamityDeath();
			}

			return Main.expertMode && (!WorldEvents.desoMode || Debug.debug_toggleDesoMode);
		}

		public override bool UseItem(Player player){
			Main.PlaySound(SoundID.ForceRoar, player.Center, 0);

			if(!WorldEvents.desoMode){
				MiscUtils.SendMessage("An otherworldly chaos has been unleashed...  No turning back now.", MiscUtils.TausFavouriteColour);
				WorldEvents.desoMode = true;
			}else{
				MiscUtils.SendMessage("The otherworldy chaos recedes...  For now.", MiscUtils.TausFavouriteColour);
				WorldEvents.desoMode = false;
			}

			for(int i = 0; i < Main.maxNPCs; i++){
				NPC npc = Main.npc[i];
				if(npc.active && npc.boss)
					player.KillMe(PlayerDeathReason.ByCustomReason($"{player.name} was consumed by the chaos."), 9999, 0);
			}

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
