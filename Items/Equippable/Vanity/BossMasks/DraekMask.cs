using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class DraekMask : ModItem {
		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 22;
			Item.rare = ItemRarityID.Green;
			Item.vanity = true;
		}

		public override bool DrawHead()/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false if you returned false */{
			return false;
		}
	}
}
