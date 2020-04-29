using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.DebugOrTogglers{
	public class PotentiometerItem : ModItem{
		public override string Texture => "CosmivengeonMod/Items/DebugOrTogglers/Potentiometer";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Potentiometer");
			Tooltip.SetDefault("Click to show the angle from the player's center\nto the mouse position.");
		}

		public override void SetDefaults(){
			item.width = 20;
			item.height = 20;
			item.maxStack = 99;
			item.rare = ItemRarityID.Pink;
			item.useAnimation = 20;
			item.useTime = 20;
			item.useStyle = ItemUseStyleID.HoldingUp;
			item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.MenuTick, 0);
			item.consumable = false;
		}

		public override bool CanUseItem(Player player){
			if(!CosmivengeonMod.debug_canUsePotentiometer){
				Main.NewText("Sorry, but you can't use this item.", Color.LightGray);
				return false;
			}
			return true;
		}

		public override bool UseItem(Player player){
			float angle = Vector2.Normalize(player.Center - Main.MouseWorld).ToRotation();

			Main.NewText(string.Format("POTENTIOMETER: {0} radians, {1} degrees", angle, MathHelper.ToDegrees(angle)));

			return true;
		}
	}
}
