using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Frostbite{
	public class CrystaliceShardFragmentProjectile : ModProjectile{
		public const float MAX_VELOCITY = 12f;

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Ice Fragment");
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults(){
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.alpha = 255;
			projectile.width = 8;
			projectile.height = 8;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
		}

		private bool spawned = false;
		private bool gravity = false;

		public override void AI(){
			if(!spawned){
				spawned = true;
				gravity = projectile.ai[0] == 1;

				if(!gravity)
					projectile.timeLeft = 120;
				
				switch(projectile.ai[1]){
					case 0:
						projectile.thrown = true;
						break;
					case 1:
						projectile.melee = true;
						break;
					case 2:
						projectile.magic = true;
						break;
				}

				projectile.frame = Main.rand.Next(3);
			}

			if(projectile.alpha > 0)
				projectile.alpha -= 15;
			if(projectile.alpha < 0)
				projectile.alpha = 0;

			if(gravity)
				projectile.velocity.Y += 9f / 60f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

			projectile.velocity.Y.Clamp(-MAX_VELOCITY, MAX_VELOCITY);
		}

		public override void Kill(int timeLeft){
			Main.PlaySound(SoundID.Item27.WithVolume(0.55f), projectile.Center);
		}
	}
}
