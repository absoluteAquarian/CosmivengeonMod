using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Items.Draek{
	public class DraekSummon : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Mysterious Geode");
			Tooltip.SetDefault("Summons the master of the forest\nMust be used in a forest biome");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = 5;
			item.useAnimation = 45;
			item.useTime = 45;
			item.useStyle = 4;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0);
			item.consumable = true;
		}

		public override bool CanUseItem(Player player){
			//Return false if "Draek" has already been summoned and the player isn't in the forest biome
			bool forest = CosmivengeonMod.PlayerIsInForest(player);

			if(!NPC.AnyNPCs(mod.NPCType("Draek"))){
				if(!forest){
					Main.NewText("\"The geode was unresponsive.  Maybe I should try using it in the forest?\"", 255, 255, 255);
					return false;
				}
				return true;
			}
			return false;
		}

		public override bool UseItem(Player player){
			//Spawn "Draek"
			NPC.SpawnOnPlayer(player.whoAmI, mod.NPCType("Draek"));
			return true;
		}

		public override void AddRecipes(){
			ModRecipe recipe = new ModRecipe(mod);
			recipe.AddIngredient(ItemID.StoneBlock, 50);
			recipe.AddIngredient(ItemID.Emerald, 1);
			recipe.AddTile(TileID.Anvils);
			recipe.SetResult(this);
			recipe.AddRecipe();
		}
	}
}
