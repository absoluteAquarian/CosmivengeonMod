using CosmivengeonMod.Buffs.Harmful;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class QueenBeeHoneyShot : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Honey Glob");
			ProjectileID.Sets.TrailCacheLength[projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[projectile.type] = 0;
		}

		private int draw;

		public override void SetDefaults(){
			projectile.height = 20;
			projectile.width = 20;
			projectile.scale = 64f / projectile.height;
			projectile.penetrate = 1;
			projectile.timeLeft = 600;
			projectile.hostile = true;

			draw = Main.rand.Next(4);
			projectile.netUpdate = true;
		}

		public override void SendExtraAI(BinaryWriter writer){
			writer.Write(draw);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			draw = reader.ReadInt32();
		}

		public override void AI(){
			if(projectile.ai[0] == 0f){
				projectile.ai[0] = 1f;

				Main.PlaySound(SoundID.Item95.WithPitchVariance(0.15f).WithVolume(1.5f), projectile.Center);
			}

			if(Main.rand.NextFloat() < 0.6f){
				Dust dust = Dust.NewDustDirect(projectile.position + new Vector2(6), projectile.width - 3, projectile.height - 3, DustID.t_Honey);
				dust.noGravity = true;
				dust.velocity = Vector2.Zero;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			Texture2D texture = Main.projectileTexture[projectile.type];
			int cacheMax = ProjectileID.Sets.TrailCacheLength[projectile.type] - 1;

			for(int i = cacheMax; i >= 0; i--){
				float opacity = i * 0.2f;
				
				spriteBatch.Draw(texture, projectile.oldPos[cacheMax - i] + projectile.Size / 2f - Main.screenPosition, null, lightColor * opacity, projectile.rotation, texture.Size() / 2f, projectile.scale, (SpriteEffects)draw, 0);
			}

			spriteBatch.Draw(texture, projectile.Center - Main.screenPosition, null, lightColor, projectile.rotation, texture.Size() / 2f, projectile.scale, (SpriteEffects)draw, 0);
			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			target.AddBuff(ModContent.BuffType<Sticky>(), (int)(3.5f * 60));
		}
	}
}
