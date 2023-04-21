using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons.Draek{
	public class EarthBoltProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults(){
			Projectile.width = 16;
			Projectile.height = 16;
			Projectile.aiStyle = 0;
			Projectile.friendly = true;
			Projectile.hostile = false;
			Projectile.DamageType = DamageClass.Magic;
			Projectile.penetrate = 4;
			Projectile.timeLeft = 600;
			Projectile.ignoreWater = true;
			Projectile.tileCollide = true;
		}

		public override void AI(){
			//Spawn some dust
			int dustIndex = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 74);

			Main.dust[dustIndex].velocity = Vector2.Zero;

			//Rotate the sprite
			//5 rotations per second
			Projectile.rotation += MathHelper.ToRadians(5f * 360f / 60f);

			//Add a green light from the projectile
			Lighting.AddLight(Projectile.Center, 0f, 1f, 0f);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			target.AddBuff(BuffID.Poisoned, 4 * 60);
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			//If collide with tile, reduce the penetrate.
			Projectile.penetrate--;

			if(Projectile.penetrate <= 0){
				Projectile.Kill();
			}else{
				Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
				SoundEngine.PlaySound(SoundID.Item10, Projectile.position);

				if(Projectile.velocity.X != oldVelocity.X)
					Projectile.velocity.X = -oldVelocity.X;
				if(Projectile.velocity.Y != oldVelocity.Y)
					Projectile.velocity.Y = -oldVelocity.Y;
			}

			return false;
		}

		public override bool PreDraw(ref Color lightColor) {
			//Redraw the projectile with the color not influenced by light
			Vector2 drawOrigin = new Vector2(TextureAssets.Projectile[Projectile.type].Value.Width * 0.5f, Projectile.height * 0.5f);
			float scale = Projectile.scale;

			for (int k = 0; k < Projectile.oldPos.Length; k++){
				Vector2 drawPos = Projectile.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, Projectile.gfxOffY);
				Color color = Projectile.GetAlpha(lightColor) * ((float)(Projectile.oldPos.Length - k) / (float)Projectile.oldPos.Length);
				
				spriteBatch.Draw(TextureAssets.Projectile[Projectile.type].Value, drawPos, null, color, Projectile.rotation, drawOrigin, scale, SpriteEffects.None, 0f);

				scale *= 0.9f;
			}

			return true;
		}

		public override void Kill(int timeLeft){
			// This code and the similar code above in OnTileCollide spawn dust from the tiles collided with. SoundID.Item10 is the bounce sound you hear.
			Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
			SoundEngine.PlaySound(SoundID.Item10, Projectile.position);
		}
	}
}
