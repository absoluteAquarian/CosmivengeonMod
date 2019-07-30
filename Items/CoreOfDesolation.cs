using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items{
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

		public override bool UseItem(Player player){
			if(!CosmivengeonMod.desoMode){
				Main.NewText("An otherworldly chaos has been unleashed...  No turning back now.", CosmivengeonMod.TausFavouriteColour);
				CosmivengeonMod.desoMode = true;
				return true;
			}else if(CosmivengeonMod.debug_toggleDesoMode){
				Main.NewText("Wait, what?  You aren't supposed to be able to toggle that!  Oh well, I guess it can't be helped.", CosmivengeonMod.TausFavouriteColour);
				CosmivengeonMod.desoMode = false;
				return true;
			}
			Main.NewText("Nice try, but the deed has already been done.", CosmivengeonMod.TausFavouriteColour);
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
