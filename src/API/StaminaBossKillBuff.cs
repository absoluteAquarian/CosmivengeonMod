using CosmivengeonMod.Abilities;
using CosmivengeonMod.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CosmivengeonMod.API {
	public abstract class StaminaBossKillBuff : ModType {
		public int Type { get; private set; }

		public abstract int NPCType { get; }

		public virtual string FlavorText => Language.GetTextValue($"Mods.{Mod.Name}.StaminaBuffText.{Name}");

		protected sealed override void Register() {
			Type = StaminaBossKillBuffLoader.Add(this);
			ModTypeLookup<StaminaBossKillBuff>.Register(this);
		}

		public sealed override void SetupContent() => SetStaticDefaults();

		public abstract IEnumerable<string> GetBossNames();

		public abstract void ModifyStamina(out StaminaStatModifier stat);

		protected virtual IEnumerable<string> GetBuffLines(StaminaStatModifier stat) {
			var attackSpeed = stat.attackSpeed.active.ApplyTo(100) - 100;
			var runAcceleration = stat.runAcceleration.active.ApplyTo(100) - 100;
			var maxRunSpeed = stat.maxRunSpeed.active.ApplyTo(100) - 100;
			var attackSpeedExhausted = stat.attackSpeed.exhausted.ApplyTo(100) - 100;
			var runAccelerationExhausted = stat.runAcceleration.exhausted.ApplyTo(100) - 100;
			var maxRunSpeedExhausted = stat.maxRunSpeed.exhausted.ApplyTo(100) - 100;
			var restorationRate = stat.restorationRate.active.ApplyTo(100) - 100;
			var restorationRateExhausted = stat.restorationRate.exhausted.ApplyTo(100) - 100;
			var consumptionRate = stat.consumptionRate.ApplyTo(100) - 100;
			var maxQuantity = stat.maxQuantity.ApplyTo(Stamina.ValueScalar);

			if (attackSpeed != 0)
				yield return $"Active attack speed rate: {attackSpeed:+0.###;-0.###}%";

			if (runAcceleration != 0 || maxRunSpeed != 0) {
				StringBuilder sb = new();

				if (runAcceleration != 0)
					sb.Append($"Active run acceleration rate: {runAcceleration:+0.###;-0.###}%");

				if (maxRunSpeed != 0) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append($"Active max run speed: {maxRunSpeed:+0.###;-0.###}%");
				}

				yield return sb.ToString();
			}

			if (attackSpeedExhausted != 0)
				yield return $"Exhausted attack speed rate: {attackSpeedExhausted:+0.###;-0.###}%";

			if (runAccelerationExhausted != 0 || maxRunSpeedExhausted != 0) {
				StringBuilder sb = new();

				if (runAccelerationExhausted != 0)
					sb.Append($"Exhausted run acceleration rate: {runAccelerationExhausted:+0.###;-0.###}%");

				if (maxRunSpeedExhausted != 0) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append($"Exhausted max run speed: {maxRunSpeedExhausted:+0.###;-0.###}%");
				}

				yield return sb.ToString();
			}

			if (restorationRate != 0 || restorationRateExhausted != 0 || consumptionRate != 0) {
				StringBuilder sb = new();

				if (restorationRate != 0)
					sb.Append($"Idle restoration rate: {restorationRate:+0.###;-0.###}%");

				if (restorationRateExhausted != 0) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append($"Exhausted restoration rate: {restorationRateExhausted:+0.###;-0.###}%");
				}

				if (consumptionRate != 0) {
					if (sb.Length > 0)
						sb.Append(", ");

					sb.Append($"Active consumption rate: {consumptionRate:+0.###;-0.###}%");
				}

				yield return sb.ToString();
			}

			if (maxQuantity != 0)
				yield return $"Maximum Stamina: {maxQuantity:+0.;-0.} units";
		}

		public virtual void TransmuteNPCType(ref int type) { }

		internal IEnumerable<string> GetClientMessage() {
			string flavorText = FlavorText;

			const StringSplitOptions options = StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries;

			// Get flavor text lines
			if (flavorText.Contains('\n') && flavorText.Split('\n', options) is { Length: > 1 } lines) {
				foreach (string line in lines)
					yield return line;
			} else if (!string.IsNullOrWhiteSpace(flavorText))
				yield return flavorText;

			// Get stamina buff lines
			ModifyStamina(out StaminaStatModifier stat);

			foreach (string buffLine in GetBuffLines(stat))
				yield return buffLine;
		}
	}

	public static class StaminaBossKillBuffLoader {
		private class Loadable : ILoadable {
			public void Load(Mod mod) { }

			public void Unload() {
				buffs.Clear();
				npcToBuffData.Clear();
			}
		}

		private static readonly List<StaminaBossKillBuff> buffs = new();

		private static readonly Dictionary<int, StaminaBossKillBuff> npcToBuffData = new();

		public static int Count => buffs.Count;

		public static IReadOnlyList<StaminaBossKillBuff> Buffs => buffs.AsReadOnly();

		internal static int Add(StaminaBossKillBuff buff) {
			buffs.Add(buff);
			return Count - 1;
		}

		public static StaminaBossKillBuff Get(int id) => id < 0 || id >= Count ? null : buffs[id];

		internal static void ValidateBuffData() {
			HashSet<int> foundIDs = new();

			foreach (var buff in buffs) {
				int npc = buff.NPCType;

				if (npc < 0 || npc >= NPCLoader.NPCCount)
					throw new IndexOutOfRangeException($"NPC ID for buff data \"{buff.FullName}\" ({npc}) was outside the range of valid IDs");

				if (!ContentSamples.NpcsByNetId[npc].boss)
					throw new ArgumentException($"NPC ID for buff data \"{buff.FullName}\" ({npc}) did not refer to a boss NPC");

				if (!buff.GetBossNames().Any())
					throw new Exception($"Buff data \"{buff.FullName}\" returned an empty collection for GetBossNames()");

				if (!foundIDs.Add(npc))
					throw new Exception($"NPC ID for buff data \"{buff.FullName}\" ({npc}) was already reserved for other data");

				npcToBuffData[npc] = buff;
			}
		}

		public static bool TryFindBuffData(int npcType, out StaminaBossKillBuff buff) => npcToBuffData.TryGetValue(npcType, out buff);

		public static IEnumerable<StaminaBossKillBuff> Filter(StaminaBuffCollection killedBosses) {
			foreach (var boss in killedBosses) {
				int id = boss.GetNPCID();

				if (!npcToBuffData.TryGetValue(id, out var data))
					throw new IndexOutOfRangeException("Boss data collection contained invalid data");

				yield return data;
			}
		}
	}
}
