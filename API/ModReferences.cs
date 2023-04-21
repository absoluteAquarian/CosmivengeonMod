using CosmivengeonMod.DataStructures;

namespace CosmivengeonMod.API {
	public static class ModReferences {
		public static ModReference BossChecklist;
		public static ModReference Calamity;
		public static ModReference CheatSheet;
		public static ModReference YABHB;

		public static void Load() {
			BossChecklist = new ModReference("BossChecklist");
			Calamity = new ModReference("CalamityMod");
			CheatSheet = new ModReference("CheatSheet");
			YABHB = new ModReference("FKBossHealthBar");
		}

		public static void Unload() {
			BossChecklist?.Unload();
			Calamity?.Unload();
			CheatSheet?.Unload();
			YABHB?.Unload();
		}
	}
}
