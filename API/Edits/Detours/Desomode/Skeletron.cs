using CosmivengeonMod.Projectiles.Desomode;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.API.Edits.Detours.Desomode{
	public static partial class DesolationModeBossAI{
		/// <summary>
		/// Runs a modified AI for Skeletron
		/// </summary>
		public static void AI_SkeletronHead(NPC npc){
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

			npc.defense = npc.defDefense;

			//Spawn these hands
			if((npc.ai[0] == 0f || (npc.ai[0] == 1f && npc.life < npc.lifeMax * 0.25f && npc.ai[1] == 0f)) && Main.netMode != NetmodeID.MultiplayerClient){
				npc.TargetClosest();
				npc.ai[0]++;

				if(npc.ai[0] == 2f)
					Main.PlaySound(SoundID.Roar, npc.Center, 0);

				int num154 = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)npc.position.Y + npc.height / 2, 36, npc.whoAmI);
				Main.npc[num154].ai[0] = -1f;
				Main.npc[num154].ai[1] = npc.whoAmI;
				Main.npc[num154].target = npc.target;
				Main.npc[num154].netUpdate = true;
				Main.npc[num154].Helper().Timer = Main.rand.Next(30, 51);

				DetourNPCHelper.SendData(num154);

				if(npc.ai[0] == 2f)
					Main.npc[num154].life = 500;

				num154 = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)npc.position.Y + npc.height / 2, 36, npc.whoAmI);
				Main.npc[num154].ai[0] = 1f;
				Main.npc[num154].ai[1] = npc.whoAmI;
				Main.npc[num154].ai[3] = 150f;
				Main.npc[num154].target = npc.target;
				Main.npc[num154].netUpdate = true;
				Main.npc[num154].Helper().Timer = Main.rand.Next(30, 51);

				if(npc.ai[0] == 2f)
					Main.npc[num154].life = 500;

				DetourNPCHelper.SendData(num154);
			}

			//Target is dead or too far away
			if(npc.Target().dead || Math.Abs(npc.position.X - npc.Target().position.X) > 2000f || Math.Abs(npc.position.Y - npc.Target().position.Y) > 2000f){
				npc.TargetClosest();
				if(npc.Target().dead || Math.Abs(npc.position.X - npc.Target().position.X) > 2000f || Math.Abs(npc.position.Y - npc.Target().position.Y) > 2000f)
					npc.ai[1] = 3f;
			}

			//It's daytime and the boss isn't already spinning
			//Get A N G E R Y
			if(Main.dayTime && npc.ai[1] != 3f && npc.ai[1] != 2f){
				npc.ai[1] = 2f;
				Main.PlaySound(SoundID.Roar, (int)npc.position.X, (int)npc.position.Y, 0);
			}

			int num155 = 0;
			for(int num156 = 0; num156 < 200; num156++){
				if(Main.npc[num156].active && Main.npc[num156].type == npc.type + 1)
					num155++;
			}

			npc.defense += num155 * 25;
			if((num155 < 2 || npc.life < npc.lifeMax * 0.75) && npc.ai[1] == 0f){
				float num157 = 150;
				if(num155 == 0)
					num157 = 60;
				if(num155 != 0 && npc.life < npc.lifeMax * 0.25f)
					num157 = 100;

				if(Main.netMode != NetmodeID.MultiplayerClient && npc.ai[2] % num157 == 0f){
					Vector2 center3 = npc.Center;
					if(Collision.CanHit(center3, 1, 1, npc.Target().position, npc.Target().width, npc.Target().height)){
						float num161 = 3f;
						if(num155 == 0)
							num161 += 2f;

						float num162 = npc.Target().position.X + npc.Target().width * 0.5f - center3.X + Main.rand.Next(-20, 21);
						float num163 = npc.Target().position.Y + npc.Target().height * 0.5f - center3.Y + Main.rand.Next(-20, 21);
						float num164 = (float)Math.Sqrt(num162 * num162 + num163 * num163);
						num164 = num161 / num164;
						num162 *= num164;
						num163 *= num164;

						Vector2 vector16 = new Vector2(num162 * 1f + Main.rand.Next(-50, 51) * 0.01f, num163 * 1f + Main.rand.Next(-50, 51) * 0.01f);
						vector16.Normalize();
						vector16 *= num161;
						vector16 += npc.velocity;
						num162 = vector16.X;
						num163 = vector16.Y;
						int num165 = MiscUtils.TrueDamage(90);
						int num166 = ProjectileID.Skull;
						center3 += vector16 * 5f;

						int num167 = Projectile.NewProjectile(center3.X, center3.Y, num162, num163, num166, num165, 0f, Main.myPlayer, -1f);
						Main.projectile[num167].timeLeft = 300;
					}
				}
			}

			if(npc.ai[1] == 0f){
				npc.damage = npc.defDamage;
				npc.ai[2] += 1f;
				//800 ticks --> 240 ticks
				if(npc.ai[2] >= 800f - 560f * (1f - (float)npc.life / npc.lifeMax)){
					npc.ai[2] = 0f;
					npc.ai[1] = 1f;
					npc.TargetClosest();
					npc.netUpdate = true;
				}

				npc.rotation = npc.velocity.X / 15f;
				//Normal: 0.02, 2, 0.05, 8
				//Expert: 0.03, 4, 0.07, 9.5
				float num168 = 0.04f;
				float num169 = 8f;
				float num170 = 0.1f;
				float num171 = 12f;

				if(npc.position.Y > npc.Target().position.Y - 250f){
					if(npc.velocity.Y > 0f)
						npc.velocity.Y *= 0.98f;

					npc.velocity.Y -= num168;
					if(npc.velocity.Y > num169)
						npc.velocity.Y = num169;
				}else if(npc.position.Y < npc.Target().position.Y - 250f){
					if(npc.velocity.Y < 0f)
						npc.velocity.Y *= 0.98f;

					npc.velocity.Y += num168;
					if(npc.velocity.Y < 0f - num169)
						npc.velocity.Y = 0f - num169;
				}

				if(npc.position.X + npc.width / 2 > npc.Target().position.X + npc.Target().width / 2){
					if(npc.velocity.X > 0f)
						npc.velocity.X *= 0.98f;

					npc.velocity.X -= num170;
					if(npc.velocity.X > num171)
						npc.velocity.X = num171;
				}

				if(npc.position.X + npc.width / 2 < npc.Target().position.X + npc.Target().width / 2){
					if(npc.velocity.X < 0f)
						npc.velocity.X *= 0.98f;

					npc.velocity.X += num170;
					if(npc.velocity.X < 0f - num171)
						npc.velocity.X = 0f - num171;
				}
			}else if(npc.ai[1] == 1f){
				npc.defense -= npc.defDefense;
				npc.ai[2] += 1f;
				if(npc.ai[2] == 2f)
					Main.PlaySound(SoundID.Roar, (int)npc.position.X, (int)npc.position.Y, 0);

				//300 ticks --> 120 ticks
				if(npc.ai[2] >= 300f - 180f * (1f - (float)npc.life / npc.lifeMax)){
					npc.ai[2] = 0f;
					npc.ai[1] = 0f;
				}

				npc.rotation += npc.direction * 0.3f;
				Vector2 vector17 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num172 = npc.Target().position.X + npc.Target().width / 2 - vector17.X;
				float num173 = npc.Target().position.Y + npc.Target().height / 2 - vector17.Y;
				float num174 = (float)Math.Sqrt(num172 * num172 + num173 * num173);
				float num175 = 5.75f;

				npc.damage = (int)(npc.defDamage * 1.3);
				if(num174 > 150f)
					num175 *= 1.05f;
				if(num174 > 200f)
					num175 *= 1.1f;
				if(num174 > 250f)
					num175 *= 1.1f;
				if(num174 > 300f)
					num175 *= 1.1f;
				if(num174 > 350f)
					num175 *= 1.1f;
				if(num174 > 400f)
					num175 *= 1.1f;
				if(num174 > 450f)
					num175 *= 1.1f;
				if(num174 > 500f)
					num175 *= 1.1f;
				if(num174 > 550f)
					num175 *= 1.1f;
				if(num174 > 600f)
					num175 *= 1.1f;

				switch (num155){
					case 0:
						num175 *= 1.2f;
						break;
					case 1:
						num175 *= 1.1f;
						break;
				}

				num174 = num175 / num174;
				npc.velocity.X = num172 * num174;
				npc.velocity.Y = num173 * num174;
			}else if(npc.ai[1] == 2f){
				npc.damage = 1000;
				npc.defense = 9999;

				npc.rotation += npc.direction * 0.3f;

				Vector2 vector18 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num176 = npc.Target().position.X + npc.Target().width / 2 - vector18.X;
				float num177 = npc.Target().position.Y + npc.Target().height / 2 - vector18.Y;
				float num178 = (float)Math.Sqrt(num176 * num176 + num177 * num177);

				num178 = 8f / num178;
				npc.velocity.X = num176 * num178;
				npc.velocity.Y = num177 * num178;
			}else if(npc.ai[1] == 3f){
				npc.velocity.Y += 0.1f;
				if(npc.velocity.Y < 0f)
					npc.velocity.Y *= 0.95f;

				npc.velocity.X *= 0.95f;
				if(npc.timeLeft > 50)
					npc.timeLeft = 50;
			}

			if(npc.ai[1] != 2f && npc.ai[1] != 3f && num155 != 0){
				int num179 = Dust.NewDust(new Vector2(npc.position.X + npc.width / 2 - 15f - npc.velocity.X * 5f, npc.position.Y + npc.height - 2f), 30, 10, 5, (0f - npc.velocity.X) * 0.2f, 3f, 0, default, 2f);
				Main.dust[num179].noGravity = true;
				Main.dust[num179].velocity.X *= 1.3f;
				Main.dust[num179].velocity.X += npc.velocity.X * 0.4f;
				Main.dust[num179].velocity.Y += 2f + npc.velocity.Y;

				for(int num180 = 0; num180 < 2; num180++){
					num179 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + 120f), npc.width, 60, 5, npc.velocity.X, npc.velocity.Y, 0, default, 2f);
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
		public static void AI_SkeletronHand(NPC npc){
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

			npc.spriteDirection = -(int)npc.ai[0];
			if(!npc.Following().active || npc.Following().aiStyle != 11){
				npc.ai[2] += 10f;
				if(npc.ai[2] > 50f || Main.netMode != NetmodeID.Server){
					npc.life = -1;
					npc.HitEffect();
					npc.active = false;
				}
			}

			if(npc.ai[2] == 0f || npc.ai[2] == 3f){
				if(npc.Following().ai[1] == 3f && npc.timeLeft > 10)
					npc.timeLeft = 10;

				float retreatFriction = 0.92f;
				float retreatFactorX = 0.3f;
				float retreatFactorY = 1.2f;
				float velCapX = 11f;
				float velCapY = 9f;
				if(npc.Following().ai[1] != 0f){
					if(npc.position.Y > npc.Following().position.Y - 100f){
						if(npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if(npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					}else if(npc.position.Y < npc.Following().position.Y - 100f){
						if(npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if(npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if(npc.position.X + npc.width / 2 > npc.Following().position.X + npc.Following().width / 2 - 120f * npc.ai[0]){
						if(npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if(npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if(npc.position.X + npc.width / 2 < npc.Following().position.X + npc.Following().width / 2 - 120f * npc.ai[0]){
						if(npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if(npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}
				}else{
					npc.ai[3] += 1.5f;

					if(npc.ai[3] >= 300f){
						npc.ai[2] += 1f;
						npc.ai[3] = 0f;
						npc.netUpdate = true;
					}

					retreatFactorX = 0.12f;
					retreatFactorY = 0.08f;
					velCapY = 7f;

					if(npc.position.Y > npc.Following().position.Y + 230f){
						if(npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if(npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					}else if(npc.position.Y < npc.Following().position.Y + 230f){
						if(npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if(npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if(npc.position.X + npc.width / 2 > npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0]){
						if(npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if(npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if(npc.position.X + npc.width / 2 < npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0]){
						if(npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if(npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}

					if(npc.position.Y > npc.Following().position.Y + 230f){
						if(npc.velocity.Y > 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y -= retreatFactorY;
						if(npc.velocity.Y > velCapY)
							npc.velocity.Y = velCapY;
					}else if(npc.position.Y < npc.Following().position.Y + 230f){
						if(npc.velocity.Y < 0f)
							npc.velocity.Y *= retreatFriction;

						npc.velocity.Y += retreatFactorY;
						if(npc.velocity.Y < -velCapY)
							npc.velocity.Y = -velCapY;
					}

					if(npc.position.X + npc.width / 2 > npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0]){
						if(npc.velocity.X > 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X -= retreatFactorX;
						if(npc.velocity.X > velCapX)
							npc.velocity.X = velCapX;
					}

					if(npc.position.X + npc.width / 2 < npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0]){
						if(npc.velocity.X < 0f)
							npc.velocity.X *= retreatFriction;

						npc.velocity.X += retreatFactorX;
						if(npc.velocity.X < -velCapX)
							npc.velocity.X = -velCapX;
					}
				}

				Vector2 vector19 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num181 = npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0] - vector19.X;
				float num182 = npc.Following().position.Y + 230f - vector19.Y;
				npc.rotation = (float)Math.Atan2(num182, num181) + 1.57f;
			}else if(npc.ai[2] == 1f){
				Vector2 vector20 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num184 = npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0] - vector20.X;
				float num185 = npc.Following().position.Y + 230f - vector20.Y;
				float num186;

				npc.rotation = (float)Math.Atan2(num185, num184) + 1.57f;
				npc.velocity.X *= 0.95f;
				npc.velocity.Y -= 0.1f;

				npc.velocity.Y -= 0.06f;
				if(npc.velocity.Y < -13f)
					npc.velocity.Y = -13f;

				if(npc.position.Y < npc.Following().position.Y - 200f){
					npc.TargetClosest();
					npc.ai[2] = 2f;

					vector20 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					num184 = npc.Target().position.X + npc.Target().width / 2 - vector20.X;
					num185 = npc.Target().position.Y + npc.Target().height / 2 - vector20.Y;
					num186 = (float)Math.Sqrt(num184 * num184 + num185 * num185);
					num186 = 21f / num186;

					npc.velocity.X = num184 * num186;
					npc.velocity.Y = num185 * num186;
					npc.netUpdate = true;
				}
			}else if(npc.ai[2] == 2f){
				if(npc.position.Y > npc.Target().position.Y || npc.velocity.Y < 0f)
					npc.ai[2] = 3f;
			}else if(npc.ai[2] == 4f){
				Vector2 vector21 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
				float num187 = npc.Following().position.X + npc.Following().width / 2 - 200f * npc.ai[0] - vector21.X;
				float num188 = npc.Following().position.Y + 230f - vector21.Y;
				float num189;

				npc.rotation = (float)Math.Atan2(num188, num187) + 1.57f;
				npc.velocity.Y *= 0.95f;
				npc.velocity.X += 0.1f * (0f - npc.ai[0]);

				npc.velocity.X += 0.07f * (0f - npc.ai[0]);
				if(npc.velocity.X < -12f)
					npc.velocity.X = -12f;
				else if(npc.velocity.X > 12f)
					npc.velocity.X = 12f;

				if(npc.position.X + npc.width / 2 < npc.Following().position.X + npc.Following().width / 2 - 500f || npc.position.X + npc.width / 2 > npc.Following().position.X + npc.Following().width / 2 + 500f){
					npc.TargetClosest();

					npc.ai[2] = 5f;

					vector21 = new Vector2(npc.position.X + npc.width * 0.5f, npc.position.Y + npc.height * 0.5f);
					num187 = npc.Target().position.X + npc.Target().width / 2 - vector21.X;
					num188 = npc.Target().position.Y + npc.Target().height / 2 - vector21.Y;
					num189 = (float)Math.Sqrt(num187 * num187 + num188 * num188);
					num189 = 22f / num189;

					npc.velocity.X = num187 * num189;
					npc.velocity.Y = num188 * num189;
					npc.netUpdate = true;
				}
			}else if(npc.ai[2] == 5f && ((npc.velocity.X > 0f && npc.Center.X > npc.Target().Center.X) || (npc.velocity.X < 0f && npc.Center.X < npc.Target().Center.X)))
				npc.ai[2] = 0f;

			npc.Helper().Timer--;
			if(npc.Helper().Timer <= 0){
				Main.PlaySound(SoundID.NPCHit2, npc.Center);

				MiscUtils.SpawnProjectileSynced(npc.Center, npc.DirectionTo(npc.Target().Center) * 14f, ModContent.ProjectileType<SkeletronBone>(), 32, 4f);

				npc.Helper().Timer = Main.rand.Next(60, 75);

				DetourNPCHelper.SendData(npc.whoAmI);
			}
		}
	}
}
