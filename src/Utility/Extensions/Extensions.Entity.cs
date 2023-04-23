using Microsoft.Xna.Framework;
using Terraria;

namespace CosmivengeonMod.Utility {
	public static partial class Extensions {
		public static bool WithinDistance(this Entity ent, Vector2 pos, float distance)
			=> ent.DistanceSQ(pos) < distance * distance;

		public static void SmoothStep(this Entity entity, ref Vector2 pos, int width = -1, int height = -1) {
			//Smooth steps down/up slopes
			if (entity is NPC npc) {
				if (npc.velocity.Y == 0f)
					Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
				if (npc.velocity.Y >= 0)
					Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY, specialChecksMode: 1);
			} else if (entity is Projectile proj) {
				width = width == -1 ? proj.width : width;
				height = height == -1 ? proj.height : height;

				if (proj.velocity.Y == 0f)
					Collision.StepDown(ref pos, ref proj.velocity, width, height, ref proj.stepSpeed, ref proj.gfxOffY);
				if (proj.velocity.Y >= 0)
					Collision.StepUp(ref pos, ref proj.velocity, width, height, ref proj.stepSpeed, ref proj.gfxOffY, specialChecksMode: 1);
			}
		}
	}
}
