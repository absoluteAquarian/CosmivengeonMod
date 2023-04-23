using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class CrystaliceShardProjectile : ModProjectile {
		public const float MAX_VELOCITY = 10f;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Crystalice Shard");
		}

		public override void SetDefaults() {
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.width = 14;
			Projectile.height = 14;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.timeLeft = 45;
		}

		public override void AI() {
			Projectile.TryDecrementAlpha(15);

			Projectile.velocity.Y += 7f / 60f;

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

			Projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);
		}

		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item27, Projectile.Center);
		}
	}
}
