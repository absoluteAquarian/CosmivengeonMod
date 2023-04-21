using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss {
	public class FrostbiteIcicle : ModProjectile {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Icicle");
			Main.projFrames[Projectile.type] = 2;
		}

		public override void SetDefaults() {
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.friendly = false;
			Projectile.hostile = true;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 5 * 60;
			Projectile.alpha = 255;
		}

		private bool spawned = false;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				Projectile.frame = Main.rand.Next(2);
			}

			if (Projectile.alpha > 0)
				Projectile.alpha -= 10;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;

			//Cyan light
			Lighting.AddLight(Projectile.Center, 132f / 255f, 1f, 1f);

			Projectile.velocity.Y += 9f / 60f;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
		}
	}
}
