using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class CrystaliceShardProjectile : ModProjectile{
		public const float MAX_VELOCITY = 10f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Crystalice Shard");
		}

		public override void SetDefaults(){
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.width = 14;
			projectile.height = 14;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.timeLeft = 45;
		}

		public override void AI(){
			projectile.TryDecrementAlpha(15);

			projectile.velocity.Y += 7f / 60f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

			projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			projectile.Kill();
			return true;
		}

		public override void Kill(int timeLeft){
			Main.PlaySound(SoundID.Item27, projectile.Center);
		}
	}
}
