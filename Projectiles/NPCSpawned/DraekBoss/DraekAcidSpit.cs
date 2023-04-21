using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss{
	public class DraekAcidSpit : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Acid Spit");
		}

		public override void SetDefaults(){
			Projectile.alpha = 75;
			Projectile.width = 8;
			Projectile.height = 8;
			Projectile.penetrate = 1;
			Projectile.tileCollide = true;
			Projectile.friendly = false;
			Projectile.hostile = true;
		}

		private bool hasSpawned = false;

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				Projectile.velocity = Vector2.Normalize(new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center) * 10f;

				SoundEngine.PlaySound(SoundID.NPCDeath19.WithVolume(0.6f), Projectile.Center);
			}

			Projectile.velocity.Y += 8f / 60f;

			Projectile.velocity.Y.Clamp(-10, 10);

			if(Main.rand.NextFloat() < 0.75f){
				Dust dust = Dust.NewDustPerfect(Projectile.Center, 74);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}

			Projectile.rotation += MathHelper.ToRadians(4f * 6f) * Math.Sign(Projectile.velocity.X);
		}
	}
}
