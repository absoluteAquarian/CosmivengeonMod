using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using MonoMod.Cil;
using SerousCommonLib.API;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits {
	internal class BestiaryLootAlternateRedirectionDetour : Edit {
		public override void LoadEdits() {
			IL.Terraria.GameContent.Bestiary.BestiaryDatabase.ExtractDropsForNPC += BestiaryDatabase_ExtractDropsForNPC;
		}

		public override void UnloadEdits() {
			IL.Terraria.GameContent.Bestiary.BestiaryDatabase.ExtractDropsForNPC -= BestiaryDatabase_ExtractDropsForNPC;
		}

		private static void BestiaryDatabase_ExtractDropsForNPC(ILContext il) {
			ILHelper.CommonPatchingWrapper(il, CoreMod.Instance, ExtractDropsForNPCPatch);
		}

		private static bool ExtractDropsForNPCPatch(ILCursor c, ref string badReturnReason) {
			if (!c.TryGotoNext(MoveType.After, i => i.MatchLdarg(1),
				i => i.MatchLdarg(2))) {
				badReturnReason = "Could not find arguments for ItemDropDatabase.GetRulesForNPCID() call";
				return false;
			}

			c.EmitDelegate(static (int npcID) => {
				if (npcID == ModContent.NPCType<Draek>())
					npcID = ModContent.NPCType<DraekP2Head>();  // Phase 2 is hidden from bestiary, so Phase 1 needs to report the drops

				return npcID;
			});

			return true;
		}
	}
}
