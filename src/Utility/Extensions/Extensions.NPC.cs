using Terraria;

namespace CosmivengeonMod.Utility {
	public static partial class Extensions {
		/// <summary>
		/// Checks whether the mouse is inside this NPC's collision box or within <paramref name="pixels"/> distance of the collision box.
		/// </summary>
		/// <param name="npc">The npc whose collision box is being used.</param>
		/// <param name="pixels">The distance in pixels that the mouse can be from <paramref name="npc"/>'s collision box in both coordinate axes.</param>
		/// <returns></returns>
		public static bool MouseWithinRange(this NPC npc, float pixels)
			=> Main.MouseWorld.X > npc.position.X - pixels && Main.MouseWorld.X < npc.position.X + npc.width + pixels
				&& Main.MouseWorld.Y > npc.position.Y - pixels && Main.MouseWorld.Y < npc.position.Y + npc.height + pixels;

		/// <summary>
		/// Scales this <paramref name="npc"/>'s health by the scale <paramref name="factor"/> provided.
		/// </summary>
		/// <param name="npc">The NPC instance.</param>
		/// <param name="factor">The scale factor that this <paramref name="npc"/>'s health is scaled by.</param>
		public static void ScaleHealthBy(this NPC npc, float factor) {
			float bossScale = MiscUtils.CalculateBossHealthScale(out _);

			npc.lifeMax = (int)(npc.lifeMax * Main.GameModeInfo.EnemyMaxLifeMultiplier);
			npc.lifeMax = (int)(npc.lifeMax * factor * bossScale);
		}

		public static void SetImmune(this NPC npc, int type) => npc.buffImmune[type] = true;
	}
}
