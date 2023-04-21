using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class SkeletronBone : ModProjectile{
		public override string Texture => $"Terraria/Projectile_{ProjectileID.SkeletonBone}";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Bone");
			Main.projFrames[Projectile.type] = Main.projFrames[ProjectileID.SkeletonBone];
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		public override void SetDefaults(){
			Projectile.CloneDefaults(ProjectileID.SkeletonBone);
			Projectile.aiStyle = -1;
			Projectile.scale *= 1.75f;
			Projectile.timeLeft = 600;
			Projectile.tileCollide = false;
		}

		public override Color? GetAlpha(Color lightColor) => Color.White;

		public override void AI(){
			float persec = 3f;
			Projectile.rotation += (Projectile.velocity.X >= 0).ToDirectionInt() * MathHelper.ToRadians(persec * 6f);
		}

		public override bool PreDraw(ref Color lightColor){
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int cacheMax = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1;

			for(int i = cacheMax; i >= 0; i--){
				float opacity = i * 0.2f;
				
				spriteBatch.Draw(texture, Projectile.oldPos[cacheMax - i] + Projectile.Size / 2f - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
			}

			spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
			return false;
		}
	}
}
