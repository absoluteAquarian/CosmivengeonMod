using CosmivengeonMod.Players;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Frostbite{
	[AutoloadEquip(EquipType.Front, EquipType.Back)]
	public class SnowscaleCoat : ModItem{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Snowscale Coat");
			Tooltip.SetDefault("Grants 6 defense and increases damage by 5% while in the snow biome" +
				"\nGrants an immunity to the [c/0099ff:Chilled] debuff" +
				"\nAll attacks have a small chance to inflict [c/6fa8dc:Frostburn]");
		}

		public override void SetDefaults(){
			item.accessory = true;
			item.width = 32;
			item.height = 34;
			item.rare = ItemRarityID.Blue;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			if(player.ZoneSnow){
				player.statDefense += 6;
				player.allDamage += 0.05f;
			}
			player.buffImmune[BuffID.Chilled] = true;
			player.GetModPlayer<AccessoriesPlayer>().snowCoat = true;
		}
	}
}
