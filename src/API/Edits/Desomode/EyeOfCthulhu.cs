using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Effects;
using Terraria.ID;
using static Terraria.ModLoader.PlayerDrawLayer;

namespace CosmivengeonMod.API.Edits.Desomode {
	public static partial class DesolationModeBossAI {
		/// <summary>
		/// Runs a modified AI for the Eye of Cthulhu.
		/// </summary>
		public static void AI_EyeOfCthulhu(NPC npc) {
			/*    NOTES
			 *  - ai[0]: which phase the Eye of Cthulhu is in
			 *  - ai[1]: phase progress
			 *  - ai[2]: global timer
			 *  - ai[3]: timer for minion spawns and charges?
			 *  
			 *  - localAI[] is unused
			 */

			/*	IDEA IDEA IDEA IDEA (basically fishron charge but cooler)
			 *	- new panic phase (<4% HP --> <20% HP)
			 *	- more darkness via shader (make boss glow slightly to account for this)
			 *	- boss fades out and slowly moves backwards: (-1 --> 0)
			 *	- minimap icon disappears while invisible
			 *	- spawn pillars of dust next to the player (70-tile spacing???)
			 *	- brighter dust starts clumping together in whatever row the boss will charge from (dust clump will lock vertically just before boss reappears): (0 --> 1)
			 *	- charges are always horizontal
			 *	- make the boss fade in and slowly moving backwards (1 --> 2) before charging (2 --> 0)
			 *	- prioritize the opposite of whatever direction the player is moving towards to prevent running away
			 *	- time between charges decreases as health decreases
			 *	- stop minion spawning
			 */

			//Force the boss to start in phase 2
			if (!npc.Helper().Flag) {
				npc.Helper().Flag = true;
				npc.ai[0] = 1f;
			}

			bool sendGlobalNPCData = false;

			bool onlyDoFastCharges = false;
			//This flag used to indicate the panic phase.  Let's override its use for the new attack instead!
			bool desomodePanicPhase = false;
			//Vanilla:
			//if(npc.life < npc.lifeMax * 0.12)
			if (npc.life < npc.lifeMax * 0.65f)
				onlyDoFastCharges = true;
			//Vanilla: does continuous chargest at <4% HP.  Desomode AI does a new attack instead at <20% HP
			if (npc.life < npc.lifeMax * 0.20f)
				desomodePanicPhase = true;

			//Begin the transition to phase 3
			if (desomodePanicPhase && !npc.Helper().Flag2) {
				npc.Helper().Flag2 = true;
				npc.ai[0] = 6f;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;
				npc.ai[3] = -1f;
				npc.Helper().EoC_PhaseTransitionVelocityLength = 7f;

				SoundEngine.PlaySound(SoundID.Roar, npc.position);
			}

			bool forceLockedRotation = npc.ai[0] == 6f && npc.ai[3] == 0f;
			bool forceStaticRotation = npc.ai[0] == 6f && npc.ai[3] == -1f;

			//Can't be bothered to give this a proper name
			float num4 = 16f;

			if (npc.target < 0 || npc.target == 255 || npc.Target().dead || !npc.Target().active) {
				npc.TargetClosest(true);
				npc.Helper().EoC_PlayerTargetMovementDirection = 0;
			}

			bool targetIsDead = npc.Target().dead;
			Vector2 from = npc.Bottom - new Vector2(0, 59);
			Vector2 to = npc.Target().Center;

			float rotationToTarget = (from - to).ToRotation();
			if (forceLockedRotation)
				rotationToTarget = npc.Helper().EoC_PlayerTargetMovementDirection == 1 ? 0 : MathHelper.Pi;
			rotationToTarget += MathHelper.PiOver2;

			//Track the target player's direction so the boss can use the info for phase 3
			if (!targetIsDead) {
				//If the player isn't moving, then update one of the timer array members at random
				//Otherwise, update the corresponding timer
				if (npc.Target().velocity.X == 0) {
					npc.Helper().EoC_PlayerTargetMovementTimers[Main.rand.Next(2)]++;

					sendGlobalNPCData = true;
				} else if (npc.Target().velocity.X < 0)
					npc.Helper().EoC_PlayerTargetMovementTimers[0]++;
				else
					npc.Helper().EoC_PlayerTargetMovementTimers[1]++;
			}

			if (rotationToTarget < 0f)
				rotationToTarget += MathHelper.TwoPi;
			else if (rotationToTarget > MathHelper.TwoPi)
				rotationToTarget -= MathHelper.TwoPi;

			float rotationChange = 0f;
			//Vanilla phase 1 is ignored entirely
			if (npc.ai[0] == 3f && npc.ai[1] == 0f)
				rotationChange = 0.05f;
			else if (npc.ai[0] == 3f && npc.ai[1] == 2f && npc.ai[2] > 40f)
				rotationChange = 0.08f;
			else if (npc.ai[0] == 3f && npc.ai[1] == 4f && npc.ai[2] > num4)
				rotationChange = 0.15f;
			else if (npc.ai[0] == 3f && npc.ai[1] == 5f)
				rotationChange = 0.05f;
			else if (npc.ai[0] == 6f) {
				//New desomode attack
				rotationChange = 0f;
			}

			rotationChange *= 1.5f;

			if (desomodePanicPhase)
				rotationChange = 0f;

			if (npc.rotation < rotationToTarget) {
				if (rotationToTarget - npc.rotation > MathHelper.Pi)
					npc.rotation -= rotationChange;
				else
					npc.rotation += rotationChange;
			} else if (npc.rotation > rotationToTarget) {
				if (npc.rotation - rotationToTarget > MathHelper.Pi)
					npc.rotation += rotationChange;
				else
					npc.rotation -= rotationChange;
			}

			//No rotations in desomode attack
			if (!forceLockedRotation && npc.rotation > rotationToTarget - rotationChange && npc.rotation < rotationToTarget + rotationChange)
				npc.rotation = rotationToTarget;

			if (npc.rotation < 0f)
				npc.rotation += MathHelper.TwoPi;
			else if (npc.rotation > MathHelper.TwoPi)
				npc.rotation -= MathHelper.TwoPi;

			if (!forceLockedRotation && npc.rotation > rotationToTarget - rotationChange && npc.rotation < rotationToTarget + rotationChange)
				npc.rotation = rotationToTarget;

			if (forceStaticRotation || forceLockedRotation)
				npc.rotation = rotationToTarget;

			if (Main.rand.NextBool(5)) {
				int num9 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, npc.height / 2, DustID.Blood, npc.velocity.X, 2f);
				Dust dust = Main.dust[num9];
				dust.velocity.X *= 0.5f;
				dust.velocity.Y *= 0.1f;
			}

			npc.reflectsProjectiles = false;

			// TODO 1.4.4:  Main.dayTime should be Main.IsItDay()
			if (Main.dayTime || targetIsDead) {
				npc.velocity.Y -= 0.04f;

				//Boss might be invisible from phase 3.  If so, force the boss back into visibility
				npc.alpha -= 10;
				if (npc.alpha < 0)
					npc.alpha = 0;

				if (npc.ai[0] == 6f)
					npc.rotation = rotationToTarget;

				npc.EncourageDespawn(10);
				return;
			}
			
			// NOTE: if (npc.ai[0] == 0f) block is removed due to it only applying to Phase 1 and desomode skips phase 1

			if (npc.ai[0] == 1f || npc.ai[0] == 2f) {
				//Do the transition to phase 2 (spawn gores and more minions)

				//Rotation acceleration increased from 0.005 to 0.0125
				if (npc.ai[0] == 1f) {
					npc.ai[2] += 0.0125f;

					if (npc.ai[2] > 0.5)
						npc.ai[2] = 0.5f;
				} else {
					npc.ai[2] -= 0.0125f;
					if (npc.ai[2] < 0f)
						npc.ai[2] = 0f;
				}

				npc.rotation += npc.ai[2];
				npc.ai[1] += 1f;

				if (Main.getGoodWorld)
					npc.reflectsProjectiles = true;

				//Modulo decreased from 20/10 to 10/5
				int mod = 10;
				// Vanilla: increased spawn rate happens at 33% HP
				// NOTE: Forced fast charges happen at 65% HP
				if (Main.getGoodWorld && npc.life < npc.lifeMax * 0.8f)
					mod = 5;

				if (npc.ai[1] % mod == 0f) {
					//Vanilla: Spawn extra minions during the transition to phase 2

					float num29 = 5f;
					Vector2 vector4 = npc.Center;
					Vector2 pos = new Vector2(Main.rand.Next(-200, 200), Main.rand.Next(-200, 200));

					if (Main.getGoodWorld)
						pos *= 3;

					float num32 = pos.Length();
					num32 = num29 / num32;

					Vector2 position2 = vector4;
					Vector2 vector5 = pos * num32;
					position2 += vector5 * 10f;

					EyeOfCthulhu_SpawnSummon(npc, position2, vector5, belchSound: false);
				}

				//Time shortened from 100 to 40
				if (npc.ai[1] == 40f) {
					npc.ai[0] += 1f;
					npc.ai[1] = 0f;

					if (npc.ai[0] == 3f)
						npc.ai[2] = 0f;
					else {
						SoundEngine.PlaySound(SoundID.NPCHit1, npc.position);

						//Spawn the mouth gores
						for (int num34 = 0; num34 < 2; num34++) {
							//No GoreID's for these?  smh
							Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 8);
							Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 7);
							Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 6);
						}

						for (int num35 = 0; num35 < 20; num35++)
							Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f);

						SoundEngine.PlaySound(SoundID.ForceRoar, npc.position);
					}
				}

				Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f);
				npc.velocity *= 0.98f;

				if (Math.Abs(npc.velocity.X) < 0.1f)
					npc.velocity.X = 0f;
				if (Math.Abs(npc.velocity.Y) < 0.1f)
					npc.velocity.Y = 0f;
			} else if (npc.ai[0] != 6f) {
				//Vanilla Phase 2: No iris and no longer spawns minions (desomode does keep spawning them though)
				//Not at the panic phase yet
				if (onlyDoFastCharges) {
					npc.defense = 15;
					npc.damage = npc.GetAttackDamage_ScaledByStrength(60);
				}

				if (npc.ai[1] == 0f && onlyDoFastCharges)
					npc.ai[1] = 5f;

				if (npc.ai[1] == 0f) {
					//Hovering near the player
					npc.Helper().Timer++;

					float speed = 6f;
					float acceleration = 0.07f;

					Vector2 diff = npc.Target().Center - new Vector2(0, 120) - npc.Center;

					float distance = diff.Length();

					if (distance > 400f) {
						speed += 1f;
						acceleration += 0.05f;

						if (distance > 600f) {
							speed += 1f;
							acceleration += 0.05f;

							if (distance > 800f) {
								speed += 1f;
								acceleration += 0.05f;
							}
						}
					}

					if (Main.getGoodWorld) {
						speed += 1f;
						acceleration += 0.1f;
					}

					distance = speed / distance;
					diff *= distance;

					EyeOfCthulhu_MovementAcceleration(npc, acceleration, diff);

					npc.ai[2] += 1f;

					if (npc.ai[2] >= 200f) {
						npc.ai[1] = 1f;
						npc.ai[2] = 0f;
						npc.ai[3] = 0f;

						if (npc.life < npc.lifeMax * 0.4)
							npc.ai[1] = 3f;

						npc.target = 255;
						npc.netUpdate = true;
					}
				} else if (npc.ai[1] == 1f) {
					//Charging
					SoundEngine.PlaySound(SoundID.ForceRoar, npc.position);

					npc.rotation = rotationToTarget;

					float chargeStrength = 7.5f;
					if (npc.ai[3] == 1f)
						chargeStrength *= 1.15f;
					if (npc.ai[3] == 2f)
						chargeStrength *= 1.3f;
					if (Main.getGoodWorld)
						chargeStrength *= 1.2f;

					Vector2 diff = npc.Target().Center - npc.Center;
					float num44 = diff.Length();
					num44 = chargeStrength / num44;
					npc.velocity = diff * num44;

					npc.ai[1] = 2f;

					npc.netUpdate = true;
					if (npc.netSpam > 10)
						npc.netSpam = 10;
				} else if (npc.ai[1] == 2f) {
					//Currently in a charge
					npc.ai[2] += 1f;

					if (npc.ai[2] >= 50f) {
						npc.velocity *= 0.97f;
						npc.velocity *= 0.98f;

						if (Math.Abs(npc.velocity.X) < 0.1f)
							npc.velocity.X = 0f;
						if (Math.Abs(npc.velocity.Y) < 0.1f)
							npc.velocity.Y = 0f;
					} else
						npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

					if (npc.ai[2] >= 90) {
						npc.ai[3] += 1f;
						npc.ai[2] = 0f;
						npc.target = 255;
						npc.rotation = rotationToTarget;

						if (npc.ai[3] >= 3f) {
							npc.ai[1] = 0f;
							npc.ai[3] = 0f;

							if (Main.netMode != NetmodeID.MultiplayerClient && npc.life < npc.lifeMax * 0.5f) {
								npc.ai[1] = 3f;
								npc.ai[3] += Main.rand.Next(1, 4);
							}

							npc.netUpdate = true;
							if (npc.netSpam > 10)
								npc.netSpam = 10;
						} else
							npc.ai[1] = 1f;
					}
				} else if (npc.ai[1] == 3f) {
					//REALLY FAST charge or Fast charge cooldown

					if (npc.ai[3] == 4f && onlyDoFastCharges && npc.Center.Y > npc.Target().Center.Y) {
						npc.TargetClosest(true);
						npc.ai[1] = 0f;
						npc.ai[2] = 0f;
						npc.ai[3] = 0f;
						npc.netUpdate = true;

						if (npc.netSpam > 10)
							npc.netSpam = 10;
					} else if (Main.netMode != NetmodeID.MultiplayerClient) {
						npc.TargetClosest(true);

						float chargeVelocity = 20f;

						Vector2 diff = npc.Target().Center - npc.Center;

						float num50 = Math.Abs(npc.Target().velocity.X) + Math.Abs(npc.Target().velocity.Y) / 4f;
						num50 += 10f - num50;

						if (num50 < 5f)
							num50 = 5f;
						else if (num50 > 15f)
							num50 = 15f;

						//Vanilla:
						//if(npc.ai[2] == -1f && !desomodePanicPhase){
						if (npc.ai[2] == -1f) {
							num50 *= 4f;
							chargeVelocity *= 1.3f;
						}

						diff -= npc.Target().velocity * num50 / new Vector2(1, 4);
						diff.X *= 1f + Main.rand.Next(-10, 11) * 0.01f;
						diff.Y *= 1f + Main.rand.Next(-10, 11) * 0.01f;

						float length = diff.Length();
						float origLength = length;

						length = chargeVelocity / length;
						npc.velocity = diff * length;

						npc.velocity += new Vector2(Main.rand.Next(-20, 21) * 0.1f, Main.rand.Next(-20, 21) * 0.1f);

						if (origLength < 100f) {
							if (Math.Abs(npc.velocity.X) > Math.Abs(npc.velocity.Y)) {
								float num55 = Math.Abs(npc.velocity.X);
								float num56 = Math.Abs(npc.velocity.Y);

								if (npc.Center.X > npc.Target().Center.X)
									num56 *= -1f;
								if (npc.Center.Y > npc.Target().Center.Y)
									num55 *= -1f;

								npc.velocity.X = num56;
								npc.velocity.Y = num55;
							}
						} else if (Math.Abs(npc.velocity.X) > Math.Abs(npc.velocity.Y)) {
							float num57 = (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y)) / 2f;
							float num58 = num57;

							if (npc.Center.X > npc.Target().Center.X)
								num58 *= -1f;
							if (npc.Center.Y > npc.Target().Center.Y)
								num57 *= -1f;

							npc.velocity.X = num58;
							npc.velocity.Y = num57;
						}

						npc.ai[1] = 4f;
						npc.netUpdate = true;

						if (npc.netSpam > 10)
							npc.netSpam = 10;
					}
				} else if (npc.ai[1] == 4f) {
					//Vanilla:
					//REALLY FAST charge cooldown
					npc.Helper().Timer++;

					if (npc.ai[2] == 0f)
						SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.position);

					npc.ai[2] += 1f;

					if (npc.ai[2] == num4 && Vector2.Distance(npc.position, npc.Target().position) < 200f)
						npc.ai[2]--;

					if (npc.ai[2] >= num4) {
						npc.velocity *= 0.95f;

						if (Math.Abs(npc.velocity.X) < 0.1f)
							npc.velocity.X = 0f;
						if (Math.Abs(npc.velocity.Y) < 0.1f)
							npc.velocity.Y = 0f;
					} else
						npc.rotation = npc.velocity.ToRotation() - MathHelper.PiOver2;

					if (npc.ai[2] >= num4 + 13f) {
						npc.netUpdate = true;
						if (npc.netSpam > 10)
							npc.netSpam = 10;

						npc.ai[3] += 1f;
						npc.ai[2] = 0f;

						if (npc.ai[3] >= 5f) {
							npc.ai[1] = 0f;
							npc.ai[3] = 0f;

							if (npc.target >= 0 && Main.getGoodWorld && Collision.CanHit(npc.position, npc.width, npc.height, npc.Target().position, npc.width, npc.height)) {
								SoundEngine.PlaySound(SoundID.ForceRoar, npc.position);
								npc.ai[0] = 2f;
								npc.ai[1] = 0f;
								npc.ai[2] = 0f;
								npc.ai[3] = 1f;
								npc.netUpdate = true;
							}
						} else
							npc.ai[1] = 3f;
					}
				} else if (npc.ai[1] == 5f) {
					//Pre-panic phase cooldown
					npc.Helper().Timer++;

					float num62 = 9f;
					float num63 = 0.3f;

					Vector2 diff = npc.Target().Center + new Vector2(0, 600f) - npc.Center;

					float num66 = diff.Length();
					num66 = num62 / num66;
					diff *= num66;
					
					EyeOfCthulhu_MovementAcceleration(npc, num63, diff);

					npc.ai[2] += 1f;

					if (npc.ai[2] >= 70f) {
						npc.TargetClosest(true);
						npc.ai[1] = 3f;
						npc.ai[2] = -1f;
						npc.ai[3] = Main.rand.Next(-3, 1);
						npc.netUpdate = true;
					}
				}
			} else {
				//Desomode attack subphase
				EyeOfCthulhu_SpawnBloodDustWalls(npc);

				float movementFactor = 8 * 16 / 60f;

				if (npc.ai[3] == -1f) {
					//Initial charge towards the player
					//Once the boss is close enough, it backs away suddenly (spooky!)
					//Then there's a brief moment while the player gets used to the new attack

					if (npc.ai[1] == 0f) {
						//Charge increasingly faster towards the player until the boss gets within 8 tiles
						Vector2 direction = npc.DirectionTo(npc.Target().Center);
						npc.velocity = direction * npc.Helper().EoC_PhaseTransitionVelocityLength;

						npc.Helper().EoC_PhaseTransitionVelocityLength *= 1.01f;

						if (npc.DistanceSQ(npc.Target().Center) < 256 * 8 * 8) {
							//Make the boss back off
							npc.ai[1] = 1f;
							npc.ai[2] = 45f;

							SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.position);

							npc.velocity = Vector2.Normalize(npc.velocity) * -10f;
						}
					} else if (npc.ai[1] == 1f) {
						npc.ai[2]--;

						npc.velocity *= 0.98f;

						//Slowly make the npc fade
						npc.alpha += 6;
						if (npc.alpha > 255)
							npc.alpha = 255;

						if (npc.ai[2] == 0f) {
							//Spawn the blood walls and wait a short amount of time
							npc.Helper().Flag3 = true;
							npc.dontTakeDamage = true;
							npc.ai[1] = 2f;
							npc.ai[2] = 120f;

							if (Main.netMode != NetmodeID.Server && !FilterCollection.Screen_EoC.Active) {
								Filters.Scene.Activate(FilterCollection.Name_Screen_EoC);
								npc.Helper().EoC_UsingShader = true;

								sendGlobalNPCData = true;
							}
						}
					} else if (npc.ai[1] == 2f) {
						npc.ai[2]--;

						//Shader hasn't already been fully processed
						if (Main.netMode != NetmodeID.Server && npc.Helper().EoC_UsingShader)
							FilterCollection.Screen_EoC.GetShader().UseProgress(1f - npc.ai[2] / 120f);

						if (npc.ai[2] == 0f) {
							//Timer has ended.  Start the actual attack!
							npc.ai[1] = 0f;
							npc.ai[2] = 30f;
							npc.ai[3] = 0f;
						}
					}
				} else if (npc.ai[3] == 0f) {
					//"This is where the fun begins"
					//Actual attack is here

					//BossCursor is nicer than BossChecklist.  I can just set this bool to false and have the icon not show up!
					//No need for annoying IL editing
					//Besides, who even uses that PDA function anyway :chaelure:
					npc.dontCountMe = true;

					if (npc.ai[1] == 0f) {
						//Determine the relative height to charge from and which direction
						npc.Helper().EoC_TargetHeight = Main.rand.Next(-150, 151);

						sendGlobalNPCData = true;

						npc.Helper().EoC_PlayerTargetMovementDirection = (npc.Helper().EoC_PlayerTargetMovementTimers[0] > npc.Helper().EoC_PlayerTargetMovementTimers[1]) ? -1 : 1;
						npc.Helper().EoC_PlayerTargetMovementTimers = new int[2];

						npc.ai[1] = 1f;

						npc.Helper().EoC_FadePositionOffset = Vector2.Zero;
					} else if (npc.ai[1] == 1f) {
						npc.dontTakeDamage = false;

						//Slowly fade the boss in
						int dir = npc.Helper().EoC_PlayerTargetMovementDirection;

						npc.alpha -= (int)(255f / npc.Helper().EoC_TimerTarget + 1);
						if (npc.alpha < 0)
							npc.alpha = 0;

						npc.ai[2]--;

						if (npc.ai[2] == 0f)
							npc.ai[1] = 2f;

						//Spawn extra dust around the boss's target height and side
						float targetX = npc.Target().Center.X + dir * 40 * 16;
						float targetY = npc.Target().Center.Y + npc.Helper().EoC_TargetHeight;

						targetX.Clamp(npc.width / 2f, Main.maxTilesX * 16 - npc.width / 2f);
						targetY.Clamp(npc.height / 2f, Main.maxTilesY * 16 - npc.height / 2f);

						Vector2 targetCenter = new Vector2(targetX, targetY);
						Vector2 innerTop = targetCenter - new Vector2(0, npc.height / 2f);
						Vector2 outerBottom = targetCenter + new Vector2(16 * dir, npc.height / 2f);

						Vector2 usePosition = dir == -1 ? new Vector2(outerBottom.X, innerTop.Y) : innerTop;
						int width = 16;
						int height = npc.height;
						for (int i = 0; i < 20; i++) {
							Dust dust = Dust.NewDustDirect(usePosition, width, height, DustID.Ichor, Scale: 1.5f);
							dust.noGravity = true;
						}

						npc.Helper().EoC_FadePositionOffset.X += dir * movementFactor;
						npc.Center = targetCenter + npc.Helper().EoC_FadePositionOffset;
					} else if (npc.ai[1] == 2f) {
						SoundEngine.PlaySound(SoundID.ForceRoarPitched, npc.position);
						int dir = -npc.Helper().EoC_PlayerTargetMovementDirection;

						float velocity = 24f * dir;
						if (npc.life < npc.lifeMax * 0.15f)
							velocity *= 1.1f;
						if (npc.life < npc.lifeMax * 0.1f)
							velocity *= 1.2f;
						if (npc.life < npc.lifeMax * 0.05f)
							velocity *= 1.3f;

						npc.velocity.X = velocity;
						npc.velocity.Y = 0;

						npc.ai[1] = 3f;
					} else if (npc.ai[1] == 3f) {
						float targetX = npc.Target().Center.X - npc.Helper().EoC_PlayerTargetMovementDirection * 30 * 16;
						targetX.Clamp(npc.width / 2f, Main.maxTilesX * 16 - npc.width / 2f);

						npc.ai[2]++;

						if ((npc.velocity.X > 0 && npc.Left.X >= targetX) || (npc.velocity.X < 0 && npc.Right.X <= targetX)) {
							npc.ai[1] = 4f;
							npc.ai[2] = 40f;
						}
					} else if (npc.ai[1] == 4f) {
						npc.dontTakeDamage = true;

						npc.ai[2]--;

						npc.alpha += 7;
						if (npc.alpha > 255)
							npc.alpha = 255;

						if (npc.ai[2] == 0f) {
							npc.ai[1] = 0f;

							int timer = 60;
							if (npc.life < npc.lifeMax * 0.15f)
								timer = 45;
							else if (npc.life < npc.lifeMax * 0.1f)
								timer = 30;
							else if (npc.life < npc.lifeMax * 0.05f)
								timer = 20;
							npc.ai[2] = npc.Helper().EoC_TimerTarget = timer;
						} else {
							//Slow the boss down as it fades away
							if (Math.Abs(npc.velocity.X) > 4f)
								npc.velocity.X = 4f * Math.Sign(npc.velocity.X);
							else
								npc.velocity.X *= 0.98f;
						}
					}
				}
			}

			//Don't spawn minions in the panic phase since that would look weird
			if (!desomodePanicPhase && npc.Helper().Timer > (onlyDoFastCharges ? 240 : 180)) {
				npc.Helper().Timer = 0;
				EyeOfCthulhu_SpawnSummon(npc, npc.Center, npc.DirectionTo(npc.Target().Center) * 3f);
			}

			if (sendGlobalNPCData)
				DetourNPCHelper.SendData(npc.whoAmI);
		}

		private static void EyeOfCthulhu_MovementAcceleration(NPC npc, float movementFactor, Vector2 maxVelocity) {
			if (npc.velocity.X < maxVelocity.X) {
				npc.velocity.X += movementFactor;
				if (npc.velocity.X < 0f && maxVelocity.X > 0f)
					npc.velocity.X += movementFactor;
			} else if (npc.velocity.X > maxVelocity.X) {
				npc.velocity.X -= movementFactor;
				if (npc.velocity.X > 0f && maxVelocity.X < 0f)
					npc.velocity.X -= movementFactor;
			}

			if (npc.velocity.Y < maxVelocity.Y) {
				npc.velocity.Y += movementFactor;
				if (npc.velocity.Y < 0f && maxVelocity.Y > 0f)
					npc.velocity.Y += movementFactor;
			} else if (npc.velocity.Y > maxVelocity.Y) {
				npc.velocity.Y -= movementFactor;
				if (npc.velocity.Y > 0f && maxVelocity.Y < 0f)
					npc.velocity.Y -= movementFactor;
			}
		}

		private static void EyeOfCthulhu_SpawnBloodDustWalls(NPC npc) {
			if (DetourNPCHelper.EoC_FirstBloodWallNPC == -1)
				DetourNPCHelper.EoC_FirstBloodWallNPC = npc.whoAmI;

			//If the boss should, spawn blood walls centered on the player
			if (npc.Helper().Flag3 && !npc.Target().dead && DetourNPCHelper.EoC_FirstBloodWallNPC == npc.whoAmI) {
				//Boss will spawn within 150 units above and below the player (boss center is clamped to this area)
				int total = 300 + npc.height;
				Vector2 topRight = npc.Target().Center - new Vector2(40 * 16, total / 2f);
				Vector2 bottomLeft = npc.Target().Center + new Vector2(40 * 16, total / 2f);
				//Clamp the positions to be within the map
				topRight.X.Clamp(npc.width / 2f, Main.maxTilesX * 16 - npc.width / 2f);
				topRight.Y.Clamp(npc.width / 2f, Main.maxTilesY * 16 - npc.width / 2f);
				bottomLeft.X.Clamp(npc.width / 2f, Main.maxTilesX * 16 - npc.width / 2f);
				bottomLeft.Y.Clamp(npc.width / 2f, Main.maxTilesY * 16 - npc.width / 2f);

				Point corner = topRight.ToTileCoordinates();
				Point otherCorner = bottomLeft.ToTileCoordinates();
				int origY = corner.Y;
				int dustsPerTile = 1;

				for (corner.Y = origY; corner.Y <= otherCorner.Y; corner.Y++) {
					//1 dust per tile
					Vector2 spawn = corner.ToWorldCoordinates(0, 0);
					for (int i = 0; i < dustsPerTile; i++) {
						Dust dust = Dust.NewDustDirect(spawn, 16, 16, DustID.Blood, Scale: 2f);
						dust.noGravity = true;
					}
				}
				corner.X = otherCorner.X;
				for (corner.Y = origY; corner.Y <= otherCorner.Y; corner.Y++) {
					//1 dust per tile
					Vector2 spawn = corner.ToWorldCoordinates(0, 0);
					for (int i = 0; i < dustsPerTile; i++) {
						Dust dust = Dust.NewDustDirect(spawn, 16, 16, DustID.Blood, Scale: 2f);
						dust.noGravity = true;
					}
				}
			}
		}

		private static void EyeOfCthulhu_SpawnSummon(NPC npc, Vector2 position, Vector2 velocity, bool belchSound = true) {
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				//Spawn the minion
				int spawn = NPC.NewNPC(npc.GetSource_FromAI(), (int)position.X, (int)position.Y, NPCID.ServantofCthulhu, Target: npc.target);
				Main.npc[spawn].velocity = velocity;
				if (Main.netMode == NetmodeID.Server && spawn < 200)
					NetMessage.SendData(MessageID.SyncNPC, number: spawn);
			}

			//Play the "belch" sound when a minion spawns
			if (belchSound)
				SoundEngine.PlaySound(SoundID.NPCDeath13, position);
			else
				SoundEngine.PlaySound(SoundID.NPCHit1, position);

			//And spawn some dust where the minion originated from
			for (int m = 0; m < 10; m++)
				Dust.NewDust(position, 20, 20, DustID.Blood, velocity.X * 0.4f, velocity.Y * 0.4f);
		}
	}
}
