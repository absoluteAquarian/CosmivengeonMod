﻿using Terraria.ModLoader;

namespace CosmivengeonMod{
	public static class ModReferences{
		public static QuickReference BossChecklist;
		public static QuickReference Calamity;
		public static QuickReference CheatSheet;
		public static QuickReference YABHB;

		public static void Load(){
			BossChecklist = new QuickReference("BossChecklist");
			Calamity = new QuickReference("CalamityMod");
			CheatSheet = new QuickReference("CheatSheet");
			YABHB = new QuickReference("FKBossHealthBar");
		}

		public static void Unload(){
			BossChecklist?.Unload();
			Calamity?.Unload();
			CheatSheet?.Unload();
			YABHB?.Unload();
		}
	}
}
