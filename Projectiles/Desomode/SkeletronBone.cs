using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class SkeletronBone : ModProjectile{
		public override string Texture => $"Terraria/Projectile_{ProjectileID.SkeletonBone}";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Bone");
			Main.projFrames[projectile.type] = Main.projFrames[ProjectileID.SkeletonBone];
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		public override void SetDefaults(){
			projectile.CloneDefaults(ProjectileID.SkeletonBone);
			projectile.aiStyle = -1;
			projectile.scale *= 1.75f;
			projectile.timeLeft = 600;
			projectile.tileCollide = false;
		}

		public override Color? GetAlpha(Color lightColor) => Color.White;

		public override void AI(){
			float persec = 3f;
			projectile.rotation += (projectile.velocity.X >= 0).ToDirectionInt() * MathHelper.ToRadians(persec * 6f);
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = Main.projectileTexture[projectile.type];
			int cacheMax = ProjectileID.Sets.TrailCacheLength[projectile.type] - 1;

			for(int i = cacheMax; i >= 0; i--){
				float opacity = i * 0.2f;
				
				spriteBatch.Draw(texture, projectile.oldPos[cacheMax - i] + projectile.Size / 2f - Main.screenPosition, null, lightColor * opacity, projectile.rotation, texture.Size() / 2f, projectile.scale, SpriteEffects.None, 0);
			}

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, texture.Size() / 2f, projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
