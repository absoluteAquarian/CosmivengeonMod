using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Global {
	public class AmmoOverride : GlobalItem {
		public override void SetDefaults(Item item) {
			//Allows the Frostfire's Breath to use the vanilla ice blocks as ammo
			//This might override the use of Ice Blocks by other mods!
			if (new int[] { ItemID.IceBlock, ItemID.PinkIceBlock, ItemID.PurpleIceBlock, ItemID.RedIceBlock }.Contains(item.type))
				item.ammo = ItemID.IceBlock;
		}
	}
}
