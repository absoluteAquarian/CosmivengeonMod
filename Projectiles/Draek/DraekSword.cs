using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CosmivengeonMod.Projectiles.Draek{
	public class DraekSword : ModProjectile{
		public int DesoDebuffTime = 2 * 60;

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

		private int npcOwner = -1;

		public ref float Phase => ref projectile.ai[1];

		private float expert_revolutionAngle;

		private Vector2 expert_revolutionOffset;

		private int timer;

		private int extra, extra2;

		private Player target;

		public override void AI(){
			//If the projectile just spawned, spawn the "extra" hitboxes
			//Also set the velocity
			if(!hasSpawned){
				hasSpawned = true;

				target = Main.player[(int)projectile.ai[0]];
				npcOwner = (int)projectile.ai[1];

				//These two will be used later for swinging at the target player in Desolation Mode
				projectile.ai[0] = 0f;
				projectile.ai[1] = 0f;

				//All AI types have the same initial movement
				projectile.velocity = Vector2.Normalize(target.Center - projectile.Center) * 20f;

				extra = Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, -1);
				extra2 = Projectile.NewProjectile(projectile.position, Vector2.Zero, ModContent.ProjectileType<DraekSwordExtra>(), projectile.damage, projectile.knockBack, Main.myPlayer, projectile.whoAmI, 1);

				if(extra == Main.maxProjectiles || extra2 == Main.maxProjectiles){
					projectile.Kill();
					return;
				}
			}

			NPC parent = Main.npc[npcOwner];

			//Check if the NPC has died or if it's not the boss
			if(!parent.active || parent.type != ModContent.NPCType<NPCs.Draek.Draek>() || (parent.modNPC as NPCs.Draek.Draek).AI_Attack == NPCs.Draek.Draek.Attack_Retrieve_Sword){
				projectile.Kill();
				return;
			}

			//Normal mode AI just flies in a straight line
			if(!Main.expertMode && !CosmivengeonWorld.desoMode){
				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
				return;
			}

			//Both mode AIs start out the same
			if(Phase == 0){
				if(timer == 26){
					Phase = 1;
					timer = -1;
				}

				projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}
			
			if(!CosmivengeonWorld.desoMode){
				//Expert mode: sword flies forward a bit, then it spins around draek for a few seconds.
				//Then it points at the player and yeets itself toward the player
				if(Phase > 0 && Phase < 3)
					expert_revolutionAngle += MathHelper.ToRadians(6f * 1.65f);  //1.65 revolutions per second

				expert_revolutionOffset = Vector2.UnitX.RotatedBy(expert_revolutionAngle) * (parent.height + 5 * 16);

				Vector2 dirFromOwner = projectile.DirectionFrom(parent.Center);

				if(Phase == 1){
					//Trying to get back in range of the boss
					projectile.velocity += -dirFromOwner * 13f / 60f;
					
					if(Math.Sign(-dirFromOwner.X) != Math.Sign(projectile.velocity.X))
						projectile.velocity.X *= 1f - 12f / 60f;
					if(Math.Sign(-dirFromOwner.Y) != Math.Sign(projectile.velocity.Y))
						projectile.velocity.Y *= 1f - 12f / 60f;

					SpawnMoveDust(0.19f);

					if(projectile.WithinDistance(parent.Center, parent.height + 5 * 16)){
						Phase = 2;
						timer = -1;

						expert_revolutionAngle = projectile.DirectionFrom(parent.Center).ToRotation() + MathHelper.ToRadians(Main.rand.NextFloat(-30, 30));
					}
				}else if(Phase == 2){
					//Revolving around the parent NPC
					projectile.Center = parent.Center + expert_revolutionOffset;

					if(timer > 3f * 60){
						Phase = 3;
						timer = -1;
					}

					SpawnMoveDust(0.3f);
				}else if(Phase == 3){
					if(timer > 25){
						Phase = 4;
						projectile.velocity = projectile.DirectionTo(target.Center) * 20f;
					}
				}

				if(Phase < 4)
					projectile.timeLeft = 2 * 60;

				if(Phase > 0 && Phase < 3)
					projectile.rotation = dirFromOwner.ToRotation() + MathHelper.PiOver2;
				else if(Phase == 3){
					float targetRotation = projectile.DirectionTo(target.Center).ToRotation() + MathHelper.PiOver2;

					//Ease the targetting rotation
					projectile.rotation = MathHelper.Lerp(projectile.rotation, targetRotation, 0.25f);
				}else
					projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
			}else{
				//Desolation Mode: sword acts like the Terraprisma summon, but with a twist
				projectile.timeLeft = 60;

				if(!target.WithinDistance(parent.Center, 40 * 16) && Phase < 2)
					Phase = -1;
				else if(Phase < 2 && timer >= 0){
					Phase = 2;

					timer = 0;
				}

				if(Phase == -1){
					//Try to stay behind the boss
					Vector2 pos = parent.Bottom - new Vector2(0, 0.667f * parent.height);
					pos += parent.spriteDirection == 1 ? new Vector2(0, 150) : new Vector2(0, -150);

					//Only try to change movement if the projectile is far enough away
					if(!projectile.WithinDistance(pos, 2 * 16)){
						Vector2 dir = projectile.DirectionTo(pos);

						if(Math.Sign(projectile.velocity.X) != Math.Sign(dir.X))
							projectile.velocity.X *= 1f - 5f / 60f;
						if(Math.Sign(projectile.velocity.Y) != Math.Sign(dir.Y))
							projectile.velocity.Y *= 1f - 5f / 60f;

						projectile.velocity += dir * 12f / 60f;

						const float velCap = 18.5f;
						if(projectile.velocity.LengthSquared() > velCap * velCap)
							projectile.velocity = Vector2.Normalize(projectile.velocity) * velCap;
					}

					float factorX = projectile.velocity.X / 12f;
					factorX.Clamp(-1, 1);
					projectile.rotation = MathHelper.Pi + MathHelper.ToRadians(30 * factorX);

					//If any swords are nearby, move both away from each other
					for(int i = 0; i < Main.maxProjectiles; i++){
						Projectile proj = Main.projectile[i];
						const float dist = 4 * 16;
						if(i != projectile.whoAmI && proj.active && proj.type == projectile.type && projectile.DistanceSQ(proj.Center) < dist * dist){
							Vector2 dir = projectile.DirectionTo(proj.Center);
							Vector2 midpoint = projectile.Center + (proj.Center - projectile.Center) / 2;

							projectile.Center = midpoint + -dir * dist / 2;
							proj.Center = midpoint + dir * dist / 2;
						}
					}

					SpawnMoveDust(0.3f);
				}else if(Phase == 2){
					//Player is too close!  Stab them
					var dir = projectile.DirectionTo(target.Center);
					projectile.rotation = MathHelper.PiOver2 + MathHelper.Lerp(projectile.rotation, dir.ToRotation() + MathHelper.PiOver2, 0.25f);

					projectile.velocity = Vector2.Zero;

					if(timer == 45){
						timer = -1;
						Phase = 3;

						projectile.velocity = dir * 12f;
					}
				}else if(Phase == 3){
					SpawnMoveDust(0.3f);

					projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;

					if(timer == 80){
						Phase = -1;

						timer = -151;
					}
				}
			}

			timer++;
		}

		private void SpawnMoveDust(float chance){
			Projectile projExtra = Main.projectile[extra];
			Projectile projExtra2 = Main.projectile[extra2];

			Vector2 start = projExtra.Center;
			Vector2 end = projExtra2.Center;

			int count = 20;
			Vector2 step = (end - start) / count;

			for(int i = 0; i < count; i++){
				Vector2 offset = start + step * i;
				SpawnDustInner(projectile.Center + offset, new Vector2(4), chance);
			}
		}

		private void SpawnDustInner(Vector2 position, Vector2 box, float chance){
			if(Main.rand.NextFloat() < chance){
				Dust dust = Dust.NewDustDirect(position - box / 2f, (int)box.X, (int)box.Y, 74);
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