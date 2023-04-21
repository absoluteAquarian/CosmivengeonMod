using CosmivengeonMod.Players;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons{
	public abstract class WalkingSummon : ModProjectile{
		public enum ActionStates{
			None,
			AttackingNPC,
			MovingToPlayer,
			FlyingToPlayer
		};

		public enum AnimationStates{
			Idle,
			Moving,
			Jumping,
			Flying
		}

		public ActionStates State = ActionStates.None;
		public AnimationStates AnimationState = AnimationStates.Idle;

		public int StandBehindDistance
			=> (!ownerPlayer.dJumpEffectSandstorm ? -ownerPlayer.direction : (Projectile.Center.X >= ownerPlayer.Center.X).ToDirectionInt()) * 30 * (Projectile.minionPos + 1);

		internal Player ownerPlayer = null;
		internal MinionPlayer minionOwner = null;
		internal NPC npcTarget = null;

		internal float MaxTargetDistance => ownerPlayer.HasMinionAttackTargetNPC ? 80 * 16 : 40 * 16;
		internal static readonly float MinFlyDistance = 25 * 16;
		internal static readonly float GoIdleDistance = 3f * 16;

		internal Vector2 OwnerPositionOffset => State == ActionStates.FlyingToPlayer
			? ownerPlayer.Top + new Vector2(StandBehindDistance, 0f)
			: new Vector2(ownerPlayer.Bottom.X + StandBehindDistance, Projectile.Bottom.Y - Projectile.height / 2f - 4);

		public override bool MinionContactDamage() => npcTarget != null;

		public override bool? CanCutTiles() => npcTarget != null;

		public sealed override void SetStaticDefaults(){
			Main.projPet[Projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[Projectile.type] = true; //This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[Projectile.type] = true;  //Required to have multiple of the same summon

			ExtraStaticDefaults();
		}

		public sealed override void SetDefaults(){
			Projectile.netImportant = true;
			Projectile.penetrate = -1;
			Projectile.minion = true;
			Projectile.minionSlots = 1f;
			Projectile.tileCollide = true;
			Projectile.friendly = true;
			Projectile.hostile = false;

			ExtraDefaults();
		}

		public abstract void ExtraStaticDefaults();
		public abstract void ExtraDefaults();

		public sealed override void AI(){
			ownerPlayer = Main.player[Projectile.owner];
			minionOwner = ownerPlayer.GetModPlayer<MinionPlayer>();

			UpdateTime();

			//Check for new hostile targets unless the player has selected one.
			//If the player did that, use the NPC from that selection
			if(ownerPlayer.HasMinionAttackTargetNPC){
				npcTarget = Main.npc[ownerPlayer.MinionAttackTargetNPC];
				goto afterTargetSet;
			}
			
			var nearbyHostiles = Main.npc.Where(n => n.active && n.CanBeChasedBy() && (!n.friendly || n.boss) && Projectile.DistanceSQ(n.Center) < MaxTargetDistance * MaxTargetDistance);
			if(nearbyHostiles.Any())
				npcTarget = nearbyHostiles.OrderBy(n => Projectile.DistanceSQ(n.Center)).FirstOrDefault();
			else{
				//No nearby targets
				npcTarget = null;
			}
afterTargetSet:

			PreCheckPlayerState();

			//If we don't have a target, try to move to the player depending on what state they're in
			if(npcTarget is null){
				//If we're close to the intended target position, stop flying or go idle
				//Otherwise, if the player is too far away AND the player is in a mount that can fly/hover and it's active AND the player isn't close to any solid tiles below them
				//Otherwise, just walk towards the player
				if(Projectile.DistanceSQ(OwnerPositionOffset) < GoIdleDistance * GoIdleDistance && !((ownerPlayer.mount?.Active ?? false) && (ownerPlayer.mount.CanFly || ownerPlayer.mount.CanHover) && !Collision.SolidCollision(ownerPlayer.position, ownerPlayer.width, 8 * 16))){
					State = ActionStates.None;
					AnimationState = AnimationStates.Idle;
					Projectile.tileCollide = true;
				}else if(Projectile.DistanceSQ(ownerPlayer.Center) > MinFlyDistance * MinFlyDistance && Projectile.DistanceSQ(OwnerPositionOffset) > GoIdleDistance * GoIdleDistance){
					State = ActionStates.FlyingToPlayer;
					AnimationState = AnimationStates.Flying;
					Projectile.tileCollide = false;
				}else if(State != ActionStates.FlyingToPlayer){
					State = ActionStates.MovingToPlayer;
					AnimationState = AnimationStates.Moving;
					Projectile.tileCollide = true;
				}

				//If the minion is really far away (2000 pixels), teleport them to the player's center
				if(Projectile.DistanceSQ(ownerPlayer.Center) > 2000 * 2000)
					Projectile.Center = ownerPlayer.Center;
			}else{
				Projectile.tileCollide = true;

				//We have a target.  try to attack it
				State = ActionStates.AttackingNPC;

				if(Projectile.DistanceSQ(ownerPlayer.Center) > MaxTargetDistance * MaxTargetDistance){
					State = ActionStates.FlyingToPlayer;
					AnimationState = AnimationStates.Flying;
					Projectile.tileCollide = false;
					npcTarget = null;
				}

				PostCheckTooFarFromPlayer();
			}

			PostCheckPlayerState();

			//Handle the various action states
			if(State == ActionStates.AttackingNPC){
				Behaviour_AttackNPC();
			}else if(State == ActionStates.FlyingToPlayer){
				if(Projectile.DistanceSQ(OwnerPositionOffset) > 2.25f * 256){
					//Vector to the target position
					Vector2 toTarget = Projectile.DirectionTo(OwnerPositionOffset);

					float scalar;
					if(Projectile.Distance(OwnerPositionOffset) > 10 * 16)
						scalar = 3.225f;
					else if(Projectile.Distance(OwnerPositionOffset) > 7.5f * 16)
						scalar = 2.518f;
					else if(Projectile.Distance(OwnerPositionOffset) > 5 * 16)
						scalar = 1.725f;
					else if(Projectile.Distance(OwnerPositionOffset) > 3 * 16)
						scalar = 1.235f;
					else
						scalar = 0.897f;

					//Friction
					Projectile.velocity *= 1 - 1.25f / 60;

					Projectile.velocity += toTarget * scalar * 12.55f / 60;
				}

				//Spawn the flying dust
				SpawnFlyDust();

				//Set the rotation based on the X-velocity
				Projectile.rotation = MiscUtils.RotationFromVelocity(Projectile.velocity.X, 7.5f, 20f);
			}else if(State == ActionStates.MovingToPlayer){
				Vector2 toTarget = Projectile.DirectionTo(OwnerPositionOffset);

				//Friction
				if(Math.Sign(Projectile.velocity.X) != Math.Sign(toTarget.X))
					Projectile.velocity.X *= 1 - 5f / 60;

				Projectile.velocity.X += toTarget.X * 8.575f / 60;
				Projectile.rotation = 0;

				ClampWalkSpeed();

				Do_SmoothStep();
			}else if(State == ActionStates.None){
				Projectile.velocity.X *= 1 - (Projectile.velocity.Y != 0 ? 3.25f : 9.3f) / 60f;

				if(Math.Abs(Projectile.velocity.X) < 0.3f)
					Projectile.velocity.X = 0;

				Projectile.rotation = 0;

				//Stopped holding a movement direction
				if(Math.Abs(ownerPlayer.velocity.X) < Math.Abs(ownerPlayer.oldVelocity.X) || ownerPlayer.velocity.X == 0)
					AnimationState = AnimationStates.Idle;
				else
					AnimationState = AnimationStates.Moving;
			}

			UpdateAnimation();

			if(State != ActionStates.FlyingToPlayer)
				UpdateGravity();

			Projectile.spriteDirection = Projectile.velocity.X > 0
				? -1
				: (Projectile.velocity.X < 0
					? 1
					: Projectile.spriteDirection);

			PostUpdate();
		}

		public abstract void UpdateTime();
		public abstract void UpdateAnimation();
		public virtual void UpdateDirection(){ }
		public virtual void UpdateGravity(){ }

		public virtual void SpawnFlyDust(){ }

		public virtual void PreCheckPlayerState(){ }
		public virtual void PostCheckPlayerState(){ }
		public virtual void PostCheckTooFarFromPlayer(){ }
		public virtual void PostUpdate(){ }

		public virtual void Behaviour_AttackNPC(){ }
		public virtual void ClampWalkSpeed(){ }

		public virtual void Do_SmoothStep(){
			Collision.StepUp(ref Projectile.position, ref Projectile.velocity, Projectile.width, Projectile.height, ref Projectile.stepSpeed, ref Projectile.gfxOffY);
		}
	}
}
