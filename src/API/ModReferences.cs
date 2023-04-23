using CosmivengeonMod.DataStructures;

namespace CosmivengeonMod.API {
	public static class ModReferences {
		public static ModReference BossChecklist;
		public static ModReference Calamity;
		public static ModReference YABHB;

		public static void Load() {
			BossChecklist = new ModReference("BossChecklist");
			Calamity = new ModReference("CalamityMod");
			YABHB = new ModReference("FKBossHealthBar");
		}

		public static void Unload() {
			BossChecklist?.Unload();
			Calamity?.Unload();
			YABHB?.Unload();
		}
	}
}
