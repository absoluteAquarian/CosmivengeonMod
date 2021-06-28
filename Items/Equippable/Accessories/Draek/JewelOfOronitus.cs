using CosmivengeonMod.DataStructures;
using CosmivengeonMod.Players;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Items.Equippable.Accessories.Draek{
	[AutoloadEquip(EquipType.Neck)]
	public class JewelOfOronitus : HidableTooltip{
		public override string ItemName => "Jewel of Oronitus";

		public override string FlavourText => "Damage dealt and damage reduction increased by 5%" +
				"\nMovement speed increased by 10%" +
				"\nFall speed increased by 20%" +
				"\nGrants an earthblessed mid-air jump" +
				"\nAn ancient artifact, last donned by the rock serpent Draek." +
				"\nIts original master, Oronitus, was seemingly lost to time many years ago...";

		public override void SafeSetStaticDefaults(){
			Main.RegisterItemAnimation(item.type, new JewelOfOronitusAnimation());
		}

		public override void SetDefaults(){
			item.width = 26;
			item.height = 34;
			item.accessory = true;
			item.value = Item.sellPrice(gold: 1, silver: 50);
			item.rare = ItemRarityID.Expert;
			item.expert = true;
		}

		public override void UpdateAccessory(Player player, bool hideVisual){
			player.allDamage += 0.05f;
			player.endurance += 0.05f;
			player.moveSpeed *= 1.1f;
			player.accRunSpeed *= 1.1f;
			player.maxFallSpeed *= 1.2f;
			player.gravity *= 1.2f;
			player.GetModPlayer<AccessoriesPlayer>().oronitusJump.abilityActive = true;
		}
	}

	public class JewelOfOronitusAnimation : DrawAnimation{
		public JewelOfOronitusAnimation(){
			FrameCount = 25;
			TicksPerFrame = 5;
		}

		public override Rectangle GetFrame(Texture2D texture)
			=> texture.Frame(1, FrameCount, 0, Frame);

		public override void Update(){
			if(++FrameCounter % TicksPerFrame == 0)
				Frame = ++Frame % FrameCount;
		}
	}
}
