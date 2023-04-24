using CosmivengeonMod.NPCs.Global;
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
		/// Runs a modified AI for the Brain of Cthulhu
		/// </summary>
		public static void AI_BrainOfCthulhu(NPC npc) {
			/*    NOTES
			 *  - ai[0]: State variable.  state > 0 is slow movement while creepers are alive, state < 0 is faster movement in Phase 2
			 *  - ai[1]: Teleport X-position
			 *  - ai[2]: Teleport Y-position
			 *  - ai[3]: Indirect alpha assignment when starting a teleport during Phase 2 (not needed???)
			 *  - localAI[0]: Flag for initial Creeper spawns
			 *  - localAI[1]: Teleport delay timer
			 *  - localAI[2]: Flag for roar and gore spawning when transitioning to Phase 2
			 *  - localAI[3]: "All targets dead" timer and acceleration
			 */
			NPC.crimsonBoss = npc.whoAmI;

			//Spawn the Creepers
			if (Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f) {
				npc.localAI[0] = 1f;

				// Vanilla sets the creeper count to 20 in non-FTW and 40 in FTW via NPC.GetBrainOfCthuluCreepersCount
				int creeperCount = 30;
				if (Main.getGoodWorld)
					creeperCount = 48;

				for (int i = 0; i < creeperCount; i++) {
					float spawnX = npc.Center.X;
					float spawnY = npc.Center.Y;

					spawnX += Main.rand.Next(-npc.width, npc.width);
					spawnY += Main.rand.Next(-npc.height, npc.height);

					int spawned = NPC.NewNPC(npc.GetSource_FromAI(), (int)spawnX, (int)spawnY, NPCID.Creeper);

					Main.npc[spawned].velocity = new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.1f;
					Main.npc[spawned].netUpdate = true;
				}
			}

			//If the target player is too far away, despawn
			if (Main.netMode != NetmodeID.MultiplayerClient) {
				npc.TargetClosest(true);

				int maxDistance = 6000;
				if (Math.Abs(npc.Center.X - npc.Target().Center.X) + Math.Abs(npc.Center.Y - npc.Target().Center.Y) > maxDistance) {
					npc.active = false;
					npc.life = 0;

					if (Main.netMode == NetmodeID.Server)
						NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
				}
			}

			if (npc.active)
				BoC_AttackCheck(npc);

			if (npc.ai[0] < 0f) {
				// TODO 1.4.4: implement this
				/*
				if (Main.getGoodWorld)
					NPC.brainOfGravity = npc.whoAmI;
				*/

				// Perform the visual transition to phase 2
				if (npc.localAI[2] == 0f) {
					SoundEngine.PlaySound(SoundID.NPCHit1, npc.position);
					npc.localAI[2] = 1f;
					Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 392, 1f);
					Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 393, 1f);
					Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 394, 1f);
					Gore.NewGore(npc.GetSource_FromAI(), npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 395, 1f);

					for (int i = 0; i < 20; i++)
						Dust.NewDust(npc.position, npc.width, npc.height, DustID.Blood, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f);

					SoundEngine.PlaySound(SoundID.Roar, npc.position);
				}

				npc.dontTakeDamage = false;
				//Vanilla: npc.knockBackResist = 0.5f;
				npc.knockBackResist = 0f;
				npc.GetGlobalNPC<StatsNPC>().endurance = 0.05f;
				//Hide the boss from Boss Cursor's radar
				npc.dontCountMe = true;

				npc.TargetClosest(true);
				Vector2 npcCenter = npc.Center;
				float distanceToTargetX = npc.Target().Center.X - npcCenter.X;
				float distanceToTargetY = npc.Target().Center.Y - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
				float movementFactor = 8f;

				distanceToTarget = movementFactor / distanceToTarget;
				distanceToTargetX *= distanceToTarget;
				distanceToTargetY *= distanceToTarget;

				npc.velocity.X = (npc.velocity.X * 50f + distanceToTargetX) / 51f;
				npc.velocity.Y = (npc.velocity.Y * 50f + distanceToTargetY) / 51f;

				if (npc.ai[0] == -1f) {
					// Before teleporting

					if (Main.netMode != NetmodeID.MultiplayerClient) {
						int maxDelayTimer = 60 + Main.rand.Next(120);
						if (Main.netMode != NetmodeID.SinglePlayer)
							maxDelayTimer += Main.rand.Next(30, 90);

						BoC_CheckTeleport(npc, maxDelayTimer, 10, 12, hitsDelayTeleport: true, checkLineOfSight: false, ai0ResetTo: -2f);
					}
				} else if (npc.ai[0] == -2f) {
					// Teleporting
					npc.velocity *= 0.9f;

					//Teleports happen much slower in singleplayer
					//Boss fading away also happens slower
					if (Main.netMode != NetmodeID.SinglePlayer)
						npc.ai[3] += 15f;
					else
						npc.ai[3] += 25f;

					if (npc.ai[3] >= 255f) {
						npc.ai[3] = 255f;
						npc.position.X = npc.ai[1] * 16f - npc.width / 2;
						npc.position.Y = npc.ai[2] * 16f - npc.height / 2;
						SoundEngine.PlaySound(SoundID.Item8, npc.Center);
						npc.ai[0] = -3f;
						npc.netUpdate = true;
						npc.netSpam = 0;
					}

					npc.alpha = (int)npc.ai[3];
				} else if (npc.ai[0] == -3f) {
					// After teleporting
					// Boss fading in happens much slower in singleplayer 
					if (Main.netMode != NetmodeID.SinglePlayer)
						npc.ai[3] -= 15f;
					else
						npc.ai[3] -= 25f;

					if (npc.ai[3] <= 0f) {
						npc.ai[3] = 0f;
						npc.ai[0] = -1f;
						npc.netUpdate = true;
						npc.netSpam = 0;
					}

					npc.alpha = (int)npc.ai[3];
				}
			} else {
				// Move towards the player
				npc.TargetClosest(true);

				Vector2 npcCenter = new Vector2(npc.Center.X, npc.Center.Y);
				float distanceToTargetX = npc.Target().Center.X - npcCenter.X;
				float distanceToTargetY = npc.Target().Center.Y - npcCenter.Y;
				float distanceToTarget = (float)Math.Sqrt(distanceToTargetX * distanceToTargetX + distanceToTargetY * distanceToTargetY);
				float movementFactor = 1f;

				if (Main.getGoodWorld)
					movementFactor *= 3;

				if (distanceToTarget < movementFactor) {
					// Boss's velocity vector length is always at least 1
					npc.velocity.X = distanceToTargetX;
					npc.velocity.Y = distanceToTargetY;
				} else {
					distanceToTarget = movementFactor / distanceToTarget;
					npc.velocity.X = distanceToTargetX * distanceToTarget;
					npc.velocity.Y = distanceToTargetY * distanceToTarget;
				}

				if (npc.ai[0] == 0f) {
					if (Main.netMode != NetmodeID.MultiplayerClient) {
						int creeperCount = 0;

						// Count how many Creepers are alive
						for (int i = 0; i < Main.maxNPCs; i++) {
							if (Main.npc[i].active && Main.npc[i].type == NPCID.Creeper)
								creeperCount++;
						}

						//If no Creepers are alive, start the transition to phase 2
						if (creeperCount == 0) {
							npc.ai[0] = -1f;
							npc.localAI[1] = 0f;
							npc.alpha = 0;
							npc.netUpdate = true;
						}

						//Teleports happen every 2-5 seconds
						BoC_CheckTeleport(npc, 120 + Main.rand.Next(300), 12, 40, hitsDelayTeleport: false, checkLineOfSight: true, ai0ResetTo: 1f);
					}
				} else if (npc.ai[0] == 1f) {
					//Before teleporting - boss slowly fades away
					//Teleporting - boss is completely invisible
					npc.alpha += 5;

					if (npc.alpha >= 255) {
						SoundEngine.PlaySound(SoundID.Item8, npc.Center);
						npc.alpha = 255;
						npc.position.X = npc.ai[1] * 16f - npc.width / 2;
						npc.position.Y = npc.ai[2] * 16f - npc.height / 2;
						npc.ai[0] = 2f;
					}
				} else if (npc.ai[0] == 2f) {
					//After teleporting - boss slowly fades in
					npc.alpha -= 5;

					if (npc.alpha <= 0) {
						npc.alpha = 0;
						npc.ai[0] = 0f;
					}
				}
			}

			// If the target player is dead or not in the Crimson biome
			if (npc.Target().dead || !npc.Target().ZoneCrimson) {
				//Increment the timer until it reaches 2 seconds
				if (npc.localAI[3] < 120f)
					npc.localAI[3] += 1f;

				// If the timer has reached more than 1 second, increase the NPC's velocity faster and faster
				if (npc.localAI[3] > 60f)
					npc.velocity.Y += (npc.localAI[3] - 60f) * 0.25f;

				// And make the NPC semi-transparent for good measure
				npc.ai[0] = 2f;
				npc.alpha = 10;
				return;
			}

			//This member should always tend towards being <= 0
			if (npc.localAI[3] > 0f)
				npc.localAI[3] -= 1f;
		}

		private static void BoC_CheckTeleport(NPC npc, int maxDelayTimer, int minTeleportDistance, int maxTeleportDistance, bool hitsDelayTeleport, bool checkLineOfSight, float ai0ResetTo) {
			// Attacking the boss delays the teleport by 5 ticks
			npc.localAI[1] += 1f;
			if (hitsDelayTeleport && npc.justHit)
				npc.localAI[1] -= Main.rand.Next(5);

			if (npc.localAI[1] >= maxDelayTimer) {
				npc.localAI[1] = 0f;
				npc.TargetClosest(true);

				int teleportAttempts = 0;
				Player target = npc.Target();

				// Find a valid position to teleport to
				do {
					teleportAttempts++;
								
					int targetTileX = (int)target.Center.X / 16;
					int targetTileY = (int)target.Center.Y / 16;

					float tileStride = 16f;

					int teleportOffsetX = Main.rand.Next(minTeleportDistance, maxTeleportDistance + 1);
					int teleportOffsetY = Main.rand.Next(minTeleportDistance, maxTeleportDistance + 1);

					if (Main.rand.NextBool(2))
						teleportOffsetX *= -1;

					if (Main.rand.NextBool(2))
						teleportOffsetY *= -1;

					Vector2 possibleTarget = new Vector2(teleportOffsetX * 16, teleportOffsetY * 16);
					if (Vector2.Dot(target.velocity.SafeNormalize(Vector2.UnitY), possibleTarget.SafeNormalize(Vector2.UnitY)) > 0f)
						possibleTarget += possibleTarget.SafeNormalize(Vector2.Zero) * tileStride * target.velocity.Length();

					targetTileX += (int)(possibleTarget.X / 16f);
					targetTileY += (int)(possibleTarget.Y / 16f);

					if (teleportAttempts > 100 || (!WorldGen.SolidTile(targetTileX, targetTileY) && (!checkLineOfSight || teleportAttempts > 75 || Collision.CanHit(new Vector2(targetTileX * 16, targetTileX * 16), 1, 1, target.position, target.width, target.height)))) {
						npc.ai[3] = 0f;
						npc.ai[0] = ai0ResetTo;
						npc.ai[1] = targetTileX;
						npc.ai[2] = targetTileY;
						npc.netUpdate = true;
						npc.netSpam = 0;
						break;
					}
				} while (teleportAttempts <= 100);
			}
		}

		private static readonly SoundStyle zapSound = new SoundStyle("Cosmivengeon/Sounds/Custom/Zap") {
			PlayOnlyIfFocused = true,
			Volume = 0.75f,
			SoundLimitBehavior = SoundLimitBehavior.ReplaceOldest
		};

		private static void BoC_AttackCheck(NPC npc) {
			var helperData = npc.Helper();

			//First psychic attack
			helperData.Timer++;

			int offset = BrainPsychicMine.Attack_Timer_Max - BrainPsychicMine.Attack_Death_Delay;
			int timerMax;
			if (npc.ai[0] < 0)
				timerMax = offset - 60;
			else
				timerMax = (int)(2.5f * 60 + offset);

			if (helperData.Timer > timerMax) {
				helperData.Timer = 0;

				int proj = MiscUtils.SpawnProjectileSynced(npc.GetSource_FromAI(),
					npc.Target().Center, Vector2.Zero,
					ModContent.ProjectileType<BrainPsychicMine>(),
					Main.masterMode ? 100 : 80,
					4f,
					ai1: npc.target,
					owner: Main.myPlayer);

				if (npc.ai[0] < 0)
					(Main.projectile[proj].ModProjectile as BrainPsychicMine).fastAttack = true;
			}

			//Second psychic attack
			helperData.Timer2++;

			int target = npc.ai[0] < 0f ? 180 : 300;

			if (helperData.Timer2 == target - 60)
				SoundEngine.PlaySound(zapSound, npc.Target().Center - new Vector2(0, 50 * 16));

			if (helperData.Timer2 > target) {
				helperData.Timer2 = 0;

				int dir = npc.Target().velocity.X > 0 ? 1 : -1;

				for (int i = -8; i < 8; i++) {
					int positionOffset = i;

					positionOffset *= 16 * 16;

					Vector2 position = npc.Target().Center + new Vector2(positionOffset, -BrainPsychicLightning.FinalHeight / 2);

					int proj = MiscUtils.SpawnProjectileSynced(npc.GetSource_FromAI(), position, Vector2.Zero,
						ModContent.ProjectileType<BrainPsychicLightning>(),
						Main.masterMode ? 90 : 65,
						3.5f);

					BrainPsychicLightning lightning = Main.projectile[proj].ModProjectile as BrainPsychicLightning;
					lightning.AttackDelay = (int)(-dir * 180f * i / 15f);

					if (npc.ai[0] < 0)
						lightning.fastAttack = true;
				}
			}
		}
	}
}
