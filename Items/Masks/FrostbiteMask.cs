using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Masks{
	[AutoloadEquip(EquipType.Head)]
	public class FrostbiteMask : ModItem{
		public override void SetDefaults(){
			item.width = 32;
			item.height = 34;
			item.rare = ItemRarityID.Blue;
			item.vanity = true;
		}

		public override bool DrawHead(){
			return false;
		}
	}
}