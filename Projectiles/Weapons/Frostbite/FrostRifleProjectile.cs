using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class FrostRifleProjectile : ModProjectile{
		public override string Texture => "CosmivengeonMod/Projectiles/Weapons/Frostbite/CrystaliceShardFragmentProjectile";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Icicle");
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults(){
			projectile.width = 8;
			projectile.height = 8;
			projectile.scale = 10f / 8f;
			projectile.ranged = true;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.penetrate = 3;
			projectile.timeLeft = 180;
			projectile.extraUpdates = 1;
			projectile.alpha = 255;

			projectile.frame = Main.rand.Next(3);
		}

		public override void AI(){
			projectile.TryDecrementAlpha(15);

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
		}

		public override void Kill(int timeLeft){
			Main.PlaySound(SoundID.Item27.WithVolume(0.45f), projectile.Center);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			target.AddBuff(BuffID.Frostburn, 3 * 60);
		}
	}
}
