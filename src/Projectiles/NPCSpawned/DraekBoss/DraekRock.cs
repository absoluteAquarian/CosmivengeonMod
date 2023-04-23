using CosmivengeonMod.Systems;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss {
	public class DraekRock : ModProjectile {
		public override void SetStaticDefaults() {
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults() {
			Projectile.height = WorldEvents.desoMode ? 20 : 15;
			Projectile.width = WorldEvents.desoMode ? 20 : 15;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = true;
			Projectile.ignoreWater = true;
			Projectile.penetrate = -1;
			Projectile.timeLeft = 300;
			Projectile.aiStyle = 0;
			Projectile.alpha = 0;
			Projectile.scale = 2f;

			DrawOriginOffsetX = -Projectile.width / 2f;
			DrawOriginOffsetY = (int)(Projectile.height / 2f);
		}

		public override void AI() {
			if (Projectile.velocity.Y < Projectile.ai[0])
				Projectile.velocity.Y += Projectile.ai[1];

			//Change the animation frame every 6 frames
			if (++Projectile.frameCounter >= 6) {
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
			}
		}
	}
}
