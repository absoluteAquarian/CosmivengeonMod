using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.Detours{
	public static class DetourPlayer{
		public static void Load(){
			On.Terraria.Player.StatusPlayer += HookStatusPlayer;
		}

		private static void HookStatusPlayer(On.Terraria.Player.orig_StatusPlayer orig, Player self, NPC npc){
			if(npc.type == NPCID.BrainofCthulhu && CosmivengeonWorld.desoMode){
				if(Main.rand.NextBool(3)){
					int num = Main.rand.Next(8);
					if(num == 2)
						num = Main.rand.Next(8);

					float num2 = Main.rand.NextFloat(0.75f, 1.5f) * 60f;

					//Removed case for Confused debuff
					switch(num){
						case 0:
							self.AddBuff(BuffID.Poisoned, (int)(num2 * 3.5f));
							break;
						case 1:
							self.AddBuff(BuffID.Darkness, (int)(num2 * 2f));
							break;
						case 2:
							self.AddBuff(BuffID.Cursed, (int)(num2 * 0.5f));
							break;
						case 3:
							self.AddBuff(BuffID.Bleeding, (int)(num2 * 5f));
							break;
						case 4:
							self.AddBuff(BuffID.Slow, (int)(num2 * 3.5f));
							break;
						case 5:
							self.AddBuff(BuffID.Weak, (int)(num2 * 7.5f));
							break;
						case 6:
							self.AddBuff(BuffID.Silenced, (int)(num2 * 1f));
							break;
						case 7:
							self.AddBuff(BuffID.BrokenArmor, (int)(num2 * 6.5f));
							break;
					}
				}
			}else
				orig(self, npc);
		}
	}
}
