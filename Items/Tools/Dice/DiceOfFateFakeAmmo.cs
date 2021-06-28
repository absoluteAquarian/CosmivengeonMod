using CosmivengeonMod.Utility;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Tools.Dice{
	public class DiceOfFateFakeAmmo : ModItem{
		public override string Texture => MiscUtils.GetPlaceholderTexture("Item");

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Unknown");
			Tooltip.SetDefault("You shouldn't be seeing this");
		}

		public override void SetDefaults(){
			item.ammo = item.type;
		}
	}
}
