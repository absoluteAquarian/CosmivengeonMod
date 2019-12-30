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

		private bool spawned = false;

		public override void AI(){
			if(!spawned){
				spawned = true;

				if(projectile.ai[0] != 0f)
					projectile.timeLeft = 6 * 60;
			}

			projectile.TryDecrementAlpha(15);

			if(projectile.ai[1] == 0f)
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

			//Don't spawn extra projectiles if this wasn't from the throwing weapon
			if(projectile.ai[0] != 0f)
				return;

			//Spawn 3 smaller shards in a 30 degree cone
			// in the direction this projectile was moving
			for(int i = 0; i < 3; i++){
				Projectile.NewProjectile(
					projectile.Center,
					projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)),
					ModContent.ProjectileType<CrystaliceShardFragmentProjectile>(),
					(int)(projectile.damage * 0.8f),
					projectile.knockBack * 0.75f,
					Main.myPlayer,
					1f,
					0f
				);
			}
		}
	}
}
