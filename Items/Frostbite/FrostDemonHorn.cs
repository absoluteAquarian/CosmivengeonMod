using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Frostbite{
	[AutoloadEquip(EquipType.Front)]
	public class FrostDemonHorn : ModItem{
		public override bool CloneNewInstances => true;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Demon's Horn");
			Tooltip.SetDefault("A frozen horn salvaged from the frost lizard" +
				"\nIf the player takes more than <N> damage, the horn will be [c/aaaaaa:Broken]" +
				"\nHorn regenerates after 10 seconds" +
				"\nStats while [c/0099ff:Whole]:" +
				"\n- Movement speed is increased by 35%" +
				"\n- Damage is increased by 10%" +
				"\n- Defense is reduced by 15" +
				"\nStats while [c/666666:Broken]:" +
				"\n- Movement speed is decreased by 15%");
		}

		public override void SetDefaults(){
			item.width = 18;
			item.height = 14;
			item.rare = ItemRarityID.Blue;
			item.accessory = true;
			item.value = Item.sellPrice(silver: 8, copper: 20);
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			CosmivengeonPlayer modPlayer = player.GetModPlayer<CosmivengeonPlayer>();

			if(modPlayer.frostHorn_Broken){
				player.moveSpeed *= 0.85f;
			}else{
				player.moveSpeed *= 1.35f;
				player.allDamage += 0.1f;
				player.statDefense -= 15;
				player.AddBuff(ModContent.BuffType<Buffs.FrostHornWhole>(), 2);
			}

			modPlayer.equipped_FrostHorn = true;
		}

		public override void ModifyTooltips(List<TooltipLine> tooltips){
			if(tooltips.Any(t => t.Name == "SocialDesc"))
				return;

			TooltipLine line = tooltips.Find(t => t.text.Contains("<N>"));

			List<string> text = line.text.Split(new string[]{ "<N>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
			text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.25f)}");
			line.text = string.Join("", text.ToArray());
		}
	}
}
