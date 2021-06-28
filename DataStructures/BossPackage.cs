using CosmivengeonMod.Enums;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.DataStructures{
	public class BossPackage{
		public int bossID;
		public int altBossID;
		public string badSummonUseMessage;
		public Func<Player, bool> useRequirement;
		public Func<float, int> music;

		public static Dictionary<CosmivengeonBoss, BossPackage> bossInfo;

		public BossPackage(int id, int altID, string badUseMessage, Func<Player, bool> requirement, Func<float, int> musicFunc){
			if(ModContent.GetModNPC(id)?.mod != CoreMod.Instance)
				throw new ArgumentException("ID wasn't a valid Cosmivengeon boss ID");
			if(altID > 0 && ModContent.GetModNPC(altID)?.mod != CoreMod.Instance)
				throw new ArgumentException("Alternate ID wasn't a valid Cosmivengeon boss ID");

			bossID = id;
			altBossID = altID;
			badSummonUseMessage = badUseMessage;
			useRequirement = requirement;
			music = musicFunc;
		}
	}
}
