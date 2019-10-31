using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons{
	public class StoneskipperProjectile : ModProjectile{
		public override void SetDefaults(){
			projectile.width = 8;
			projectile.height = 8;
			projectile.aiStyle = 1;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.ranged = true;
			projectile.penetrate = 1;
			projectile.timeLeft = 5 * 60;
			projectile.alpha = 255;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;
			aiType = ProjectileID.Bullet;
		}

		public override void AI(){
			if(projectile.velocity.Length() < Items.Draek.Stoneskipper.ShootSpeed)
				projectile.velocity = Vector2.Normalize(projectile.velocity) * Items.Draek.Stoneskipper.ShootSpeed;

			//Add a green light from the projectile
			Lighting.AddLight(projectile.Center, 0f, 1f, 0f);

			//Set the rotation to the projectile's velocity vector + PI
			projectile.rotation = projectile.velocity.ToRotation();
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			target.AddBuff(BuffID.Poisoned, 5 * 60);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			//Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
			Vector2 drawPos = projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
			Color color = projectile.GetAlpha(lightColor);

			spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);

			return true;
		}

		public override void Kill(int timeLeft){
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Item10, projectile.position);
		}
	}
}
