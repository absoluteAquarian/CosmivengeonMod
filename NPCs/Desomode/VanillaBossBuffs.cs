using CosmivengeonMod.Buffs;
using CosmivengeonMod.Detours;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class VanillaBossBuffs : GlobalNPC{
		public override void SetDefaults(NPC npc){
			if(!Main.expertMode || !CosmivengeonWorld.desoMode)
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
			if(!CosmivengeonWorld.desoMode)
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
			}

			//Minion buffs!
			switch(npc.type){
				case NPCID.Creeper:
					npc.lifeMax = 180;
					npc.damage = 35;
					npc.defense = 15;
					npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance = 0.08f;
					npc.knockBackResist = 0.6f;
					break;
			}
		}
	}
}
