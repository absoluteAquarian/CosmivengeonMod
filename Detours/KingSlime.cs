﻿using CosmivengeonMod.Projectiles.Desomode;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		/// <summary>
		/// Runs a modified AI for King Slime.
		/// </summary>
		public static void AI_KingSlime(NPC npc){
			/*    NOTES
			 *  - ai[0]: Used as a timer for the hops
			 *  - ai[1]: Counter for determining what hops to do
			 *  - ai[2]: Used as a timer for the fade-teleports (for when it should happen)
			 *  - ai[3]: Used to keep track of what 1/20th section of the max health the boss is in.
			 *           Once the boss goes down into a new section, more slime minions are spanwed.
			 *           In Desolation mode, this is changed to every 1/12th section due to more slimes spawning.
			 *  - localAi[0]: Unused by vanilla code
			 *  - localAI[1]: The X-position of the target player's Bottom coordinate
			 *  - localAI[2]: The Y-position of the target player's Bottom coordinate
			 *  - localAI[3]: The flag for if the boss has done the first tick of its AI execution.
			 *                The initial value for ai[0] and the initial player target are set when this is 0.
			 */

			float mainTimerProgress = 1f;
			bool dontUpdateJumpCounter = false;
			bool dontSpawnFadeDust = false;
			npc.aiAction = 0;

			//Initializing ai[3]
			if(npc.ai[3] == 0f && npc.life > 0)
				npc.ai[3] = npc.lifeMax;

			//Initializing boss AI things:  ai[0] and npc.target
			if(npc.localAI[3] == 0f && Main.netMode != NetmodeID.MultiplayerClient){
				npc.ai[0] = -100f;
				npc.localAI[3] = 1f;
				npc.TargetClosest(true);
				npc.netUpdate = true;
			}

			//If the current target is dead, try to find a new target
			//If that new target is dead, reverse direction immediately and set timeLeft to 0 (instantly despawns when too far away)
			if(npc.Target().dead){
				npc.TargetClosest(true);

				if(npc.Target().dead){
					npc.timeLeft = 0;
					if(npc.Target().Center.X < npc.Center.X)
						npc.direction = 1;
					else
						npc.direction = -1;
				}
			}

			//The boss has finished the hops or was hopping AND the teleport timer has finished
			//(This runs after king slime fades away via the dust stuff)
			if(!npc.Target().dead && npc.ai[2] >= 300f && npc.ai[1] < 5f && npc.velocity.Y == 0f){
				npc.ai[2] = 0f;
				npc.ai[0] = 0f;
				npc.ai[1] = 5f;

				if(Main.netMode != NetmodeID.MultiplayerClient){
					npc.TargetClosest(false);

					Point npcTileCenter = npc.Center.ToTileCoordinates();
					Point targetTileCenter = npc.Target().Center.ToTileCoordinates();

					//Teleports can be at a random tile within a 20x20 tile square centered on the player target
					int teleportPosVariance = 10;

					//Teleports must be in a 14x14 tile square centered on the player target
					int teleportCheckVariance = 7;

					int positionsChecked = 0;
					bool teleportFound = false;

					//If the boss is too far away from the player, just teleport directly onto them (very cheeky)
					//Vanilla (125 tiles): 2000 * 2000
					if(npc.DistanceSQ(npc.Target().Center) > 50 * 50 * 256){
						//More than 50 tiles away
						teleportFound = true;
						positionsChecked = 100;
					}

					//Keep checking new positions until we find one
					while(!teleportFound && positionsChecked < 100){
						positionsChecked++;
						int teleportX = Main.rand.Next(targetTileCenter.X - teleportPosVariance, targetTileCenter.X + teleportPosVariance + 1);
						int teleportY = Main.rand.Next(targetTileCenter.Y - teleportPosVariance, targetTileCenter.Y + 1);

						bool teleportOutsideVarianceRange = Math.Abs(teleportX - targetTileCenter.X) > teleportCheckVariance || Math.Abs(teleportY - targetTileCenter.Y) > teleportCheckVariance;
						bool teleportNotOnNPCLocation = teleportY != npcTileCenter.Y || teleportX != npcTileCenter.X;

						if(teleportOutsideVarianceRange && teleportNotOnNPCLocation && !Main.tile[teleportX, teleportY].nactive()){
							int teleportYOffset = 0;

							if(CosmivengeonUtils.TileIsSolidOrPlatform(teleportX, teleportY))
								teleportYOffset = 1;
							else{
								while(teleportYOffset < 150 && teleportY + teleportYOffset < Main.maxTilesY){
									if(CosmivengeonUtils.TileIsSolidOrPlatform(teleportX, teleportY + teleportYOffset)){
										teleportYOffset--;
										break;
									}
									teleportYOffset++;
								}
							}

							teleportY += teleportYOffset;
							
							//Only teleport to the tile if it doesn't have lava and we can't see the player
							if(!Main.tile[teleportX, teleportY].lava() && !Collision.CanHitLine(npc.Center, 0, 0, npc.Target().Center, 0, 0)){
								npc.localAI[1] = teleportX * 16 + 8;
								npc.localAI[2] = teleportY * 16 + 16;
								break;
							}
						}
					}

					//Couldn't find a random position to teleport to OR we jumped to this code due to being too far away
					//In either case, the teleport position will be right on top of the target player
					if(positionsChecked >= 100){
						Vector2 bottom = Main.player[Player.FindClosest(npc.position, npc.width, npc.height)].Bottom;
						npc.localAI[1] = bottom.X;
						npc.localAI[2] = bottom.Y;
					}

					//Spawn additional slimes where King Slime used to be
					int summonsToSpawn = Main.rand.Next(1, 3);
					for(int i = 0; i < summonsToSpawn; i++){
						int typeToSpawn = NPCID.BlueSlime;

						//Spawn a spiked slime in Expert mode with a 25% chance
						if(Main.expertMode && Main.rand.NextBool(4))
							typeToSpawn = NPCID.SlimeSpiked;

						KingSlime_SpawnSummon(npc, typeToSpawn);
					}
				}
			}

			//Increment the "do the fade" timer if the boss can't see the player or the player is too far above the boss
			if(!Collision.CanHitLine(npc.Center, 0, 0, npc.Target().Center, 0, 0))
				npc.ai[2]++;
			//Vanilla:  > 320f
			if(Math.Abs(npc.Top.Y - npc.Target().Bottom.Y) > 160f)
				npc.ai[2]++;
			//Desolation mode:  also increment ai[2] if the boss is too far away from the player
			if(npc.DistanceSQ(npc.Target().Center) > 50 * 50 * 256)
				npc.ai[2]++;

			if(npc.ai[1] == 5f){
				//This code runs immediately after a teleport
				dontUpdateJumpCounter = true;
				npc.aiAction = 1;
				npc.ai[0] += 1f;
				mainTimerProgress = MathHelper.Clamp((60f - npc.ai[0]) / 60f, 0f, 1f);
				mainTimerProgress = 0.5f + mainTimerProgress * 0.5f;

				if(npc.ai[0] >= 60f)
					dontSpawnFadeDust = true;

				if(npc.ai[0] == 60f)
					Gore.NewGore(npc.Center + new Vector2(-40f, -npc.height / 2f), npc.velocity, 734, 1f);

				if(npc.ai[0] >= 60f && Main.netMode != NetmodeID.MultiplayerClient){
					npc.Bottom = new Vector2(npc.localAI[1], npc.localAI[2]);
					npc.ai[1] = 6f;
					npc.ai[0] = 0f;
					npc.netUpdate = true;
				}

				if(Main.netMode == NetmodeID.MultiplayerClient && npc.ai[0] >= 120f){
					npc.ai[1] = 6f;
					npc.ai[0] = 0f;
				}

				if(!dontSpawnFadeDust){
					for(int num240 = 0; num240 < 10; num240++){
						int num241 = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, 4, npc.velocity.X, npc.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
						Main.dust[num241].noGravity = true;
						Main.dust[num241].velocity *= 0.5f;
					}
				}
			}else if(npc.ai[1] == 6f){
				//This code runs after the code that runs after a teleport
				dontUpdateJumpCounter = true;
				npc.aiAction = 0;
				npc.ai[0] += 1f;
				mainTimerProgress = MathHelper.Clamp(npc.ai[0] / 30f, 0f, 1f);
				mainTimerProgress = 0.5f + mainTimerProgress * 0.5f;

				if(npc.ai[0] >= 30f && Main.netMode != NetmodeID.MultiplayerClient){
					npc.ai[1] = 0f;
					npc.ai[0] = 0f;
					npc.netUpdate = true;
					npc.TargetClosest(true);
				}

				if(Main.netMode == NetmodeID.MultiplayerClient && npc.ai[0] >= 60f){
					npc.ai[1] = 0f;
					npc.ai[0] = 0f;
					npc.TargetClosest(true);
				}

				for(int num242 = 0; num242 < 10; num242++){
					int num243 = Dust.NewDust(npc.position + Vector2.UnitX * -20f, npc.width + 40, npc.height, 4, npc.velocity.X, npc.velocity.Y, 150, new Color(78, 136, 255, 80), 2f);
					Main.dust[num243].noGravity = true;
					Main.dust[num243].velocity *= 2f;
				}
			}

			npc.dontTakeDamage = npc.hide = dontSpawnFadeDust;

			//Small hop count is updated here
			npc.Helper().Timer2 = npc.life >= npc.lifeMax * 0.75f ? 1 : 4 - (int)(4f * npc.life / npc.lifeMax + 1);

			//If we just landed from a jump or are still on the ground, do these things
			if(npc.velocity.Y == 0f){
				//Friction
				npc.velocity.X *= 0.8f;
				if(Math.Abs(npc.velocity.X) < 0.1f)
					npc.velocity.X = 0f;

				//Check which hop to do
				if(!dontUpdateJumpCounter){
					//Vanilla: npc.ai[0] += 2f;
					npc.ai[0] += 3f;

					if(npc.life < npc.lifeMax * 0.8f)
						npc.ai[0]++;
					if(npc.life < npc.lifeMax * 0.6f)
						npc.ai[0]++;
					if(npc.life < npc.lifeMax * 0.4f)
						npc.ai[0] += 2f;
					if(npc.life < npc.lifeMax * 0.2f)
						npc.ai[0] += 3f;
					if(npc.life < npc.lifeMax * 0.1f)
						npc.ai[0] += 4f;

					//Do the jumps
					//Vanilla AI:
					//ai[0] == 3: big jump
					//ai[0] == 2: small jump
					//ai[0] == 0 or 1: normal jump
					//Desolation mode AI:
					//ai[0] == 3: big jump
					//ai[0] == 2: small jumps while helper's GeneralTimer3 > 0 (if health is less than 300, only do small hops)
					//ai[0] == 0 or 1: normal jump (immediately jumps to ai[0] = 2 if health is < 33% max)
					if(npc.ai[0] >= 0f){
						npc.netUpdate = true;
						npc.TargetClosest(true);

						bool hpLow = npc.life < 300;
						if(npc.ai[1] == 3f){
							npc.velocity.Y = -13f;
							npc.velocity.X += 5f * npc.direction;
							npc.ai[0] = -200f;
							npc.ai[1] = 0f;
						}else if(hpLow || (npc.ai[1] == 2f && npc.Helper().Timer3 > 0)){
							npc.Helper().Timer3--;

							npc.velocity.Y = -6f;
							npc.velocity.X = 8f * npc.direction;
							npc.ai[0] = npc.Helper().Timer3 == 0 ? -120f : 60f;

							if(npc.Helper().Timer3 == 0 && !hpLow)
								npc.ai[1]++;
						}else{
							npc.Helper().Timer3 = npc.Helper().Timer2;

							npc.velocity.Y = -8f;
							npc.velocity.X += 6f * npc.direction;
							npc.ai[0] = -120f;

							if(npc.life > npc.lifeMax * 0.3333f)
								npc.ai[1]++;
							else
								npc.ai[1] = 2;
						}
					}else if(npc.ai[0] >= -30f)
						npc.aiAction = 1;
				}
			}else if(npc.target < 255 && ((npc.direction == 1 && npc.velocity.X < 3f) || (npc.direction == -1 && npc.velocity.X > -3f))){
				//Make turning around easier
				if((npc.direction == -1 && npc.velocity.X < 0.1f) || (npc.direction == 1 && npc.velocity.X > -0.1f))
					npc.velocity.X += 0.2f * npc.direction;
				else
					npc.velocity.X *= 0.93f;
			}

			//Desolation mode AI:  fires larger King Slime spike projectiles at the player constantly AND faster as King Slime loses health
			//If the player is too close to King Slime, do the same attack as the lesser Spiked Slimes
			if(Main.netMode != NetmodeID.MultiplayerClient){
				if(Math.Abs(npc.Target().Center.X - npc.Center.X) < 6 * 16 * npc.scale){
					//Update the spike shoot timer
					if(npc.life > 300)
						npc.Helper().Timer++;

					//5 projectiles in a cone above King Slime
					if(npc.life > 300 && npc.Helper().Timer > 70){
						npc.Helper().Timer = 0;
						for(int i = -2; i < 3; i++){
							Vector2 rotatedVelocity = (-Vector2.UnitY).RotatedBy(MathHelper.ToRadians(20 * i)) * 8f;
							int proj = Projectile.NewProjectile(npc.Top + new Vector2(0, 20), rotatedVelocity, ModContent.ProjectileType<KingSlimeSpike>(), CosmivengeonUtils.TrueDamage(40), 0.33f, Main.myPlayer);
							NetMessage.SendData(MessageID.SyncProjectile, number: proj);
						}
					}
				}else{
					//Update the spike shoot timer
					npc.Helper().Timer++;
					if(npc.life < npc.lifeMax * 0.75f)
						npc.Helper().Timer++;
					if(npc.life < npc.lifeMax * 0.5f)
						npc.Helper().Timer++;
					//Slower spike shooting during panic phase
					if(npc.life < npc.lifeMax * 0.25f && npc.life > 300)
						npc.Helper().Timer += 2;
					else if(npc.life <= 300)
						npc.Helper().Timer--;

					//Timer used to be 130, now it's 200
					if(npc.Helper().Timer >= 200){
						npc.Helper().Timer = 0;

						Vector2 velocity = Vector2.Zero;
						float rangeX = npc.Target().Top.X - npc.Center.X;
						rangeX = Math.Sign(rangeX) * Math.Max(Math.Abs(rangeX), 20);

						velocity.X = rangeX + Math.Sign(rangeX) * 30;
						velocity.Y = npc.Target().position.Y - npc.Center.Y - Main.rand.Next(40, 200);
						float num13 = velocity.Length();
						num13 = 10f / num13;
						velocity *= num13;

						//Three projectile in a sort-of cone pointed towards the player
						for(int i = -1; i < 2; i++){
							Vector2 rotatedVelocity = velocity.RotatedBy(MathHelper.ToRadians(15 * i));
							int proj = Projectile.NewProjectile(npc.Top + new Vector2(0, 20), rotatedVelocity, ModContent.ProjectileType<KingSlimeSpike>(), CosmivengeonUtils.TrueDamage(40), 0.33f, Main.myPlayer);
							NetMessage.SendData(MessageID.SyncProjectile, number: proj);
						}
					}
				}
			}

			//Spawn slimy dust
			int num244 = Dust.NewDust(npc.position, npc.width, npc.height, 4, npc.velocity.X, npc.velocity.Y, 255, new Color(0, 80, 255, 80), npc.scale * 1.2f);
			Main.dust[num244].noGravity = true;
			Main.dust[num244].velocity *= 0.5f;

			if(npc.life > 0){
				float lifeFactor = (float)npc.life / npc.lifeMax;
				lifeFactor = lifeFactor * 0.5f + 0.75f;
				lifeFactor *= mainTimerProgress;

				//Change the boss's size
				if(lifeFactor != npc.scale){
					npc.position.X += npc.width / 2;
					npc.position.Y += npc.height;
					npc.scale = lifeFactor;
					npc.width = (int)(98f * npc.scale);
					npc.height = (int)(92f * npc.scale);
					npc.position.X -= npc.width / 2;
					npc.position.Y -= npc.height;
				}
				
				//If the game isn't a multiplayer client, check if the boss should spawn slime minions
				if(Main.netMode != NetmodeID.MultiplayerClient){
					int healthFactor = (int)(npc.lifeMax / 12f);

					//Check if the boss has gone down into another health sector
					if(npc.life + healthFactor < npc.ai[3]){
						npc.ai[3] = npc.life;
						int summonsToSpawn = Main.rand.Next(1, 4);

						for(int i = 0; i < summonsToSpawn; i++){
							int typeToSpawn = NPCID.BlueSlime;

							//Spawn a spiked slime in Expert mode with a 25% chance
							if(Main.expertMode && Main.rand.NextBool(4))
								typeToSpawn = NPCID.SlimeSpiked;

							KingSlime_SpawnSummon(npc, typeToSpawn);
						}

						/* Desolation Mode AI Change:
						 *   Spawn two extra slimes of any of the following variants with their respective weights:
						 *   Green Slime  - 11%
						 *   Blue Slime   - 12%
						 *   Red Slime    - 20%
						 *   Purple Slime - 15%
						 *   Yellow Slime - 20%
						 *   Black Slime  - 15%
						 *   Mother Slime - 6%
						 *   Pinky        - 1%
						 */
						WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
						wRand.Add(NPCID.GreenSlime,  0.11);
						wRand.Add(NPCID.BlueSlime,   0.12);
						wRand.Add(NPCID.RedSlime,    0.2);
						wRand.Add(NPCID.PurpleSlime, 0.15);
						wRand.Add(NPCID.YellowSlime, 0.2);
						wRand.Add(NPCID.BlackSlime,  0.15);
						wRand.Add(NPCID.MotherSlime, 0.06);
						wRand.Add(NPCID.Pinky,       0.01);

						for(int i = 0; i < 2; i++)
							KingSlime_SpawnSummon(npc, wRand.Get());
					}
				}
			}
		}

		private static void KingSlime_SpawnSummon(NPC npc, int type){
			int x = (int)(npc.position.X + Main.rand.Next(npc.width - 32));
			int y = (int)(npc.position.Y + Main.rand.Next(npc.height - 32));

			int summonWhoAmI = NPC.NewNPC(x, y, type);
			NPC hitSummon = Main.npc[summonWhoAmI];

			//Vanilla: hitSummon.velocity.X = Main.rand.Next(-15, 16) * 0.1f;
			hitSummon.velocity.X = Math.Sign(npc.Target().Center.X - npc.Center.X) * Main.rand.NextFloat(8, 17.5f);
			//Vanilla: hitSummon.velocity.Y = Main.rand.Next(-30, 1) * 0.1f;
			hitSummon.velocity.Y = Main.rand.NextFloat(-5, -2);

			//Vanilla: hitSummon.ai[0] = -1000 * Main.rand.Next(3);
			hitSummon.ai[0] = -200 * Main.rand.Next(3);
			hitSummon.ai[1] = 0f;

			if(Main.netMode == NetmodeID.Server && summonWhoAmI < 200)
				NetMessage.SendData(MessageID.SyncNPC, number: summonWhoAmI);
		}
	}
}
