using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSword : ModProjectile{
		public int DesoDebuffTime => AI_Spin_Timer >= 0 ? 3 * 60 : 45;

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
		private bool spinStarted = false;
		private bool spinFinished = false;

		private const int Spin_Timer_Max = 90;
		private int AI_Spin_Timer = -1;
		private const int FlyAfterSpinTimerMax = 45;
		private int AI_FlyAfterSpin_Timer = FlyAfterSpinTimerMax;

		private int baseDamage;

		private float prevHomingDistance = -1f;

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

				baseDamage = projectile.damage;
			}

			//If we're in Desolation Mode, make the projectile spin for 1.5 seconds
			if(projectile.ai[1] > 0){
				if(AI_Spin_Timer >= 0){
					if(!spinStarted){
						spinStarted = true;
						projectile.damage = (int)(baseDamage * 0.2f);
					}

					float ratio = 1f - (float)AI_Spin_Timer / Spin_Timer_Max;
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
					spinStarted = false;
					projectile.damage = baseDamage;

					spinFinished = true;
					projectile.ai[1]--;
					AI_FlyAfterSpin_Timer = FlyAfterSpinTimerMax;

					projectile.velocity = projectile.DirectionTo(target.Center) * 20f;
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
				//Better homing effect:  use the opposite direction if we're moving away from the target
				Vector2 normalizedTarget = projectile.DirectionTo(target.Center);

				if(prevHomingDistance > 0 && prevHomingDistance < projectile.Distance(target.Center)){
					if(Math.Abs(normalizedTarget.X) > Math.Abs(normalizedTarget.Y))
						normalizedTarget.X *= -1;
					else if(Math.Abs(normalizedTarget.Y) > Math.Abs(normalizedTarget.X))
						normalizedTarget.Y *= -1;
				}

				projectile.velocity += normalizedTarget * 0.42f;

				prevHomingDistance = projectile.Distance(target.Center);
			}

			Vector2 normalizedVelocity = Vector2.Normalize(projectile.velocity);

			//Force the sword to travel at 20 px/frame
			if(projectile.velocity.Length() != 20f)
				projectile.velocity = normalizedVelocity * 20f;

			projectile.rotation = projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);

			//Don't spawn dust if the sword is spinning
			if(CosmivengeonWorld.desoMode && AI_Spin_Timer > 0)
				return;

			if(Main.rand.NextFloat() < 0.1667f){
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 236);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}
			if(Main.rand.NextFloat() < 0.125f){
				Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, 74);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}
		}

		public override void OnHitPlayer(Player target, int damage, bool crit){
			//Only apply the Primordial Wrath debuff if the world is in Desolation mode
			if(CosmivengeonWorld.desoMode){
				//Apply a shorter debuff time while not spinning
				target.AddBuff(ModContent.BuffType<Buffs.PrimordialWrath>(), DesoDebuffTime);
			}
		}
	}
}