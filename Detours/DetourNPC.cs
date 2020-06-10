using CosmivengeonMod.NPCs.Draek;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Detours{
	public static class DetourNPC{
		public static void Load(){
			On.Terraria.NPC.SetDefaults += HookSetDefaults;
		}

		public static void HookSetDefaults(On.Terraria.NPC.orig_SetDefaults orig, NPC self, int Type, float scaleOverride){
			orig(self, Type, scaleOverride);

			if(Main.gameMenu)
				return;

			int activePlayers = 0;
			if(Main.netMode != NetmodeID.SinglePlayer)
				for(int i = 0; i < Main.maxPlayers; i++)
					if(Main.player[i].active)
						activePlayers++;

			int p1 = ModContent.NPCType<Draek>();
			int p2 = ModContent.NPCType<DraekP2Head>();
			if(self.type == p1){
				self.lifeMax = Draek.BaseHealth;
				
				self.ScaleHealthBy(Draek.GetHealthAugmentation(), activePlayers);
				
				int hp = GetDraekMaxHealth(activePlayers);
				(self.modNPC as Draek).HealthThreshold = hp - self.lifeMax;
				self.lifeMax = hp;

				self.life = self.lifeMax;
			}else if(self.type == p2){
				self.lifeMax = DraekP2Head.BaseHealth;

				self.ScaleHealthBy(DraekP2Head.GetHealthAugmentation(), activePlayers);
				
				int curMax = self.lifeMax;
				(self.modNPC as DraekP2Head).ActualMaxHealth = curMax;
				self.lifeMax = GetDraekMaxHealth(activePlayers);
				
				self.life = curMax;
			}
		}

		public static int GetDraekMaxHealth(int playerCount)
			=> (int)(Draek.BaseHealth * (1f + Draek.GetHealthAugmentation()) * (playerCount + 1)
				+ DraekP2Head.BaseHealth * (1f + DraekP2Head.GetHealthAugmentation()) * (playerCount + 1));
	}
}
