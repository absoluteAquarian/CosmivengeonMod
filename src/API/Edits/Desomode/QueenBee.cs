using CosmivengeonMod.NPCs.Desomode;
using CosmivengeonMod.Projectiles.Desomode;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod.API.Edits.Desomode {
	public static partial class DesolationModeBossAI {
		/// <summary>
		/// Runs a modified AI for Queen Bee
		/// </summary>
		public static void AI_QueenBee(NPC npc) {
			/*  AI Notes:
			 *  ai[0] == -1 | Choosing an attack
			 *  ai[0] == 0  | Hovering around; Charge at the player if in line
			 *  ai[0] == 1  | Spwawning bees
			 *  ai[0] == 2  | Hovering around
			 *  ai[0] == 3  | Shooting stingers while moving
			 *  ai[0] == 4  | Charging at the player when really far away
			 *  ai[0] == 5  | Moving to the side, then despawning
			 */
			var helperData = npc.Helper();

			int nearbyPlayers = 0;
			for (int i = 0; i < Main.maxPlayers; i++) {
				Player plr = Main.player[i];
				if (plr.active && !plr.dead && npc.DistanceSQ(plr.Center) < 1000f * 1000f)
					nearbyPlayers++;
			}

			// Expert mode gradually increases QG's defense by 20
			// Desomode will gradually increase it by 30 instead!
			int num599 = (int)(30f * (1f - npc.life / (float)npc.lifeMax));
			npc.defense = npc.defDefense + num599;

			int timer2Max = 30;

			//Wait after getting enraged
			if (helperData.Timer2 > 0) {
				helperData.Timer2--;

				Vector2 oldSize = npc.Size;

				npc.scale += 0.3f / timer2Max;
				npc.Size = npc.Desomode().QB_baseSize * (1f + npc.scale - npc.Desomode().QB_baseScale);

				npc.position -= npc.Size - oldSize;

				return;
			}

			// Aura timer
			if (helperData.Flag)
				helperData.Timer++;

			// Aura check
			if (npc.life < npc.lifeMax * 0.25f && !helperData.Flag && npc.ai[0] != 0f) {
				helperData.Flag = true;

				SoundEngine.PlaySound(SoundID.ForceRoar, npc.Center);

				helperData.Timer2 = timer2Max;

				npc.ai[0] = 0f;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;

				return;
			}

			if (npc.target < 0 || npc.target == 255 || npc.Target().dead || !npc.Target().active)
				npc.TargetClosest();

			Player target = npc.Target();

			float environmentActionStrength = 0f;
			if (npc.position.Y / 16f < Main.worldSurface)
				environmentActionStrength += 1f;

			if (!target.ZoneJungle)
				environmentActionStrength += 1f;

			if (Main.getGoodWorld)
				environmentActionStrength += 0.5f;

			float nonDespawningDistance = Vector2.Distance(npc.Center, target.Center);
			if (npc.ai[0] != 5f) {
				if (npc.timeLeft < 60)
					npc.timeLeft = 60;

				if (nonDespawningDistance > 3000f) {
					npc.ai[0] = 4f;
					npc.netUpdate = true;
				}
			}

			if (target.dead) {
				npc.ai[0] = 5f;
				npc.netUpdate = true;
			}

			if (npc.ai[0] == 5f) {
				npc.velocity.Y *= 0.98f;
				if (npc.velocity.X < 0f)
					npc.direction = -1;
				else
					npc.direction = 1;

				npc.spriteDirection = npc.direction;
				if (npc.position.X < Main.maxTilesX * 8) {
					if (npc.velocity.X > 0f)
						npc.velocity.X *= 0.98f;
					else
						npc.localAI[0] = 1f;

					npc.velocity.X -= 0.08f;
				}
				else {
					if (npc.velocity.X < 0f)
						npc.velocity.X *= 0.98f;
					else
						npc.localAI[0] = 1f;

					npc.velocity.X += 0.08f;
				}

				npc.EncourageDespawn(10);
			} else if (npc.ai[0] == -1f) {
				if (Main.netMode == NetmodeID.MultiplayerClient)
					return;

				float currentAttack = npc.ai[1];
				int nextAttack;
				do {
					nextAttack = Main.rand.Next(3);
					switch (nextAttack) {
						case 1:
							nextAttack = 2;
							break;
						case 2:
							nextAttack = 3;
							break;
					}
				} while (nextAttack == currentAttack);

				npc.ai[0] = nextAttack;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;
			} else if (npc.ai[0] == 0f) {
				int duration = 2;
				if (npc.life < npc.lifeMax / 2)
					duration++;
				if (npc.life < npc.lifeMax / 3)
					duration++;
				if (npc.life < npc.lifeMax / 5)
					duration++;

				duration += (int)environmentActionStrength;

				if (npc.ai[1] > 2 * duration && npc.ai[1] % 2f == 0f) {
					// Choose another attack
					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}

				if (npc.ai[1] % 2f == 0f) {
					npc.TargetClosest();

					helperData.Timer3++;

					// Expert: Charge only if the boss's Y-position is within 20 pixels (1.25 tiles) of the player's Y-position
					// Desomode: The same, but the range is increased within 6 tiles of the player AND the boss is at least 20 tiles away from the player
					float rangeY = (6f + environmentActionStrength) * 16f;
					if (helperData.Timer3 > 90 || (Math.Abs(npc.Center.Y - target.Center.Y) < rangeY && Math.Abs(npc.Center.X - target.Center.X) > 20f * 16)) {
						helperData.Timer3 = 0;

						npc.localAI[0] = 1f;
						npc.ai[1] += 1f;
						npc.ai[2] = 0f;

						// Setting the hover speed
						// Vanilla increases it by 2 per check.  Desomode increases it by 3 instead
						float chargeStrength = 16f;
						if (npc.life < npc.lifeMax * 0.75)
							chargeStrength += 3f;
						if (npc.life < npc.lifeMax * 0.5)
							chargeStrength += 3f;
						if (npc.life < npc.lifeMax * 0.25)
							chargeStrength += 3f;
						if (npc.life < npc.lifeMax * 0.1)
							chargeStrength += 3f;

						npc.velocity = npc.DirectionTo(target.Center) * chargeStrength;

						npc.spriteDirection = npc.direction;

						SoundEngine.PlaySound(helperData.Flag ? SoundID.ForceRoarPitched : SoundID.ForceRoar, npc.position);

						return;
					}

					npc.localAI[0] = 0f;

					// Ascend/descend faster depending on how much health the boss has left
					float velocityCap = 12f;
					float acceleration = 0.2f;
					if (npc.life < npc.lifeMax * 0.75) {
						velocityCap += 1f;
						acceleration += 0.1f;
					}
					if (npc.life < npc.lifeMax * 0.5) {
						velocityCap += 1f;
						acceleration += 0.1f;
					}
					if (npc.life < npc.lifeMax * 0.25) {
						velocityCap += 2f;
						acceleration += 0.1f;
					}
					if (npc.life < npc.lifeMax * 0.1) {
						velocityCap += 2f;
						acceleration += 0.2f;
					}

					velocityCap += 3f * environmentActionStrength;
					acceleration += 0.5f * environmentActionStrength;

					// Move up or down depending on where the NPC is relative to the player
					if (npc.position.Y + npc.height / 2 < target.position.Y + target.height / 2)
						npc.velocity.Y += acceleration;
					else
						npc.velocity.Y -= acceleration;

					if (npc.velocity.Y < -velocityCap)
						npc.velocity.Y = -velocityCap;

					if (npc.velocity.Y > velocityCap)
						npc.velocity.Y = velocityCap;

					if (Math.Abs(npc.position.X + npc.width / 2 - (target.position.X + target.width / 2)) > 600f)
						npc.velocity.X += 0.15f * npc.direction;
					else if (Math.Abs(npc.position.X + npc.width / 2 - (target.position.X + target.width / 2)) < 300f)
						npc.velocity.X -= 0.15f * npc.direction;
					else
						npc.velocity.X *= 0.8f;

					if (npc.velocity.X < -16f)
						npc.velocity.X = -16f;

					if (npc.velocity.X > 16f)
						npc.velocity.X = 16f;

					npc.spriteDirection = npc.direction;
					return;
				}

				if (npc.velocity.X < 0f)
					npc.direction = -1;
				else
					npc.direction = 1;

				npc.spriteDirection = npc.direction;
				int maxDistance = 600;
				if (npc.life < npc.lifeMax * 0.1)
					maxDistance = 300;
				else if (npc.life < npc.lifeMax * 0.25)
					maxDistance = 450;
				else if (npc.life < npc.lifeMax * 0.5)
					maxDistance = 500;
				else if (npc.life < npc.lifeMax * 0.75)
					maxDistance = 550;

				int expectedDirection = 1;
				if (npc.Center.X < target.Center.X)
					expectedDirection = -1;

				maxDistance -= (int)(100 * environmentActionStrength);

				if ((npc.direction == expectedDirection && Math.Abs(npc.Center.X - target.Center.X) > maxDistance) || Math.Abs(npc.Center.Y - target.Center.Y) > maxDistance * 1.5f) {
					npc.ai[2] = 1f;

					if (environmentActionStrength > 0)
						npc.velocity *= 0.5f;
				}

				if (npc.ai[2] == 1f) {
					npc.TargetClosest();
					npc.spriteDirection = npc.direction;
					npc.localAI[0] = 0f;
					npc.velocity *= 0.9f;

					float num611 = 0.1f;
					if (npc.life < npc.lifeMax / 2) {
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if (npc.life < npc.lifeMax / 3) {
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if (npc.life < npc.lifeMax / 5) {
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if (environmentActionStrength > 0)
						npc.velocity *= 0.7f;

					if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num611) {
						npc.ai[2] = 0f;
						npc.ai[1] += 1f;
						npc.netUpdate = true;
					}
				} else
					npc.localAI[0] = 1f;
			} else if (npc.ai[0] == 2f) {
				npc.TargetClosest();
				npc.spriteDirection = npc.direction;
				float num613 = 0.1f;

				Vector2 npcCenter = npc.Center;
				float distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
				float distanceToTargetY = target.position.Y + target.height / 2 - 200f - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
				if (distanceToTarget < 200f) {
					npc.ai[0] = 1f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
					return;
				}

				if (npc.velocity.X < distanceToTargetX) {
					npc.velocity.X += num613;
					if (npc.velocity.X < 0f && distanceToTargetX > 0f)
						npc.velocity.X += num613;
				} else if (npc.velocity.X > distanceToTargetX) {
					npc.velocity.X -= num613;
					if (npc.velocity.X > 0f && distanceToTargetX < 0f)
						npc.velocity.X -= num613;
				}

				if (npc.velocity.Y < distanceToTargetY) {
					npc.velocity.Y += num613;
					if (npc.velocity.Y < 0f && distanceToTargetY > 0f)
						npc.velocity.Y += num613;
				} else if (npc.velocity.Y > distanceToTargetY) {
					npc.velocity.Y -= num613;
					if (npc.velocity.Y > 0f && distanceToTargetY < 0f)
						npc.velocity.Y -= num613;
				}
			} else if (npc.ai[0] == 1f) {
				npc.localAI[0] = 0f;
				npc.TargetClosest();

				Vector2 beeSpawnPosition = new Vector2(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);
				Vector2 npcCenter = npc.Center;
				float distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
				float distanceToTargetY = target.position.Y + target.height / 2 - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);

				npc.ai[1] += 1f;

				npc.ai[1] += nearbyPlayers / 2;
				if (npc.life < npc.lifeMax * 0.75)
					npc.ai[1] += 0.25f;

				if (npc.life < npc.lifeMax * 0.5)
					npc.ai[1] += 0.25f;

				if (npc.life < npc.lifeMax * 0.25)
					npc.ai[1] += 0.25f;

				if (npc.life < npc.lifeMax * 0.1)
					npc.ai[1] += 0.25f;

				bool canSpawnBee = false;
				// Vanila uses "40 - 18 * str"
				int maxAI = (int)(30f - 12f * environmentActionStrength);
				if (npc.ai[1] > maxAI) {
					npc.ai[1] = 0f;
					npc.ai[2]++;
					canSpawnBee = true;
				}

				// Spawn the bees/hornets
				// Boss ignores any tile collision checks
				if (canSpawnBee) {
					SoundEngine.PlaySound(SoundID.NPCHit1, npc.position);

					if (Main.netMode != NetmodeID.MultiplayerClient) {
						WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
						wRand.Add(ModContent.NPCType<ModifiedHornet>(), 0.18);
						wRand.Add(NPCID.Bee, 0.82 / 2);
						wRand.Add(NPCID.BeeSmall, 0.82 / 2);

						NPC bee = NPC.NewNPCDirect(npc.GetSource_FromAI(), (int)beeSpawnPosition.X, (int)beeSpawnPosition.Y, wRand.Get());
						bee.velocity = npc.DirectionTo(target.Center).RotateDegrees(rotateByDegrees: 0, rotateByRandomDegrees: 30) * Main.rand.NextFloat(3f, 6f);
						bee.netUpdate = true;
						bee.localAI[0] = 60f;
						bee.noTileCollide = true;
						// TODO 1.4.4
						// bee.CanBeReplacedByOtherNPCs = true;
					}
				}

				//Move towards the player if they're too far away or the boss can't see them
				if (distanceToTarget > 400f || !Collision.CanHit(new Vector2(beeSpawnPosition.X, beeSpawnPosition.Y - 30f), 1, 1, target.position, target.width, target.height)) {
					float acceleration = 0.1f;
					npcCenter = beeSpawnPosition;
					distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
					distanceToTargetY = target.position.Y + target.height / 2 - npcCenter.Y;

					if (npc.velocity.X < distanceToTargetX) {
						npc.velocity.X += acceleration;
						if (npc.velocity.X < 0f && distanceToTargetX > 0f)
							npc.velocity.X += acceleration;
					} else if (npc.velocity.X > distanceToTargetX) {
						npc.velocity.X -= acceleration;
						if (npc.velocity.X > 0f && distanceToTargetX < 0f)
							npc.velocity.X -= acceleration;
					}

					if (npc.velocity.Y < distanceToTargetY) {
						npc.velocity.Y += acceleration;
						if (npc.velocity.Y < 0f && distanceToTargetY > 0f)
							npc.velocity.Y += acceleration;
					} else if (npc.velocity.Y > distanceToTargetY) {
						npc.velocity.Y -= acceleration;
						if (npc.velocity.Y > 0f && distanceToTargetY < 0f)
							npc.velocity.Y -= acceleration;
					}
				} else
					npc.velocity *= 0.9f;

				npc.spriteDirection = npc.direction;
				if (npc.ai[2] > 5f) {
					// Choose another attack
					npc.ai[0] = -1f;
					npc.ai[1] = 1f;
					npc.netUpdate = true;
				}
			} else if (npc.ai[0] == 3f) {
				float acceleration = 0.075f + 0.2f * environmentActionStrength;

				Vector2 spawnOrigin = new Vector2(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);
				Vector2 npcCenter = npc.Center;
				float distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
				float distanceToTargetY = target.position.Y + target.height / 2 - 300f - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);

				npc.ai[1] += 1f;

				// Desomode change: boss shoots stingers at the next-fastest rate until enraged, then shoots at the fastest rate
				int rate = helperData.Flag ? 15 : 25;
				rate -= (int)(5 * environmentActionStrength);

				// Spawn stingers
				// Boss ignores any tile collision checks
				if (npc.ai[1] % rate == rate - 1 && npc.position.Y + npc.height < target.position.Y) {
					SoundEngine.PlaySound(SoundID.Item17, npc.position);
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						float stingerVelocity = 10f;

						if (npc.life < npc.lifeMax * 0.1f)
							stingerVelocity += 3f;

						stingerVelocity += 7f * environmentActionStrength;

						int varianceX = (int)(80 - 39 * environmentActionStrength);
						int varianceY = (int)(40 - 19 * environmentActionStrength);

						if (varianceX < 1)
							varianceX = 1;
						if (varianceY < 1)
							varianceY = 1;

						Vector2 shootTarget = target.Center - spawnOrigin + new Vector2(Main.rand.Next(-varianceX, varianceX + 1), Main.rand.Next(-varianceY, varianceY + 1));
						Vector2 targetVelocity = target.velocity * 60 * 0.1f;
						// Make the stingers aim slightly towards where the player will probably be in ~0.1 seconds
						shootTarget += targetVelocity;
						shootTarget = Vector2.Normalize(shootTarget) * stingerVelocity;

						int num633 = MiscUtils.TrueDamage(Main.masterMode ? 120 : 80);
						int num634 = ProjectileID.Stinger;
						if (Main.rand.NextFloat() < 0.085f) {
							// Recalculate the velocity without the randomness
							shootTarget = target.position - spawnOrigin;
							shootTarget += targetVelocity;
							shootTarget = Vector2.Normalize(shootTarget) * stingerVelocity;

							num634 = ModContent.ProjectileType<QueenBeeHoneyShot>();
							shootTarget *= 1.8f;
							num633 = MiscUtils.TrueDamage(Main.masterMode ? 200 : 150);

							npc.ai[1] += 9f;
						}

						int num635 = Projectile.NewProjectile(npc.GetSource_FromAI(), spawnOrigin, shootTarget, num634, num633, 0f, Main.myPlayer);
						Main.projectile[num635].timeLeft = 300;
						Main.projectile[num635].tileCollide = false;
					}
				}

				if (!Collision.CanHit(new Vector2(spawnOrigin.X, spawnOrigin.Y - 30f), 1, 1, target.position, target.width, target.height)) {
					acceleration = 0.1f;
					if (environmentActionStrength > 0)
						acceleration = 0.5f;

					if (npc.velocity.X < distanceToTargetX) {
						npc.velocity.X += acceleration;
						if (npc.velocity.X < 0f && distanceToTargetX > 0f)
							npc.velocity.X += acceleration;
					} else if (npc.velocity.X > distanceToTargetX) {
						npc.velocity.X -= acceleration;
						if (npc.velocity.X > 0f && distanceToTargetX < 0f)
							npc.velocity.X -= acceleration;
					}

					if (npc.velocity.Y < distanceToTargetY) {
						npc.velocity.Y += acceleration;
						if (npc.velocity.Y < 0f && distanceToTargetY > 0f)
							npc.velocity.Y += acceleration;
					} else if (npc.velocity.Y > distanceToTargetY) {
						npc.velocity.Y -= acceleration;
						if (npc.velocity.Y > 0f && distanceToTargetY < 0f)
							npc.velocity.Y -= acceleration;
					}
				} else if (distanceToTarget > 100f) {
					npc.TargetClosest();
					npc.spriteDirection = npc.direction;
					if (npc.velocity.X < distanceToTargetX) {
						npc.velocity.X += acceleration;
						if (npc.velocity.X < 0f && distanceToTargetX > 0f)
							npc.velocity.X += acceleration * 2f;
					} else if (npc.velocity.X > distanceToTargetX) {
						npc.velocity.X -= acceleration;
						if (npc.velocity.X > 0f && distanceToTargetX < 0f)
							npc.velocity.X -= acceleration * 2f;
					}

					if (npc.velocity.Y < distanceToTargetY) {
						npc.velocity.Y += acceleration;
						if (npc.velocity.Y < 0f && distanceToTargetY > 0f)
							npc.velocity.Y += acceleration * 2f;
					} else if (npc.velocity.Y > distanceToTargetY) {
						npc.velocity.Y -= acceleration;
						if (npc.velocity.Y > 0f && distanceToTargetY < 0f)
							npc.velocity.Y -= acceleration * 2f;
					}
				}

				float time = 20f - 5 * environmentActionStrength;
				if (npc.ai[1] > rate * time) {
					// Choose another attack
					npc.ai[0] = -1f;
					npc.ai[1] = 3f;
					npc.netUpdate = true;
				}
			} else if (npc.ai[0] == 4f) {
				npc.localAI[0] = 1f;
				float chargeStrength = 14f;
				Vector2 distanceToTarget = target.Center - npc.Center;
				distanceToTarget.Normalize();
				distanceToTarget *= chargeStrength;
				npc.velocity = (npc.velocity * chargeStrength + distanceToTarget) / (chargeStrength + 1f);
				if (npc.velocity.X < 0f)
					npc.direction = -1;
				else
					npc.direction = 1;

				npc.spriteDirection = npc.direction;

				// Boss is close enough.  Go back to normal AI
				if (nonDespawningDistance < 2000f) {
					npc.ai[0] = -1f;
					npc.localAI[0] = 0f;
				}
			}
		}
	}
}
