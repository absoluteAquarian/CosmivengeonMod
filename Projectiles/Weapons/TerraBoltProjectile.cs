using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Weapons{
	public class TerraBoltProjectile : ModProjectile{
		public override void SetStaticDefaults(){
			Main.projFrames[projectile.type] = 3;
		}

		public override void SetDefaults(){
			projectile.width = 16;
			projectile.height = 16;
			projectile.friendly = true;
			projectile.hostile = false;
			projectile.penetrate = 5;
			projectile.timeLeft = 5 * 60;
			projectile.alpha = 0;
			projectile.ignoreWater = true;
			projectile.tileCollide = true;

			projectile.usesLocalNPCImmunity = true;
			projectile.localNPCHitCooldown = 3;
		}

		private bool hasSpawned = false;

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				projectile.frame = Main.rand.Next(3);
				projectile.rotation = Main.rand.NextFloat(MathHelper.TwoPi);
			}
			
			projectile.rotation += MathHelper.ToRadians((5f + Main.rand.NextFloat(-2f, 2f)) * 360f / 60f);
		}

		public override void OnHitNPC(NPC target, int damage, float knockback, bool crit){
			target.AddBuff(ModContent.BuffType<Buffs.PrimordialWrath>(), 7 * 60);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//Redraw the projectile with the color not influenced by light
			Texture2D texture = Main.projectileTexture[projectile.type];

			Vector2 drawOrigin = new Vector2(texture.Width * 0.5f, projectile.height * 0.5f);
			Vector2 drawPos = projectile.position - Main.screenPosition + drawOrigin + new Vector2(0f, projectile.gfxOffY);
			Color color = projectile.GetAlpha(lightColor);
			Rectangle drawFrame = new Rectangle(0, texture.Height / Main.projFrames[projectile.type] * projectile.frame, texture.Width, texture.Height / Main.projFrames[projectile.type]);

			spriteBatch.Draw(texture, drawPos, drawFrame, color, projectile.rotation, drawOrigin, projectile.scale, SpriteEffects.None, 0f);

			return true;
		}

		public override void Kill(int timeLeft){
			Collision.HitTiles(projectile.position + projectile.velocity, projectile.velocity, projectile.width, projectile.height);
			Main.PlaySound(SoundID.Item10, projectile.position);
		}
	}
}
