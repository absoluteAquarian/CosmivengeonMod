using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Vanity.BossMasks{
	[AutoloadEquip(EquipType.Head)]
	public class DraekMask : ModItem{
		public override void SetDefaults(){
			item.width = 28;
			item.height = 22;
			item.rare = ItemRarityID.Green;
			item.vanity = true;
		}

		public override bool DrawHead(){
			return false;
		}
	}
}
