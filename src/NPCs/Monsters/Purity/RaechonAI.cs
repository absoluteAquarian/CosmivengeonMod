using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Monsters.Purity {
	public abstract class RaechonAI : ModNPC {
		public abstract float JumpStrength { get; }
		public abstract float GetWalkSpeed(bool cantSee, bool noTarget);

		public ref float AIState => ref NPC.ai[0];
		public ref float AITimer => ref NPC.ai[1];
		public ref float AITimerTarget => ref NPC.ai[2];
		public ref float AISubstate => ref NPC.ai[3];

		private int hopTimer;
		private int hopCount;
		private int lookTimer;

		public const int State_LookingForPlayers = 1;
		public const int State_FoundPlayer_NoSee = 2;
		public const int State_FoundPlayer_CanSee = 3;

		public Player Target;

		public bool InvalidTarget => Target is null || !Target.active || Target.dead || !NPC.WithinDistance(Target.Center, 40 * 16);

		private int animFrame;

		private float vel_targetX = 100;

		private int faceDir;

		private Vector2[] CoordVectors => new Vector2[] {
			NPC.TopLeft    + new Vector2(-8),     NPC.Top    + new Vector2(0, -8), NPC.TopRight    + new Vector2(8, -8),
			NPC.Left       + new Vector2(-8, 0),  NPC.Center,                      NPC.Right       + new Vector2(8, 0),
			NPC.BottomLeft + new Vector2(-8, 8),  NPC.Bottom + new Vector2(0, 8),  NPC.BottomRight + new Vector2(8)
		};

		public bool CanSee(Player plr) => CoordVectors.Any(v => Collision.CanHit(v - new Vector2(1), 2, 2, plr.Center - new Vector2(1), 2, 2));

		public override void AI() {
			//AI states:
			//1: cannot see any players and doesn't have one as a target, walking around and being idle
			//2: has a target player but can't see them
			//3: has a target player and can see them
			AITimer++;

			if (NPC.collideX) {
				NPC.velocity.X = 0;
				NPC.position.X = NPC.oldPosition.X;
			}

			//Make the animation move faster if the NPC is moving faster
			if (NPC.velocity.X != 0)
				NPC.frameCounter += Math.Max(3.5 * Math.Abs(NPC.velocity.X) / vel_targetX, 0.8);
			else {
				NPC.frameCounter = 0;
				animFrame = 0;
			}

			if (NPC.frameCounter > 10) {
				NPC.frameCounter = 0;
				animFrame = ++animFrame % Main.npcFrameCount[NPC.type];
			}

			//On the first tick, set the animation states properly
			if (AIState == 0) {
				AIState = State_LookingForPlayers;
				AISubstate = 2;
				AITimer = 0;

				faceDir = Main.rand.Next(new int[] { -1, 1 });
				NPC.velocity.X = Main.rand.NextFloat(-3f, 3.00001f);

				NPC.netUpdate = true;
			}

			NPC.SmoothStep(ref NPC.position);

			if (AIState == State_LookingForPlayers) {
				//Attempt to find a player target
				//If one is found, set it and update the netcode
				Target = null;
				NPC.target = -1;
				for (int i = 0; i < Main.maxPlayers; i++) {
					Player plr = Main.player[i];

					if (!plr.active || plr.dead || !NPC.WithinDistance(plr.Center, 40 * 16))
						continue;

					//Check if no blocks are in the way
					//If there aren't any, then target this player
					if (CanSee(plr)) {
						Target = plr;
						NPC.target = plr.whoAmI;
						AIState = State_FoundPlayer_CanSee;
						//Inform netcode
						NPC.netUpdate = true;
						break;
					}
				}

				if (NPC.velocity.X > 0)
					faceDir = 1;
				else if (NPC.velocity.X < 0)
					faceDir = -1;

				float walk = GetWalkSpeed(true, true);

				Do_AIState_0_Movement(walk / 0.6667f, walk, walk * 2.1f, true);
			} else if (AIState == State_FoundPlayer_CanSee) {
				//If we can no longer see the player, move on to AI state 1 or 2
				if (InvalidTarget) {
					AIState = State_LookingForPlayers;
					Target = null;
					NPC.target = -1;
					return;
				}
				if (Target != null && !CanSee(Target)) {
					AIState = State_FoundPlayer_NoSee;
					return;
				}

				//We can see the player, try to move towards them
				faceDir = (Target.Center.X >= NPC.Center.X).ToDirectionInt();

				float walk = GetWalkSpeed(false, false);

				//Basically a copy of AIState 0's movement, but without the waiting and with faster movement
				Do_AIState_0_Movement(walk / 0.45f, walk, walk, false);
			} else if (AIState == State_FoundPlayer_NoSee) {
				if (Target != null && CanSee(Target)) {
					AIState = State_FoundPlayer_CanSee;
					return;
				}

				if (NPC.velocity.X > 0)
					faceDir = 1;
				else if (NPC.velocity.X < 0)
					faceDir = -1;

				float walk = GetWalkSpeed(true, false);

				//Basically a copy of AIState 0's movement, but without the waiting and with faster movement
				Do_AIState_0_Movement(walk / 0.45f, walk, walk * 0.6f, false);

				lookTimer++;
				if (lookTimer > 5 * 60) {
					AIState = State_LookingForPlayers;
					Reset();
				}
			}

			//Gravity
			NPC.velocity.Y += 6.35f / 60f;

			NPC.spriteDirection = faceDir;
		}

		private void Reset() {
			AITimer = 0;
			AITimerTarget = 0;
			lookTimer = 0;

			Target = null;
			NPC.target = -1;

			NPC.netUpdate = true;
		}

		private void Do_AIState_0_Movement(float acceleration, float velCap, float anim_target, bool doWait) {
			//Cycles between several substates:
			//0: Reset wait timer and turn around
			//1: Wait until timer has reached the random value from substate 0; apply friction
			//2: Reset wait timer
			//3: Try to move forwards until the timer has reached the random value from substate 2
			if (!doWait && AISubstate == 0)
				AISubstate = 2;

			vel_targetX = anim_target;

			if (AISubstate == 0) {
				AITimer = 0;
				if (doWait)
					AITimerTarget = Main.rand.Next(75, 161);
				else
					AITimerTarget = 0;

				AISubstate = 1;
				NPC.netUpdate = true;
			} else if (AISubstate == 1) {
				//Friction
				NPC.velocity.X *= 1f - 2.635f / 60f;

				if (NPC.velocity.Y != 0)
					AITimer--;
				else if (Math.Abs(NPC.velocity.X) < 0.025f)
					NPC.velocity.X = 0;

				if (AITimer >= AITimerTarget) {
					//Turn around
					faceDir *= -1;
					NPC.velocity.X = 0;

					AISubstate = 2;
					NPC.netUpdate = true;
				}
			} else if (AISubstate == 2) {
				AITimer = 0;
				AISubstate = 3;
				NPC.netUpdate = true;
				AITimerTarget = Main.rand.Next(170, 280);
			} else if (AISubstate == 3) {
				if (NPC.velocity.Y == 0) {
					NPC.velocity.X += faceDir * acceleration / 60f;

					NPC.velocity.X.Clamp(-velCap, velCap);
				}

				if (AITimer >= AITimerTarget) {
					AISubstate = 0;
					NPC.netUpdate = true;
				}

				TryHop();
			}
		}

		private void TryHop() {
			//If we've hit a wall, start a second timer
			//If that timer reaches 35 ticks, do a random amount of small hops (crab be stubby though)
			if (NPC.collideX && NPC.velocity.Y == 0) {
				hopTimer++;

				if (hopTimer == 35) {
					hopCount = Main.rand.Next(1, 4);
					NPC.netUpdate = true;
				}

				if (hopTimer >= 35 && hopCount > 0) {
					//Offset the NPC's position to be slightly away from the wall
					NPC.position.X -= NPC.spriteDirection * 4;

					NPC.velocity.Y = JumpStrength;
					hopCount--;
				} else if (hopTimer >= 35 && hopCount == 0) {
					hopTimer = 0;
					AISubstate = 0;
				}

				animFrame = 0;
			} else if (!NPC.collideX) {
				//Not colliding with any tiles horizontally
				int hc = hopCount;

				hopTimer = 0;
				hopCount = 0;

				//Update netcode if the hop count actually reset
				if (hc != hopCount)
					NPC.netUpdate = true;
			}
		}

		public override void SendExtraAI(BinaryWriter writer) {
			writer.Write((byte)AIState);
			writer.Write((byte)(Target?.whoAmI ?? -1));
			writer.Write((short)hopTimer);
			writer.Write((byte)hopCount);
			writer.Write((short)lookTimer);
			writer.Write((byte)animFrame);
			writer.Write((sbyte)faceDir);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			byte state = reader.ReadByte();
			byte plr = reader.ReadByte();

			if (state != State_LookingForPlayers && plr >= 0)
				Target = Main.player[plr];

			hopTimer = reader.ReadInt16();
			hopCount = reader.ReadByte();
			lookTimer = reader.ReadInt16();
			animFrame = reader.ReadByte();
			faceDir = reader.ReadSByte();
		}

		public override void FindFrame(int frameHeight) {
			NPC.frame.Y = animFrame * frameHeight;
		}

		public override void OnHitByItem(Player player, Item item, int damage, float knockback, bool crit) {
			TargetPlayer(player);
		}

		public override void OnHitByProjectile(Projectile projectile, int damage, float knockback, bool crit) {
			TargetPlayer(Main.player[projectile.owner]);
		}

		private void TargetPlayer(Player player) {
			//Retaliate against that player immediately unless we're already chasing after one
			if (NPC.target != -1 && AIState != State_LookingForPlayers)
				return;

			Target = player;
			NPC.target = player.whoAmI;
			AIState = State_FoundPlayer_CanSee;
		}
	}
}
