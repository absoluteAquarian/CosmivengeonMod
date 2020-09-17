using CosmivengeonMod.NPCs.Desomode;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.Remoting.Messaging;
using Terraria;
using Terraria.ID;
using Terraria.Utilities;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		public const int EoW_SegmentType_SpitVileSpit = 0;
		public const int EoW_SegmentType_SpawnEaters = 1;
		public const int EoW_SegmentType_SpitCursedFlames = 2;

		/// <summary>
		/// Runs a modified AI for the Eater of Worlds
		/// <para>Note: This AI is not for The Destroyer.  See aiStyle 37 in the source for that.</para>
		/// </summary>
		public static void AI_EaterOfWorlds(NPC npc){
			//Normally i'd have a really long comment here explaining each npc.ai[] member but i can't be bothered to do that rn
			// TODO: do that

			//Only spit things if this segment isn't a head segment carrying a player
			if(Main.netMode != NetmodeID.MultiplayerClient){
				int spawnX = (int)(npc.position.X + npc.width / 2 + npc.velocity.X);
				int spawnY = (int)(npc.position.Y + npc.height / 2 + npc.velocity.Y);

				bool spawnVileSpit = npc.Helper().EoW_SegmentType == EoW_SegmentType_SpitVileSpit;
				Vector2 toPlayer = npc.DirectionTo(npc.Target().Center) * 7.5f;

				//Cursed flames spit should happen less often!
				if(npc.type == NPCID.EaterofWorldsBody && npc.position.Y / 16f < Main.worldSurface){
					if(Main.rand.Next(900) == 0){
						npc.TargetClosest(true);
						if(Collision.CanHitLine(npc.Center, 1, 1, npc.Target().Center, 1, 1)){
							if(spawnVileSpit)
								NPC.NewNPC(spawnX, spawnY, NPCID.VileSpit, ai1: 1f);
							else if(Main.rand.NextFloat() < 0.35f)
								Projectile.NewProjectile(spawnX, spawnY, toPlayer.X, toPlayer.Y, ProjectileID.CursedFlameHostile, CosmivengeonUtils.TrueDamage(60), 3.5f, Main.myPlayer);
						}
					}
				}else if(npc.type == NPCID.EaterofWorldsHead && npc.life > 0){	
					int num3 = 90;
					num3 += (int)(npc.life / (float)npc.lifeMax * 60f * 5f);
					if(Main.rand.Next(num3) == 0){
						npc.TargetClosest(true);
						if(Collision.CanHitLine(npc.Center, 1, 1, npc.Target().Center, 1, 1)){
							if(spawnVileSpit)
								NPC.NewNPC(spawnX, spawnY, NPCID.VileSpit, ai1: 1f);
							else if(Main.rand.NextFloat() < 0.35f)
								Projectile.NewProjectile(spawnX, spawnY, toPlayer.X, toPlayer.Y, ProjectileID.CursedFlameHostile, CosmivengeonUtils.TrueDamage(60), 3.5f, Main.myPlayer);
						}
					}
				}
			}

			npc.realLife = -1;

			if(npc.target < 0 || npc.target == 255 || npc.Target().dead){
				DetourNPCHelper.EoW_ResetGrab(npc, npc.Target());

				npc.TargetClosest(true);
			}

			if(npc.Target().dead || !npc.Target().ZoneCorrupt){
				if(npc.timeLeft > 300)
					npc.timeLeft = 300;
				npc.velocity.Y += 0.2f;
			}

			if(Main.netMode != NetmodeID.MultiplayerClient){
				if((npc.type == NPCID.EaterofWorldsHead || npc.type == NPCID.EaterofWorldsBody) && npc.ai[0] == 0f){
					WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
					wRand.Add(EoW_SegmentType_SpitVileSpit, 0.5);
					wRand.Add(EoW_SegmentType_SpawnEaters, 0.3);
					wRand.Add(EoW_SegmentType_SpitCursedFlames, 0.2);

					if(npc.type == NPCID.EaterofWorldsHead){
						//Vanilla segments range from 45 to 55 in Normal Mode and 49 to 60 segments in Expert Mode
						//Desolation Mode will have the segments range from 55 to 68
						npc.ai[2] = Main.rand.Next(55, 69);

						npc.Desomode().EoW_WormSegmentsCount = (int)npc.ai[2];
						npc.Helper().EoW_SegmentType = wRand.Get();

						npc.ai[0] = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type + 1, npc.whoAmI, 0f, 0f, 0f, 0f, 255);

						npc.Desomode().EoW_WormSegmentsCount = npc.WormFollower().Desomode().EoW_WormSegmentsCount;
					}else if(npc.type == NPCID.EaterofWorldsBody && npc.ai[2] > 0f){
						npc.ai[0] = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
						npc.Helper().EoW_SegmentType = wRand.Get();

						npc.Desomode().EoW_WormSegmentsCount = npc.WormFollower().Desomode().EoW_WormSegmentsCount;
					}else{
						npc.ai[0] = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height), npc.type + 1, npc.whoAmI, 0f, 0f, 0f, 0f, 255);
						npc.Helper().EoW_SegmentType = wRand.Get();

						npc.Desomode().EoW_WormSegmentsCount = npc.WormFollower().Desomode().EoW_WormSegmentsCount;
					}

					npc.WormFollower().ai[1] = npc.whoAmI;
					npc.WormFollower().ai[2] = npc.ai[2] - 1f;
					npc.netUpdate = true;
				}

				if(!npc.WormFollowing().active && !npc.WormFollower().active){
					npc.life = 0;
					npc.HitEffect(0, 10.0);
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.StrikeNPC, number: npc.whoAmI);
				}

				if(npc.type == NPCID.EaterofWorldsHead && !npc.WormFollower().active){
					npc.life = 0;
					npc.HitEffect(0, 10.0);
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.StrikeNPC, number: npc.whoAmI);
				}
					
				if(npc.type == NPCID.EaterofWorldsTail && !npc.WormFollowing().active){
					npc.life = 0;
					npc.HitEffect(0, 10.0);
					npc.checkDead();
					npc.active = false;
					NetMessage.SendData(MessageID.StrikeNPC, number: npc.whoAmI);
				}

				if(npc.type == NPCID.EaterofWorldsBody && (!npc.WormFollowing().active || npc.WormFollowing().aiStyle != npc.aiStyle)){
					npc.type = NPCID.EaterofWorldsHead;
					int whoAmI = npc.whoAmI;
					float num25 = npc.life / (float)npc.lifeMax;
					float num26 = npc.ai[0];
					int segment = npc.Helper().EoW_SegmentType;

					npc.SetDefaultsKeepPlayerInteraction(npc.type);
					npc.life = (int)(npc.lifeMax * num25);
					npc.ai[0] = num26;
					npc.TargetClosest(true);
					npc.netUpdate = true;
					npc.whoAmI = whoAmI;
					npc.Helper().EoW_SegmentType = segment;

					//Recalculate the amount of segments
					EoW_RecalculateSegments(npc, head: true);
				}

				if(npc.type == NPCID.EaterofWorldsBody && (!npc.WormFollower().active || npc.WormFollower().aiStyle != npc.aiStyle)){
					npc.type = NPCID.EaterofWorldsTail;
					int whoAmI2 = npc.whoAmI;
					float num27 = npc.life / (float)npc.lifeMax;
					float num28 = npc.ai[1];
					int segment = npc.Helper().EoW_SegmentType;

					npc.SetDefaultsKeepPlayerInteraction(npc.type);
					npc.life = (int)(npc.lifeMax * num27);
					npc.ai[1] = num28;
					npc.TargetClosest(true);
					npc.netUpdate = true;
					npc.whoAmI = whoAmI2;
					npc.Helper().EoW_SegmentType = segment;

					EoW_RecalculateSegments(npc, head: false);
				}
			}

			if(!npc.active && Main.netMode == NetmodeID.Server)
				NetMessage.SendData(MessageID.StrikeNPC, number: npc.whoAmI);

			int num29 = (int)(npc.position.X / 16f) - 1;
			int num30 = (int)((npc.position.X + npc.width) / 16f) + 2;
			int num31 = (int)(npc.position.Y / 16f) - 1;
			int num32 = (int)((npc.position.Y + npc.height) / 16f) + 2;
			if(num29 < 0)
				num29 = 0;
			if(num30 > Main.maxTilesX)
				num30 = Main.maxTilesX;
			if(num31 < 0)
				num31 = 0;
			if(num32 > Main.maxTilesY)
				num32 = Main.maxTilesY;

			bool flag2 = false;
			npc.Helper().Flag = false;
			for(int num33 = num29; num33 < num30; num33++){
				for(int num34 = num31; num34 < num32; num34++){
					if(CosmivengeonUtils.TileIsSolidOrPlatform(num33, num34)){
						Vector2 vector;
						vector.X = num33 * 16;
						vector.Y = num34 * 16;

						if(npc.position.X + npc.width > vector.X && npc.position.X < vector.X + 16f && npc.position.Y + npc.height > vector.Y && npc.position.Y < vector.Y + 16f){
							flag2 = true;
							if(Main.rand.Next(100) == 0 && Main.tile[num33, num34].nactive())
								WorldGen.KillTile(num33, num34, true, true, false);
						}

						//NPC is in the ground.  Set the flag to not damage a grabbed player
						Tile tile = Main.tile[num33, num34];

						if(tile.nactive() && tile.type != TileID.Platforms && !TileID.Sets.Platforms[tile.type])
							npc.Helper().Flag = true;
					}
				}
			}

			if(!flag2 && npc.type == NPCID.EaterofWorldsHead){
				Rectangle rectangle = new Rectangle((int)npc.position.X, (int)npc.position.Y, npc.width, npc.height);
				int num35 = 1000;
				bool flag3 = true;
				for(int num36 = 0; num36 < 255; num36++){
					if(Main.player[num36].active){
						Rectangle rectangle2 = new Rectangle((int)Main.player[num36].position.X - num35, (int)Main.player[num36].position.Y - num35, num35 * 2, num35 * 2);
						if(rectangle.Intersects(rectangle2)){
							flag3 = false;
							break;
						}
					}
				}

				if(flag3)
					flag2 = true;
			}

			if(npc.type == NPCID.EaterofWorldsHead)
				DetourNPCHelper.EoW_CheckGrabBite(npc);

			//speed, turn speed
			//Normal: 10, 0.07
			//Expert: 12, 0.15
			//Desomode: 16, 0.185
			float num37 = 8f;
			float num38 = 0.07f;
			if(npc.type == NPCID.EaterofWorldsHead){
				num37 = 16f;
				num38 = 0.185f;
			}

			Vector2 vector2 = npc.Center;
			float num40;
			float num41;

			if(DetourNPCHelper.EoW_GrabbingNPC == npc.whoAmI){
				num40 = npc.Desomode().EoW_GrabTarget?.X ?? npc.Target().Center.X;
				num41 = npc.Desomode().EoW_GrabTarget?.Y ?? npc.Target().Center.Y;
			}else{
				num40 = npc.Target().Center.X;
				num41 = npc.Target().Center.Y;
			}
			
			num40 = (int)(num40 / 16f) * 16;
			num41 = (int)(num41 / 16f) * 16;
			vector2.X = (int)(vector2.X / 16f) * 16;
			vector2.Y = (int)(vector2.Y / 16f) * 16;
			num40 -= vector2.X;
			num41 -= vector2.Y;

			float num53 = (float)Math.Sqrt(num40 * num40 + num41 * num41);
			if(npc.ai[1] > 0f && npc.ai[1] < Main.npc.Length){
				try{
					vector2 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					num40 = npc.WormFollowing().position.X + npc.WormFollowing().width / 2 - vector2.X;
					num41 = npc.WormFollowing().position.Y + npc.WormFollowing().height / 2 - vector2.Y;
				}catch{ }

				npc.rotation = (float)Math.Atan2(num41, num40) + 1.57f;
				num53 = (float)Math.Sqrt(num40 * num40 + num41 * num41);
				int num54 = npc.width;

				num54 = (int)(num54 * npc.scale);

				num53 = (num53 - num54) / num53;
				num40 *= num53;
				num41 *= num53;
				npc.velocity = Vector2.Zero;
				npc.position.X += num40;
				npc.position.Y += num41;
			}else{
				if(!flag2){
					npc.TargetClosest(true);
					npc.velocity.Y += 0.11f;

					if(npc.velocity.Y > num37)
						npc.velocity.Y = num37;

					if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num37 * 0.4f){
						if(npc.velocity.X < 0f)
							npc.velocity.X -= num38 * 1.1f;
						else
							npc.velocity.X += num38 * 1.1f;
					}else if(npc.velocity.Y == num37){
						if(npc.velocity.X < num40)
							npc.velocity.X += num38;
						else if(npc.velocity.X > num40)
							npc.velocity.X -= num38;
					}else if(npc.velocity.Y > 4f){
						if(npc.velocity.X < 0f)
							npc.velocity.X += num38 * 0.9f;
						else
							npc.velocity.X -= num38 * 0.9f;
					}
				}else{
					if(npc.soundDelay == 0){
						float num55 = num53 / 40f;
						if(num55 < 10f)
							num55 = 10f;
						if(num55 > 20f)
							num55 = 20f;

						npc.soundDelay = (int)num55;
						Main.PlaySound(SoundID.Roar, (int)npc.position.X, (int)npc.position.Y, 1, 1f, 0f);
					}

					num53 = (float)Math.Sqrt(num40 * num40 + num41 * num41);
					float num56 = Math.Abs(num40);
					float num57 = Math.Abs(num41);
					float num58 = num37 / num53;
					num40 *= num58;
					num41 *= num58;
					bool flag4 = false;
					if(npc.type == NPCID.EaterofWorldsHead && ((!npc.Target().ZoneCorrupt && !npc.Target().ZoneCrimson) || npc.Target().dead))
						flag4 = true;

					if(flag4){
						bool flag5 = true;
						for(int num59 = 0; num59 < 255; num59++)
							if(Main.player[num59].active && !Main.player[num59].dead && Main.player[num59].ZoneCorrupt)
								flag5 = false;

						if(flag5){
							if(Main.netMode != NetmodeID.MultiplayerClient && npc.position.Y / 16f > (Main.rockLayer + Main.maxTilesY) / 2.0){
								npc.active = false;
								int num60 = (int)npc.ai[0];

								while(num60 > 0 && num60 < 200 && Main.npc[num60].active && Main.npc[num60].aiStyle == npc.aiStyle){
									int num61 = (int)Main.npc[num60].ai[0];
									Main.npc[num60].active = false;
									npc.life = 0;

									if(Main.netMode == NetmodeID.Server)
										NetMessage.SendData(MessageID.SyncNPC, number: num60);

									num60 = num61;
								}

								if(Main.netMode == NetmodeID.Server)
									NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
							}
							num40 = 0f;
							num41 = num37;
						}
					}

					if((npc.velocity.X > 0f && num40 > 0f) || (npc.velocity.X < 0f && num40 < 0f) || (npc.velocity.Y > 0f && num41 > 0f) || (npc.velocity.Y < 0f && num41 < 0f)){
						if(npc.velocity.X < num40)
							npc.velocity.X += num38;
						else if(npc.velocity.X > num40)
							npc.velocity.X -= num38;

						if(npc.velocity.Y < num41)
							npc.velocity.Y += num38;
						else if(npc.velocity.Y > num41)
							npc.velocity.Y -= num38;

						if(Math.Abs(num41) < num37 * 0.2 && ((npc.velocity.X > 0f && num40 < 0f) || (npc.velocity.X < 0f && num40 > 0f))){
							if(npc.velocity.Y > 0f)
								npc.velocity.Y += num38 * 2f;
							else
								npc.velocity.Y -= num38 * 2f;
						}

						if(Math.Abs(num40) < num37 * 0.2 && ((npc.velocity.Y > 0f && num41 < 0f) || (npc.velocity.Y < 0f && num41 > 0f))){
							if(npc.velocity.X > 0f)
								npc.velocity.X += num38 * 2f;
							else
								npc.velocity.X -= num38 * 2f;
						}
					}else if(num56 > num57){
						if(npc.velocity.X < num40)
							npc.velocity.X += num38 * 1.1f;
						else if(npc.velocity.X > num40)
							npc.velocity.X -= num38 * 1.1f;

						if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num37 * 0.5){
							if(npc.velocity.Y > 0f)
								npc.velocity.Y += num38;
							else
								npc.velocity.Y -= num38;
						}
					}else{
						if(npc.velocity.Y < num41)
							npc.velocity.Y += num38 * 1.1f;
						else if(npc.velocity.Y > num41)
							npc.velocity.Y -= num38 * 1.1f;

						if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num37 * 0.5){
							if(npc.velocity.X > 0f)
								npc.velocity.X += num38;
							else
								npc.velocity.X -= num38;
						}
					}
				}

				npc.rotation = (float)Math.Atan2(npc.velocity.Y, npc.velocity.X) + 1.57f;

				if(npc.type == NPCID.EaterofWorldsHead){
					if(flag2){
						if(npc.localAI[0] != 1f)
							npc.netUpdate = true;

						npc.localAI[0] = 1f;
					}else{
						if(npc.localAI[0] != 0f)
							npc.netUpdate = true;

						npc.localAI[0] = 0f;
					}

					if(((npc.velocity.X > 0f && npc.oldVelocity.X < 0f) || (npc.velocity.X < 0f && npc.oldVelocity.X > 0f) || (npc.velocity.Y > 0f && npc.oldVelocity.Y < 0f) || (npc.velocity.Y < 0f && npc.oldVelocity.Y > 0f)) && !npc.justHit)
						npc.netUpdate = true;
				}
			}
		}

		private static void EoW_RecalculateSegments(NPC npc, bool head){
			int segments = 1;

			NPC headSegment, follower;
			if(head){
				//Loop until the follower is a tail segment
				follower = npc.WormFollower();
				if(follower.type == NPCID.EaterofWorldsTail)
					segments = 2;
				else{
					while(follower.active && follower.type != NPCID.EaterofWorldsTail){
						segments++;
						follower = follower.WormFollower();
					}

					segments++;
				}

				headSegment = npc;
			}else{
				//Loop until the following is a head segment
				NPC following = npc.WormFollowing();
				if(following.type == NPCID.EaterofWorldsHead)
					segments = 2;
				else{
					while(following.active && following.type != NPCID.EaterofWorldsHead){
						segments++;
						following = following.WormFollowing();
					}

					segments++;
				}

				headSegment = following;
			}

			headSegment.Desomode().EoW_WormSegmentsCount = segments;

			//Loop through each segment and assign the segment count
			follower = headSegment.WormFollower();
			while(follower.active && follower.aiStyle == headSegment.aiStyle){
				follower.Desomode().EoW_WormSegmentsCount = segments;
				follower = follower.WormFollower();
			}
		}
	}
}
