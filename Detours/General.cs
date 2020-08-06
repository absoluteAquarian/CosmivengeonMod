using CosmivengeonMod.NPCs.Desomode;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		private static Player Target(this NPC npc) => Main.player[npc.target];

		private static DetourNPCHelper Helper(this NPC npc) => npc.GetGlobalNPC<DetourNPCHelper>();
	}
}
