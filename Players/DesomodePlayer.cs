using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Players{
	public class DesomodePlayer : ModPlayer{
		public override void PostUpdateRunSpeeds(){
			if(player.HasBuff(BuffID.Slimed)){
				player.maxRunSpeed *= 0.85f;
				player.accRunSpeed *= 0.85f;
				player.runAcceleration *= 0.85f;
			}
		}

		public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit){
			int[] slimeTypes = new int[]{
				NPCID.ArmedZombieSlimed,
				NPCID.BabySlime,
				NPCID.BigSlimedZombie,
				NPCID.BlackSlime,
				NPCID.BlueSlime,
				NPCID.CorruptSlime,
				NPCID.DungeonSlime,
				NPCID.GreenSlime,
				NPCID.IceSlime,
				NPCID.IlluminantSlime,
				NPCID.JungleSlime,
				NPCID.KingSlime,
				NPCID.LavaSlime,
				NPCID.MotherSlime,
				NPCID.PurpleSlime,
				NPCID.RainbowSlime,
				NPCID.RedSlime,
				NPCID.SandSlime,
				NPCID.SlimedZombie,
				NPCID.Slimeling,
				NPCID.SlimeMasked,
				NPCID.Slimer,
				NPCID.Slimer2,
				NPCID.SlimeRibbonGreen,
				NPCID.SlimeRibbonRed,
				NPCID.SlimeRibbonWhite,
				NPCID.SlimeRibbonYellow,
				NPCID.SlimeSpiked,
				NPCID.SmallSlimedZombie,
				NPCID.SpikedIceSlime,
				NPCID.SpikedJungleSlime,
				NPCID.UmbrellaSlime,
				NPCID.YellowSlime,
				NPCID.Pinky
			};

			if(player.HasBuff(BuffID.Slimed) && slimeTypes.Contains(npc.type)){
				damage = (int)Math.Max(damage * 1.35f, damage + 1);
			}
		}

		public override void PreUpdateBuffs(){
			if(CosmivengeonWorld.desoMode && NPC.AnyNPCs(NPCID.EyeofCthulhu))
				player.AddBuff(BuffID.Darkness, 601);
		}
	}
}
