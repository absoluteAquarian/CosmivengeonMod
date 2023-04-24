using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Utilities;

namespace CosmivengeonMod.API.Edits.Desomode {
	public static partial class DesolationModeBossAI {
		public const int EoW_SegmentType_SpitVileSpit = 0;
		public const int EoW_SegmentType_SpawnEaters = 1;
		public const int EoW_SegmentType_SpitCursedFlames = 2;

		/// <summary>
		/// Runs a modified AI for the Eater of Worlds
		/// <para>Note: This AI is not for The Destroyer.  See aiStyle 37 in the source for that.</para>
		/// </summary>
		public static void AI_EaterOfWorlds(NPC npc) {
			/*    NOTES
			 *  - ai[0]: Which segment is following this segment, if any.  Not used by the tail segment
			 *  - ai[1]: Which segment this segment is following, if any.  Not used by the head segment
			 *  - ai[2]: Total segment count on the head segment, used as a counter for body segment count otherwise
			 *  - ai[3]: Unused
			 *  - localAI[0]: Flag for setting netUpdate
			 *  - localAI[1]: Unused
			 *  - localAI[2]: Unused
			 *  - localAI[3]: Unused
			 */
			var helperData = npc.Helper();

			//Only spit things if this segment isn't a head segment carrying a player
			if (Main.netMode != NetmodeID.MultiplayerClient && Main.expertMode) {
				int spawnX = (int)(npc.position.X + npc.width / 2 + npc.velocity.X);
				int spawnY = (int)(npc.position.Y + npc.height / 2 + npc.velocity.Y);

				bool spawnVileSpit = helperData.EoW_SegmentType == EoW_SegmentType_SpitVileSpit;
				Vector2 toPlayer = npc.DirectionTo(npc.Target().Center) * 7.5f;

				//Cursed flames spit should happen less often!
				if (npc.type == NPCID.EaterofWorldsBody && npc.position.Y / 16f < Main.worldSurface || Main.getGoodWorld) {
					int centerX = (int)(npc.Center.X / 16);
					int centerY = (int)(npc.Center.Y / 16);

					if (WorldGen.InWorld(centerX, centerY) && Main.tile[centerX, centerY].WallType == WallID.None) {
						int spawnChance = 900;
						float cursedChance = 0.35f;
						if (Main.getGoodWorld) {
							spawnChance /= 2;
							cursedChance = 0.55f;
						}

						if (Main.rand.NextBool(spawnChance)) {
							npc.TargetClosest(true);

							if (Collision.CanHitLine(npc.Center, 1, 1, npc.Target().Center, 1, 1)) {
								if (spawnVileSpit)
									NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, NPCID.VileSpit, ai1: 1f);
								else if (Main.rand.NextFloat() < cursedChance) {
									int damage = MiscUtils.TrueDamage(Main.masterMode ? 80 : 60);

									Projectile.NewProjectile(npc.GetSource_FromAI(), spawnX, spawnY, toPlayer.X, toPlayer.Y, ProjectileID.CursedFlameHostile, damage, 3.5f, Main.myPlayer);
								}
							}
						}
					}
				} else if (npc.type == NPCID.EaterofWorldsHead && npc.life > 0) {
					int spawnChance = 90;
					spawnChance += (int)(npc.life / (float)npc.lifeMax * 60f * 5f);

					float cursedChance = 0.35f;
					if (Main.getGoodWorld)
						cursedChance = 0.55f;

					if (Main.rand.NextBool(spawnChance)) {
						npc.TargetClosest(true);

						if (Collision.CanHitLine(npc.Center, 1, 1, npc.Target().Center, 1, 1)) {
							if (spawnVileSpit)
								NPC.NewNPC(npc.GetSource_FromAI(), spawnX, spawnY, NPCID.VileSpit, ai1: 1f);
							else if (Main.rand.NextFloat() < cursedChance) {
								int damage = MiscUtils.TrueDamage(Main.masterMode ? 80 : 60);

								Projectile.NewProjectile(npc.GetSource_FromAI(), spawnX, spawnY, toPlayer.X, toPlayer.Y, ProjectileID.CursedFlameHostile, damage, 3.5f, Main.myPlayer);
							}
						}
					}
				}
			}

			npc.realLife = -1;

			if (npc.target < 0 || npc.target == 255 || npc.Target().dead) {
				DetourNPCHelper.EoW_ResetGrab(npc, npc.Target());

				npc.TargetClosest(true);
			}

			if (npc.Target().dead || !npc.Target().ZoneCorrupt) {
				npc.EncourageDespawn(300);

				npc.velocity.Y += 0.2f;
			}

			if (Main.netMode != NetmodeID.MultiplayerClient) {
				if ((npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody) && npc.ai[0] == 0f) {
					WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
					wRand.Add(EoW_SegmentType_SpitVileSpit, 0.5);
					wRand.Add(EoW_SegmentType_SpawnEaters, 0.3);
					wRand.Add(EoW_SegmentType_SpitCursedFlames, 0.2);

					NPC follower;

					// Spawn the remaning segments
					if (npc.type == NPCID.EaterofWorldsHead) {
						// Formerly:
						//Vanilla segments range from 45 to 55 in Normal Mode and 49 to 60 segments in Expert Mode
						//Desolation Mode will have the segments range from 55 to 68
						// In 1.4:
						//Vanilla sets the segment count to 65 in Normal Mode and 70 in Expert Mode via NPC.GetEaterOfWorldsSegmentsCount
						npc.ai[2] = 75;

						npc.Desomode().EoW_WormSegmentsCount = (int)npc.ai[2];
						helperData.EoW_SegmentType = wRand.Get();

						npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type + 1, npc.whoAmI);

						follower = npc.WormFollower();
						
						follower.CopyInteractions(npc);
						follower.Desomode().EoW_WormSegmentsCount = npc.Desomode().EoW_WormSegmentsCount;
					} else if (npc.type == NPCID.EaterofWorldsBody && npc.ai[2] > 0f) {
						npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type, npc.whoAmI);
						helperData.EoW_SegmentType = wRand.Get();

						follower = npc.WormFollower();
						
						follower.CopyInteractions(npc);
						follower.Desomode().EoW_WormSegmentsCount = npc.Desomode().EoW_WormSegmentsCount;
					} else {
						npc.ai[0] = NPC.NewNPC(npc.GetSource_FromAI(), (int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type + 1, npc.whoAmI);
						helperData.EoW_SegmentType = wRand.Get();

						follower = npc.WormFollower();
						
						follower.CopyInteractions(npc);
						follower.Desomode().EoW_WormSegmentsCount = npc.Desomode().EoW_WormSegmentsCount;
					}

					follower.ai[1] = npc.whoAmI;
					follower.ai[2] = npc.ai[2] - 1f;
					npc.netUpdate = true;

					DetourNPCHelper.SendData(npc.whoAmI);
				}

				// If this segment should not be alive due to not being connected to other segments, forcibly kill it
				if (!npc.Following().active && !npc.WormFollower().active) {
					npc.life = 0;
					npc.HitEffect();
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.DamageNPC, number: npc.whoAmI);
				}

				if (npc.type == NPCID.EaterofWorldsHead && !npc.WormFollower().active) {
					npc.life = 0;
					npc.HitEffect();
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.DamageNPC, number: npc.whoAmI);
				}

				if (npc.type == NPCID.EaterofWorldsTail && !npc.Following().active) {
					npc.life = 0;
					npc.HitEffect();
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.DamageNPC, number: npc.whoAmI);
				}

				// Transform a body segment into a head or tail segment if its follower/following segment is dead
				if (npc.type == NPCID.EaterofWorldsBody && (!npc.Following().active || npc.Following().aiStyle != npc.aiStyle)) {
					npc.type = NPCID.EaterofWorldsHead;
					int whoAmI = npc.whoAmI;
					float num25 = npc.life / (float)npc.lifeMax;
					float num26 = npc.ai[0];
					int segment = helperData.EoW_SegmentType;

					npc.SetDefaultsKeepPlayerInteraction(npc.type);
					npc.life = (int)(npc.lifeMax * num25);
					npc.ai[0] = num26;
					npc.TargetClosest(true);
					npc.netUpdate = true;
					npc.whoAmI = whoAmI;
					helperData.EoW_SegmentType = segment;

					//Recalculate the amount of segments
					EoW_RecalculateSegments(npc, head: true);
				}

				if (npc.type == NPCID.EaterofWorldsBody && (!npc.WormFollower().active || npc.WormFollower().aiStyle != npc.aiStyle)) {
					npc.type = NPCID.EaterofWorldsTail;
					int whoAmI2 = npc.whoAmI;
					float num27 = npc.life / (float)npc.lifeMax;
					float num28 = npc.ai[1];
					int segment = helperData.EoW_SegmentType;

					npc.SetDefaultsKeepPlayerInteraction(npc.type);
					npc.life = (int)(npc.lifeMax * num27);
					npc.ai[1] = num28;
					npc.TargetClosest(true);
					npc.netUpdate = true;
					npc.whoAmI = whoAmI2;
					helperData.EoW_SegmentType = segment;

					EoW_RecalculateSegments(npc, head: false);
				}

				if (!npc.active && Main.netMode == NetmodeID.Server)
					NetMessage.SendData(MessageID.DamageNPC, number: npc.whoAmI);
			}

			int npcTileXStart = (int)(npc.position.X / 16f) - 1;
			int npcTileXEnd = (int)((npc.position.X + npc.width) / 16f) + 2;
			int npcTileYStart = (int)(npc.position.Y / 16f) - 1;
			int npcTileYEnd = (int)((npc.position.Y + npc.height) / 16f) + 2;
			if (npcTileXStart < 0)
				npcTileXStart = 0;
			if (npcTileXEnd > Main.maxTilesX)
				npcTileXEnd = Main.maxTilesX;
			if (npcTileYStart < 0)
				npcTileYStart = 0;
			if (npcTileYEnd > Main.maxTilesY)
				npcTileYEnd = Main.maxTilesY;

			bool tooFarAway = false;
			helperData.Flag = false;
			for (int num33 = npcTileXStart; num33 < npcTileXEnd; num33++) {
				for (int num34 = npcTileYStart; num34 < npcTileYEnd; num34++) {
					if (MiscUtils.TileIsSolidOrPlatform(num33, num34)) {
						Vector2 vector;
						vector.X = num33 * 16;
						vector.Y = num34 * 16;

						if (npc.position.X + npc.width > vector.X && npc.position.X < vector.X + 16f && npc.position.Y + npc.height > vector.Y && npc.position.Y < vector.Y + 16f) {
							tooFarAway = true;
							if (Main.rand.NextBool(100) && Main.tile[num33, num34].HasUnactuatedTile)
								WorldGen.KillTile(num33, num34, true, true, false);
						}

						//NPC is in the ground.  Set the flag to not damage a grabbed player
						Tile tile = Main.tile[num33, num34];

						if (tile.HasUnactuatedTile && tile.TileType != TileID.Platforms && !TileID.Sets.Platforms[tile.TileType])
							helperData.Flag = true;
					}
				}
			}

			if (!tooFarAway && npc.type == NPCID.EaterofWorldsHead) {
				Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
				int playerDetectionRange = 1000;
				bool closeEnoughToPlayer = true;
				for (int num36 = 0; num36 < 255; num36++) {
					if (Main.player[num36].active) {
						Rectangle rectangle2 = new Rectangle((int)Main.player[num36].position.X - playerDetectionRange, (int)Main.player[num36].position.Y - playerDetectionRange, playerDetectionRange * 2, playerDetectionRange * 2);
						if (rectangle.Intersects(rectangle2)) {
							closeEnoughToPlayer = false;
							break;
						}
					}
				}

				if (closeEnoughToPlayer)
					tooFarAway = true;
			}

			if (npc.type == NPCID.EaterofWorldsHead)
				DetourNPCHelper.EoW_CheckGrabBite(npc);

			//speed, turn speed
			//Normal: 10, 0.07
			//Expert: 12, 0.15
			//FTW: +4, +0.05
			//Desomode: 16, 0.185
			float maxSpeed = 8f;
			float turnAcceleration = 0.07f;
			if (npc.type == NPCID.EaterofWorldsHead) {
				maxSpeed = 16f;
				turnAcceleration = 0.185f;

				if (Main.getGoodWorld) {
					maxSpeed += 4;
					turnAcceleration += 0.05f;
				}
			}

			Vector2 npcCenter = npc.Center;
			float playerTargetX;
			float playerTargetY;

			if (DetourNPCHelper.EoW_GrabbingNPC == npc.whoAmI) {
				playerTargetX = npc.Desomode().EoW_GrabTarget?.X ?? npc.Target().Center.X;
				playerTargetY = npc.Desomode().EoW_GrabTarget?.Y ?? npc.Target().Center.Y;
			} else {
				playerTargetX = npc.Target().Center.X;
				playerTargetY = npc.Target().Center.Y;
			}

			playerTargetX = (int)(playerTargetX / 16f) * 16;
			playerTargetY = (int)(playerTargetY / 16f) * 16;
			npcCenter.X = (int)(npcCenter.X / 16f) * 16;
			npcCenter.Y = (int)(npcCenter.Y / 16f) * 16;
			playerTargetX -= npcCenter.X;
			playerTargetY -= npcCenter.Y;

			float distanceToPlayer = (float)Math.Sqrt(playerTargetX * playerTargetX + playerTargetY * playerTargetY);
			if (npc.ai[1] > 0f && npc.ai[1] < Main.npc.Length) {
				try {
					npcCenter = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					playerTargetX = npc.Following().position.X + npc.Following().width / 2 - npcCenter.X;
					playerTargetY = npc.Following().position.Y + npc.Following().height / 2 - npcCenter.Y;
				} catch { }

				npc.rotation = (float)Math.Atan2(playerTargetY, playerTargetX) + 1.57f;
				distanceToPlayer = (float)Math.Sqrt(playerTargetX * playerTargetX + playerTargetY * playerTargetY);
				int npcWidth = npc.width;

				if (Main.getGoodWorld)
					npcWidth = 62;

				npcWidth = (int)(npcWidth * npc.scale);

				distanceToPlayer = (distanceToPlayer - npcWidth) / distanceToPlayer;
				playerTargetX *= distanceToPlayer;
				playerTargetY *= distanceToPlayer;
				npc.velocity = Vector2.Zero;
				npc.position.X += playerTargetX;
				npc.position.Y += playerTargetY;
			} else {
				if (!tooFarAway) {
					npc.TargetClosest(true);
					npc.velocity.Y += 0.11f;

					if (npc.velocity.Y > maxSpeed)
						npc.velocity.Y = maxSpeed;

					if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < maxSpeed * 0.4f) {
						if (npc.velocity.X < 0f)
							npc.velocity.X -= turnAcceleration * 1.1f;
						else
							npc.velocity.X += turnAcceleration * 1.1f;
					} else if (npc.velocity.Y == maxSpeed) {
						if (npc.velocity.X < playerTargetX)
							npc.velocity.X += turnAcceleration;
						else if (npc.velocity.X > playerTargetX)
							npc.velocity.X -= turnAcceleration;
					} else if (npc.velocity.Y > 4f) {
						if (npc.velocity.X < 0f)
							npc.velocity.X += turnAcceleration * 0.9f;
						else
							npc.velocity.X -= turnAcceleration * 0.9f;
					}
				} else {
					if (npc.soundDelay == 0) {
						float nextDelay = distanceToPlayer / 40f;
						if (nextDelay < 10f)
							nextDelay = 10f;
						if (nextDelay > 20f)
							nextDelay = 20f;

						npc.soundDelay = (int)nextDelay;
						SoundEngine.PlaySound(SoundID.WormDig, npc.position);
					}

					distanceToPlayer = (float)Math.Sqrt(playerTargetX * playerTargetX + playerTargetY * playerTargetY);
					float absTargetX = Math.Abs(playerTargetX);
					float absTargetY = Math.Abs(playerTargetY);
					float timeToReachTarget = maxSpeed / distanceToPlayer;
					playerTargetX *= timeToReachTarget;
					playerTargetY *= timeToReachTarget;
					bool attemptImmediateDespawn = false;
					if (npc.type == NPCID.EaterofWorldsHead && ((!npc.Target().ZoneCorrupt && !npc.Target().ZoneCrimson) || npc.Target().dead))
						attemptImmediateDespawn = true;

					if (attemptImmediateDespawn) {
						bool immediatelyDespawn = true;
						for (int num59 = 0; num59 < Main.maxPlayers; num59++) {
							if (Main.player[num59].active && !Main.player[num59].dead && Main.player[num59].ZoneCorrupt)
								immediatelyDespawn = false;
						}

						if (immediatelyDespawn) {
							if (Main.netMode != NetmodeID.MultiplayerClient && npc.position.Y / 16f > (Main.rockLayer + Main.maxTilesY) / 2.0) {
								npc.active = false;
								int num60 = (int)npc.ai[0];

								while (num60 > 0 && num60 < Main.maxNPCs && Main.npc[num60].active && Main.npc[num60].aiStyle == npc.aiStyle) {
									int num61 = (int)Main.npc[num60].ai[0];
									Main.npc[num60].active = false;
									npc.life = 0;

									if (Main.netMode == NetmodeID.Server)
										NetMessage.SendData(MessageID.SyncNPC, number: num60);

									num60 = num61;
								}

								if (Main.netMode == NetmodeID.Server)
									NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
							}
							playerTargetX = 0f;
							playerTargetY = maxSpeed;
						}
					}

					if ((npc.velocity.X > 0f && playerTargetX > 0f) || (npc.velocity.X < 0f && playerTargetX < 0f) || (npc.velocity.Y > 0f && playerTargetY > 0f) || (npc.velocity.Y < 0f && playerTargetY < 0f)) {
						if (npc.velocity.X < playerTargetX)
							npc.velocity.X += turnAcceleration;
						else if (npc.velocity.X > playerTargetX)
							npc.velocity.X -= turnAcceleration;

						if (npc.velocity.Y < playerTargetY)
							npc.velocity.Y += turnAcceleration;
						else if (npc.velocity.Y > playerTargetY)
							npc.velocity.Y -= turnAcceleration;

						if (Math.Abs(playerTargetY) < maxSpeed * 0.2 && ((npc.velocity.X > 0f && playerTargetX < 0f) || (npc.velocity.X < 0f && playerTargetX > 0f))) {
							if (npc.velocity.Y > 0f)
								npc.velocity.Y += turnAcceleration * 2f;
							else
								npc.velocity.Y -= turnAcceleration * 2f;
						}

						if (Math.Abs(playerTargetX) < maxSpeed * 0.2 && ((npc.velocity.Y > 0f && playerTargetY < 0f) || (npc.velocity.Y < 0f && playerTargetY > 0f))) {
							if (npc.velocity.X > 0f)
								npc.velocity.X += turnAcceleration * 2f;
							else
								npc.velocity.X -= turnAcceleration * 2f;
						}
					} else if (absTargetX > absTargetY) {
						if (npc.velocity.X < playerTargetX)
							npc.velocity.X += turnAcceleration * 1.1f;
						else if (npc.velocity.X > playerTargetX)
							npc.velocity.X -= turnAcceleration * 1.1f;

						if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < maxSpeed * 0.5) {
							if (npc.velocity.Y > 0f)
								npc.velocity.Y += turnAcceleration;
							else
								npc.velocity.Y -= turnAcceleration;
						}
					} else {
						if (npc.velocity.Y < playerTargetY)
							npc.velocity.Y += turnAcceleration * 1.1f;
						else if (npc.velocity.Y > playerTargetY)
							npc.velocity.Y -= turnAcceleration * 1.1f;

						if (Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < maxSpeed * 0.5) {
							if (npc.velocity.X > 0f)
								npc.velocity.X += turnAcceleration;
							else
								npc.velocity.X -= turnAcceleration;
						}
					}
				}

				npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + 1.57f;

				if (npc.type == NPCID.EaterofWorldsHead) {
					if (tooFarAway) {
						if (npc.localAI[0] != 1f)
							npc.netUpdate = true;

						npc.localAI[0] = 1f;
					} else {
						if (npc.localAI[0] != 0f)
							npc.netUpdate = true;

						npc.localAI[0] = 0f;
					}

					if (((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
						npc.netUpdate = true;
				}
			}
		}

		private static void EoW_RecalculateSegments(NPC npc, bool head) {
			int segments = 1;

			NPC headSegment, follower;
			if (head) {
				//Loop until the follower is a tail segment
				follower = npc.WormFollower();
				if (follower.type == NPCID.EaterofWorldsTail)
					segments = 2;
				else {
					while (follower.active && follower.type != NPCID.EaterofWorldsTail) {
						segments++;
						follower = follower.WormFollower();
					}

					segments++;
				}

				headSegment = npc;
			} else {
				//Loop until the following is a head segment
				NPC following = npc.Following();
				if (following.type == NPCID.EaterofWorldsHead)
					segments = 2;
				else {
					while (following.active && following.type != NPCID.EaterofWorldsHead) {
						segments++;
						following = following.Following();
					}

					segments++;
				}

				headSegment = following;
			}

			headSegment.Desomode().EoW_WormSegmentsCount = segments;

			//Loop through each segment and assign the segment count
			follower = headSegment.WormFollower();
			while (follower.active && follower.aiStyle == headSegment.aiStyle) {
				follower.Desomode().EoW_WormSegmentsCount = segments;
				follower = follower.WormFollower();
			}

			DetourNPCHelper.SendData(npc.whoAmI);
		}
	}
}
