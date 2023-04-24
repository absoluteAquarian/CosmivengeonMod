using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits.Desomode {
	public static partial class DesolationModeBossAI {
		public const int WoF_Attack_EyeLasers = 0;
		public const int WoF_Attack_DemonSickles = 1;
		public const int WoF_Attack_ImpFireballs = 2;
		public const int WoF_Attack_CursedFlamethrower = 3;

		public static void AI_WallOfFleshMouth(NPC npc) {
			var helperData = npc.Helper();

			//Despawn if reached the edge of the world
			if (npc.position.X < 160f || npc.position.X > (Main.maxTilesX - 10) * 16)
				npc.active = false;

			if (npc.localAI[0] == 0f) {
				npc.localAI[0] = 1f;
				//Wall bottom, wall top
				Main.wofDrawAreaBottom = -1;
				Main.wofDrawAreaTop = -1;
			}

			if (Main.getGoodWorld && Main.netMode != NetmodeID.MultiplayerClient && Main.rand.NextBool(180) && NPC.CountNPCS(NPCID.FireImp) < 4) {
				int maxAttempts = 1000;
				for (int i = 0; i < maxAttempts; i++) {
					int spawnX = (int)(npc.Center.X / 16f);
					int spawnY = (int)(npc.Center.Y / 16f);
					if (npc.target >= 0) {
						spawnX = (int)(npc.Target().Center.X / 16f);
						spawnY = (int)(npc.Target().Center.Y / 16f);
					}

					spawnX += Main.rand.Next(-50, 51);
					for (spawnY += Main.rand.Next(-50, 51); spawnY < Main.maxTilesY - 10 && !WorldGen.SolidTile(spawnX, spawnY); spawnY++);

					spawnY--;

					if (!WorldGen.SolidTile(spawnX, spawnY)) {
						int imp = NPC.NewNPC(Entity.GetSource_NaturalSpawn(), spawnX * 16 + 8, spawnY * 16, NPCID.FireImp);
						if (Main.netMode == NetmodeID.Server && imp < Main.maxNPCs)
							NetMessage.SendData(MessageID.SyncNPC, number: imp);

						break;
					}
				}
			}

			if (npc.life < npc.lifeMax / 2f && !helperData.Flag) {
				helperData.Flag = true;

				SoundEngine.PlaySound(SoundID.Roar, npc.Center);
			}

			if (!helperData.Flag)
				helperData.Timer2 = WoF_Attack_EyeLasers;

			npc.ai[1] += 1f;
			if (npc.ai[2] == 0f) {
				if (npc.life < npc.lifeMax * 0.5)
					npc.ai[1] += 1f;

				if (npc.life < npc.lifeMax * 0.2)
					npc.ai[1] += 1f;

				if (npc.ai[1] > 2700f)
					npc.ai[2] = 1f;
			}

			//Spit out a leech
			if (npc.ai[2] > 0f && npc.ai[1] > 60f) {
				int maxLeechPerCycle = 3;
				if (npc.life < npc.lifeMax * 0.3)
					maxLeechPerCycle++;

				npc.ai[2] += 1f;
				npc.ai[1] = 0f;
				if (npc.ai[2] > maxLeechPerCycle)
					npc.ai[2] = 0f;

				if (Main.netMode != NetmodeID.MultiplayerClient && NPC.CountNPCS(NPCID.LeechHead) < 10) {
					int leech = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height / 2 + 20f), NPCID.LeechHead, 1);
					Main.npc[leech].velocity.X = npc.direction * 8;
				}
			}

			if (!helperData.Flag) {
				//Scream at the player
				npc.localAI[3] += 1f;
				if (npc.localAI[3] >= 600 + Main.rand.Next(1000)) {
					npc.localAI[3] = -Main.rand.Next(200);
					SoundEngine.PlaySound(SoundID.NPCDeath10, npc.position);
				}
			}

			Main.wofNPCIndex = npc.whoAmI;

			//Find where the bottom and top of the wall should move towards
			int topmostY = Main.UnderworldLayer + 10;
			int bottommostY = topmostY + 70;
			int npcLeft = (int)(npc.position.X / 16f);
			int npcRight = (int)((npc.position.X + npc.width) / 16f);
			int npcBottom = (int)((npc.position.Y + npc.height / 2) / 16f);
			int attempt = 0;
			int drawMargin = npcBottom + 7;
			while (attempt < 15 && drawMargin > Main.UnderworldLayer) {
				drawMargin++;
				if (drawMargin > Main.maxTilesY - 10) {
					drawMargin = Main.maxTilesY - 10;
					break;
				}

				if (drawMargin < topmostY)
					continue;

				for (int x = npcLeft; x <= npcRight; x++) {
					try {
						if (WorldGen.InWorld(x, drawMargin, 2) && (WorldGen.SolidTile(x, drawMargin) || Main.tile[x, drawMargin].LiquidAmount > 0))
							attempt++;
					} catch {
						attempt += 15;
					}
				}
			}

			drawMargin += 4;
			if (Main.wofDrawAreaBottom == -1) {
				Main.wofDrawAreaBottom = drawMargin * 16;
			} else if (Main.wofDrawAreaBottom > drawMargin * 16) {
				Main.wofDrawAreaBottom--;
				if (Main.wofDrawAreaBottom < drawMargin * 16)
					Main.wofDrawAreaBottom = drawMargin * 16;
			} else if (Main.wofDrawAreaBottom < drawMargin * 16) {
				Main.wofDrawAreaBottom++;
				if (Main.wofDrawAreaBottom > drawMargin * 16)
					Main.wofDrawAreaBottom = drawMargin * 16;
			}

			attempt = 0;
			drawMargin = npcBottom - 7;
			while (attempt < 15 && drawMargin < Main.maxTilesY - 10) {
				drawMargin--;
				if (drawMargin <= 10) {
					drawMargin = 10;
					break;
				}

				if (drawMargin < bottommostY)
					continue;

				if (drawMargin < topmostY) {
					drawMargin = topmostY;
					break;
				}

				for (int x = npcLeft; x <= npcRight; x++) {
					try {
						if (WorldGen.InWorld(x, drawMargin, 2) && (WorldGen.SolidTile(x, drawMargin) || Main.tile[x, drawMargin].LiquidAmount > 0))
							attempt++;
					} catch {
						attempt += 15;
					}
				}
			}

			drawMargin -= 4;
			if (Main.wofDrawAreaTop == -1) {
				Main.wofDrawAreaTop = drawMargin * 16;
			} else if (Main.wofDrawAreaTop > drawMargin * 16) {
				Main.wofDrawAreaTop--;
				if (Main.wofDrawAreaTop < drawMargin * 16)
					Main.wofDrawAreaTop = drawMargin * 16;
			} else if (Main.wofDrawAreaTop < drawMargin * 16) {
				Main.wofDrawAreaTop++;
				if (Main.wofDrawAreaTop > drawMargin * 16)
					Main.wofDrawAreaTop = drawMargin * 16;
			}

			Main.wofDrawAreaTop = (int)MathHelper.Clamp(Main.wofDrawAreaTop, topmostY * 16f, bottommostY * 16f);
			Main.wofDrawAreaBottom = (int)MathHelper.Clamp(Main.wofDrawAreaBottom, topmostY * 16f, bottommostY * 16f);
			if (Main.wofDrawAreaTop > Main.wofDrawAreaBottom - 160)
				Main.wofDrawAreaTop = Main.wofDrawAreaBottom - 160;
			else if (Main.wofDrawAreaBottom < Main.wofDrawAreaTop + 160)
				Main.wofDrawAreaBottom = Main.wofDrawAreaTop + 160;

			//Mouth should move slowly towards the middle of the wall?
			//This segment gets nulled by the following 'npc.velocity.Y = 0f;' line, so idk why it's here
			float drawCenter = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2 - npc.height / 2;
			if (npc.position.Y > drawCenter + 1f)
				npc.velocity.Y = -1f;
			else if (npc.position.Y < drawCenter - 1f)
				npc.velocity.Y = 1f;

			npc.velocity.Y = 0f;
			int lowestCenter = (Main.maxTilesY - 180) * 16;
			if (drawCenter < lowestCenter)
				drawCenter = lowestCenter;

			npc.position.Y = drawCenter;

			//Default move rate of 1.5px/tick
			//Speed up as the boss loses health
			float speed = 2.75f;
			//Original increases are listed below
			if (npc.life < npc.lifeMax * 0.75)
				speed += 0.275f;  //0.25
			if (npc.life < npc.lifeMax * 0.66)
				speed += 0.325f;  //0.3
			if (npc.life < npc.lifeMax * 0.5)
				speed += 0.425f;  //0.4
			if (npc.life < npc.lifeMax * 0.33)
				speed += 0.325f;  //0.3
			if (npc.life < npc.lifeMax * 0.25)
				speed += 0.525f;  //0.5
			if (npc.life < npc.lifeMax * 0.1)
				speed += 0.625f;  //0.6
			if (npc.life < npc.lifeMax * 0.05)
				speed += 0.625f;  //0.6
			if (npc.life < npc.lifeMax * 0.035)
				speed += 0.625f;  //0.6
			if (npc.life < npc.lifeMax * 0.025)
				speed += 0.625f;  //0.6

			//Expert mode is always active, so these two increases will always happen anyway
			//Original: 1.35, 0.35
			speed *= 1.4f;
			speed += 0.4f;

			if (Main.getGoodWorld) {
				speed *= 1.1f;
				speed += 0.2f;
			}

			//Just spawned, X-velocity is 0
			//Find a target and face towards them
			Player target = npc.Target();

			if (npc.velocity.X == 0f) {
				npc.TargetClosest();

				target = npc.Target();

				if (target.dead) {
					float num370 = float.PositiveInfinity;
					int directionToPlayer = 0;
					for (int i = 0; i < Main.maxPlayers; i++) {
						// Possible bug?  This should be using Main.player[i] instead of Main.player[npc.target]
						Player player = target;
						if (player.active) {
							float num373 = npc.Distance(player.Center);
							if (num370 > num373) {
								num370 = num373;
								directionToPlayer = (npc.Center.X < player.Center.X) ? 1 : -1;
							}
						}
					}

					npc.direction = directionToPlayer;
				}

				npc.velocity.X = npc.direction;
			}

			if (npc.velocity.X < 0f) {
				npc.velocity.X = -speed;
				npc.direction = -1;
			} else {
				npc.velocity.X = speed;
				npc.direction = 1;
			}

			if (target.dead || !target.gross) {
				npc.TargetClosest_WOF();
				target = npc.Target();
			}

			// Despawn if all targets are dead
			if (target.dead) {
				npc.localAI[1] += 1f / 180f;
				if (npc.localAI[1] >= 1f) {
					SoundEngine.PlaySound(SoundID.NPCDeath10, npc.position);
					
					npc.life = 0;
					npc.active = false;

					if (Main.netMode != NetmodeID.MultiplayerClient)
						NetMessage.SendData(MessageID.DamageNPC, number: npc.whoAmI, number2: -1f);

					return;
				}
			} else {
				npc.localAI[1] = MathHelper.Clamp(npc.localAI[1] - 1f / 30f, 0f, 1f);
			}

			npc.spriteDirection = npc.direction;
			Vector2 npcCenter = npc.Center;
			float distanceToTargetX = target.Center.X - npcCenter.X;
			float distanceToTargetY = target.Center.Y - npcCenter.Y;
			float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);

			distanceToTargetX *= distanceToTarget;
			distanceToTargetY *= distanceToTarget;
			
			if (npc.direction > 0) {
				if (target.Center.X > npc.Center.X)
					npc.rotation = (float)Math.Atan2(-distanceToTargetY, -distanceToTargetX) + 3.14f;
				else
					npc.rotation = 0f;
			} else if (target.Center.X < npc.Center.X)
				npc.rotation = (float)Math.Atan2(distanceToTargetY, distanceToTargetX) + 3.14f;
			else
				npc.rotation = 0f;

			// Desolation: always try to stay within 70 tiles of the target player
			float targetX = target.Center.X - npc.direction * 70 * 16;
			if ((npc.direction == 1 && npc.Center.X < targetX) || (npc.direction == -1 && npc.Center.X > targetX)) {
				npc.Center = new Vector2(targetX, npc.Center.Y);
				helperData.Flag2 = true;
			} else
				helperData.Flag2 = false;

			// Expert mode WoF spawns extra "The Hungry"s
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				int spawnChance = (int)(1f + npc.life / (float)npc.lifeMax * 10f);
				spawnChance *= spawnChance;
				if (spawnChance < 400)
					spawnChance = (spawnChance * 19 + 400) / 20;
				if (spawnChance < 60)
					spawnChance = (spawnChance * 3 + 60) / 4;
				if (spawnChance < 20)
					spawnChance = (spawnChance + 20) / 2;

				spawnChance = (int)(spawnChance * 0.7);
				if (Main.rand.NextBool(spawnChance)) {
					int numHungry = 0;
					float[] hungryAIs = new float[10];
					for (int i = 0; i < 200; i++) {
						if (numHungry < 10 && Main.npc[i].active && Main.npc[i].type == NPCID.TheHungry) {
							hungryAIs[numHungry] = Main.npc[i].ai[0];
							numHungry++;
						}
					}

					int chance = 1 + numHungry * 2;
					if (numHungry < 10 && Main.rand.Next(chance) <= 1) {
						int possiblePosition = -1;
						for (int i = 0; i < 1000; i++) {
							int ai = Main.rand.Next(10);
							float num354 = ai * 0.1f - 0.05f;
							bool canSpawn = true;
							for (int j = 0; j < numHungry; j++) {
								if (num354 == hungryAIs[j]) {
									canSpawn = false;
									break;
								}
							}

							if (canSpawn) {
								possiblePosition = ai;
								break;
							}
						}

						if (possiblePosition >= 0) {
							int num356 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)drawCenter, NPCID.TheHungry, npc.whoAmI);
							Main.npc[num356].ai[0] = possiblePosition * 0.1f - 0.05f;
						}
					}
				}
			}

			// Spawn the eye parts and the initial "The Hungry"s
			if (npc.localAI[0] == 1f && Main.netMode != NetmodeID.MultiplayerClient) {
				npc.localAI[0] = 2f;

				drawCenter = (npc.Center.Y + Main.wofDrawAreaTop) / 2f;
				int num357 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)drawCenter, NPCID.WallofFleshEye, npc.whoAmI, 1);
				
				drawCenter = (npc.Center.Y + Main.wofDrawAreaBottom) / 2f;
				num357 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)drawCenter, NPCID.WallofFleshEye, npc.whoAmI, -1);
				
				drawCenter = (npc.Center.Y + Main.wofDrawAreaBottom) / 2f;
				for (int num358 = 0; num358 < 11; num358++)
					num357 = NPC.NewNPC(npc.GetSource_FromAI(), (int)npc.position.X, (int)drawCenter, NPCID.TheHungry, npc.whoAmI, num358 * 0.1f - 0.05f);
			}

			// Phase 2 stuff
			if (helperData.Flag && Main.netMode != NetmodeID.MultiplayerClient) {
				helperData.Timer++;

				if (npc.life < npc.lifeMax * 0.3333f)
					helperData.Timer++;
				if (npc.life < npc.lifeMax * 0.25f)
					helperData.Timer++;
				if (npc.life < npc.lifeMax * 0.1667f)
					helperData.Timer++;

				if (helperData.Timer2 <= WoF_Attack_EyeLasers)
					helperData.Timer = 0;
				else if (helperData.Timer >= 300 && helperData.Timer2 == WoF_Attack_DemonSickles) {
					//Spawn demon scythes that have no tile collision
					helperData.Timer = 0;

					float x = target.Center.X - npc.direction * 30 * 16;

					for (int i = -3; i < 4; i++) {
						float y = target.Center.Y + i * 10 * 16;

						int damage = MiscUtils.TrueDamage(Main.masterMode ? 120 : 80);

						Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), new Vector2(x, y), Vector2.UnitX * npc.direction * 1.85f, ProjectileID.DemonSickle, damage, 0f, Main.myPlayer);
						proj.tileCollide = false;
						proj.timeLeft = 4 * 60;
					}

					WoF_ChooseNextAttack(npc);
				} else if (helperData.Timer2 == WoF_Attack_ImpFireballs) {
					float x = target.Center.X - npc.direction * 45 * 16;
					float offY = 10 * 16;

					if (helperData.Timer < 200) {
						float range = 48 * (200 - helperData.Timer) / 200f;

						for (float i = -3.5f; i <= 3.5f; i++) {
							float y = target.Center.Y + i * offY;

							for (int j = 0; j < 4; j++) {
								Dust dust = Dust.NewDustDirect(new Vector2(x - range, y - range), (int)(range + 0.5f) * 2, (int)(range + 0.5f) * 2, DustID.Torch);
								dust.noGravity = true;
								dust.velocity.X = target.velocity.X;
							}
						}
					} else {
						//Spawn imp fireballs that can't be hit
						helperData.Timer = 0;

						for (float i = -3.5f; i <= 3.5f; i++) {
							float y = target.Center.Y + i * offY;

							Vector2 pos = new Vector2(x, y);
							Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), pos, target.DirectionFrom(pos) * 9, ProjectileID.ImpFireball, MiscUtils.TrueDamage(Main.masterMode ? 88 : 66), 0f, Main.myPlayer);
							proj.friendly = false;
							proj.hostile = true;
							proj.tileCollide = false;
							proj.timeLeft = 6 * 60;
						}

						WoF_ChooseNextAttack(npc);
					}
				} else if (helperData.Timer >= 200 && helperData.Timer2 == WoF_Attack_CursedFlamethrower) {
					if (helperData.Timer > 200 + 120) {
						helperData.Timer = 0;

						WoF_ChooseNextAttack(npc);
					}

					Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center - new Vector2(32 * npc.direction), npc.DirectionTo(target.Center) * 14f, ProjectileID.EyeFire, 50, 0f, Main.myPlayer);
					proj.tileCollide = false;

					SoundEngine.PlaySound(SoundID.Item34, npc.Center);
				}
			}
		}

		private static void WoF_ChooseNextAttack(NPC npc) {
chooseAgain:
			int val = Main.rand.Next(4);
			switch (val) {
				case WoF_Attack_EyeLasers:
					npc.Helper().Timer2 = -1;
					break;
				default:
					if (val == WoF_Attack_CursedFlamethrower && npc.Target().tongued)
						goto chooseAgain;

					npc.Helper().Timer2 = val;
					break;
			}

			npc.netUpdate = true;
		}

		public static void AI_WallOfFleshEye(NPC npc) {
			if (Main.wofNPCIndex < 0) {
				npc.active = false;
				return;
			}

			//Match the eye health with the mouth health
			npc.realLife = Main.wofNPCIndex;
			NPC wof = Main.npc[Main.wofNPCIndex];

			if (wof.life > 0)
				npc.life = wof.life;

			//Keep targetting whichever player is closest
			npc.TargetClosest();

			Player target = npc.Target();

			//Keep the eyes in line with the mouth
			npc.position.X = wof.position.X;
			npc.direction = wof.direction;

			npc.spriteDirection = npc.direction;

			//Find which eye spot this eye should move towards
			float eyePosTargetY = (Main.wofDrawAreaBottom + Main.wofDrawAreaTop) / 2;
			eyePosTargetY = npc.ai[0] <= 0f ? eyePosTargetY + (Main.wofDrawAreaBottom - eyePosTargetY) / 2f : eyePosTargetY - (eyePosTargetY - Main.wofDrawAreaTop) / 2f;
			eyePosTargetY -= npc.height / 2;
			if (npc.position.Y > eyePosTargetY + 1f)
				npc.velocity.Y = -4f;
			else if (npc.position.Y < eyePosTargetY - 1f)
				npc.velocity.Y = 4f;
			else {
				npc.velocity.Y = 0f;
				npc.position.Y = eyePosTargetY;
			}

			//Cap vertical velocity to 5 (Normal/Expert) or 8 (Desolation)
			if (npc.velocity.Y > 8f)
				npc.velocity.Y = 8f;
			if (npc.velocity.Y < -8f)
				npc.velocity.Y = -8f;

			Vector2 npcCenter = npc.Center;
			float distanceToTargetX = target.Center.X - npcCenter.X;
			float distanceToTargetY = target.Center.Y - npcCenter.Y;
			float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
			distanceToTargetX *= distanceToTarget;
			distanceToTargetY *= distanceToTarget;
			bool playerIsInFrontOfBoss = true;
			if (npc.direction > 0) {
				if (target.position.X + target.width / 2 > npc.position.X + npc.width / 2) {
					npc.rotation = (float)Math.Atan2(0f - distanceToTargetY, 0f - distanceToTargetX) + 3.14f;
				} else {
					npc.rotation = 0f;
					playerIsInFrontOfBoss = false;
				}
			} else if (target.position.X + target.width / 2 < npc.position.X + npc.width / 2) {
				npc.rotation = (float)Math.Atan2(distanceToTargetY, distanceToTargetX) + 3.14f;
			} else {
				npc.rotation = 0f;
				playerIsInFrontOfBoss = false;
			}

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if (wof.Helper().Timer2 > 0) {
				npc.localAI[1] = 0;
				npc.localAI[2] = 0;
				return;
			}

			//Timing for laser firing
			int numLasers = 4;
			npc.localAI[1] += 1f;
			if (wof.life < wof.lifeMax * 0.75f) {
				npc.localAI[1] += 1f;
				numLasers++;
			}
			if (wof.life < wof.lifeMax * 0.5f) {
				npc.localAI[1] += 1f;
				numLasers++;
			}
			if (wof.life < wof.lifeMax * 0.25f) {
				npc.localAI[1] += 1f;
				numLasers++;
			}
			if (wof.life < wof.lifeMax * 0.1f) {
				npc.localAI[1] += 2f;
				numLasers++;
			}

			npc.localAI[1] += 0.5f;
			numLasers++;
			//Originally was lifeMax * 0.1
			//Desolation changes this to lifeMax * 0.05
			if (wof.life < wof.lifeMax * 0.05f) {
				npc.localAI[1] += 2f;
				numLasers++;
			}

			if (npc.localAI[2] == 0f) {
				//Original: 600f
				//Value is lower in phase 2, but the attacks are implemented in a pattern so it's actually longer between each attack
				if ((!wof.Helper().Flag && npc.localAI[1] > 500f) || (wof.Helper().Flag && npc.localAI[1] > 300f)) {
					npc.localAI[2] = 1f;
					npc.localAI[1] = 0f;
				}
			} else {
				if (npc.localAI[1] <= 45f)
					return;

				npc.localAI[1] = 0f;
				npc.localAI[2] += 1f;
				if (npc.localAI[2] >= numLasers) {
					npc.localAI[2] = 0f;

					//Indicate that this eye has finished firing its lasers
					if (wof.Helper().Flag)
						wof.Helper().Timer2++;

					if (wof.Helper().Timer2 == WoF_Attack_EyeLasers + 1)
						WoF_ChooseNextAttack(wof);
				}

				if (playerIsInFrontOfBoss) {
					//Lasers move faster the lower the boss's health is
					//Normal/Expert: 9 -> 13
					//Desolation: 12 -> 16
					//Lasers deal more damage the lower the boss's health is
					//Normal/Expert: 22/44 -> 30/60
					//Desolation: 60 -> 100
					float num365 = 12f;
					int num366 = 60;
					int num367 = ProjectileID.EyeLaser;
					if (wof.life < wof.lifeMax * 0.5) {
						num366 += 10;
						num365 += 1f;
					}

					if (wof.life < wof.lifeMax * 0.25) {
						num366 += 10;
						num365 += 1f;
					}

					if (wof.life < wof.lifeMax * 0.1) {
						num366 += 20;
						num365 += 2f;
					}

					if (Main.masterMode)
						num366 += 40;

					npcCenter = npc.Center;
					distanceToTargetX = target.Center.X - npcCenter.X;
					distanceToTargetY = target.Center.Y - npcCenter.Y;
					distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
					distanceToTarget = num365 / distanceToTarget;
					distanceToTargetX *= distanceToTarget;
					distanceToTargetY *= distanceToTarget;
					npcCenter.X += distanceToTargetX;
					npcCenter.Y += distanceToTargetY;

					Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npcCenter, new Vector2(distanceToTargetX, distanceToTargetY), num367, MiscUtils.TrueDamage(num366), 0f, Main.myPlayer);
					proj.tileCollide = false;
				}
			}
		}
	}

	public static partial class DesolationModeMonsterAI {
		public static void AI_TheHungry(NPC npc) {
			if (npc.justHit)
				npc.ai[1] = 10f;

			if (Main.wofNPCIndex < 0) {
				npc.active = false;
				return;
			}

			NPC wof = Main.npc[Main.wofNPCIndex];

			npc.TargetClosest();

			Player target = npc.Target();

			npc.damage = npc.defDamage;

			float acceleration = 0.1f;
			float maxDistance = 300f;
			float dmg = 0;
			if (wof.life < wof.lifeMax * 0.25) {
				dmg = 85;
				npc.defense = 40;
				acceleration += 0.1f;
			} else if (wof.life < wof.lifeMax * 0.5) {
				dmg = 70;
				npc.defense = 30;
				acceleration += 0.066f;
			} else if (wof.life < wof.lifeMax * 0.75) {
				dmg = 55;
				acceleration += 0.033f;
			}

			if (dmg > 0)
				npc.damage = npc.GetAttackDamage_ScaledByStrength(dmg);

			if (npc.whoAmI % 4 == 0)
				maxDistance *= 1.75f;

			if (npc.whoAmI % 4 == 1)
				maxDistance *= 1.5f;

			if (npc.whoAmI % 4 == 2)
				maxDistance *= 1.25f;

			if (npc.whoAmI % 3 == 0)
				maxDistance *= 1.5f;

			if (npc.whoAmI % 3 == 1)
				maxDistance *= 1.25f;

			maxDistance *= 0.75f;

			float wofCenterX = wof.position.X + wof.width / 2;
			float wofWallRange = Main.wofDrawAreaBottom - Main.wofDrawAreaTop;
			float anchorY = Main.wofDrawAreaTop + wofWallRange * npc.ai[0];

			npc.ai[2] += 1f;
			if (npc.ai[2] > 100f) {
				maxDistance = (int)(maxDistance * 1.3f);
				if (npc.ai[2] > 200f)
					npc.ai[2] = 0f;
			}

			Vector2 anchor = new Vector2(wofCenterX, anchorY);
			float distanceToTargetX = target.position.X + target.width / 2 - npc.width / 2 - anchor.X;
			float distanceToTargetY = target.position.Y + target.height / 2 - npc.height / 2 - anchor.Y;
			float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
			if (npc.ai[1] == 0f) {
				if (distanceToTarget > maxDistance) {
					distanceToTarget = maxDistance / distanceToTarget;
					distanceToTargetX *= distanceToTarget;
					distanceToTargetY *= distanceToTarget;
				}

				if (npc.position.X < wofCenterX + distanceToTargetX) {
					npc.velocity.X += acceleration;
					if (npc.velocity.X < 0f && distanceToTargetX > 0f)
						npc.velocity.X += acceleration * 2.5f;
				} else if (npc.position.X > wofCenterX + distanceToTargetX) {
					npc.velocity.X -= acceleration;
					if (npc.velocity.X > 0f && distanceToTargetX < 0f)
						npc.velocity.X -= acceleration * 2.5f;
				}

				if (npc.position.Y < anchorY + distanceToTargetY) {
					npc.velocity.Y += acceleration;
					if (npc.velocity.Y < 0f && distanceToTargetY > 0f)
						npc.velocity.Y += acceleration * 2.5f;
				} else if (npc.position.Y > anchorY + distanceToTargetY) {
					npc.velocity.Y -= acceleration;
					if (npc.velocity.Y > 0f && distanceToTargetY < 0f)
						npc.velocity.Y -= acceleration * 2.5f;
				}

				float maxVelocity = 6.75f;
				if (Main.wofNPCIndex >= 0) {
					float moreVelocity = 2.5f;
					float wofLifeFraction = wof.life / wof.lifeMax;
					if (wofLifeFraction < 0.75)
						moreVelocity += 0.7f;

					if (wofLifeFraction < 0.5)
						moreVelocity += 0.7f;

					if (wofLifeFraction < 0.25)
						moreVelocity += 0.9f;

					if (wofLifeFraction < 0.1)
						moreVelocity += 0.9f;

					moreVelocity *= 1.25f;
					moreVelocity += 0.3f;
					maxVelocity += moreVelocity * 0.35f;
					if (npc.Center.X < wof.Center.X && wof.velocity.X > 0f)
						maxVelocity += 6f;

					if (npc.Center.X > wof.Center.X && wof.velocity.X < 0f)
						maxVelocity += 6f;
				}

				if (npc.velocity.X > maxVelocity)
					npc.velocity.X = maxVelocity;

				if (npc.velocity.X < 0f - maxVelocity)
					npc.velocity.X = 0f - maxVelocity;

				if (npc.velocity.Y > maxVelocity)
					npc.velocity.Y = maxVelocity;

				if (npc.velocity.Y < 0f - maxVelocity)
					npc.velocity.Y = 0f - maxVelocity;
			} else if (npc.ai[1] > 0f) {
				npc.ai[1] -= 1f;
			} else {
				npc.ai[1] = 0f;
			}

			if (distanceToTargetX > 0f) {
				npc.spriteDirection = 1;
				npc.rotation = (float)Math.Atan2(distanceToTargetY, distanceToTargetX);
			}

			if (distanceToTargetX < 0f) {
				npc.spriteDirection = -1;
				npc.rotation = (float)Math.Atan2(distanceToTargetY, distanceToTargetX) + 3.14f;
			}

			Lighting.AddLight((int)(npc.position.X + npc.width / 2) / 16, (int)(npc.position.Y + npc.height / 2) / 16, 0.3f, 0.2f, 0.1f);

			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			//Occasionally spit ichor shots
			npc.ai[3]++;
			if (npc.ai[3] >= Main.rand.Next(350, 600)) {
				npc.ai[3] = 0;

				int damage = MiscUtils.TrueDamage(Main.masterMode ? 80 : 50);
				Vector2 wofTargetCenter = Main.npc[Main.wofNPCIndex].Target().Center;
				wofTargetCenter.Y -= 3 * 16;
				Projectile proj = Projectile.NewProjectileDirect(npc.GetSource_FromAI(), npc.Center, npc.DirectionTo(wofTargetCenter) * 4f, ProjectileID.IchorSplash, damage, 3f, Main.myPlayer);
				proj.hostile = true;
				proj.friendly = false;
				proj.timeLeft = 60 * 3;
				proj.penetrate = 1;
			}
		}

		public static void AI_TheHungryII(NPC npc) {
			AI_002_FloatingEye(npc);
		}
	}
}
