using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class CrystaliceShardFragmentProjectile : ModProjectile {
		public const float MAX_VELOCITY = 12f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Ice Fragment");
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.alpha = 255;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
		}

		private bool spawned = false;
		private bool gravity = false;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				gravity = Projectile.ai[0] == 1;

				if (!gravity)
					Projectile.timeLeft = 120;

				switch (Projectile.ai[1]) {
					case 0:
						Projectile.DamageType = DamageClass.Throwing;
						break;
					case 1:
						Projectile.DamageType = DamageClass.Melee;
						break;
					case 2:
						Projectile.DamageType = DamageClass.Magic;
						break;
				}

				Projectile.frame = Main.rand.Next(3);
			}

			if (Projectile.alpha > 0)
				Projectile.alpha -= 15;
			if (Projectile.alpha < 0)
				Projectile.alpha = 0;

			if (gravity)
				Projectile.velocity.Y += 9f / 60f;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);
		}

		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.55f }, Projectile.Center);
		}
	}
}
