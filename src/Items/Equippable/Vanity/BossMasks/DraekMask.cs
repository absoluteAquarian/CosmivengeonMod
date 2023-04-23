using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Vanity.BossMasks {
	[AutoloadEquip(EquipType.Head)]
	public class DraekMask : ModItem {
		public override void SetStaticDefaults() {
			ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false;
		}

		public override void SetDefaults() {
			Item.width = 28;
			Item.height = 22;
			Item.rare = ItemRarityID.Green;
			Item.vanity = true;
		}
	}
}
