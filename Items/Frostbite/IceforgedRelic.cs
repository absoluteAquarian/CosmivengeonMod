using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	public class IceforgedRelic : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Iceforged Relic");
			Tooltip.SetDefault("<add later>");
		}

		public override void SetDefaults(){
			item.rare = ItemRarityID.Blue;
		}

		public override bool CanUseItem(Player player) => false;
	}
}
