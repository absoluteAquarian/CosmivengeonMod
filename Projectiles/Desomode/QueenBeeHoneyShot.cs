using CosmivengeonMod.Buffs.Harmful;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Desomode{
	public class QueenBeeHoneyShot : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Honey Glob");
			ProjectileID.Sets.TrailCacheLength[Projectile.type] = 5;
			ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
		}

		private int draw;

		public override void SetDefaults(){
			Projectile.height = 20;
			Projectile.width = 20;
			Projectile.scale = 64f / Projectile.height;
			Projectile.penetrate = 1;
			Projectile.timeLeft = 600;
			Projectile.hostile = true;

			draw = Main.rand.Next(4);
			Projectile.netUpdate = true;
		}

		public override void SendExtraAI(BinaryWriter writer){
			writer.Write(draw);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			draw = reader.ReadInt32();
		}

		public override void AI(){
			if(Projectile.ai[0] == 0f){
				Projectile.ai[0] = 1f;

				SoundEngine.PlaySound(SoundID.Item95.WithPitchVariance(0.15f).WithVolume(1.5f), Projectile.Center);
			}

			if(Main.rand.NextFloat() < 0.6f){
				Dust dust = Dust.NewDustDirect(Projectile.position + new Vector2(6), Projectile.width - 3, Projectile.height - 3, DustID.t_Honey);
				dust.noGravity = true;
				dust.velocity = Vector2.Zero;
			}
		}

		public override bool PreDraw(ref Color lightColor){
			Texture2D texture = TextureAssets.Projectile[Projectile.type].Value;
			int cacheMax = ProjectileID.Sets.TrailCacheLength[Projectile.type] - 1;

			for(int i = cacheMax; i >= 0; i--){
				float opacity = i * 0.2f;
				
				spriteBatch.Draw(texture, Projectile.oldPos[cacheMax - i] + Projectile.Size / 2f - Main.screenPosition, null, lightColor * opacity, Projectile.rotation, texture.Size() / 2f, Projectile.scale, (SpriteEffects)draw, 0);
			}

			spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition, null, lightColor, Projectile.rotation, texture.Size() / 2f, Projectile.scale, (SpriteEffects)draw, 0);
			return false;
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			target.AddBuff(ModContent.BuffType<Sticky>(), (int)(3.5f * 60));
		}
	}
}
