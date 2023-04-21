using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Debugging {
	public class PotentiometerItem : ModItem {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Potentiometer");
			Tooltip.SetDefault(CoreMod.Descriptions.DebugItem +
				"\nClick to show the angle from the player's center to the mouse position.");
		}

		public override void SetDefaults() {
			Item.width = 20;
			Item.height = 20;
			Item.maxStack = 99;
			Item.rare = ItemRarityID.Pink;
			Item.useAnimation = 20;
			Item.useTime = 20;
			Item.useStyle = ItemUseStyleID.HoldUp;
			Item.UseSound = new Terraria.Audio.LegacySoundStyle(SoundID.MenuTick, 0);
			Item.consumable = false;
		}

		public override bool CanUseItem(Player player) {
			if (!Debug.debug_canUsePotentiometer) {
				Main.NewText("Sorry, but you can't use this item.", Color.LightGray);
				return false;
			}
			return true;
		}

		public override bool? UseItem(Player player)/* tModPorter Suggestion: Return null instead of false */{
			float angle = Vector2.Normalize(player.Center - Main.MouseWorld).ToRotation();

			Main.NewText(string.Format("POTENTIOMETER: {0} radians, {1} degrees", angle, MathHelper.ToDegrees(angle)));

			return true;
		}
	}
}
