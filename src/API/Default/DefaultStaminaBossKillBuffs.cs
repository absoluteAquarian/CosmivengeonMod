using CosmivengeonMod.Abilities;
using CosmivengeonMod.NPCs.Bosses.DraekBoss;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss;
using System.Collections.Generic;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Default {
	public class KingSlimeStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.KingSlime;

		public override IEnumerable<string> GetBossNames() {
			yield return "King Slime";
			yield return "KS";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.restorationRate.active += 0.04f;
			stat.restorationRate.exhausted += 0.02f;
			stat.consumptionRate -= 0.01f;
			stat.maxQuantity.Base += 0.05f * Stamina.DefaultMaxQuantity;
		}
	}

	public class FrostbiteStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => ModContent.NPCType<Frostbite>();

		public override IEnumerable<string> GetBossNames() {
			yield return "Frostbite";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.exhausted += 0.01f;
			stat.runAcceleration.exhausted += 0.0075f;
			stat.maxRunSpeed.exhausted += 0.015f;
			stat.restorationRate.exhausted += 0.015f;
		}
	}

	public class EyeOfCthulhuStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.EyeofCthulhu;

		public override IEnumerable<string> GetBossNames() {
			yield return "Eye of Cthulhu";
			yield return "EoC";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.025f;
			stat.runAcceleration.active += 0.04f;
			stat.maxRunSpeed.active += 0.02f;
		}
	}

	public class EaterOfWorldsStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.EaterofWorldsHead;

		public override bool BossIsOnlyConditionallyABoss => true;

		public override IEnumerable<string> GetBossNames() {
			yield return "Eater of Worlds";
			yield return "EoW";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.exhausted += 0.01f;
			stat.runAcceleration.exhausted += 0.015f;
			stat.maxRunSpeed.exhausted += 0.02f;
			stat.restorationRate.exhausted += 0.01f;
		}

		public override void TransmuteNPCType(ref int type) {
			if (type == NPCID.EaterofWorldsBody || type == NPCID.EaterofWorldsTail)
				type = NPCID.EaterofWorldsHead;
		}
	}

	public class BrainOfCthulhuStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.BrainofCthulhu;

		public override IEnumerable<string> GetBossNames() {
			yield return "Brain of Cthulhu";
			yield return "BoC";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.015f;
			stat.runAcceleration.active += 0.025f;
			stat.maxRunSpeed.active += 0.02f;
		}
	}

	public class DraekStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => ModContent.NPCType<DraekP2Head>();

		public override IEnumerable<string> GetBossNames() {
			yield return "Draek";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.01f;
			stat.runAcceleration.active += 0.015f;
			stat.maxRunSpeed.active += 0.015f;
			stat.restorationRate.active += 0.05f;
			stat.consumptionRate -= 0.01f;
			stat.maxQuantity.Base += 0.05f * Stamina.DefaultMaxQuantity;
		}

		public override void TransmuteNPCType(ref int type) {
			if (type == ModContent.NPCType<Draek>())
				type = ModContent.NPCType<DraekP2Head>();
		}
	}

	// TODO: DD2 Dark Mage

	public class QueenBeeStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.QueenBee;

		public override IEnumerable<string> GetBossNames() {
			yield return "Queen Bee";
			yield return "QB";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.015f;
			stat.runAcceleration.active += 0.0225f;
			stat.attackSpeed.exhausted += 0.0075f;
			stat.runAcceleration.exhausted += 0.01f;
		}
	}

	public class SkeletronStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.SkeletronHead;

		public override IEnumerable<string> GetBossNames() {
			yield return "Skeletron";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.015f;
			stat.runAcceleration.active += 0.025f;
			stat.maxRunSpeed.active += 0.03f;
			stat.attackSpeed.exhausted += 0.02f;
			stat.runAcceleration.exhausted += 0.015f;
			stat.maxRunSpeed.exhausted += 0.01f;
			stat.restorationRate.active += 0.06f;
			stat.restorationRate.exhausted += 0.04f;
			stat.consumptionRate -= 0.015f;
			stat.maxQuantity.Base += 0.075f * Stamina.DefaultMaxQuantity;
		}
	}

	// TODO: Deerclops

	public class WallOfFleshStaminaBuff : StaminaBossKillBuff {
		public override int NPCType => NPCID.WallofFlesh;

		public override IEnumerable<string> GetBossNames() {
			yield return "Wall of Flesh";
			yield return "WoF";
		}

		public override void ModifyStamina(out StaminaStatModifier stat) {
			stat = StaminaStatModifier.Default;
			stat.attackSpeed.active += 0.02f;
			stat.runAcceleration.active += 0.03f;
			stat.maxRunSpeed.active += 0.025f;
			stat.attackSpeed.exhausted += 0.015f;
			stat.runAcceleration.exhausted += 0.02f;
			stat.maxRunSpeed.exhausted += 0.015f;
			stat.restorationRate.active += 0.08f;
			stat.restorationRate.exhausted += 0.05f;
			stat.consumptionRate -= 0.02f;
			stat.maxQuantity.Base += 0.125f * Stamina.DefaultMaxQuantity;
		}
	}
}
