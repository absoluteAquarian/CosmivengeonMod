using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Utility.Extensions;
using Microsoft.Xna.Framework;
using System;
using Terraria;

namespace CosmivengeonMod.Projectiles.Summons{
	public class BabySnek : WalkingSummon{
		public override void ExtraStaticDefaults(){
			DisplayName.SetDefault("Yamanu");
			Main.projFrames[projectile.type] = 11;
		}

		public override void ExtraDefaults(){
			projectile.scale = 0.8f;
			projectile.width = 66;
			projectile.height = 62;

			drawOffsetX = -8;
			drawOriginOffsetY = -4;
		}

		private bool inLine = false;
		private bool farAbove = false;
		private bool slightlyAbove = false;

		private int timer_SmallHop = 0;
		private int timer_BigHop = 0;
		private int timer_DoorStuck = 0;
		private const int JumpDelayMax = 12;
		private int timer_jumpDelay = JumpDelayMax;

		private readonly float vel_SmallHop = -9f;
		private readonly float vel_BigHop = -13.2f;
		private int jumpTries = 0;

		private static readonly int max_SmallHop = 24;
		private static readonly int max_BigHop = 60;

		private const int IdleOffset = 0;	//1 frame
		private const int MoveOffset = 1;	//3 frames
		private const int JumpOffset = 4;	//2 frames
		private const int FlyOffset = 6;	//5 frames

		private int animTimer_Idle = 0;
		private int animTimer_Move = 0;
		private int animTimer_Fly = 0;

		private readonly int animDelay_Idle = 12;
		private readonly int animDelay_Move = 15;
		private readonly int animDelay_Fly = 12;

		private readonly int animFactor_Move = 8;

		private Rectangle BetterHitbox;

		public override void ModifyDamageHitbox(ref Rectangle hitbox){
			Rectangle rect = BetterHitbox;
			
			//Resize the hitbox and update its position
			rect.X = (int)(rect.X * projectile.scale + projectile.position.X);
			rect.Y = (int)(rect.Y * projectile.scale + projectile.position.Y);
			rect.Width = (int)(rect.Width * projectile.scale);
			rect.Height = (int)(rect.Height * projectile.scale);

			hitbox = rect;
		}

		private bool firstTick = true;
		private bool onGround;

		public override void UpdateTime(){
			if(ownerPlayer.dead)
				minionOwner.babySnek = false;
			if(minionOwner.babySnek)
				projectile.timeLeft = 2;
		}

		public override void PostCheckTooFarFromPlayer(){
			if(onGround && npcTarget != null){
				inLine = npcTarget.Bottom.Y >= projectile.Center.Y && projectile.Center.Y >= npcTarget.Top.Y;
				farAbove = !inLine && npcTarget.Bottom.Y < projectile.Center.Y - 6 * 16;
				slightlyAbove = !farAbove && npcTarget.Bottom.Y < projectile.Center.Y;

				if(AnimationState == AnimationStates.Jumping)
					AnimationState = AnimationStates.Moving;
			}
		}

		public override void Behaviour_AttackNPC(){
			if(Math.Abs(npcTarget.Center.X - projectile.Center.X) > 12){
				//Friction
				projectile.velocity.X *= 1 - 1.25f / 60;

				float acceleration = projectile.velocity.Y != 0 || projectile.oldVelocity.Y != 0 ? 11.35f : 8.95f;

				projectile.velocity.X += (npcTarget.Center.X >= projectile.Center.X).ToDirectionInt() * acceleration / 60;
			}

			//If the NPC is in line with the minion, just move towards it (do nothing; reset the timers)
			//Otherwise, if it's barely above the minion, start doing the small hops
			//Otherwise, if it's far above the minion, start doing the big hops
			//Only do the hops if the NPC is close enough
			bool closeEnough = npcTarget.Left.X - 5 * 16 < projectile.Center.X && projectile.Center.X < npcTarget.Right.X + 5 * 16;

			if(onGround && (inLine || !closeEnough)){
				timer_BigHop = max_BigHop * 2 / 3;
				timer_SmallHop = max_SmallHop / 2;
				AnimationState = AnimationStates.Moving;
				timer_jumpDelay = JumpDelayMax;
			}else if(slightlyAbove){
				JumpHelper(vel_SmallHop, ref timer_SmallHop, max_SmallHop, true, ref onGround);
			}else if(farAbove){
				JumpHelper(vel_BigHop, ref timer_BigHop, max_BigHop, false, ref onGround);
			}

			projectile.rotation = 0f;

			Do_SmoothStep();
		}

		public override void ClampWalkSpeed(){
			projectile.velocity.X.Clamp(-animFactor_Move, animFactor_Move);
		}

		public override void PreCheckPlayerState(){
			onGround = projectile.velocity.Y == 0 && projectile.oldVelocity.Y >= 0;
		}

		public override void UpdateGravity(){
			projectile.velocity.Y += 18.45f / 60;
			if(projectile.velocity.Y > 16)
				projectile.velocity.Y = 16;
		}

		public override void UpdateAnimation(){
			DoAnimation(onGround);
		}

		public override void PostUpdate(){
			projectile.velocity.X.Clamp(-16, 16);
			projectile.velocity.Y.Clamp(-16, 16);

			UpdateHitbox();

			if(firstTick)
				firstTick = false;
		}

		private void JumpHelper(float velocity, ref int timer, int timerMax, bool smallHop, ref bool onGround){
			if(smallHop)
				timer_BigHop = max_BigHop * 2 / 3;
			else
				timer_SmallHop = max_SmallHop / 2;

			timer--;

			if(onGround && timer < 0){
				timer_jumpDelay--;
				AnimationState = AnimationStates.Jumping;

				if(timer_jumpDelay <= 0){
					timer = timerMax;
					timer_jumpDelay = JumpDelayMax;
					projectile.velocity.Y = velocity * Main.rand.NextFloat(0.875f, 1.225f);
					onGround = false;
					projectile.netUpdate = true;
				}
			}else if(onGround){
				AnimationState = AnimationStates.Moving;
			}
		}

		private void UpdateHitbox(){
			switch(projectile.frame){
				case IdleOffset:
					BetterHitbox = new Rectangle(12, 14, 32, 47);
					break;
				case MoveOffset:
					BetterHitbox = new Rectangle(0, 48, 66, 13);
					break;
				case MoveOffset + 1:
					BetterHitbox = new Rectangle(0, 46, 62, 16);
					break;
				case MoveOffset + 2:
					BetterHitbox = new Rectangle(0, 44, 60, 18);
					break;
				case JumpOffset:
					BetterHitbox = new Rectangle(12, 24, 36, 38);
					break;
				case JumpOffset + 1:
					BetterHitbox = new Rectangle(12, 0, 28, 62);
					break;
				case FlyOffset:
					BetterHitbox = new Rectangle(0, 28, 66, 14);
					break;
				case FlyOffset + 1:
					BetterHitbox = new Rectangle(0, 26, 66, 18);
					break;
				case FlyOffset + 2:
					BetterHitbox = new Rectangle(0, 28, 66, 14);
					break;
				case FlyOffset + 3:
					BetterHitbox = new Rectangle(0, 26, 66, 18);
					break;
				case FlyOffset + 4:
					BetterHitbox = new Rectangle(0, 26, 66, 16);
					break;
				default:
					goto case IdleOffset;
			}

			if(projectile.spriteDirection == -1)
				BetterHitbox.X = 66 - BetterHitbox.X - BetterHitbox.Width;
		}

		private void DoAnimation(bool onGround){
			if(AnimationState == AnimationStates.Flying){
				UpdateAnimation(ref animTimer_Fly, 5, animDelay_Fly, FlyOffset);
				animTimer_Idle = 0;
				animTimer_Move = 0;
			}else if(AnimationState == AnimationStates.Jumping){
				projectile.frame = JumpOffset + (!onGround ? 1 : 0);
				animTimer_Idle = 0;
				animTimer_Move = 0;
				animTimer_Fly = 0;
			}else if(AnimationState == AnimationStates.Moving){
				int timerAdd = (int)(3 * Utils.Clamp(Math.Abs(projectile.velocity.X), 0, animFactor_Move) / animFactor_Move);
				UpdateAnimation(ref animTimer_Move, 4, animDelay_Move, MoveOffset, timerAdd);
				if(projectile.frame == MoveOffset + 3)
					projectile.frame = MoveOffset + 1;

				animTimer_Idle = 0;
				animTimer_Fly = 0;
			}else if(AnimationState == AnimationStates.Idle){
				UpdateAnimation(ref animTimer_Idle, 1, animDelay_Idle, IdleOffset);
				animTimer_Move = 0;
				animTimer_Fly = 0;
			}
		}

		private void UpdateAnimation(ref int timer, int frameCount, int delay, int offset, int extraAdd = 0){
			timer += 1 + extraAdd;

			projectile.frame = timer % (frameCount * delay) / delay + offset;
		}

		public override bool OnTileCollide(Vector2 oldVelocity){
			Vector2 nextTL = projectile.TopLeft + oldVelocity - new Vector2(4f, 0f);
			Vector2 nextTR = projectile.TopRight + oldVelocity + new Vector2(4f, 0f);
			Vector2 nextBL = projectile.BottomLeft + oldVelocity - new Vector2(4f, 0f);
			Vector2 nextBR = projectile.BottomRight + oldVelocity + new Vector2(4f, 0f);

			if(projectile.velocity.X == 0 && projectile.velocity.Y == 0 && (oldVelocity.X < 0 && (MiscUtils.TileIsSolidNotPlatform(nextTL) || MiscUtils.TileIsSolidNotPlatform(nextBL)) || (oldVelocity.X > 0 && (MiscUtils.TileIsSolidNotPlatform(nextTR) || MiscUtils.TileIsSolidNotPlatform(nextBR))))){
				timer_DoorStuck++;

				//We've been touching a wall for 35 ticks
				if(timer_DoorStuck >= 35){
					float vel = jumpTries < 4 ? vel_SmallHop : vel_SmallHop * 1.212f;
					int nextJumpsCount = jumpTries + 1;

					if(timer_jumpDelay > 0){
						timer_jumpDelay--;
						projectile.frame = JumpOffset;
					}else{
						timer_DoorStuck = 0;
						projectile.velocity.Y = vel;
						jumpTries = nextJumpsCount;
						projectile.frame = JumpOffset + 1;
					}

					AnimationState = AnimationStates.Jumping;

					UpdateHitbox();
				}
			}else if(projectile.velocity.X != 0 && timer_DoorStuck > 0){
				timer_DoorStuck = 0;
				timer_jumpDelay = JumpDelayMax;
				jumpTries = 0;
			}

			Do_SmoothStep();

			return false;
		}

		public override void SpawnFlyDust() {
			Dust dust = Dust.NewDustDirect(projectile.Center, 6, 6, 66);
			dust.noGravity = true;
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough){
			//(value < null) and (null > value) will always be false, so no need to check for the things being not null first
			fallThrough = npcTarget?.Bottom.Y > projectile.Bottom.Y + 16 || (npcTarget is null && projectile.Bottom.Y < ownerPlayer?.Top.Y);
			return true;
		}
	}
}
