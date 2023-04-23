using CosmivengeonMod.API.Edits.Desomode;
using CosmivengeonMod.Systems;
using SerousCommonLib.API;
using System.Linq;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits.Detours {
	internal class DesolationModeBossAIRedirectsDetour : Edit {
		public static int[] DesomodeAITypes;

		public override void LoadEdits() {
			DesomodeAITypes = new int[] {
				// Bosses
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
				// Minions
				NPCID.TheHungry,
				NPCID.TheHungryII
			};

			On.Terraria.NPC.VanillaAI += NPC_VanillaAI;
		}

		public override void UnloadEdits() {
			DesomodeAITypes = null;

			On.Terraria.NPC.VanillaAI -= NPC_VanillaAI;
		}

		public static void NPC_VanillaAI(On.Terraria.NPC.orig_VanillaAI orig, NPC self) {
			//Desolation mode AI changes
			if (WorldEvents.desoMode && DesomodeAITypes.Contains(self.type)) {
				switch (self.type) {
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
			} else
				orig(self);
		}
	}
}
