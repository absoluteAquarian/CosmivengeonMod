using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class CoreOfDesolation : ModItem{
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Core of Desolation");
			Tooltip.SetDefault("Activates Desolation Mode.");
		}

		public override void SetDefaults() {
			item.width = 20;
			item.height = 20;
			item.maxStack = 1;
			item.rare = 5;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			item.consumable = false;
		}

		public override bool CanUseItem(Player player){
			if(CosmivengeonWorld.desoMode && !CosmivengeonMod.debug_toggleDesoMode)
				Main.NewText("Nice try, but the deed has already been done.", CosmivengeonUtils.TausFavouriteColour);
			return !(CosmivengeonWorld.desoMode && !CosmivengeonMod.debug_toggleDesoMode);
		}

		public override bool UseItem(Player player){
			if(!CosmivengeonWorld.desoMode){
				Main.NewText("An otherworldly chaos has been unleashed...  No turning back now.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = true;
				return true;
			}else if(CosmivengeonMod.debug_toggleDesoMode){
				Main.NewText("Wait, what?  You aren't supposed to be able to toggle that!  Oh well, I guess it can't be helped.", CosmivengeonUtils.TausFavouriteColour);
				CosmivengeonWorld.desoMode = false;
				return true;
			}
			return false;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddTile(TileID.DemonAltar);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
