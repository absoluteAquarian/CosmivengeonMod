using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Vanity.BossMasks{
	[AutoloadEquip(EquipType.Head)]
	public class FrostbiteMask : ModItem{
		public override void SetDefaults(){
			Item.width = 32;
			Item.height = 34;
			Item.rare = ItemRarityID.Blue;
			Item.vanity = true;
		}

		public override bool DrawHead()/* tModPorter Note: Removed. In SetStaticDefaults, use ArmorIDs.Head.Sets.DrawHead[Item.headSlot] = false if you returned false */{
			return false;
		}
	}
}
