using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class FrostbiteMask : ModItem {
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
		}

		public override void SetDefaults() {
			Item.width = 32;
			Item.height = 34;
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}
	}
}
