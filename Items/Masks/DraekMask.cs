using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Masks{
	[AutoloadEquip(EquipType.Head)]
	public class DraekMask : ModItem{
		public override void SetDefaults(){
			item.width = 40;
			item.height = 22;
			item.rare = 1;
			item.vanity = true;
		}

		public override bool DrawHead(){
			return false;
		}
	}
}
