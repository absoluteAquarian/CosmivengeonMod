using CosmivengeonMod.Buffs;
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

			switch(npc.type){
				case NPCID.KingSlime:
					//Base health: 2000
					npc.lifeMax = 2000;
					npc.ScaleHealthBy(0.625f, numPlayers);	//3250 max health for one player; +62.5% per player
					npc.defense = 20;
					npc.damage = 95;
					break;
				case NPCID.EyeofCthulhu:
					//Base health: 2800
					npc.lifeMax = 2800;
					npc.ScaleHealthBy(0.45f, numPlayers);	//4060 max health for one player; +45% per player
					npc.defense = 23;
					npc.damage = 45;
					break;
			}
		}
	}
}
