using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Global{
	public class DesomodeBossBuffs : GlobalNPC{
		public override void SetDefaults(NPC npc){
			if(!Main.expertMode || !WorldEvents.desoMode)
				return;

			if(npc.type == NPCID.KingSlime){
				npc.SetImmune(BuffID.OnFire);
				npc.SetImmune(BuffID.Frostburn);
				npc.SetImmune(BuffID.CursedInferno);
				npc.SetImmune(BuffID.ShadowFlame);
				npc.SetImmune(BuffID.Daybreak);
				npc.SetImmune(ModContent.BuffType<PrimordialWrath>());
				npc.lavaImmune = true;
			}
		}

		public override void ScaleExpertStats(NPC npc, int numPlayers, float bossLifeScale){
			if(!WorldEvents.desoMode)
				return;

			float scaleEoW = 0.75f;

			switch(npc.type){
				case NPCID.KingSlime:
					//Base health: 2000
					npc.lifeMax = 2000;
					npc.ScaleHealthBy(0.8125f);		//3250 max health for one player
					npc.defense = 20;
					npc.damage = 95;
					break;
				case NPCID.EyeofCthulhu:
					//Base health: 2800
					npc.lifeMax = 2800;
					npc.ScaleHealthBy(0.725f);		//4060 max health for one player
					npc.defense = 23;
					npc.damage = 45;
					break;
				case NPCID.EaterofWorldsHead:
					//Base health: 65
					npc.lifeMax = 65;
					npc.ScaleHealthBy(scaleEoW);	//97 max health for one player
					npc.defense = 7;
					npc.damage = 80;
					break;
				case NPCID.EaterofWorldsBody:
					//Base health: 150
					npc.lifeMax = 150;
					npc.ScaleHealthBy(scaleEoW);	//225 max health for one player
					npc.defense = 10;
					npc.damage = 40;
					break;
				case NPCID.EaterofWorldsTail:
					//Base health: 220
					npc.lifeMax = 220;
					npc.ScaleHealthBy(scaleEoW);	//330 max health for one player
					npc.defense = 15;
					npc.damage = 25;
					break;
				case NPCID.BrainofCthulhu:
					//Base health: 1000
					npc.lifeMax = 1000;
					npc.ScaleHealthBy(1.15f);		//2300 max health for one player
					npc.defense = 20;
					npc.damage = 66;
					break;
				case NPCID.QueenBee:
					//Base health: 3400
					npc.lifeMax = 3400;
					npc.ScaleHealthBy(0.85f);		//5780 max health for one player
					npc.defense = 15;
					npc.damage = 60;
					break;
				case NPCID.SkeletronHead:
					//Base health: 4400
					npc.lifeMax = 4400;
					npc.ScaleHealthBy(1.1f);		//9680 max health for one player
					npc.defense = 15;
					npc.damage = 90;
					break;
				case NPCID.SkeletronHand:
					//Base health: 600
					npc.lifeMax = 600;
					npc.ScaleHealthBy(1.6f);		//1920 max health for one player
					npc.defense = 20;
					npc.damage = 100;
					break;
				case NPCID.WallofFlesh:
					npc.lifeMax = 8000;
					npc.ScaleHealthBy(0.925f);		//14800 max health for one player
					npc.defense = 9999;
					npc.damage = 200;
					npc.GetGlobalNPC<StatsNPC>().endurance = 0.99f;
					break;
				case NPCID.WallofFleshEye:
					npc.lifeMax = 8000;
					npc.ScaleHealthBy(0.925f);		//14800 max health for one player
					npc.defense = 12;
					npc.damage = 200;
					break;
			}

			//Minion buffs!
			switch(npc.type){
				case NPCID.Creeper:
					npc.lifeMax = 180;
					npc.damage = 35;
					npc.defense = 15;
					npc.GetGlobalNPC<StatsNPC>().endurance = 0.08f;
					npc.knockBackResist = 0.6f;
					break;
				case NPCID.TheHungry:
					npc.lifeMax = 400;
					npc.defense = 20;
					npc.damage = 80;
					npc.GetGlobalNPC<StatsNPC>().endurance = 0.05f;
					npc.knockBackResist = 0.9f;
					break;
				case NPCID.TheHungryII:
					npc.lifeMax = 150;
					npc.defense = 10;
					npc.damage = 80;
					npc.GetGlobalNPC<StatsNPC>().endurance = 0.1f;
					npc.knockBackResist = 0.6f;
					break;
			}
		}
	}
}
