using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;

namespace CosmivengeonMod.API.Edits.Detours.Desomode{
	public static partial class DesolationModeBossAI{
		public const int WoF_Attack_EyeLasers = 0;
		public const int WoF_Attack_DemonSickles = 1;
		public const int WoF_Attack_ImpFireballs = 2;
		public const int WoF_Attack_CursedFlamethrower = 3;

		public static void AI_WallOfFleshMouth(NPC npc){
			//Despawn if reached the edge of the world
			if(npc.position.X < 160f || npc.position.X > (Main.maxTilesX - 10) * 16)
				npc.active = false;

			if(npc.localAI[0] == 0f){
				npc.localAI[0] = 1f;
				//Wall bottom, wall top
				Main.wofB = -1;
				Main.wofT = -1;
			}

			if(npc.life < npc.lifeMax / 2f && !npc.Helper().Flag){
				npc.Helper().Flag = true;

				SoundEngine.PlaySound(SoundID.Roar, npc.Center);
			}

			if(!npc.Helper().Flag)
				npc.Helper().Timer2 = WoF_Attack_EyeLasers;

			npc.ai[1] += 1f;
			if(npc.ai[2] == 0f){
				if(npc.life < npc.lifeMax * 0.5)
					npc.ai[1] += 1f;

				if(npc.life < npc.lifeMax * 0.2)
					npc.ai[1] += 1f;

				if(npc.ai[1] > 2700f)
					npc.ai[2] = 1f;
			}

			//Spit out a leech
			if(npc.ai[2] > 0f && npc.ai[1] > 60f){
				int num332 = 3;
				if(npc.life < npc.lifeMax * 0.3)
					num332++;

				npc.ai[2] += 1f;
				npc.ai[1] = 0f;
				if(npc.ai[2] > num332)
					npc.ai[2] = 0f;

				if(Main.netMode != NetmodeID.MultiplayerClient){
					int num333 = NPC.NewNPC((int)(npc.position.X + npc.width / 2), (int)(npc.position.Y + npc.height / 2 + 20f), NPCID.LeechHead, 1);
					Main.npc[num333].velocity.X = npc.direction * 8;
				}
			}

			if(!npc.Helper().Flag){
				//Scream at the player
				npc.localAI[3] += 1f;
				if(npc.localAI[3] >= 600 + Main.rand.Next(1000)){
					npc.localAI[3] = -Main.rand.Next(200);
					SoundEngine.PlaySound(SoundID.NPCDeath10, npc.position);
				}
			}

			Main.wof = npc.whoAmI;

			//Find where the bottom and top of the wall should move towards
			int num334 = (int)(npc.position.X / 16f);
			int num335 = (int)((npc.position.X + npc.width) / 16f);
			int num336 = (int)((npc.position.Y + npc.height / 2) / 16f);
			int num337 = 0;
			int num338 = num336 + 7;
			while(num337 < 15 && num338 > Main.maxTilesY - 200){
				num338++;
				for(int num339 = num334; num339 <= num335; num339++){
					try{
						if(WorldGen.SolidTile(num339, num338) || Main.tile[num339, num338].LiquidAmount > 0)
							num337++;
					}catch{
						num337 += 15;
					}
				}
			}

			num338 += 4;
			if(Main.wofB == -1){
				Main.wofB = num338 * 16;
			}else if(Main.wofB > num338 * 16){
				Main.wofB--;
				if(Main.wofB < num338 * 16)
					Main.wofB = num338 * 16;
			}else if(Main.wofB < num338 * 16){
				Main.wofB++;
				if(Main.wofB > num338 * 16)
					Main.wofB = num338 * 16;
			}

			num337 = 0;
			num338 = num336 - 7;
			while(num337 < 15 && num338 < Main.maxTilesY - 10){
				num338--;
				for(int num340 = num334; num340 <= num335; num340++){
					try{
						if(WorldGen.SolidTile(num340, num338) || Main.tile[num340, num338].LiquidAmount > 0)
							num337++;
					}catch{
						num337 += 15;
					}
				}
			}

			num338 -= 4;
			if(Main.wofT == -1){
				Main.wofT = num338 * 16;
			}else if(Main.wofT > num338 * 16){
				Main.wofT--;
				if(Main.wofT < num338 * 16)
					Main.wofT = num338 * 16;
			}else if(Main.wofT < num338 * 16){
				Main.wofT++;
				if(Main.wofT > num338 * 16)
					Main.wofT = num338 * 16;
			}

			//Mouth should move slowly towards the middle of the wall?
			//This segment gets nulled by the following 'npc.velocity.Y = 0f;' line, so idk why it's here
			float num341 = (Main.wofB + Main.wofT) / 2 - npc.height / 2;
			if(npc.position.Y > num341 + 1f)
				npc.velocity.Y = -1f;
			else if(npc.position.Y < num341 - 1f)
				npc.velocity.Y = 1f;

			npc.velocity.Y = 0f;
			int num342 = (Main.maxTilesY - 180) * 16;
			if(num341 < num342)
				num341 = num342;

			npc.position.Y = num341;

			//Default move rate of 1.5px/tick
			//Speed up as the boss loses health
			float num343 = 2.75f;
			//Original increases are listed below
			if(npc.life < npc.lifeMax * 0.75)
				num343 += 0.275f;  //0.25
			if(npc.life < npc.lifeMax * 0.66)
				num343 += 0.325f;  //0.3
			if(npc.life < npc.lifeMax * 0.5)
				num343 += 0.425f;  //0.4
			if(npc.life < npc.lifeMax * 0.33)
				num343 += 0.325f;  //0.3
			if(npc.life < npc.lifeMax * 0.25)
				num343 += 0.525f;  //0.5
			if(npc.life < npc.lifeMax * 0.1)
				num343 += 0.625f;  //0.6
			if(npc.life < npc.lifeMax * 0.05)
				num343 += 0.625f;  //0.6
			if(npc.life < npc.lifeMax * 0.035)
				num343 += 0.625f;  //0.6
			if(npc.life < npc.lifeMax * 0.025)
				num343 += 0.625f;  //0.6

			//Expert mode is always active, so these two increases will always happen anyway
			//Original: 1.35, 0.35
			num343 *= 1.4f;
			num343 += 0.4f;

			//Just spawned, X-velocity is 0
			//Find a target and face towards them
			if(npc.velocity.X == 0f){
				npc.TargetClosest();
				npc.velocity.X = npc.direction;
			}

			if(npc.velocity.X < 0f){
				npc.velocity.X = -num343;
				npc.direction = -1;
			}else{
				npc.velocity.X = num343;
				npc.direction = 1;
			}

			npc.spriteDirection = npc.direction;
			Vector2 vector34 = npc.Center;
			float num344 = npc.Target().Center.X - vector34.X;
			float num345 = npc.Target().Center.Y - vector34.Y;
			float num346 = (float)Math.Sqrt(num344 * num344 + num345 * num345);
			num344 *= num346;
			num345 *= num346;
			if(npc.direction > 0){
				if(npc.Target().Center.X > npc.Center.X)
					npc.rotation = (float)Math.Atan2(-num345, -num344) + 3.14f;
				else
					npc.rotation = 0f;
			}else if(npc.Target().Center.X < npc.Center.X)
				npc.rotation = (float)Math.Atan2(num345, num344) + 3.14f;
			else
				npc.rotation = 0f;

			//Desolation: always try to stay within 70 tiles of the target player
			float targetX = npc.Target().Center.X - npc.direction * 70 * 16;
			if((npc.direction == 1 && npc.Center.X < targetX) || (npc.direction == -1 && npc.Center.X > targetX)){
				npc.Center = new Vector2(targetX, npc.Center.Y);
				npc.Helper().Flag2 = true;
			}else
				npc.Helper().Flag2 = false;

			//Expert mode WoF spawns extra "The Hungry"s
			if(Main.netMode != NetmodeID.MultiplayerClient){
				int num348 = (int)(1f + npc.life / (float)npc.lifeMax * 10f);
				num348 *= num348;
				if(num348 < 400)
					num348 = (num348 * 19 + 400) / 20;
				if(num348 < 60)
					num348 = (num348 * 3 + 60) / 4;
				if(num348 < 20)
					num348 = (num348 + 20) / 2;

				num348 = (int)(num348 * 0.7);
				if(Main.rand.Next(num348) == 0){
					int num349 = 0;
					float[] array = new float[10];
					for(int i = 0; i < 200; i++){
						if(num349 < 10 && Main.npc[i].active && Main.npc[i].type == NPCID.TheHungry){
							array[num349] = Main.npc[i].ai[0];
							num349++;
						}
					}

					int maxValue = 1 + num349 * 2;
					if(num349 < 10 && Main.rand.Next(maxValue) <= 1){
						int num351 = -1;
						for(int num352 = 0; num352 < 1000; num352++){
							int num353 = Main.rand.Next(10);
							float num354 = num353 * 0.1f - 0.05f;
							bool flag27 = true;
							for(int num355 = 0; num355 < num349; num355++){
								if(num354 == array[num355]){
									flag27 = false;
									break;
								}
							}

							if(flag27){
								num351 = num353;
								break;
							}
						}

						if(num351 >= 0){
							int num356 = NPC.NewNPC((int)npc.position.X, (int)num341, NPCID.TheHungry, npc.whoAmI);
							Main.npc[num356].ai[0] = num351 * 0.1f - 0.05f;
						}
					}
				}
			}

			//Spawn the eye parts and the initial "The Hungry"s
			if(npc.localAI[0] == 1f && Main.netMode != NetmodeID.MultiplayerClient){
				npc.localAI[0] = 2f;
				num341 = (Main.wofB + Main.wofT) / 2;
				num341 = (num341 + Main.wofT) / 2f;
				int num357 = NPC.NewNPC((int)npc.position.X, (int)num341, NPCID.WallofFleshEye, npc.whoAmI);
				Main.npc[num357].ai[0] = 1f;
				num341 = (Main.wofB + Main.wofT) / 2;
				num341 = (num341 + Main.wofB) / 2f;
				num357 = NPC.NewNPC((int)npc.position.X, (int)num341, NPCID.WallofFleshEye, npc.whoAmI);
				Main.npc[num357].ai[0] = -1f;
				num341 = (Main.wofB + Main.wofT) / 2;
				num341 = (num341 + Main.wofB) / 2f;

				for(int num358 = 0; num358 < 11; num358++){
					num357 = NPC.NewNPC((int)npc.position.X, (int)num341, NPCID.TheHungry, npc.whoAmI);
					Main.npc[num357].ai[0] = num358 * 0.1f - 0.05f;
				}
			}

			//Phase 2 stuff
			if(npc.Helper().Flag && Main.netMode != NetmodeID.MultiplayerClient){
				npc.Helper().Timer++;

				if(npc.life < npc.lifeMax * 0.3333f)
					npc.Helper().Timer++;
				if(npc.life < npc.lifeMax * 0.25f)
					npc.Helper().Timer++;
				if(npc.life < npc.lifeMax * 0.1667f)
					npc.Helper().Timer++;

				if(npc.Helper().Timer2 <= WoF_Attack_EyeLasers)
					npc.Helper().Timer = 0;
				else if(npc.Helper().Timer >= 300 && npc.Helper().Timer2 == WoF_Attack_DemonSickles){
					//Spawn demon scythes that have no tile collision
					npc.Helper().Timer = 0;

					float x = npc.Target().Center.X - npc.direction * 30 * 16;

					for(int i = -3; i < 4; i++){
						float y = npc.Target().Center.Y + i * 10 * 16;

						Projectile proj = Projectile.NewProjectileDirect(new Vector2(x, y), Vector2.UnitX * npc.direction * 1.85f, ProjectileID.DemonSickle, MiscUtils.TrueDamage(80), 0f, Main.myPlayer);
						proj.tileCollide = false;
						proj.timeLeft = 4 * 60;
					}

					WoF_ChooseNextAttack(npc);
				}else if(npc.Helper().Timer2 == WoF_Attack_ImpFireballs){
					float x = npc.Target().Center.X - npc.direction * 45 * 16;
					float offY = 10 * 16;

					if(npc.Helper().Timer < 200){
						float range = 48 * (200 - npc.Helper().Timer) / 200f;

						for(float i = -3.5f; i <= 3.5f; i++){
							float y = npc.Target().Center.Y + i * offY;

							for(int j = 0; j < 4; j++){
								Dust dust = Dust.NewDustDirect(new Vector2(x - range, y - range), (int)(range + 0.5f) * 2, (int)(range + 0.5f) * 2, DustID.Torch);
								dust.noGravity = true;
								dust.velocity.X = npc.Target().velocity.X;
							}
						}
					}else{
						//Spawn imp fireballs that can't be hit
						npc.Helper().Timer = 0;

						for(float i = -3.5f; i <= 3.5f; i++){
							float y = npc.Target().Center.Y + i * offY;

							Vector2 pos = new Vector2(x, y);
							Projectile proj = Projectile.NewProjectileDirect(pos, npc.Target().DirectionFrom(pos) * 9, ProjectileID.ImpFireball, MiscUtils.TrueDamage(66), 0f, Main.myPlayer);
							proj.friendly = false;
							proj.hostile = true;
							proj.tileCollide = false;
							proj.timeLeft = 6 * 60;
						}

						WoF_ChooseNextAttack(npc);
					}
				}else if(npc.Helper().Timer >= 200 && npc.Helper().Timer2 == WoF_Attack_CursedFlamethrower){
					if(npc.Helper().Timer > 200 + 120){
						npc.Helper().Timer = 0;

						WoF_ChooseNextAttack(npc);
					}

					Projectile proj = Projectile.NewProjectileDirect(npc.Center - new Vector2(32 * npc.direction), npc.DirectionTo(npc.Target().Center) * 14f, ProjectileID.EyeFire, 50, 0f, Main.myPlayer);
					proj.tileCollide = false;

					SoundEngine.PlaySound(SoundID.Item34, npc.Center);
				}
			}
		}

		private static void WoF_ChooseNextAttack(NPC npc){
chooseAgain:
			int val = Main.rand.Next(4);
			switch(val){
				case WoF_Attack_EyeLasers:
					npc.Helper().Timer2 = -1;
					break;
				default:
					if(val == WoF_Attack_CursedFlamethrower && npc.Target().tongued)
						goto chooseAgain;

					npc.Helper().Timer2 = val;
					break;
			}

			npc.netUpdate = true;
		}

		public static void AI_WallOfFleshEye(NPC npc){
			if(Main.wof < 0){
				npc.active = false;
				return;
			}

			//Match the eye health with the mouth health
			npc.realLife = Main.wof;
			NPC wof = Main.npc[Main.wof];

			if(wof.life > 0)
				npc.life = wof.life;

			//Keep targetting whichever player is closest
			npc.TargetClosest();

			//Keep the eyes in line with the mouth
			npc.position.X = wof.position.X;
			npc.direction = wof.direction;

			npc.spriteDirection = npc.direction;

			//Find which eye spot this eye should move towards
			float num359 = (Main.wofB + Main.wofT) / 2;
			num359 = npc.ai[0] <= 0f ? num359 + (Main.wofB - num359) / 2f : num359 - (num359 - Main.wofT) / 2f;
			num359 -= npc.height / 2;
			if(npc.position.Y > num359 + 1f)
				npc.velocity.Y = -4f;
			else if(npc.position.Y < num359 - 1f)
				npc.velocity.Y = 4f;
			else{
				npc.velocity.Y = 0f;
				npc.position.Y = num359;
			}

			//Cap vertical velocity to 5 (Normal/Expert) or 8 (Desolation)
			if(npc.velocity.Y > 8f)
				npc.velocity.Y = 8f;
			if(npc.velocity.Y < -8f)
				npc.velocity.Y = -8f;

			Vector2 vector35 = npc.Center;
			float num360 = npc.Target().Center.X - vector35.X;
			float num361 = npc.Target().Center.Y - vector35.Y;
			float num362 = (float)Math.Sqrt(num360 * num360 + num361 * num361);
			num360 *= num362;
			num361 *= num362;
			bool flag28 = true;
			if(npc.direction > 0){
				if(npc.Target().position.X + npc.Target().width / 2 > npc.position.X + npc.width / 2){
					npc.rotation = (float)Math.Atan2(0f - num361, 0f - num360) + 3.14f;
				}else{
					npc.rotation = 0f;
					flag28 = false;
				}
			}else if(npc.Target().position.X + npc.Target().width / 2 < npc.position.X + npc.width / 2){
				npc.rotation = (float)Math.Atan2(num361, num360) + 3.14f;
			}else{
				npc.rotation = 0f;
				flag28 = false;
			}

			if(Main.netMode == NetmodeID.MultiplayerClient)
				return;

			if(wof.Helper().Timer2 > 0){
				npc.localAI[1] = 0;
				npc.localAI[2] = 0;
				return;
			}

			//Timing for laser firing
			//num364 is the amount of lasers fired per cycle
			int num364 = 4;
			npc.localAI[1] += 1f;
			if(wof.life < wof.lifeMax * 0.75f){
				npc.localAI[1] += 1f;
				num364++;
			}
			if(wof.life < wof.lifeMax * 0.5f){
				npc.localAI[1] += 1f;
				num364++;
			}
			if(wof.life < wof.lifeMax * 0.25f){
				npc.localAI[1] += 1f;
				num364++;
			}
			if(wof.life < wof.lifeMax * 0.1f){
				npc.localAI[1] += 2f;
				num364++;
			}

			npc.localAI[1] += 0.5f;
			num364++;
			//Originally was lifeMax * 0.1
			//Desolation changes this to lifeMax * 0.05
			if(wof.life < wof.lifeMax * 0.05f){
				npc.localAI[1] += 2f;
				num364++;
			}

			if(npc.localAI[2] == 0f){
				//Original: 600f
				//Value is lower in phase 2, but the attacks are implemented in a pattern so it's actually longer between each attack
				if((!wof.Helper().Flag && npc.localAI[1] > 500f) || (wof.Helper().Flag && npc.localAI[1] > 300f)){
					npc.localAI[2] = 1f;
					npc.localAI[1] = 0f;
				}
			}else{
				if(npc.localAI[1] <= 45f)
					return;

				npc.localAI[1] = 0f;
				npc.localAI[2] += 1f;
				if(npc.localAI[2] >= num364){
					npc.localAI[2] = 0f;

					//Indicate that this eye has finished firing its lasers
					if(wof.Helper().Flag)
						wof.Helper().Timer2++;

					if(wof.Helper().Timer2 == WoF_Attack_EyeLasers + 1)
						WoF_ChooseNextAttack(wof);
				}

				if(flag28){
					//Lasers move faster the lower the boss's health is
					//Normal/Expert: 9 -> 13
					//Desolation: 12 -> 16
					//Lasers deal more damage the lower the boss's health is
					//Normal/Expert: 22/44 -> 30/60
					//Desolation: 60 -> 100
					float num365 = 12f;
					int num366 = 60;
					int num367 = ProjectileID.EyeLaser;
					if(wof.life < wof.lifeMax * 0.5){
						num366 += 10;
						num365 += 1f;
					}

					if(wof.life < wof.lifeMax * 0.25){
						num366 += 10;
						num365 += 1f;
					}

					if(wof.life < wof.lifeMax * 0.1){
						num366 += 20;
						num365 += 2f;
					}

					vector35 = npc.Center;
					num360 = npc.Target().Center.X - vector35.X;
					num361 = npc.Target().Center.Y - vector35.Y;
					num362 = (float)Math.Sqrt(num360 * num360 + num361 * num361);
					num362 = num365 / num362;
					num360 *= num362;
					num361 *= num362;
					vector35.X += num360;
					vector35.Y += num361;
					
					Projectile proj = Projectile.NewProjectileDirect(vector35, new Vector2(num360, num361), num367, MiscUtils.TrueDamage(num366), 0f, Main.myPlayer);
					proj.tileCollide = false;
				}
			}
		}
	}

	public static partial class DesolationModeMonsterAI{
		public static void AI_TheHungry(NPC npc){
			if(npc.justHit)
				npc.ai[1] = 10f;

			if(Main.wof < 0){
				npc.active = false;
				return;
			}

			NPC wof = Main.npc[Main.wof];

			npc.TargetClosest();
			float num369 = 0.1f;
			float num370 = 300f;
			if(wof.life < wof.lifeMax * 0.25){
				npc.damage = (int)(85f * Main.GameModeInfo.EnemyDamageMultiplier);
				num369 += 0.1f;
			}else if(wof.life < wof.lifeMax * 0.5){
				npc.damage = (int)(70f * Main.GameModeInfo.EnemyDamageMultiplier);
				num369 += 0.066f;
			}else if(wof.life < wof.lifeMax * 0.75){
				npc.damage = (int)(55 * Main.GameModeInfo.EnemyDamageMultiplier);
				num369 += 0.033f;
			}

			if(npc.whoAmI % 4 == 0)
				num370 *= 1.75f;

			if(npc.whoAmI % 4 == 1)
				num370 *= 1.5f;

			if(npc.whoAmI % 4 == 2)
				num370 *= 1.25f;

			if(npc.whoAmI % 3 == 0)
				num370 *= 1.5f;

			if(npc.whoAmI % 3 == 1)
				num370 *= 1.25f;

			num370 *= 0.75f;

			float num371 = wof.position.X + wof.width / 2;
			float num372 = Main.wofB - Main.wofT;
			float y2 = Main.wofT + num372 * npc.ai[0];
			npc.ai[2] += 1f;
			if(npc.ai[2] > 100f){
				num370 = (int)(num370 * 1.3f);
				if(npc.ai[2] > 200f)
					npc.ai[2] = 0f;
			}

			Vector2 vector36 = new Vector2(num371, y2);
			float num373 = npc.Target().position.X + npc.Target().width / 2 - npc.width / 2 - vector36.X;
			float num374 = npc.Target().position.Y + npc.Target().height / 2 - npc.height / 2 - vector36.Y;
			float num375 = (float)Math.Sqrt(num373 * num373 + num374 * num374);
			if(npc.ai[1] == 0f){
				if(num375 > num370){
					num375 = num370 / num375;
					num373 *= num375;
					num374 *= num375;
				}

				if(npc.position.X < num371 + num373){
					npc.velocity.X += num369;
					if(npc.velocity.X < 0f && num373 > 0f)
						npc.velocity.X += num369 * 2.5f;
				}else if(npc.position.X > num371 + num373){
					npc.velocity.X -= num369;
					if(npc.velocity.X > 0f && num373 < 0f)
						npc.velocity.X -= num369 * 2.5f;
				}

				if(npc.position.Y < y2 + num374){
					npc.velocity.Y += num369;
					if(npc.velocity.Y < 0f && num374 > 0f)
						npc.velocity.Y += num369 * 2.5f;
				}else if(npc.position.Y > y2 + num374){
					npc.velocity.Y -= num369;
					if(npc.velocity.Y > 0f && num374 < 0f)
						npc.velocity.Y -= num369 * 2.5f;
				}

				float num376 = 6.75f;
				if(Main.wof >= 0){
					float num377 = 2.5f;
					float num378 = wof.life / wof.lifeMax;
					if(num378 < 0.75)
						num377 += 0.7f;

					if(num378 < 0.5)
						num377 += 0.7f;

					if(num378 < 0.25)
						num377 += 0.9f;

					if(num378 < 0.1)
						num377 += 0.9f;

					num377 *= 1.25f;
					num377 += 0.3f;
					num376 += num377 * 0.35f;
					if(npc.Center.X < wof.Center.X && wof.velocity.X > 0f)
						num376 += 6f;

					if(npc.Center.X > wof.Center.X && wof.velocity.X < 0f)
						num376 += 6f;
				}

				if(npc.velocity.X > num376)
					npc.velocity.X = num376;

				if(npc.velocity.X < 0f - num376)
					npc.velocity.X = 0f - num376;

				if(npc.velocity.Y > num376)
					npc.velocity.Y = num376;

				if(npc.velocity.Y < 0f - num376)
					npc.velocity.Y = 0f - num376;
			}else if(npc.ai[1] > 0f){
				npc.ai[1] -= 1f;
			}else{
				npc.ai[1] = 0f;
			}

			if(num373 > 0f){
				npc.spriteDirection = 1;
				npc.rotation = (float)Math.Atan2(num374, num373);
			}

			if(num373 < 0f){
				npc.spriteDirection = -1;
				npc.rotation = (float)Math.Atan2(num374, num373) + 3.14f;
			}

			Lighting.AddLight((int)(npc.position.X + npc.width / 2) / 16, (int)(npc.position.Y + npc.height / 2) / 16, 0.3f, 0.2f, 0.1f);

			if(Main.netMode == NetmodeID.MultiplayerClient)
				return;

			//Occasionally spit ichor shots
			npc.ai[3]++;
			if(npc.ai[3] >= Main.rand.Next(350, 600)){
				npc.ai[3] = 0;

				int dmg = MiscUtils.TrueDamage(50);
				Vector2 target = Main.npc[Main.wof].Target().Center;
				target.Y -= 3 * 16;
				Projectile proj = Projectile.NewProjectileDirect(npc.Center, npc.DirectionTo(target) * 4f, ProjectileID.IchorSplash, dmg, 3f, Main.myPlayer);
				proj.hostile = true;
				proj.friendly = false;
				proj.timeLeft = 60 * 3;
				proj.penetrate = 1;
			}
		}

		public static void AI_TheHungryII(NPC npc){
			AI_002_FloatingEye(npc);
		}
	}
}
