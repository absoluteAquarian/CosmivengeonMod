using CosmivengeonMod.API.Edits.Desomode;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using SerousCommonLib.API;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits {
	internal class DraekBossHealthMagicDetour : Edit {
		public override void LoadEdits() {
			On.Terraria.NPC.SetDefaults += NPC_SetDefaults;
		}

		public override void UnloadEdits() {
			On.Terraria.NPC.SetDefaults -= NPC_SetDefaults;
		}

		public static void NPC_SetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, NPCSpawnParams spawnparams) {
			orig(self, Type, spawnparams);

			int p1 = ModContent.NPCType<Draek>();
			int p2 = ModContent.NPCType<DraekP2Head>();
			float scale = MiscUtils.CalculateBossHealthScale(out _);

			bool expert = spawnparams.gameModeData is { } mode && (mode.IsExpertMode || mode.IsMasterMode);

			if (self.type == p1) {
				self.lifeMax = Draek.BaseHealth;

				if (expert)
					self.ScaleHealthBy(Draek.GetHealthAugmentation());

				int hp = expert ? GetDraekMaxHealth(scale) : Draek.BaseHealth + DraekP2Head.BaseHealth;
				(self.ModNPC as Draek).HealthThreshold = hp - self.lifeMax;
				self.lifeMax = hp;

				self.life = self.lifeMax;
			} else if (self.type == p2) {
				self.lifeMax = DraekP2Head.BaseHealth;

				if (expert)
					self.ScaleHealthBy(DraekP2Head.GetHealthAugmentation());

				int curMax = self.lifeMax;
				(self.ModNPC as DraekP2Head).ActualMaxHealth = curMax;
				self.lifeMax = expert ? GetDraekMaxHealth(scale) : Draek.BaseHealth + DraekP2Head.BaseHealth;

				self.life = curMax;
			}

			// FKBossHealthBar can cause this to throw errors for whatever reason.  Cringe!
			self.GetGlobalNPC<StatsNPC>().baseEndurance = self.GetGlobalNPC<StatsNPC>().endurance;

			// World is in Desolation Mode and the boss is a boss (wow)
			if (WorldEvents.desoMode && self.boss) {
				float normalFactor = 5f;
				float expertFactor = 2.5f;
				self.value *= normalFactor / expertFactor;
			}

			self.Desomode().QB_baseScale = self.scale;
			self.Desomode().QB_baseSize = self.Size;
		}

		public static int GetDraekMaxHealth(float bossScale)
			=> (int)(Draek.BaseHealth * Main.GameModeInfo.EnemyMaxLifeMultiplier * Draek.GetHealthAugmentation() * bossScale
				+ DraekP2Head.BaseHealth * Main.GameModeInfo.EnemyMaxLifeMultiplier * DraekP2Head.GetHealthAugmentation() * bossScale);
	}
}
