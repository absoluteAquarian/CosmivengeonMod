using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSword : ModProjectile{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Forsaken Oronoblade");
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

			drawOffsetX = -44;
			drawOriginOffsetY = -52;
		}

		private bool hasSpawned = false;
		private bool spinFinished = false;

		private const int Spin_Timer_Max = 90;
		private int AI_Spin_Timer = -1;
		private const int FlyAfterSpinTimerMax = 45;
		private int AI_FlyAfterSpin_Timer = FlyAfterSpinTimerMax;

		public override void AI(){
			Player target = Main.player[(int)projectile.ai[0]];
			
			//If the projectile just spawned, spawn the "extra" hitboxes
			//Also, check if we're in normal mode and set the velocity
			if(!hasSpawned){
				hasSpawned = true;

				if(!Main.expertMode && !CosmivengeonWorld.desoMode)
					projectile.velocity = Vector2.Normalize(target.Center - projectile.Center) * 20f;

				Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, -1);
				Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, 1);
			}

			//If we're in Desolation Mode, make the projectile spin for 1.5 seconds
			if(projectile.ai[1] > 0){
				if(AI_Spin_Timer >= 0){
					float ratio = 1f - (AI_Spin_Timer / Spin_Timer_Max);
					float spin = MathHelper.ToRadians(0.25f * 360f / 60f);
					float spinAdd = MathHelper.ToRadians(6f * 360f / 60f);

					AI_Spin_Timer--;
				
					projectile.rotation += spin + spinAdd * ratio;

					projectile.velocity *= 0.86f;

					projectile.timeLeft = 2 * 60;
					return;
				}

				//Finally, set the intended velocity
				if(!spinFinished && AI_Spin_Timer < 0){
					spinFinished = true;
					projectile.ai[1]--;
					AI_FlyAfterSpin_Timer = FlyAfterSpinTimerMax;

					projectile.velocity = Vector2.Normalize(target.Center - projectile.Center) * 20f;
				}

				if(spinFinished){
					if(AI_FlyAfterSpin_Timer >= 0)
						AI_FlyAfterSpin_Timer--;
					else{
						spinFinished = false;
						AI_Spin_Timer = Spin_Timer_Max;
					}
				}
			}

			//In expert mode, make the projectile home in to the target player slightly
			if(Main.expertMode && !CosmivengeonWorld.desoMode){
				Vector2 normalizedTarget = Vector2.Normalize(target.Center - projectile.Center);
				projectile.velocity += normalizedTarget * 0.42f;
			}

			Vector2 normalizedVelocity = Vector2.Normalize(projectile.velocity);

			//Force the sword to travel at 20 px/frame
			if(projectile.velocity.Length() < 20f)
				projectile.velocity = normalizedVelocity * 20f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			if(Main.rand.Next(6) == 0)
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 236);
			if(Main.rand.Next(8) == 0)
				Dust.NewDust(projectile.position, projectile.width, projectile.height, 74);
		}
	}
}