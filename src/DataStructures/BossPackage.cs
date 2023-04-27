using CosmivengeonMod.Enums;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures {
	public class BossPackage {
		public readonly int bossID;
		public readonly int altBossID;
		public readonly string invalidSummonUseMessage;
		public readonly Func<Player, bool> checkSummonRequirement;
		public readonly WeightedTable<int> musicTable;

		internal static Dictionary<CosmivengeonBoss, BossPackage> bossInfo;

		public BossPackage(int bossID, int altBossID, string invalidSummonUseMessage, Func<Player, bool> checkSummonRequirement, WeightedTable<int> musicTable) {
			if (ModContent.GetModNPC(bossID)?.Mod != CoreMod.Instance)
				throw new ArgumentException("ID wasn't a valid Cosmivengeon boss ID");
			if (altBossID > 0 && ModContent.GetModNPC(altBossID)?.Mod != CoreMod.Instance)
				throw new ArgumentException("Alternate ID wasn't a valid Cosmivengeon boss ID");

			this.bossID = bossID;
			this.altBossID = altBossID;
			this.invalidSummonUseMessage = invalidSummonUseMessage;
			this.checkSummonRequirement = checkSummonRequirement;
			this.musicTable = musicTable;
		}

		public static void SetBossInfo(CosmivengeonBoss boss, BossPackage info) => bossInfo[boss] = info;

		public static bool TryGetBossInfo(CosmivengeonBoss boss, out BossPackage info) => bossInfo.TryGetValue(boss, out info);
	}
}
