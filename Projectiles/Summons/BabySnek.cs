using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Projectiles.Summons{
	public class BabySnek : ModProjectile{
		public override void SetStaticDefaults(){
			Main.projFrames[projectile.type] = Main.projFrames[ProjectileID.BabySlime];
			Main.projPet[projectile.type] = true;
			ProjectileID.Sets.MinionTargettingFeature[projectile.type] = true; //This is necessary for right-click targeting
			ProjectileID.Sets.MinionSacrificable[projectile.type] = true;  //Required to have multiple of the same summon
		}

		public override void SetDefaults(){
			//Copied from Projectile.SetDefaults()
			projectile.netImportant = true;
			projectile.scale = 0.8f;
			projectile.width = 50;
			projectile.height = 60;
			projectile.penetrate = -1;
			projectile.timeLeft *= 5;
			projectile.minion = true;
			projectile.minionSlots = 1f;
			projectile.tileCollide = true;
			projectile.friendly = true;
			projectile.hostile = false;

			drawOriginOffsetY = 8;
		}

		public bool Fly{
			get;
			internal set;
		} = false;
		public float DistanceToTarget{
			get;
			internal set;
		} = 0f;
		private int SmallHopTimer = 0;
		private int BigHopTimer = 0;

//		private int DebugTimer = 0;

		private const int MAX_TARGET_DISTANCE = 60 * 16;
		private int MAX_PLAYER_DISTANCE => 7 * 16 * (projectile.minionPos + 1);
		private const int MAX_PLAYER_FLY_DISTANCE = 40 * 16;
		private Vector2 Player_StandBehind_Distance => new Vector2(3 * 16, 0) * (projectile.minionPos + 1);

		private Player ownerPlayer = null;
		private CosmivengeonPlayer modPlayer = null;
		private NPC npcTarget = null;

		public override bool MinionContactDamage() => npcTarget != null;

		public override void AI(){
			ownerPlayer = Main.player[projectile.owner];
			modPlayer = ownerPlayer.GetModPlayer<CosmivengeonPlayer>();

			if(ownerPlayer.dead)
				modPlayer.babySnek = false;
			if(modPlayer.babySnek)
				projectile.timeLeft = 2;
			
			//First, check if the player is flying or the summon is too far away from the player (60 tiles).
			//If either are true, then set "fly" to true and force the summon to fly back to the player
			if(((ownerPlayer.rocketDelay2 > 0 || ownerPlayer.wingTime > 0) && Vector2.Distance(projectile.Center, ownerPlayer.Center) > MAX_PLAYER_FLY_DISTANCE) || Vector2.Distance(projectile.Center, ownerPlayer.Center) > MAX_TARGET_DISTANCE){
				Fly = true;
				projectile.tileCollide = false;
			}else if(Vector2.Distance(projectile.Center, ownerPlayer.Center) < MAX_PLAYER_DISTANCE && Fly && !(ownerPlayer.mount.Active && ownerPlayer.mount.CanFly)){
				Fly = false;
				projectile.tileCollide = true;
				projectile.rotation = 0;
			}

			if(Fly){
				AI_Fly();
			}else{
				TryTargetNPC();

				MoveToOwnerOrNPC();

				//First, check to see if the projectile can't move due to tiles
				//If this is the case, then do a small hop to try and get over the tile
				if(SmallHopTimer < 0 && projectile.position == projectile.oldPosition && DistanceToTarget > 1f){
					projectile.velocity.Y = -6f;
					SmallHopTimer = Main.rand.Next((int)(0.5f * 60), 2 * 60);
				}

				//Apply gravity
				projectile.velocity.Y += (20f / 60f);

				NoFly_ChangeFrame();

				projectile.velocity.Y = Utils.Clamp(projectile.velocity.Y, -30f, 30f);

				if(projectile.velocity.Y == 0 && projectile.oldVelocity.Y == 0)
					projectile.velocity.Y = -3f;
			}

			//Apply friction
			projectile.velocity.X *= 0.72f;

			projectile.velocity.X = Utils.Clamp(projectile.velocity.X, -10f, 10f);
			projectile.velocity.Y = Utils.Clamp(projectile.velocity.Y, -15f, 15f);

			if(Math.Abs(projectile.velocity.X) < 6f / 60f)
				projectile.velocity.X = 0f;
			if(Math.Abs(projectile.velocity.Y) < 6f / 60f)
				projectile.velocity.Y = 0f;

			projectile.spriteDirection = (projectile.velocity.X >= 0) ? 1 : -1;

			if(SmallHopTimer >= 0)
				SmallHopTimer--;
			if(BigHopTimer >= 0)
				BigHopTimer--;

			/*		DEBUG SHIT
			if(DebugTimer % 60 == 0 && npcTarget != null)
				Main.NewText($"Baby Snek - npcTarget: {npcTarget.whoAmI}, Name: {npcTarget.GivenOrTypeName}");
			else if(DebugTimer % 60 == 0 && npcTarget is null)
				Main.NewText("Baby Snek - Not currently targeting a hostile NPC.");

			DebugTimer++;
			*/
		}

		public override bool TileCollideStyle(ref int width, ref int height, ref bool fallThrough){
			if(ownerPlayer != null)
				fallThrough = projectile.Bottom.Y < ownerPlayer.Top.Y;
			return base.TileCollideStyle(ref width, ref height, ref fallThrough);
		}

		private void AI_Fly(){
			//If the pet should fly back to the player, create a reference position behind the character and make the summon target it
			projectile.velocity +=
				-Vector2.Normalize(projectile.Center - (ownerPlayer.Center - (ownerPlayer.direction * Player_StandBehind_Distance))) * 31f / 60f;

			projectile.rotation = projectile.velocity.X * 0.08f;
				
			int dust = Dust.NewDust(projectile.Bottom, 6, 6, 66);
			Main.dust[dust].noGravity = true;

			//Change the animation frame every 10 frames
			if(++projectile.frameCounter >= 10){
				projectile.frameCounter = 0;
				projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
				if(projectile.frame == 0)
					projectile.frame = 2;
			}

			DistanceToTarget = Vector2.Distance(ownerPlayer.Center, projectile.Center);
		}

		private void TryTargetNPC(){
			npcTarget = null;

			//If we're using the right-click feature, then target that NPC
			if(ownerPlayer.HasMinionAttackTargetNPC){
				npcTarget = Main.npc[ownerPlayer.MinionAttackTargetNPC];
				return;
			}

			//Target the closest enemy to the player
			for(int n = 0; n < Main.npc.Length; n++){
				NPC npc = Main.npc[n];

				//Ignore any NPCs further away than 60 blocks
				if(npc.active && Vector2.Distance(npc.Center, projectile.Center) > MAX_TARGET_DISTANCE)
					continue;

				//If we haven't set a target and this NPC is hostile, set the target to this
				if(npcTarget == null && npc.active && (!npc.friendly || npc.boss) && Vector2.Distance(npc.Center, ownerPlayer.Center) < MAX_TARGET_DISTANCE && npc.lifeMax > 5){
					npcTarget = npc;
					continue;
				}

				//If the current target is further away than this "npc" and it's hostile, then make this "npc" the new target
				//Also ignore critters, which for some reason pass this check
				if(npcTarget != null && npc.active && Vector2.Distance(npc.Center, ownerPlayer.Center) < Vector2.Distance(npcTarget.Center, ownerPlayer.Center) && (!npc.friendly || npc.boss) && npc.lifeMax > 5)
					npcTarget = npc;
			}
		}

		private void MoveToOwnerOrNPC(){
			//If we have a valid target, then move towards it
			if(npcTarget != null){
				//First, check if the enemy is above this projectile.  if it is, then attempt to jump at it if the projectile's within 3 tiles horizontally of the enemy's center
				//This projectile can only jump if it is not moving vertically
				if(BigHopTimer < 0 && projectile.velocity.Y == 0 && projectile.oldVelocity.Y == 0 && npcTarget.Bottom.Y < projectile.Bottom.Y && Math.Abs(projectile.Center.X - npcTarget.Center.X) <= 3 * 16){
					projectile.frame = 1;
					projectile.velocity.Y = -10f;
					BigHopTimer = Main.rand.Next(1 * 60, (int)(2.5f * 60));
				}

				DistanceToTarget = Vector2.Distance(projectile.Center, npcTarget.Center);

				float ratio = DistanceToTarget / MAX_PLAYER_DISTANCE;
				ratio = Utils.Clamp(ratio, 0.9f, 1.3f);
				ratio *= (7f / 60f);

				Vector2 acceleration = -Vector2.Normalize(projectile.Center - npcTarget.Center) * ratio;

				if(DistanceToTarget > 1.5f * 16 && Math.Abs(acceleration.X) < 3f)
					acceleration.X = 3f * (acceleration.X >= 0 ? 1 : -1);

				projectile.velocity.X += acceleration.X;

				if(DistanceToTarget <= 1.5f * 16)
					projectile.velocity.X *= 0.86f;
			}else{
				//Otherwise, there aren't any hostile entities nearby.  Move to behind the player
				DistanceToTarget = Vector2.Distance(projectile.Center, ownerPlayer.Center - ownerPlayer.direction * Player_StandBehind_Distance);

				if(DistanceToTarget > 1 * 16){
					float ratio = DistanceToTarget / Player_StandBehind_Distance.X / 2f;
					ratio = Utils.Clamp(ratio, 0.9f, 1.3f);
					ratio *= (7f / 60f);

					Vector2 acceleration =
						-Vector2.Normalize(projectile.Center - (ownerPlayer.Center - ownerPlayer.direction * Player_StandBehind_Distance)) * ratio;

					if(DistanceToTarget > 1.5f * 16 && Math.Abs(acceleration.X) < 3f)
						acceleration.X = 3f * (acceleration.X >= 0 ? 1 : -1);

					projectile.velocity.X += acceleration.X;
				}

				//Finally, if the projectile is beneath the player, try to jump to the player
				//Don't jump if the player is flying or if they're in a flying mount
				if(!(ownerPlayer.rocketDelay2 > 0 || ownerPlayer.wingTime > 0) && projectile.Top.Y > ownerPlayer.Bottom.Y && !(ownerPlayer.mount.Active && ownerPlayer.mount.CanFly)){
					projectile.frame = 1;
					projectile.velocity.Y = -10f;
					BigHopTimer = Main.rand.Next((int)(1.5f * 60), 3 * 60);
				}
			}
		}

		private void NoFly_ChangeFrame(){
			//Change the animation frame depending on how the summon is moving
			if(projectile.velocity.Y != 0 && projectile.oldVelocity.Y != 0)
				projectile.frame = 1;
			else if(Math.Abs(projectile.velocity.X) < 5f){
				if(++projectile.frameCounter >= 10){
					projectile.frameCounter = 0;
					projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
					if(projectile.frame == 2)
						projectile.frame = 0;
				}
			}else{
				if(++projectile.frameCounter >= 20){
					projectile.frameCounter = 0;
					projectile.frame = ++projectile.frame % Main.projFrames[projectile.type];
					if(projectile.frame == 2)
						projectile.frame = 0;
				}
			}
		}
	}
}
