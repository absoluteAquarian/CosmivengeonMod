using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Draek {
	[AutoloadEquip(EquipType.Neck)]
	public class JewelOfOronitus : HidableTooltip {
		public override string ItemName => "Jewel of Oronitus";

		public override string FlavourText => "Damage dealt and damage reduction increased by 5%" +
				"\nMovement speed increased by 10%" +
				"\nFall speed increased by 20%" +
				"\nGrants an earthblessed mid-air jump" +
				"\nAn ancient artifact, last donned by the rock serpent Draek." +
				"\nIts original master, Oronitus, was seemingly lost to time many years ago...";

		public override void SafeSetStaticDefaults() {
			Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(ticksperframe: 5, frameCount: 25));
		}

		public override void SetDefaults() {
			Item.width = 26;
			Item.height = 34;
			Item.accessory = true;
			Item.value = Item.sellPrice(gold: 1, silver: 50);
			Item.rare = ItemRarityID.Expert;
			Item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual) {
			player.GetDamage(DamageClass.Generic) += 0.05f;
			player.endurance += 0.05f;
			player.moveSpeed *= 1.1f;
			player.accRunSpeed *= 1.1f;
			player.maxFallSpeed *= 1.2f;
			player.gravity *= 1.2f;
			player.GetModPlayer<AccessoriesPlayer>().oronitusJump.hasJumpOption = true;
		}
	}
}
