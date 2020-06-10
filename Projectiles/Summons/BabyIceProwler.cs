using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons{
	public class BabyIceProwler : Summon{
		public override void ExtraStaticDefaults(){
			Main.projFrames[projectile.type] = 4;
		}

		public override void ExtraDefaults(){
			projectile.width = 44;
			projectile.height = 24;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => false;

		public override void UpdateTime(){
			if(ownerPlayer.dead)
				modPlayer.babyProwler = false;
			if(modPlayer.babyProwler)
				projectile.timeLeft = 2;
		}

		public override void Behaviour_AttackNPC(){
			float velXCap = 10f;
			float acceleration = 0.175f;

			if(projectile.velocity.Y == 0f && projectile.Center.Y > npcTarget.Bottom.Y)
				projectile.velocity.Y = Math.Min(-4 + Math.Abs(projectile.Center.Y - npcTarget.Center.Y) * 0.233f, -7) * Main.rand.NextFloat(0.835f, 1.183f);

			if(Math.Abs(projectile.Center.X - npcTarget.Center.X) > 2 * 16 && Math.Sign(projectile.velocity.X) != Math.Sign(npcTarget.Center.X - projectile.Center.X))
				projectile.velocity.X *= 1f - 3f / 60f;

			projectile.velocity.X += Math.Sign(npcTarget.Center.X - projectile.Center.X) * acceleration;
			projectile.velocity.X.Clamp(-velXCap, velXCap);

			projectile.rotation = 0;
		}

		public override void ClampWalkSpeed(){
			projectile.velocity.X.Clamp(-8, 8);
		}

		public override void UpdateAnimation(){
			if(projectile.velocity.X == 0 || projectile.velocity.Y != 0f || State == ActionStates.FlyingToPlayer)
				projectile.frame = 0;
			else if(++projectile.frameCounter > 9){
				projectile.frameCounter = 0;
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
			}
		}

		public override void UpdateGravity(){
			projectile.velocity.Y += 18.45f / 60;
			if(projectile.velocity.Y > 16f)
				projectile.velocity.Y = 16f;
		}

		public override void SpawnFlyDust(){
			Dust dust = Dust.NewDustDirect(projectile.Bottom, 6, 6, 66);
			dust.noGravity = true;
		}

		public override void Do_SmoothStep(){
			Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
		}
	}
}
