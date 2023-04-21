using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace CosmivengeonMod.Projectiles.Summons{
	public class BabyIceProwler : WalkingSummon{
		public override void ExtraStaticDefaults(){
			Main.projFrames[Projectile.type] = 4;
		}

		public override void ExtraDefaults(){
			Projectile.width = 44;
			Projectile.height = 24;
		}

		public override bool OnTileCollide(Vector2 oldVelocity) => false;

		public override void UpdateTime(){
			if(ownerPlayer.dead)
				minionOwner.babyProwler = false;
			if(minionOwner.babyProwler)
				Projectile.timeLeft = 2;
		}

		public override void Behaviour_AttackNPC(){
			float velXCap = 10f;
			float acceleration = 0.175f;

			if(Projectile.velocity.Y == 0f && Projectile.Center.Y > npcTarget.Bottom.Y)
				Projectile.velocity.Y = Math.Min(-4 + Math.Abs(Projectile.Center.Y - npcTarget.Center.Y) * 0.233f, -7) * Main.rand.NextFloat(0.835f, 1.183f);

			if(Math.Abs(Projectile.Center.X - npcTarget.Center.X) > 2 * 16 && Math.Sign(Projectile.velocity.X) != Math.Sign(npcTarget.Center.X - Projectile.Center.X))
				Projectile.velocity.X *= 1f - 3f / 60f;

			Projectile.velocity.X += Math.Sign(npcTarget.Center.X - Projectile.Center.X) * acceleration;
			Projectile.velocity.X.Clamp(-velXCap, velXCap);

			Projectile.rotation = 0;
		}

		public override void ClampWalkSpeed(){
			Projectile.velocity.X.Clamp(-8, 8);
		}

		public override void UpdateAnimation(){
			if(Projectile.velocity.X == 0 || Projectile.velocity.Y != 0f || State == ActionStates.FlyingToPlayer)
				Projectile.frame = 0;
			else if(++Projectile.frameCounter > 9){
				Projectile.frameCounter = 0;
				Projectile.frame = ++Projectile.frame % Main.projFrames[Projectile.type];
			}
		}

		public override void UpdateGravity(){
			Projectile.velocity.Y += 18.45f / 60;
			if(Projectile.velocity.Y > 16f)
				Projectile.velocity.Y = 16f;
		}

		public override void SpawnFlyDust(){
			Dust dust = Dust.NewDustDirect(Projectile.Bottom, 6, 6, 66);
			dust.noGravity = true;
		}

		public override void Do_SmoothStep(){
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
		}
	}
}
