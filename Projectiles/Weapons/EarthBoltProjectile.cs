using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons{
	public class EarthBoltProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.aiStyle = 0;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.magic = true;
			projectile.penetrate = 4;
			projectile.timeLeft = 600;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;
		}

		public override void AI(){
			//Spawn some dust
			int dustIndex = Dust.NewDust(projectile.position, projectile.width, projectile.height, 74);

			Main.dust[dustIndex].velocity = Vector2.Zero;

			//Rotate the sprite
			//5 rotations per second
			projectile.rotation += MathHelper.ToRadians(5f * 360f / 60f);

			//Add a green light from the projectile
			Lighting.AddLight(projectile.Center, 0f, 1f, 0f);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			if(!target.buffImmune[BuffID.Poisoned])
				target.AddBuff(BuffID.Poisoned, 4 * 60);
		}

		public override bool OnTileCollide(Vector2 oldVelocity) {
			//If collide with tile, reduce the penetrate.
			projectile.penetrate--;

			if(projectile.penetrate <= 0){
				projectile.Kill();
			}else{
				Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
				Main.PlaySound(SoundID.Item10, projectile.position);

				if(projectile.velocity.X != oldVelocity.X)
					projectile.velocity.X = -oldVelocity.X;
				if(projectile.velocity.Y != oldVelocity.Y)
					projectile.velocity.Y = -oldVelocity.Y;
			}

			return false;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor) {
			//Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(Main.projectileTexture[projectile.type].Width * 0.5f, projectile.height * 0.5f);
			float scale = projectile.scale;

			for (int k = 0; k < projectile.oldPos.Length; k++){
				Vector2 drawPos = projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
				Color color = projectile.GetAlpha(lightColor) * ((float)(projectile.oldPos.Length - k) / (float)projectile.oldPos.Length);
				
				spriteBatch.Draw(Main.projectileTexture[projectile.type], drawPos, null, color, projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0f);

				scale *= 0.9f;
			}

			return true;
		}

		public override void Kill(int timeLeft){
			// This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Item10, projectile.position);
		}
	}
}
