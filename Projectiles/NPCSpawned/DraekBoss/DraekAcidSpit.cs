using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss{
	public class DraekAcidSpit : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Acid Spit");
		}

		public override void SetDefaults(){
			projectile.alpha = 75;
			projectile.width = 8;
			projectile.height = 8;
			projectile.penetrate = 1;
			projectile.tileCollide = true;
			projectile.friendly = false;
			projectile.hostile = true;
		}

		private bool hasSpawned = false;

		public override void AI(){
			if(!hasSpawned){
				hasSpawned = true;
				projectile.velocity = Vector2.Normalize(new Vector2(projectile.ai[0], projectile.ai[1]) - projectile.Center) * 10f;

				Main.PlaySound(SoundID.NPCDeath19.WithVolume(0.6f), projectile.Center);
			}

			projectile.velocity.Y += 8f / 60f;

			projectile.velocity.Y.Clamp(-10, 10);

			if(Main.rand.NextFloat() < 0.75f){
				Dust dust = Dust.NewDustPerfect(projectile.Center, 74);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}

			projectile.rotation += MathHelper.ToRadians(4f * 6f) * Math.Sign(projectile.velocity.X);
		}
	}
}
