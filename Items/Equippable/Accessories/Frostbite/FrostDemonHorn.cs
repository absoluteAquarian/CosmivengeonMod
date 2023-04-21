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
		protected override bool CloneNewInstances => true;

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
			Item.width = 18;
			Item.height = 14;
			Item.rare = ItemRarityID.Blue;
			Item.accessory = true;
			Item.value = Item.sellPrice(silver: 8, copper: 20);
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			AccessoriesPlayer modPlayer = player.GetModPlayer<AccessoriesPlayer>();

			if(modPlayer.brokenFrostHorn)
				player.moveSpeed *= 0.85f;
			else{
				player.moveSpeed *= 1.35f;
				player.GetDamage(DamageClass.Generic) += 0.1f;
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
				if(tooltips[customIndex].Text.Contains("<N>"))
					break;

				customIndex++;
			}while(tooltips[customIndex].Name.StartsWith("CustomTooltip"));

			TooltipLine customLine = tooltips[customIndex];

			List<string> text = customLine.Text.Split(new string[]{ "<N>" }, StringSplitOptions.RemoveEmptyEntries).ToList();
			text.Insert(1, $"{(int)(Main.LocalPlayer.statLifeMax2 * 0.25f)}");
			customLine.Text = string.Join("", text.ToArray());
		}
	}
}
