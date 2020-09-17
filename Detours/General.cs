using CosmivengeonMod.NPCs.Desomode;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		public static Player Target(this NPC npc) => Main.player[npc.target];

		public static DetourNPCHelper Helper(this NPC npc) => npc.GetGlobalNPC<DetourNPCHelper>();

		public static DesomodeNPC Desomode(this NPC npc) => npc.GetGlobalNPC<DesomodeNPC>();

		/// <summary>
		/// Shorthand for <c>Main.npc[(int)npc.ai[0]]</c>
		/// </summary>
		public static NPC WormFollower(this NPC npc) => Main.npc[(int)npc.ai[0]];

		/// <summary>
		/// Shorthand for <c>Main.npc[(int)npc.ai[1]]</c>
		/// </summary>
		public static NPC WormFollowing(this NPC npc) => Main.npc[(int)npc.ai[1]];
	}
}
