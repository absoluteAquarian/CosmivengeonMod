﻿using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using CosmivengeonMod.Worlds;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits.Detours.Desomode{
	public static class DetourNPC{
		public static int[] DesomodeAITypes;

		public static void Load(){
			DesomodeAITypes = new int[]{
				//Bosses
				NPCID.KingSlime,
				NPCID.EyeofCthulhu,
				NPCID.EaterofWorldsHead,
				NPCID.EaterofWorldsBody,
				NPCID.EaterofWorldsTail,
				NPCID.BrainofCthulhu,
				NPCID.QueenBee,
				NPCID.SkeletronHead,
				NPCID.SkeletronHand,
				NPCID.WallofFlesh,
				NPCID.WallofFleshEye,
				//Minions
				NPCID.TheHungry,
				NPCID.TheHungryII
			};
		}

		public static void Unload(){
			DesomodeAITypes = null;
		}

		public static void NPC_SetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, float scaleOverride){
			orig(self, Type, scaleOverride);

			int p1 = ModContent.NPCType<Draek>();
			int p2 = ModContent.NPCType<DraekP2Head>();
			float scale = MiscUtils.CalculateBossHealthScale(out _);

			if(self.type == p1){
				self.lifeMax = Draek.BaseHealth;
				
				if(Main.expertMode)
					self.ScaleHealthBy(Draek.GetHealthAugmentation());
				
				int hp = Main.expertMode ? GetDraekMaxHealth(scale) : Draek.BaseHealth + DraekP2Head.BaseHealth;
				(self.ModNPC as Draek).HealthThreshold = hp - self.lifeMax;
				self.lifeMax = hp;

				self.life = self.lifeMax;
			}else if(self.type == p2){
				self.lifeMax = DraekP2Head.BaseHealth;

				if(Main.expertMode)
					self.ScaleHealthBy(DraekP2Head.GetHealthAugmentation());
				
				int curMax = self.lifeMax;
				(self.ModNPC as DraekP2Head).ActualMaxHealth = curMax;
				self.lifeMax = Main.expertMode ? GetDraekMaxHealth(scale) : Draek.BaseHealth + DraekP2Head.BaseHealth;
				
				self.life = curMax;
			}

			//FKBossHealthBar can cause this to throw errors for whatever reason.  Cringe!
			self.GetGlobalNPC<StatsNPC>().baseEndurance = self.GetGlobalNPC<StatsNPC>().endurance;

			//World is in Desolation Mode and the boss is a boss (wow)
			if(WorldEvents.desoMode && self.boss){
				float normalFactor = 5f;
				float expertFactor = 2.5f;
				self.value *= normalFactor / expertFactor;
			}

			self.Desomode().QB_baseScale = self.scale;
			self.Desomode().QB_baseSize = self.Size;
		}

		public static int NPC_GetBossHeadTextureIndex(On.Terraria.NPC.orig_GetBossHeadTextureIndex orig, NPC self){
			//Force EoC's icon to not show up if it's in the desomode attack and invisible
			//Force BoC's icon to not show up while it's in Phase 2
			bool hideEoC = self.type == NPCID.EyeofCthulhu && self.ai[0] == 6f && self.alpha > 150;
			bool hideBoC = self.type == NPCID.BrainofCthulhu && self.ai[0] < 0f;

			if(WorldEvents.desoMode && (hideEoC || hideBoC))
				return -1;
			return orig(self);
		}

		public static int GetDraekMaxHealth(float bossScale)
			=> (int)(Draek.BaseHealth * Main.GameModeInfo.EnemyMaxLifeMultiplier * Draek.GetHealthAugmentation() * bossScale
				+ DraekP2Head.BaseHealth * Main.GameModeInfo.EnemyMaxLifeMultiplier * DraekP2Head.GetHealthAugmentation() * bossScale);

		public static void NPC_VanillaAI(On.Terraria.NPC.orig_VanillaAI orig, NPC self){
			//Desolation mode AI changes
			if(WorldEvents.desoMode && DesomodeAITypes.Contains(self.type)){
				switch(self.type){
					//Bosses
					case NPCID.KingSlime:
						DesolationModeBossAI.AI_KingSlime(self);
						break;
					case NPCID.EyeofCthulhu:
						DesolationModeBossAI.AI_EyeOfCthulhu(self);
						break;
					case NPCID.EaterofWorldsHead:
					case NPCID.EaterofWorldsBody:
					case NPCID.EaterofWorldsTail:
						DesolationModeBossAI.AI_EaterOfWorlds(self);
						break;
					case NPCID.BrainofCthulhu:
						DesolationModeBossAI.AI_BrainOfCthulhu(self);
						break;
					case NPCID.QueenBee:
						DesolationModeBossAI.AI_QueenBee(self);
						break;
					case NPCID.SkeletronHead:
						DesolationModeBossAI.AI_SkeletronHead(self);
						break;
					case NPCID.SkeletronHand:
						DesolationModeBossAI.AI_SkeletronHand(self);
						break;
					case NPCID.WallofFlesh:
						DesolationModeBossAI.AI_WallOfFleshMouth(self);
						break;
					case NPCID.WallofFleshEye:
						DesolationModeBossAI.AI_WallOfFleshEye(self);
						break;
					//Minions
					case NPCID.TheHungry:
						DesolationModeMonsterAI.AI_TheHungry(self);
						break;
					case NPCID.TheHungryII:
						DesolationModeMonsterAI.AI_TheHungryII(self);
						break;
				}
			}else
				orig(self);
		}
	}
}
