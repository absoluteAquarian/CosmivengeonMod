using CosmivengeonMod.Buffs.Mechanics;
using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using System;
using System.Collections.Generic;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Frostbite{
	[AutoloadEquip(EquipType.Front)]
	public class FrostDemonHorn : HidableTooltip{
		public override bool CloneNewInstances => true;

		public override string ItemName => "Frost Demon's Horn";

		public override string FlavourText => "A frozen horn salvaged from the frost lizard" +
				"\nIf the player takes more than <N> damage, the horn" +
				"\nwill be [c/aaaaaa:Broken]" +
				"\nHorn regenerates after 10 seconds" +
				"\nStats while [c/0099ff:Whole]:" +
				"\n- Movement speed is increased by 35%" +
				"\n- Damage is increased by 10%" +
				"\n- Defense is reduced by 15" +
				"\nStats while [c/666666:Broken]:" +
				"\n- Movement speed is decreased by 15%";

		public override void SetDefaults(){
			item.width = 18;
			item.height = 14;
			item.rare = ItemRarityID.Blue;
			item.accessory = true;
			item.value = Item.sellPrice(silver: 8, copper: 20);
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			AccessoriesPlayer modPlayer = player.GetModPlayer<AccessoriesPlayer>();

			if(modPlayer.brokenFrostHorn)
				player.moveSpeed *= 0.85f;
			else{
				player.moveSpeed *= 1.35f;
				player.allDamage += 0.1f;
				player.statDefense -= 15;
				player.AddBuff(ModContent.BuffType<FrostHornWhole>(), 2);
			}

			modPlayer.frostHorn = true;
		}

		public override void SafeModifyTooltips(List<TooltipLine> tooltips){
			int customIndex = FindCustomTooltipIndex(tooltips);

			if(customIndex < 0 || tooltips.Any(t => t.Name == "SocialDesc"))
				return;

			do{
				if(tooltips[customIndex].text.Contains("<N>"))
					break;

				customIndex++;
			}while(tooltips[customIndex].Name.StartsWith("CustomTooltip"));

			TooltipLine customLine = tooltips[customIndex];

			List<string> text = customLine.text.Split(new string[]{ "<N>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
			text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.25f)}");
			customLine.text = string.Join("", text.ToArray());
		}
	}
}
