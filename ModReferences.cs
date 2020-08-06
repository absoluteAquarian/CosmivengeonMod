using Terraria.ModLoader;

namespace CosmivengeonMod{
	public static class ModReferences{
		private static bool bossCheckListInstanceLoadAttempted = false;
		private static bool calamityInstanceLoadAttempted = false;

		private static Mod bossCheckList;
		private static Mod calamity;

		public static Mod BossChecklist{
			get{
				if(!bossCheckListInstanceLoadAttempted){
					bossCheckListInstanceLoadAttempted = true;
					bossCheckList = ModLoader.GetMod("BossChecklist");
				}
				return bossCheckList;
			}
		}
		public static Mod Calamity{
			get{
				if(!calamityInstanceLoadAttempted){
					calamityInstanceLoadAttempted = true;
					calamity = ModLoader.GetMod("CalamityMod");
				}
				return calamity;
			}
		}

		public static bool BossChecklistActive => BossChecklist != null;
		public static bool CalamityActive => Calamity != null;

		public static void Unload(){
			calamity = null;
			bossCheckList = null;
		}
	}
}
