using CosmivengeonMod.NPCs.Desomode;
using CosmivengeonMod.Projectiles.Desomode;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		/// <summary>
		/// Runs a modified AI for Queen Bee
		/// </summary>
		public static void AI_QueenBee(NPC npc){
			/*  AI Notes:
			 *  ai[0] == -1 | Choosing an attack
			 *  ai[0] == 0  | Hovering around; Charge at the player if in line
			 *  ai[0] == 1  | Spwawning bees
			 *  ai[0] == 2  | Hovering around
			 *  ai[0] == 3  | Shooting stingers while moving
			 */

			int nearbyPlayers = 0;
			for(int i = 0; i < Main.maxPlayers; i++){
				Player plr = Main.player[i];
				if(plr.active && !plr.dead && npc.DistanceSQ(plr.Center) < 1000f * 1000f)
					nearbyPlayers++;
			}

			//Expert mode gradually increases QG's defense by 20
			//Desomode will gradually increase it by 30 instead!
			int num599 = (int)(30f * (1f - npc.life / (float)npc.lifeMax));
			npc.defense = npc.defDefense + num599;

			int timer2Max = 30;

			//Wait after getting enraged
			if(npc.Helper().Timer2 > 0){
				npc.Helper().Timer2--;

				Vector2 oldSize = npc.Size;

				npc.scale += 0.3f / timer2Max;
				npc.Size = npc.Desomode().QB_baseSize * (1f + npc.scale - npc.Desomode().QB_baseScale);
				
				npc.position -= npc.Size - oldSize;

				return;
			}

			//Aura timer
			if(npc.Helper().Flag)
				npc.Helper().Timer++;

			//Aura check
			if(npc.life < npc.lifeMax * 0.25f && !npc.Helper().Flag && npc.ai[0] != 0f){
				npc.Helper().Flag = true;

				Main.PlaySound(SoundID.ForceRoar, npc.Center, 0);

				npc.Helper().Timer2 = timer2Max;

				npc.ai[0] = 0f;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;

				return;
			}

			if(npc.target < 0 || npc.target == 255 || npc.Target().dead || !npc.Target().active)
				npc.TargetClosest();

			if(npc.Target().dead){
				if(npc.position.Y < Main.worldSurface * 16 + 2000)
					npc.velocity.Y += 0.04f;

				if(npc.position.X < Main.maxTilesX * 8)
					npc.velocity.X -= 0.04f;
				else
					npc.velocity.X += 0.04f;

				if(npc.timeLeft > 10)
					npc.timeLeft = 10;
			}else if(npc.ai[0] == -1f){
				if(Main.netMode == NetmodeID.MultiplayerClient)
					return;

				float num600 = npc.ai[1];
				int num601;
				do{
					num601 = Main.rand.Next(3);
					switch (num601){
						case 1:
							num601 = 2;
							break;
						case 2:
							num601 = 3;
							break;
					}
				}while(num601 == num600);

				npc.ai[0] = num601;
				npc.ai[1] = 0f;
				npc.ai[2] = 0f;
			}else if(npc.ai[0] == 0f){
				int num602 = 2;
				if(npc.life < npc.lifeMax / 2)
					num602++;
				if(npc.life < npc.lifeMax / 3)
					num602++;
				if(npc.life < npc.lifeMax / 5)
					num602++;

				if(npc.ai[1] > 2 * num602 && npc.ai[1] % 2f == 0f){
					//Choose another attack

					npc.ai[0] = -1f;
					npc.ai[1] = 0f;
					npc.ai[2] = 0f;
					npc.netUpdate = true;
					return;
				}

				if(npc.ai[1] % 2f == 0f){
					npc.TargetClosest();

					npc.Helper().Timer3++;

					//Expert: Charge only if the boss's Y-position is within 20 pixels (1.25 tiles) of the player's Y-position
					//Desomode: The same, but the range is increased within 6 tiles of the player AND the boss is at least 20 tiles away from the player
					if(npc.Helper().Timer3 > 90 || (Math.Abs(npc.Center.Y - npc.Target().Center.Y) < 6f * 16 && Math.Abs(npc.Center.X - npc.Target().Center.X) > 20f * 16)){
						npc.Helper().Timer3 = 0;

						npc.localAI[0] = 1f;
						npc.ai[1] += 1f;
						npc.ai[2] = 0f;

						//Setting the hover speed
						//Vanilla increases it by 2 per check.  Desomode increases it by 3 instead
						float num603 = 16f;
						if(npc.life < npc.lifeMax * 0.75)
							num603 += 3f;
						if(npc.life < npc.lifeMax * 0.5)
							num603 += 3f;
						if(npc.life < npc.lifeMax * 0.25)
							num603 += 3f;
						if(npc.life < npc.lifeMax * 0.1)
							num603 += 3f;

						npc.velocity = npc.DirectionTo(npc.Target().Center) * num603;

						npc.spriteDirection = npc.direction;

						if(!npc.Helper().Flag2){
							npc.Helper().Flag2 = true;
							Main.PlaySound(SoundID.Roar, npc.position);
						}else
							Main.PlaySound(SoundID.ForceRoar, npc.position, npc.Helper().Flag ? -1 : 0);

						return;
					}

					npc.localAI[0] = 0f;

					//Ascend/descend faster depending on how much health the boss has left
					float num607 = 12f;
					float num608 = 0.2f;
					if(npc.life < npc.lifeMax * 0.75){
						num607 += 1f;
						num608 += 0.1f;
					}
					if(npc.life < npc.lifeMax * 0.5){
						num607 += 1f;
						num608 += 0.1f;
					}
					if(npc.life < npc.lifeMax * 0.25){
						num607 += 2f;
						num608 += 0.1f;
					}
					if(npc.life < npc.lifeMax * 0.1){
						num607 += 2f;
						num608 += 0.2f;
					}

					//Move up or down depending on where the NPC is relative to the player
					if(npc.position.Y + npc.height / 2 < npc.Target().position.Y + npc.Target().height / 2)
						npc.velocity.Y += num608;
					else
						npc.velocity.Y -= num608;

					if(npc.velocity.Y < -num607)
						npc.velocity.Y = -num607;

					if(npc.velocity.Y > num607)
						npc.velocity.Y = num607;

					if(Math.Abs(npc.position.X + npc.width / 2 - (npc.Target().position.X + npc.Target().width / 2)) > 600f)
						npc.velocity.X += 0.15f * npc.direction;
					else if(Math.Abs(npc.position.X + npc.width / 2 - (npc.Target().position.X + npc.Target().width / 2)) < 300f)
						npc.velocity.X -= 0.15f * npc.direction;
					else
						npc.velocity.X *= 0.8f;

					if(npc.velocity.X < -16f)
						npc.velocity.X = -16f;

					if(npc.velocity.X > 16f)
						npc.velocity.X = 16f;

					npc.spriteDirection = npc.direction;
					return;
				}

				if(npc.velocity.X < 0f)
					npc.direction = -1;
				else
					npc.direction = 1;

				npc.spriteDirection = npc.direction;
				int num609 = 600;
				if(npc.life < npc.lifeMax * 0.1)
					num609 = 300;
				else if(npc.life < npc.lifeMax * 0.25)
					num609 = 450;
				else if(npc.life < npc.lifeMax * 0.5)
					num609 = 500;
				else if(npc.life < npc.lifeMax * 0.75)
					num609 = 550;

				int num610 = 1;
				if(npc.Center.X < npc.Target().Center.X)
					num610 = -1;

				if(npc.direction == num610 && npc.DistanceSQ(npc.Target().Center) > num609 * num609)
					npc.ai[2] = 1f;

				if(npc.ai[2] == 1f){
					npc.TargetClosest();
					npc.spriteDirection = npc.direction;
					npc.localAI[0] = 0f;
					npc.velocity *= 0.9f;

					float num611 = 0.1f;
					if(npc.life < npc.lifeMax / 2){
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if(npc.life < npc.lifeMax / 3){
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if(npc.life < npc.lifeMax / 5){
						npc.velocity *= 0.9f;
						num611 += 0.05f;
					}

					if(Math.Abs(npc.velocity.X) + Math.Abs(npc.velocity.Y) < num611){
						npc.ai[2] = 0f;
						npc.ai[1] += 1f;
					}
				}else
					npc.localAI[0] = 1f;
			}else if(npc.ai[0] == 2f){
				npc.TargetClosest();
				npc.spriteDirection = npc.direction;
				float num613 = 0.1f;

				Vector2 vector72 = npc.Center;
				float num614 = npc.Target().position.X + npc.Target().width / 2 - vector72.X;
				float num615 = npc.Target().position.Y + npc.Target().height / 2 - 200f - vector72.Y;
				float num616 = (float)Math.Sqrt(num614 * num614 + num615 * num615);
				if(num616 < 200f){
					npc.ai[0] = 1f;
					npc.ai[1] = 0f;
					npc.netUpdate = true;
					return;
				}

				if(npc.velocity.X < num614){
					npc.velocity.X += num613;
					if(npc.velocity.X < 0f && num614 > 0f)
						npc.velocity.X += num613;
				}else if(npc.velocity.X > num614){
					npc.velocity.X -= num613;
					if(npc.velocity.X > 0f && num614 < 0f)
						npc.velocity.X -= num613;
				}

				if(npc.velocity.Y < num615){
					npc.velocity.Y += num613;
					if(npc.velocity.Y < 0f && num615 > 0f)
						npc.velocity.Y += num613;
				}else if(npc.velocity.Y > num615){
					npc.velocity.Y -= num613;
					if(npc.velocity.Y > 0f && num615 < 0f)
						npc.velocity.Y -= num613;
				}
			}else if(npc.ai[0] == 1f){
				npc.localAI[0] = 0f;
				npc.TargetClosest();

				Vector2 vector73 = new Vector2(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);
				Vector2 vector74 = npc.Center;
				float num617 = npc.Target().position.X + npc.Target().width / 2 - vector74.X;
				float num618 = npc.Target().position.Y + npc.Target().height / 2 - vector74.Y;
				float num619 = (float)Math.Sqrt(num617 * num617 + num618 * num618);

				npc.ai[1] += 1f;

				npc.ai[1] += nearbyPlayers / 2;
				if(npc.life < npc.lifeMax * 0.75)
					npc.ai[1] += 0.25f;

				if(npc.life < npc.lifeMax * 0.5)
					npc.ai[1] += 0.25f;

				if(npc.life < npc.lifeMax * 0.25)
					npc.ai[1] += 0.25f;

				if(npc.life < npc.lifeMax * 0.1)
					npc.ai[1] += 0.25f;

				bool flag36 = false;
				if(npc.ai[1] > 20f){
					npc.ai[1] = 0f;
					npc.ai[2]++;
					flag36 = true;
				}

				//Spawn the bees/hornets
				//Boss ignores any tile collision checks
				if(flag36){
					Main.PlaySound(SoundID.NPCHit, (int)npc.position.X, (int)npc.position.Y);

					if(Main.netMode != NetmodeID.MultiplayerClient){
						WeightedRandom<int> wRand = new WeightedRandom<int>(Main.rand);
						wRand.Add(ModContent.NPCType<ModifiedHornet>(), 0.18);
						wRand.Add(NPCID.Bee, 0.82 / 2);
						wRand.Add(NPCID.BeeSmall, 0.82 / 2);

						int num621 = NPC.NewNPC((int)vector73.X, (int)vector73.Y, wRand.Get());
						Main.npc[num621].velocity = npc.DirectionTo(npc.Target().Center).RotateDegrees(rotateByDegrees: 0, rotateByRandomDegrees: 30) * Main.rand.NextFloat(3f, 6f);
						Main.npc[num621].netUpdate = true;
						Main.npc[num621].localAI[0] = 60f;
						Main.npc[num621].noTileCollide = true;
					}
				}

				//Move towards the player if they're too far away or the boss can't see them
				if(num619 > 400f || !Collision.CanHit(new Vector2(vector73.X, vector73.Y - 30f), 1, 1, npc.Target().position, npc.Target().width, npc.Target().height)){
					float num623 = 0.1f;
					vector74 = vector73;
					num617 = npc.Target().position.X + npc.Target().width / 2 - vector74.X;
					num618 = npc.Target().position.Y + npc.Target().height / 2 - vector74.Y;

					if(npc.velocity.X < num617){
						npc.velocity.X += num623;
						if(npc.velocity.X < 0f && num617 > 0f)
							npc.velocity.X += num623;
					}else if(npc.velocity.X > num617){
						npc.velocity.X -= num623;
						if(npc.velocity.X > 0f && num617 < 0f)
							npc.velocity.X -= num623;
					}

					if(npc.velocity.Y < num618){
						npc.velocity.Y += num623;
						if(npc.velocity.Y < 0f && num618 > 0f)
							npc.velocity.Y += num623;
					}else if(npc.velocity.Y > num618){
						npc.velocity.Y -= num623;
						if(npc.velocity.Y > 0f && num618 < 0f)
							npc.velocity.Y -= num623;
					}
				}else
					npc.velocity *= 0.9f;

				npc.spriteDirection = npc.direction;
				if(npc.ai[2] > 10f){
					//Choose another attack

					npc.ai[0] = -1f;
					npc.ai[1] = 1f;
					npc.netUpdate = true;
				}
			}else if(npc.ai[0] == 3f){
				float num625 = 0.075f;

				Vector2 vector75 = new Vector2(npc.position.X + npc.width / 2 + Main.rand.Next(20) * npc.direction, npc.position.Y + npc.height * 0.8f);
				Vector2 vector76 = npc.Center;
				float num626 = npc.Target().position.X + npc.Target().width / 2 - vector76.X;
				float num627 = npc.Target().position.Y + npc.Target().height / 2 - 300f - vector76.Y;
				float num628 = (float)Math.Sqrt(num626 * num626 + num627 * num627);

				npc.ai[1] += 1f;

				//Desomode change: boss shoots stingers at the next-fastest rate until enraged, then shoots at the fastest rate
				bool flag37 = false;
				if(npc.Helper().Flag){
					if(npc.ai[1] % 15f == 14f)
						flag37 = true;
				}else if(npc.ai[1] % 25f == 24f)
					flag37 = true;

				//Spawn stingers
				//Boss ignores any tile collision checks
				if(flag37 && npc.position.Y + npc.height < npc.Target().position.Y){
					Main.PlaySound(SoundID.Item17, npc.position);
					if(Main.netMode != NetmodeID.MultiplayerClient){
						float num629 = 10f;

						if(npc.life < npc.lifeMax * 0.1f)
							num629 += 3f;

						Vector2 shootTarget = npc.Target().position - vector75 + new Vector2(Main.rand.Next(-80, 81), Main.rand.Next(-40, 41));
						Vector2 targetVelocity = npc.Target().velocity * 60 * 0.1f;
						//Make the stingers aim slightly towards where the player will probably be in ~0.1 seconds
						shootTarget += targetVelocity;
						shootTarget = Vector2.Normalize(shootTarget) * num629;

						int num633 = CosmivengeonUtils.TrueDamage(80);
						int num634 = ProjectileID.Stinger;
						if(Main.rand.NextFloat() < 0.085f){
							//Recalculate the velocity without the randomness
							shootTarget = npc.Target().position - vector75;
							shootTarget += targetVelocity;
							shootTarget = Vector2.Normalize(shootTarget) * num629;

							num634 = ModContent.ProjectileType<QueenBeeHoneyShot>();
							shootTarget *= 1.8f;
							num633 = CosmivengeonUtils.TrueDamage(150);

							npc.ai[1] += 9f;
						}

						int num635 = Projectile.NewProjectile(vector75, shootTarget, num634, num633, 0f, Main.myPlayer);
						Main.projectile[num635].timeLeft = 300;
						Main.projectile[num635].tileCollide = false;
					}
				}

				if(!Collision.CanHit(new Vector2(vector75.X, vector75.Y - 30f), 1, 1, npc.Target().position, npc.Target().width, npc.Target().height)){
					if(npc.velocity.X < num626){
						npc.velocity.X += num625;
						if(npc.velocity.X < 0f && num626 > 0f)
							npc.velocity.X += num625;
					}else if(npc.velocity.X > num626){
						npc.velocity.X -= num625;
						if(npc.velocity.X > 0f && num626 < 0f)
							npc.velocity.X -= num625;
					}

					if(npc.velocity.Y < num627){
						npc.velocity.Y += num625;
						if(npc.velocity.Y < 0f && num627 > 0f)
							npc.velocity.Y += num625;
					}else if(npc.velocity.Y > num627){
						npc.velocity.Y -= num625;
						if(npc.velocity.Y > 0f && num627 < 0f)
							npc.velocity.Y -= num625;
					}
				}else if(num628 > 100f){
					npc.TargetClosest();
					npc.spriteDirection = npc.direction;
					if(npc.velocity.X < num626){
						npc.velocity.X += num625;
						if(npc.velocity.X < 0f && num626 > 0f)
							npc.velocity.X += num625 * 2f;
					}else if(npc.velocity.X > num626){
						npc.velocity.X -= num625;
						if(npc.velocity.X > 0f && num626 < 0f)
							npc.velocity.X -= num625 * 2f;
					}

					if(npc.velocity.Y < num627){
						npc.velocity.Y += num625;
						if(npc.velocity.Y < 0f && num627 > 0f)
							npc.velocity.Y += num625 * 2f;
					}else if(npc.velocity.Y > num627){
						npc.velocity.Y -= num625;
						if(npc.velocity.Y > 0f && num627 < 0f)
							npc.velocity.Y -= num625 * 2f;
					}
				}

				if(npc.ai[1] > 800f){
					//Choose another attack

					npc.ai[0] = -1f;
					npc.ai[1] = 3f;
					npc.netUpdate = true;
				}
			}
		}
	}
}
