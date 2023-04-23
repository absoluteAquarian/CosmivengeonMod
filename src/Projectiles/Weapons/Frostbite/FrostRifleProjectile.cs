using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite {
	public class FrostRifleProjectile : ModProjectile {
		public override string Texture => "CosmivengeonMod/Projectiles/Weapons/Frostbite/CrystaliceShardFragmentProjectile";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Icicle");
			Main.projFrames[Projectile.type] = 3;
		}

		public override void SetDefaults() {
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.scale = 10f / 8f;
			Projectile.DamageType = DamageClass.Ranged;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.penetrate = 3;
			Projectile.timeLeft = 180;
			Projectile.extraUpdates = 1;
			Projectile.alpha = 255;

			Projectile.frame = Main.rand.Next(3);
		}

		public override void AI() {
			Projectile.TryDecrementAlpha(15);

			Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override void Kill(int timeLeft) {
			SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.45f }, Projectile.Center);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit) {
			target.AddBuff(BuffID.Frostburn, 3 * 60);
		}
	}
}
