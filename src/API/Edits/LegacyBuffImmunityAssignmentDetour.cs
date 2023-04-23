using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Systems;
using CosmivengeonMod.Utility;
using SerousCommonLib.API;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits {
	internal class LegacyBuffImmunityAssignmentDetour : Edit {
		public override void LoadEdits() {
			On.Terraria.NPC.SetDefaults += NPC_SetDefaults;
		}

		public override void UnloadEdits() {
			On.Terraria.NPC.SetDefaults -= NPC_SetDefaults;
		}

		private static void NPC_SetDefaults(On.Terraria.NPC.orig_SetDefaults orig, Terraria.NPC self, int Type, Terraria.NPCSpawnParams spawnparams) {
			orig(self, Type, spawnparams);

			if (WorldEvents.desoMode && self.type == NPCID.KingSlime) {
				// King Slime gets more buff immunity in Desolation mode
				self.SetImmune(BuffID.OnFire);
				self.SetImmune(BuffID.Frostburn);
				self.SetImmune(BuffID.CursedInferno);
				self.SetImmune(BuffID.ShadowFlame);
				self.SetImmune(BuffID.Daybreak);
				self.SetImmune(ModContent.BuffType<PrimordialWrath>());
			}
		}
	}
}
