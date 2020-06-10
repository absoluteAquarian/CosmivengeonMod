using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons{
	public abstract class Summon : ModProjectile{
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
			=> (!ownerPlayer.dJumpEffectSandstorm ? -ownerPlayer.direction : (projectile.Center.X >= ownerPlayer.Center.X).ToDirectionInt()) * 30 * (projectile.minionPos + 1);

		internal Player ownerPlayer = null;
		internal CosmivengeonPlayer modPlayer = null;
		internal NPC npcTarget = null;

		internal float MaxTargetDistance => ownerPlayer.HasMinionAttackTargetNPC ? 80 * 16 : 40 * 16;
		internal static readonly float MinFlyDistance = 25 * 16;
		internal static readonly float GoIdleDistance = 3f * 16;

		internal Vector2 OwnerPositionOffset => State == ActionStates.FlyingToPlayer
			? ownerPlayer.Top + new Vector2(StandBehindDistance, 0f)
			: new Vector2(ownerPlayer.Bottom.X + StandBehindDistance, projectile.Bottom.Y - projectile.height / 2f - 4);

		public override bool MinionContactDamage() => npcTarget != null;

		public override bool? CanCutTiles() => npcTarget != null;

		public sealed override void SetStaticDefaults(){
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;  //Required to have multiple of the same summon

			ExtraStaticDefaults();
		}

		public sealed override void SetDefaults(){
			projectile.netImportant = true;
			projectile.penetrate = -1;
			projectile.minion = true;
			projectile.minionSlots = 1f;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.hostile = false;

			ExtraDefaults();
		}

		public abstract void ExtraStaticDefaults();
		public abstract void ExtraDefaults();

		public sealed override void AI(){
			ownerPlayer = Main.player[projectile.owner];
			modPlayer = ownerPlayer.GetModPlayer<CosmivengeonPlayer>();

			UpdateTime();

			//Check for new hostile targets unless the player has selected one.
			//If the player did that, use the NPC from that selection
			if(ownerPlayer.HasMinionAttackTargetNPC){
				npcTarget = Main.npc[ownerPlayer.MinionAttackTargetNPC];
				goto afterTargetSet;
			}
			
			var nearbyHostiles = Main.npc.Where(n => n.active && n.CanBeChasedBy() && (!n.friendly || n.boss) && projectile.DistanceSQ(n.Center) < MaxTargetDistance * MaxTargetDistance);
			if(nearbyHostiles.Any())
				npcTarget = nearbyHostiles.OrderBy(n => projectile.DistanceSQ(n.Center)).FirstOrDefault();
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
				if(projectile.DistanceSQ(OwnerPositionOffset) < GoIdleDistance * GoIdleDistance && !((ownerPlayer.mount?.Active ?? false) && (ownerPlayer.mount.CanFly || ownerPlayer.mount.CanHover) && !Collision.SolidCollision(ownerPlayer.position, ownerPlayer.width, 8 * 16))){
					State = ActionStates.None;
					AnimationState = AnimationStates.Idle;
					projectile.tileCollide = true;
				}else if(projectile.DistanceSQ(ownerPlayer.Center) > MinFlyDistance * MinFlyDistance && projectile.DistanceSQ(OwnerPositionOffset) > GoIdleDistance * GoIdleDistance){
					State = ActionStates.FlyingToPlayer;
					AnimationState = AnimationStates.Flying;
					projectile.tileCollide = false;
				}else if(State != ActionStates.FlyingToPlayer){
					State = ActionStates.MovingToPlayer;
					AnimationState = AnimationStates.Moving;
					projectile.tileCollide = true;
				}

				//If the minion is really far away (2000 pixels), teleport them to the player's center
				if(projectile.DistanceSQ(ownerPlayer.Center) > 2000 * 2000)
					projectile.Center = ownerPlayer.Center;
			}else{
				projectile.tileCollide = true;

				//We have a target.  try to attack it
				State = ActionStates.AttackingNPC;

				if(projectile.DistanceSQ(ownerPlayer.Center) > MaxTargetDistance * MaxTargetDistance){
					State = ActionStates.FlyingToPlayer;
					AnimationState = AnimationStates.Flying;
					projectile.tileCollide = false;
					npcTarget = null;
				}

				PostCheckTooFarFromPlayer();
			}

			PostCheckPlayerState();

			//Handle the various action states
			if(State == ActionStates.AttackingNPC){
				Behaviour_AttackNPC();
			}else if(State == ActionStates.FlyingToPlayer){
				if(projectile.DistanceSQ(OwnerPositionOffset) > 2.25f * 256){
					//Vector to the target position
					Vector2 toTarget = projectile.DirectionTo(OwnerPositionOffset);

					float scalar;
					if(projectile.Distance(OwnerPositionOffset) > 10 * 16)
						scalar = 3.225f;
					else if(projectile.Distance(OwnerPositionOffset) > 7.5f * 16)
						scalar = 2.518f;
					else if(projectile.Distance(OwnerPositionOffset) > 5 * 16)
						scalar = 1.725f;
					else if(projectile.Distance(OwnerPositionOffset) > 3 * 16)
						scalar = 1.235f;
					else
						scalar = 0.897f;

					//Friction
					projectile.velocity *= 1 - 1.25f / 60;

					projectile.velocity += toTarget * scalar * 12.55f / 60;
				}

				//Spawn the flying dust
				SpawnFlyDust();

				//Set the rotation based on the X-velocity
				projectile.rotation = CosmivengeonUtils.RotationFromVelocity(projectile.velocity.X, 7.5f, 20f);
			}else if(State == ActionStates.MovingToPlayer){
				Vector2 toTarget = projectile.DirectionTo(OwnerPositionOffset);

				//Friction
				if(Math.Sign(projectile.velocity.X) != Math.Sign(toTarget.X))
					projectile.velocity.X *= 1 - 5f / 60;

				projectile.velocity.X += toTarget.X * 8.575f / 60;
				projectile.rotation = 0;

				ClampWalkSpeed();

				Do_SmoothStep();
			}else if(State == ActionStates.None){
				projectile.velocity.X *= 1 - (projectile.velocity.Y != 0 ? 3.25f : 9.3f) / 60f;

				if(Math.Abs(projectile.velocity.X) < 0.3f)
					projectile.velocity.X = 0;

				projectile.rotation = 0;

				//Stopped holding a movement direction
				if(Math.Abs(ownerPlayer.velocity.X) < Math.Abs(ownerPlayer.oldVelocity.X) || ownerPlayer.velocity.X == 0)
					AnimationState = AnimationStates.Idle;
				else
					AnimationState = AnimationStates.Moving;
			}

			UpdateAnimation();

			if(State != ActionStates.FlyingToPlayer)
				UpdateGravity();

			projectile.spriteDirection = projectile.velocity.X > 0
				? -1
				: (projectile.velocity.X < 0
					? 1
					: projectile.spriteDirection);

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
			Collision.StepUp(ref projectile.position, ref projectile.velocity, projectile.width, projectile.height, ref projectile.stepSpeed, ref projectile.gfxOffY);
		}
	}
}
