using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Draek{
	public abstract class RaechonAI : ModNPC{
		public abstract float JumpStrength{ get; }
		public abstract float GetWalkSpeed(bool cantSee, bool noTarget);

		public ref float AIState => ref npc.ai[0];
		public ref float AITimer => ref npc.ai[1];
		public ref float AITimerTarget => ref npc.ai[2];
		public ref float AISubstate => ref npc.ai[3];

		private int hopTimer;
		private int hopCount;
		private int lookTimer;

		public const int State_LookingForPlayers = 1;
		public const int State_FoundPlayer_NoSee = 2;
		public const int State_FoundPlayer_CanSee = 3;

		public Player Target;

		public bool InvalidTarget => Target is null || !Target.active || Target.dead || !npc.WithinDistance(Target.Center, 40 * 16);

		private int animFrame;

		private float vel_targetX = 100;

		private int faceDir;

		private Vector2[] CoordVectors => new Vector2[]{
			npc.TopLeft    + new Vector2(-8),     npc.Top    + new Vector2(0, -8), npc.TopRight    + new Vector2(8, -8),
			npc.Left       + new Vector2(-8, 0),  npc.Center,                      npc.Right       + new Vector2(8, 0),
			npc.BottomLeft + new Vector2(-8, 8),  npc.Bottom + new Vector2(0, 8),  npc.BottomRight + new Vector2(8)
		};

		public bool CanSee(Player plr) => CoordVectors.Any(v => Collision.CanHit(v - new Vector2(1), 2, 2, plr.Center - new Vector2(1), 2, 2));

		public override void AI(){
			//AI states:
			//1: cannot see any players and doesn't have one as a target, walking around and being idle
			//2: has a target player but can't see them
			//3: has a target player and can see them
			AITimer++;

			if(npc.collideX){
				npc.velocity.X = 0;
				npc.position.X = npc.oldPosition.X;
			}

			//Make the animation move faster if the NPC is moving faster
			if(npc.velocity.X != 0)
				npc.frameCounter += Math.Max(3.5 * Math.Abs(npc.velocity.X) / vel_targetX, 0.8);
			else{
				npc.frameCounter = 0;
				animFrame = 0;
			}

			if(npc.frameCounter > 10){
				npc.frameCounter = 0;
				animFrame = ++animFrame % Main.npcFrameCount[npc.type];
			}

			//On the first tick, set the animation states properly
			if(AIState == 0){
				AIState = State_LookingForPlayers;
				AISubstate = 2;
				AITimer = 0;

				faceDir = Main.rand.Next(new int[]{ -1, 1 });
				npc.velocity.X = Main.rand.NextFloat(-3f, 3.00001f);

				npc.netUpdate = true;
			}

			npc.SmoothStep(ref npc.position);

			if(AIState == State_LookingForPlayers){
				//Attempt to find a player target
				//If one is found, set it and update the netcode
				Target = null;
				npc.target = -1;
				for(int i = 0; i < Main.maxPlayers; i++){
					Player plr = Main.player[i];

					if(!plr.active || plr.dead || !npc.WithinDistance(plr.Center, 40 * 16))
						continue;

					//Check if no blocks are in the way
					//If there aren't any, then target this player
					if(CanSee(plr)){
						Target = plr;
						npc.target = plr.whoAmI;
						AIState = State_FoundPlayer_CanSee;
						//Inform netcode
						npc.netUpdate = true;
						break;
					}
				}

				if(npc.velocity.X > 0)
					faceDir = 1;
				else if(npc.velocity.X < 0)
					faceDir = -1;

				float walk = GetWalkSpeed(true, true);

				Do_AIState_0_Movement(walk / 0.6667f, walk, walk * 2.1f, true);
			}else if(AIState == State_FoundPlayer_CanSee){
				//If we can no longer see the player, move on to AI state 1 or 2
				if(InvalidTarget){
					AIState = State_LookingForPlayers;
					Target = null;
					npc.target = -1;
					return;
				}
				if(Target != null && !CanSee(Target)){
					AIState = State_FoundPlayer_NoSee;
					return;
				}

				//We can see the player, try to move towards them
				faceDir = (Target.Center.X >= npc.Center.X).ToDirectionInt();

				float walk = GetWalkSpeed(false, false);

				//Basically a copy of AIState 0's movement, but without the waiting and with faster movement
				Do_AIState_0_Movement(walk / 0.45f, walk, walk, false);
			}else if(AIState == State_FoundPlayer_NoSee){
				if(Target != null && CanSee(Target)){
					AIState = State_FoundPlayer_CanSee;
					return;
				}

				if(npc.velocity.X > 0)
					faceDir = 1;
				else if(npc.velocity.X < 0)
					faceDir = -1;

				float walk = GetWalkSpeed(true, false);

				//Basically a copy of AIState 0's movement, but without the waiting and with faster movement
				Do_AIState_0_Movement(walk / 0.45f, walk, walk * 0.6f, false);

				lookTimer++;
				if(lookTimer > 5 * 60){
					AIState = State_LookingForPlayers;
					Reset();
				}
			}

			//Gravity
			npc.velocity.Y += 6.35f / 60f;

			npc.spriteDirection = faceDir;
		}

		private void Reset(){
			AITimer = 0;
			AITimerTarget = 0;
			lookTimer = 0;

			Target = null;
			npc.target = -1;

			npc.netUpdate = true;
		}

		private void Do_AIState_0_Movement(float acceleration, float velCap, float anim_target, bool doWait){
			//Cycles between several substates:
			//0: Reset wait timer and turn around
			//1: Wait until timer has reached the random value from substate 0; apply friction
			//2: Reset wait timer
			//3: Try to move forwards until the timer has reached the random value from substate 2
			if(!doWait && AISubstate == 0)
				AISubstate = 2;

			vel_targetX = anim_target;

			if(AISubstate == 0){
				AITimer = 0;
				if(doWait)
					AITimerTarget = Main.rand.Next(75, 161);
				else
					AITimerTarget = 0;

				AISubstate = 1;
				npc.netUpdate = true;
			}else if(AISubstate == 1){
				//Friction
				npc.velocity.X *= 1f - 2.635f / 60f;

				if(npc.velocity.Y != 0)
					AITimer--;
				else if(Math.Abs(npc.velocity.X) < 0.025f)
					npc.velocity.X = 0;

				if(AITimer >= AITimerTarget){
					//Turn around
					faceDir *= -1;
					npc.velocity.X = 0;

					AISubstate = 2;
					npc.netUpdate = true;
				}
			}else if(AISubstate == 2){
				AITimer = 0;
				AISubstate = 3;
				npc.netUpdate = true;
				AITimerTarget = Main.rand.Next(170, 280);
			}else if(AISubstate == 3){
				if(npc.velocity.Y == 0){
					npc.velocity.X += faceDir * acceleration / 60f;

					npc.velocity.X.Clamp(-velCap, velCap);
				}

				if(AITimer >= AITimerTarget){
					AISubstate = 0;
					npc.netUpdate = true;
				}

				TryHop();
			}
		}

		private void TryHop(){
			//If we've hit a wall, start a second timer
			//If that timer reaches 35 ticks, do a random amount of small hops (crab be stubby though)
			if(npc.collideX && npc.velocity.Y == 0){
				hopTimer++;

				if(hopTimer == 35){
					hopCount = Main.rand.Next(1, 4);
					npc.netUpdate = true;
				}

				if(hopTimer >= 35 && hopCount > 0){
					//Offset the NPC's position to be slightly away from the wall
					npc.position.X -= npc.spriteDirection * 4;

					npc.velocity.Y = JumpStrength;
					hopCount--;
				}else if(hopTimer >= 35 && hopCount == 0){
					hopTimer = 0;
					AISubstate = 0;
				}

				animFrame = 0;
			}else if(!npc.collideX){
				//Not colliding with any tiles horizontally
				int hc = hopCount;

				hopTimer = 0;
				hopCount = 0;

				//Update netcode if the hop count actually reset
				if(hc != hopCount)
					npc.netUpdate = true;
			}
		}

		public override void SendExtraAI(BinaryWriter writer){
			writer.Write((byte)AIState);
			writer.Write((byte)(Target?.whoAmI ?? -1));
			writer.Write((short)hopTimer);
			writer.Write((byte)hopCount);
			writer.Write((short)lookTimer);
			writer.Write((byte)animFrame);
			writer.Write((sbyte)faceDir);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			byte state = reader.ReadByte();
			byte plr = reader.ReadByte();

			if(state != State_LookingForPlayers && plr >= 0)
				Target = Main.player[plr];

			hopTimer = reader.ReadInt16();
			hopCount = reader.ReadByte();
			lookTimer = reader.ReadInt16();
			animFrame = reader.ReadByte();
			faceDir = reader.ReadSByte();
		}

		public override void FindFrame(int frameHeight){
			npc.frame.Y = animFrame * frameHeight;
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit){
			TargetPlayer(player);
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit){
			TargetPlayer(Main.player[projectile.owner]);
		}

		private void TargetPlayer(Player player){
			//Retaliate against that player immediately unless we're already chasing after one
			if(npc.target != -1 && AIState != State_LookingForPlayers)
				return;

			Target = player;
			npc.target = player.whoAmI;
			AIState = State_FoundPlayer_CanSee;
		}
	}
}
