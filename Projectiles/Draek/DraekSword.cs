using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSword : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek's Sword");
		}
		public override void SetDefaults(){
			projectile.height = 30;
			projectile.width = 30;
			projectile.friendly = false;
			projectile.hostile = true;
			projectile.tileCollide = false;
			projectile.ignoreWater = true;
			projectile.penetrate = -1;
			projectile.timeLeft = 2 * 60;
			projectile.aiStyle = 0;
			projectile.alpha = 0;
		}

		private bool hasSpawned = false;

		public override void AI(){
			Player target = Main.player[(int)projectile.ai[0]];
			
			//If the projectile just spawned, set its velocity vector
			if(!hasSpawned){
				hasSpawned = true;

				projectile.velocity = Vector2.Normalize(target.Center - projectile.Center) * 20f;
				Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, -1);
				Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, 1);
			}

			//In expert mode, make the projectile home in to the target player slightly
			if(Main.expertMode && !CosmivengeonWorld.desoMode){
				Vector2 normalizedTarget = Vector2.Normalize(target.Center - projectile.Center);
				projectile.velocity += normalizedTarget * 0.42f;
			}

			Vector2 normalizedVelocity = Vector2.Normalize(projectile.velocity);

			//Force the sword to travel at 20 blocks/sec
			if(projectile.velocity.Length() < 20f)
				projectile.velocity = normalizedVelocity * 20f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			drawOffsetX = -44;
			drawOriginOffsetY = -52;

			if(Main.rand.Next(6) == 0)
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 236);
			if(Main.rand.Next(8) == 0)
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 74);
		}
	}
}