using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Frostbite{
	public class FrostbiteIcicle : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Icicle");
			Main.projFrames[projectile.type] = 2;
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = true;
			projectile.timeLeft = 5 * 60;
			projectile.alpha = 255;
		}

		private bool spawned = false;

		public override void AI(){
			if(!spawned){
				spawned = true;
				projectile.frame = Main.rand.Next(2);
			}

			if(projectile.alpha > 0)
				projectile.alpha -= 10;
			if(projectile.alpha < 0)
				projectile.alpha = 0;

			//Cyan light
			Lighting.AddLight(projectile.Center, 132f / 255f, 1f, 1f);

			projectile.velocity.Y += 9f / 60f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
		}
	}
}
