using CosmivengeonMod.NPCs.Draek;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Detours{
	public static class DetourNPC{
		public static int[] BossTypes;

		public static void Load(){
			On.Terraria.NPC.SetDefaults += HookSetDefaults;
			On.Terraria.NPC.VanillaAI += HookVanillaAI;
			On.Terraria.NPC.GetBossHeadTextureIndex += HookGetBossHeadTextureIndex;

			BossTypes = new int[]{
				NPCID.KingSlime,
				NPCID.EyeofCthulhu,
				NPCID.EaterofWorldsHead,
				NPCID.EaterofWorldsBody,
				NPCID.EaterofWorldsTail,
				NPCID.BrainofCthulhu
			};
		}

		public static void Unload(){
			BossTypes = null;
		}

		public static void HookSetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, float scaleOverride){
			orig(self, Type, scaleOverride);

			int p1 = ModContent.NPCType<Draek>();
			int p2 = ModContent.NPCType<DraekP2Head>();
			float scale = CosmivengeonUtils.CalculateBossHealthScale(out _);

			if(self.type == p1){
				self.lifeMax = Draek.BaseHealth;
				
				self.ScaleHealthBy(Draek.GetHealthAugmentation());
				
				int hp = GetDraekMaxHealth(scale);
				(self.modNPC as Draek).HealthThreshold = hp - self.lifeMax;
				self.lifeMax = hp;

				self.life = self.lifeMax;
			}else if(self.type == p2){
				self.lifeMax = DraekP2Head.BaseHealth;

				self.ScaleHealthBy(DraekP2Head.GetHealthAugmentation());
				
				int curMax = self.lifeMax;
				(self.modNPC as DraekP2Head).ActualMaxHealth = curMax;
				self.lifeMax = GetDraekMaxHealth(scale);
				
				self.life = curMax;
			}

			self.GetGlobalNPC<CosmivengeonGlobalNPC>().baseEndurance = self.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance;
		}

		public static int HookGetBossHeadTextureIndex(On.Terraria.NPC.orig_GetBossHeadTextureIndex orig, NPC self){
			//Force EoC's icon to not show up if it's in the desomode attack and invisible
			//Force BoC's icon to not show up while it's in Phase 2
			bool hideEoC = self.type == NPCID.EyeofCthulhu && self.ai[0] == 6f && self.alpha > 150;
			bool hideBoC = self.type == NPCID.BrainofCthulhu && self.ai[0] < 0f;

			if(CosmivengeonWorld.desoMode && (hideEoC || hideBoC))
				return -1;
			return orig(self);
		}

		public static int GetDraekMaxHealth(float bossScale)
			=> (int)(Draek.BaseHealth * Main.expertLife * Draek.GetHealthAugmentation() * bossScale
				+ DraekP2Head.BaseHealth * Main.expertLife * DraekP2Head.GetHealthAugmentation() * bossScale);

		public static void HookVanillaAI(On.Terraria.NPC.orig_VanillaAI orig, NPC self){
			//Desolation mode AI changes
			if(CosmivengeonWorld.desoMode && BossTypes.Contains(self.type)){
				switch(self.type){
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
				}
			}else
				orig(self);
		}
	}
}
