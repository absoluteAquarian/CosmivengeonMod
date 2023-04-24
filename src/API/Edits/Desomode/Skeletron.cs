using CosmivengeonMod.Projectiles.Desomode;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits.Desomode {
	public static partial class DesolationModeBossAI {
		/// <summary>
		/// Runs a modified AI for Skeletron
		/// </summary>
		public static void AI_SkeletronHead(NPC npc) {
			/*   AI Notes:
			 *   ai[0] == 0 | Hasn't spawned "these hands"
			 *   ai[0] == 1 | Has spawned "these hands"
			 *   ai[0] == 2 | Has spawned the second set of "these hands"
			 *   
			 *   ai[1] == 0 | Try to hover above the player
			 *              | Lasts for 800 ticks
			 *   ai[1] == 1 | Charge at the player while spinning
			 *              | Lasts for 300 ticks
			 *   ai[1] == 2 | Dungeon Guardian mode
			 *   ai[1] == 3 | Falling down towards hell since there's nothing left to target
			 *   
			 *   ai[2]      | Timer
			 *   
			 *   ai[3]      | Unused
			 */
			var helperData = npc.Helper();

			npc.reflectsProjectiles = false;
			npc.defense = npc.defDefense;

			//Spawn these hands
			if ((npc.ai[0] == 0f || (npc.ai[0] == 1f && npc.life < npc.lifeMax * 0.25f && npc.ai[1] == 0f)) && Main.netMode != NetmodeID.MultiplayerClient) {
				npc.TargetClosest();
				npc.ai[0]++;

				if (npc.ai[0] == 2f)
					SoundEngine.PlaySound(SoundID.Roar, npc.Center);

				NPC hand = NPC.NewNPCDirect(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)npc.position.Y + npc.height / 2, NPCID.SkeletronHand, npc.whoAmI);
				hand.ai[0] = -1f;
				hand.ai[1] = npc.whoAmI;
				hand.target = npc.target;
				hand.netUpdate = true;
				hand.Helper().Timer = Main.rand.Next(30, 51);

				DetourNPCHelper.SendData(hand.whoAmI);

				if (npc.ai[0] == 2f)
					hand.life /= 3;

				hand = NPC.NewNPCDirect(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)npc.position.Y + npc.height / 2, NPCID.SkeletronHand, npc.whoAmI);
				hand.ai[0] = 1f;
				hand.ai[1] = npc.whoAmI;
				hand.ai[3] = 150f;
				hand.target = npc.target;
				hand.netUpdate = true;
				hand.Helper().Timer = Main.rand.Next(30, 51);

				if (npc.ai[0] == 2f)
					hand.life /= 3;

				DetourNPCHelper.SendData(hand.whoAmI);
			}

			if (Main.netMode == NetmodeID.MultiplayerClient && npc.localAI[0] == 0f) {
				npc.localAI[0] = 1f;
				SoundEngine.PlaySound(SoundID.Roar, npc.position);
			}

			//Target is dead or too far away
			Player target = npc.Target();

			if (target.dead || Math.Abs(npc.position.X - target.position.X) > 2000f || Math.Abs(npc.position.Y - target.position.Y) > 2000f) {
				npc.TargetClosest();

				target = npc.Target();

				if (target.dead || Math.Abs(npc.position.X - target.position.X) > 2000f || Math.Abs(npc.position.Y - target.position.Y) > 2000f)
					npc.ai[1] = 3f;
			}

			//It's daytime and the boss isn't already spinning
			//Get A N G E R Y
			// TODO 1.4.4: Main.dayTime should be Main.IsItDay()
			if (Main.dayTime && npc.ai[1] != 3f && npc.ai[1] != 2f) {
				npc.ai[1] = 2f;
				SoundEngine.PlaySound(SoundID.Roar, npc.position);
			}

			int activeHands = 0;
			for (int i = 0; i < 200; i++) {
				if (Main.npc[i].active && Main.npc[i].type == npc.type + 1)
					activeHands++;
			}

			npc.defense += activeHands * 25;
			if ((activeHands < 2 || npc.life < npc.lifeMax * 0.75) && npc.ai[1] == 0f) {
				float skullSpawnRate = 80f;
				if (activeHands == 0)
					skullSpawnRate /= 2f;
				if (activeHands != 0 && npc.life < npc.lifeMax * 0.25f)
					skullSpawnRate = 100;  // Force skull spawn rate to be slower than normal

				if (Main.getGoodWorld)
					skullSpawnRate *= 0.8f;

				if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] % skullSpawnRate == 0f) {
					Vector2 npcCenter = npc.Center;
					if (Collision.CanHit(npcCenter, 1, 1, target.position, target.width, target.height)) {
						float skullSpeed = 3f;
						if (activeHands == 0)
							skullSpeed += 2f;

						float distanceToTargetX = target.position.X + target.width * 0.5f - npcCenter.X + Main.rand.Next(-20, 21);
						float distanceToTargetY = target.position.Y + target.height * 0.5f - npcCenter.Y + Main.rand.Next(-20, 21);
						float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
						distanceToTarget = skullSpeed / distanceToTarget;
						distanceToTargetX *= distanceToTarget;
						distanceToTargetY *= distanceToTarget;

						Vector2 directionToTarget = new Vector2(distanceToTargetX + Main.rand.Next(-50, 51) * 0.01f, distanceToTargetY + Main.rand.Next(-50, 51) * 0.01f);
						directionToTarget.Normalize();
						directionToTarget *= skullSpeed;
						directionToTarget += npc.velocity;
						distanceToTargetX = directionToTarget.X;
						distanceToTargetY = directionToTarget.Y;
						int damage = MiscUtils.TrueDamage(Main.masterMode ? 150 : 90);
						int type = ProjectileID.Skull;
						npcCenter += directionToTarget * 5f;

						int skull = Projectile.NewProjectile(npc.GetSource_FromAI(), npcCenter.X, npcCenter.Y, distanceToTargetX, distanceToTargetY, type, damage, 0f, Main.myPlayer, -1f);
						Main.projectile[skull].timeLeft = 300;
					}
				}
			}

			if (npc.ai[1] == 0f) {
				npc.damage = npc.defDamage;
				npc.ai[2] += 1f;
				//800 ticks --> 240 ticks
				if (npc.ai[2] >= 800f - 560f * (1f - (float)npc.life / npc.lifeMax)) {
					npc.ai[2] = 0f;
					npc.ai[1] = 1f;
					npc.TargetClosest();
					npc.netUpdate = true;
				}

				npc.rotation = npc.velocity.X / 15f;
				//Normal: 0.02, 2, 0.05, 8
				//Expert: 0.03, 4, 0.07, 9.5
				float accelerationY = 0.04f;
				float maxVelocityY = 8f;
				float accelerationX = 0.1f;
				float maxVelocityX = 12f;

				if (Main.getGoodWorld) {
					accelerationY += 0.01f;
					maxVelocityY += 1f;
					accelerationX += 0.05f;
					maxVelocityX += 2f;
				}

				if (npc.position.Y > target.position.Y - 250f) {
					if (npc.velocity.Y > 0f)
						npc.velocity.Y *= 0.98f;

					npc.velocity.Y -= accelerationY;
					if (npc.velocity.Y > maxVelocityY)
						npc.velocity.Y = maxVelocityY;
				} else if (npc.position.Y < target.position.Y - 250f) {
					if (npc.velocity.Y < 0f)
						npc.velocity.Y *= 0.98f;

					npc.velocity.Y += accelerationY;
					if (npc.velocity.Y < 0f - maxVelocityY)
						npc.velocity.Y = 0f - maxVelocityY;
				}

				if (npc.position.X + npc.width / 2 > target.position.X + target.width / 2) {
					if (npc.velocity.X > 0f)
						npc.velocity.X *= 0.98f;

					npc.velocity.X -= accelerationX;
					if (npc.velocity.X > maxVelocityX)
						npc.velocity.X = maxVelocityX;
				}

				if (npc.position.X + npc.width / 2 < target.position.X + target.width / 2) {
					if (npc.velocity.X < 0f)
						npc.velocity.X *= 0.98f;

					npc.velocity.X += accelerationX;
					if (npc.velocity.X < 0f - maxVelocityX)
						npc.velocity.X = 0f - maxVelocityX;
				}
			} else if (npc.ai[1] == 1f) {
				if (Main.getGoodWorld) {
					if (activeHands > 0)
						npc.reflectsProjectiles = true;
					else if (Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] % 200f == 0f && NPC.CountNPCS(NPCID.DarkCaster) < 6) {
						int maxAttempts = 1000;
						for (int i = 0; i < maxAttempts; i++) {
							int spawnX = (int)(npc.Center.X / 16f) + Main.rand.Next(-50, 51);

							int spawnY;
							for (spawnY = (int)(npc.Center.Y / 16f) + Main.rand.Next(-50, 51); spawnY < Main.maxTilesY - 10 && !WorldGen.SolidTile(spawnX, spawnY); spawnY++);

							spawnY--;

							if (!WorldGen.SolidTile(spawnX, spawnY)) {
								int spawnedCaster = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), spawnX * 16 + 8, spawnY * 16, 32);

								if (Main.netMode == NetmodeID.Server && spawnedCaster < Main.maxNPCs)
									NetMessage.SendData(MessageID.SyncNPC, number: spawnedCaster);

								break;
							}
						}
					}
				}

				npc.defense -= npc.defDefense;
				npc.ai[2] += 1f;
				if (npc.ai[2] == 2f)
					SoundEngine.PlaySound(SoundID.Roar, npc.position);

				// Vanilla: 400 ticks
				// Desolation Mode: 300 ticks --> 120 ticks
				if (npc.ai[2] >= 300f - 180f * (1f - (float)npc.life / npc.lifeMax)) {
					npc.ai[2] = 0f;
					npc.ai[1] = 0f;
				}

				npc.rotation += npc.direction * 0.3f;
				Vector2 npcCenter = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
				float distanceToTargetY = target.position.Y + target.height / 2 - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
				float chargeStrength = 5.75f;

				npc.damage = (int)(npc.defDamage * 1.3);
				if (distanceToTarget > 150f)
					chargeStrength *= 1.05f;
				if (distanceToTarget > 200f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 250f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 300f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 350f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 400f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 450f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 500f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 550f)
					chargeStrength *= 1.1f;
				if (distanceToTarget > 600f)
					chargeStrength *= 1.1f;

				switch (activeHands) {
					case 0:
						chargeStrength *= 1.2f;
						break;
					case 1:
						chargeStrength *= 1.1f;
						break;
				}

				if (Main.getGoodWorld)
					chargeStrength *= 1.3f;

				distanceToTarget = chargeStrength / distanceToTarget;
				npc.velocity.X = distanceToTargetX * distanceToTarget;
				npc.velocity.Y = distanceToTargetY * distanceToTarget;
			} else if (npc.ai[1] == 2f) {
				npc.damage = 1000;
				npc.defense = 9999;

				npc.rotation += npc.direction * 0.3f;

				Vector2 npcCenter = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float distanceToTargetX = target.position.X + target.width / 2 - npcCenter.X;
				float distanceToTargetY = target.position.Y + target.height / 2 - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);

				// Vanilla:  8f velocity
				distanceToTarget = 9.5f / distanceToTarget;
				npc.velocity.X = distanceToTargetX * distanceToTarget;
				npc.velocity.Y = distanceToTargetY * distanceToTarget;
			} else if (npc.ai[1] == 3f) {
				npc.velocity.Y += 0.1f;
				if (npc.velocity.Y < 0f)
					npc.velocity.Y *= 0.95f;

				npc.velocity.X *= 0.95f;
				
				npc.EncourageDespawn(50);
			}

			if (npc.ai[1] != 2f && npc.ai[1] != 3f && activeHands != 0) {
				int num179 = Dust.NewDust(new Vector2(npc.position.X + npc.width / 2 - 15f - npc.velocity.X * 5f, npc.position.Y + npc.height - 2f), 30, 10, DustID.Blood, (0f - npc.velocity.X) * 0.2f, 3f, 0, default, 2f);
				Main.dust[num179].noGravity = true;
				Main.dust[num179].velocity.X *= 1.3f;
				Main.dust[num179].velocity.X += npc.velocity.X * 0.4f;
				Main.dust[num179].velocity.Y += 2f + npc.velocity.Y;

				for (int num180 = 0; num180 < 2; num180++) {
					num179 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + 120f), npc.width, 60, DustID.Blood, npc.velocity.X, npc.velocity.Y, 0, default, 2f);
					Main.dust[num179].noGravity = true;
					Dust dust = Main.dust[num179];
					dust.velocity -= npc.velocity;
					Main.dust[num179].velocity.Y += 5f;
				}
			}
		}

		/// <summary>
		/// Runs a modified AI for Skeletron's hands
		/// </summary>
		public static void AI_SkeletronHand(NPC npc) {
			/*   AI Notes:
			 *   ai[0] ==   -1 | Left hand
			 *   ai[0] ==    1 | Right hand
			 *   
			 *   ai[1]         | Parent Skeletron Head NPC
			 *   
			 *   ai[2] ==  0/3 | Hover near the Parent NPC (lasts for 200 ticks)
			 *   ai[2] ==    1 | Hand swipe attack from above
			 *   ai[2] ==    2 | Waiting to go below the target player from subphase 1
			 *   ai[2] ==    4 | Hand swipe attack from the sides (Always happens after swipe from above)
			 *   ai[2] ==    5 | Waiting to go far enough horizontally from subphase 4
			 *   
			 *   ai[3]         | Timer
			 */
			var helperData = npc.Helper();
			var skeletron = npc.Following();

			Player target = npc.Target();

			npc.spriteDirection = -(int)npc.ai[0];
			if (!skeletron.active || skeletron.aiStyle != 11) {
				npc.ai[2] += 10f;
				if (npc.ai[2] > 50f || Main.netMode != NetmodeID.Server) {
					npc.life = -1;
					npc.HitEffect();
					npc.active = false;
				}
			}

			if (npc.ai[2] == 0f || npc.ai[2] == 3f) {
				if (skeletron.ai[1] == 3f && npc.timeLeft > 10)
					npc.EncourageDespawn(10);

				float retreatFriction = 0.92f;
				float retreatFactorX = 0.3f;
				float retreatFactorY = 1.2f;
				float velCapX = 11f;
				float velCapY = 9f;
				if (skeletron.ai[1] != 0f) {
					if (npc.position.Y > skeletron.position.Y - 100f) {
						if (npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if (npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					} else if (npc.position.Y < skeletron.position.Y - 100f) {
						if (npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if (npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if (npc.position.X + npc.width / 2 > skeletron.position.X + skeletron.width / 2 - 120f * npc.ai[0]) {
						if (npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if (npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if (npc.position.X + npc.width / 2 < skeletron.position.X + skeletron.width / 2 - 120f * npc.ai[0]) {
						if (npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if (npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}
				} else {
					npc.ai[3] += 1.5f;

					if (npc.ai[3] >= 300f) {
						npc.ai[2] += 1f;
						npc.ai[3] = 0f;
						npc.netUpdate = true;
					}

					retreatFactorX = 0.12f;
					retreatFactorY = 0.08f;
					velCapY = 7f;

					if (npc.position.Y > skeletron.position.Y + 230f) {
						if (npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if (npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					} else if (npc.position.Y < skeletron.position.Y + 230f) {
						if (npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if (npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if (npc.position.X + npc.width / 2 > skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0]) {
						if (npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if (npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if (npc.position.X + npc.width / 2 < skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0]) {
						if (npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if (npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}

					if (npc.position.Y > skeletron.position.Y + 230f) {
						if (npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if (npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					} else if (npc.position.Y < skeletron.position.Y + 230f) {
						if (npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if (npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if (npc.position.X + npc.width / 2 > skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0]) {
						if (npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if (npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if (npc.position.X + npc.width / 2 < skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0]) {
						if (npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if (npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}
				}

				Vector2 vector19 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num181 = skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0] - vector19.X;
				float num182 = skeletron.position.Y + 230f - vector19.Y;
				npc.rotation = (float)Math.Atan2(num182, num181) + 1.57f;
			} else if (npc.ai[2] == 1f) {
				Vector2 vector20 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num184 = skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0] - vector20.X;
				float num185 = skeletron.position.Y + 230f - vector20.Y;
				float num186;

				npc.rotation = (float)Math.Atan2(num185, num184) + 1.57f;
				npc.velocity.X *= 0.95f;
				npc.velocity.Y -= 0.1f;

				npc.velocity.Y -= 0.06f;
				if (npc.velocity.Y < -13f)
					npc.velocity.Y = -13f;

				if (npc.position.Y < skeletron.position.Y - 200f) {
					npc.TargetClosest();
					npc.ai[2] = 2f;

					target = npc.Target();

					vector20 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					num184 = target.position.X + target.width / 2 - vector20.X;
					num185 = target.position.Y + target.height / 2 - vector20.Y;
					num186 = (float)Math.Sqrt(num184 * num184 + num185 * num185);
					num186 = 21f / num186;

					npc.velocity.X = num184 * num186;
					npc.velocity.Y = num185 * num186;
					npc.netUpdate = true;
				}
			} else if (npc.ai[2] == 2f) {
				if (npc.position.Y > target.position.Y || npc.velocity.Y < 0f)
					npc.ai[2] = 3f;
			} else if (npc.ai[2] == 4f) {
				Vector2 vector21 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num187 = skeletron.position.X + skeletron.width / 2 - 200f * npc.ai[0] - vector21.X;
				float num188 = skeletron.position.Y + 230f - vector21.Y;
				float num189;

				npc.rotation = (float)Math.Atan2(num188, num187) + 1.57f;
				npc.velocity.Y *= 0.95f;
				npc.velocity.X += 0.1f * (0f - npc.ai[0]);

				npc.velocity.X += 0.07f * (0f - npc.ai[0]);
				if (npc.velocity.X < -12f)
					npc.velocity.X = -12f;
				else if (npc.velocity.X > 12f)
					npc.velocity.X = 12f;

				if (npc.position.X + npc.width / 2 < skeletron.position.X + skeletron.width / 2 - 500f || npc.position.X + npc.width / 2 > skeletron.position.X + skeletron.width / 2 + 500f) {
					npc.TargetClosest();
					npc.ai[2] = 5f;

					target = npc.Target();

					vector21 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					num187 = target.position.X + target.width / 2 - vector21.X;
					num188 = target.position.Y + target.height / 2 - vector21.Y;
					num189 = (float)Math.Sqrt(num187 * num187 + num188 * num188);
					num189 = 22f / num189;

					npc.velocity.X = num187 * num189;
					npc.velocity.Y = num188 * num189;
					npc.netUpdate = true;
				}
			} else if (npc.ai[2] == 5f && ((npc.velocity.X > 0f && npc.Center.X > target.Center.X) || (npc.velocity.X < 0f && npc.Center.X < target.Center.X)))
				npc.ai[2] = 0f;

			helperData.Timer--;
			if (helperData.Timer <= 0) {
				SoundEngine.PlaySound(SoundID.NPCHit2, npc.Center);

				MiscUtils.SpawnProjectileSynced(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(target.Center) * 14f, ModContent.ProjectileType<SkeletronBone>(), Main.masterMode ? 120 : 88, 4f);

				helperData.Timer = Main.rand.Next(60, 75);

				DetourNPCHelper.SendData(npc.whoAmI);
			}
		}
	}
}
