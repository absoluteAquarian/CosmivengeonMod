using CosmivengeonMod.Projectiles.Desomode;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeBossAI{
		/// <summary>
		/// Runs a modified AI for the Brain of Cthulhu
		/// </summary>
		public static void AI_BrainOfCthulhu(NPC npc){
			NPC.crimsonBoss = npc.whoAmI;

			//Spawn the Creepers
			if(Main.netMode != NetmodeID.MultiplayerClient && npc.localAI[0] == 0f){
				npc.localAI[0] = 1f;

				for(int num761 = 0; num761 < 30; num761++){
					float num762 = npc.Center.X;
					float num763 = npc.Center.Y;

					num762 += Main.rand.Next(-npc.width, npc.width);
					num763 += Main.rand.Next(-npc.height, npc.height);
					
					int num764 = NPC.NewNPC((int)num762, (int)num763, 267);
					
					Main.npc[num764].velocity = new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.1f;
					Main.npc[num764].netUpdate = true;
				}
			}

			//If the target player is too far away, despawn
			if(Main.netMode != NetmodeID.MultiplayerClient){
				npc.TargetClosest(true);

				int num765 = 6000;
				if(npc.DistanceSQ(npc.Target().Center) > num765 * num765){
					npc.active = false;
					npc.life = 0;

					if(Main.netMode == NetmodeID.Server)
						NetMessage.SendData(MessageID.SyncNPC, number: npc.whoAmI);
				}
			}

			if(npc.active)
				BoC_AttackCheck(npc);

			if(npc.ai[0] < 0f){
				//Check for the transition to phase 2
				if(npc.localAI[2] == 0f){
					Main.PlaySound(SoundID.NPCHit, npc.position);
					npc.localAI[2] = 1f;
					Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 392, 1f);
					Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 393, 1f);
					Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 394, 1f);
					Gore.NewGore(npc.position, new Vector2(Main.rand.Next(-30, 31), Main.rand.Next(-30, 31)) * 0.2f, 395, 1f);

					for(int num766 = 0; num766 < 20; num766++)
						Dust.NewDust(npc.position, npc.width, npc.height, 5, Main.rand.Next(-30, 31) * 0.2f, Main.rand.Next(-30, 31) * 0.2f);

					Main.PlaySound(SoundID.Roar, npc.position, 0);
				}

				npc.dontTakeDamage = false;
				//Vanilla: npc.knockBackResist = 0.5f;
				npc.knockBackResist = 0f;
				npc.GetGlobalNPC<CosmivengeonGlobalNPC>().endurance = 0.05f;
				//Hide the boss from Boss Cursor's radar
				npc.dontCountMe = true;

				npc.TargetClosest(true);
				Vector2 vector94 = npc.Center;
				float num767 = npc.Target().Center.X - vector94.X;
				float num768 = npc.Target().Center.Y - vector94.Y;
				float num769 = (float)Math.Sqrt(num767 * num767 + num768 * num768);
				float num770 = 8f;

				num769 = num770 / num769;
				num767 *= num769;
				num768 *= num769;

				npc.velocity.X = (npc.velocity.X * 50f + num767) / 51f;
				npc.velocity.Y = (npc.velocity.Y * 50f + num768) / 51f;

				if(npc.ai[0] == -1f){
					//Before teleporting

					if(Main.netMode != NetmodeID.MultiplayerClient){
						//Attacking the boss delays the teleport by 5 ticks
						npc.localAI[1] += 1f;
						if(npc.justHit)
							npc.localAI[1] -= Main.rand.Next(5);

						int num771 = 60 + Main.rand.Next(120);
						if(Main.netMode != NetmodeID.SinglePlayer)
							num771 += Main.rand.Next(30, 90);

						if(npc.localAI[1] >= num771){
							npc.localAI[1] = 0f;
							npc.TargetClosest(true);

							int num772 = 0;
							int num773;
							int num774;

							//Find a valid position to teleport to
							while(true){
								num772++;
								num773 = (int)npc.Target().Center.X / 16;
								num774 = (int)npc.Target().Center.Y / 16;

								if(Main.rand.Next(2) == 0)
									num773 += Main.rand.Next(15, 21);
								else
									num773 -= Main.rand.Next(15, 21);

								if(Main.rand.Next(2) == 0)
									num774 += Main.rand.Next(15, 21);
								else
									num774 -= Main.rand.Next(15, 21);

								if(!WorldGen.SolidTile(num773, num774))
									break;

								//Teleport after 100 checks regardless of whether the target tile was solid or not
								if(num772 > 100)
									break;
							}

							npc.ai[3] = 0f;
							npc.ai[0] = -2f;
							npc.ai[1] = num773;
							npc.ai[2] = num774;
							npc.netUpdate = true;
							npc.netSpam = 0;
						}
					}
				}else if(npc.ai[0] == -2f){
					//Teleporting

					npc.velocity *= 0.9f;

					//Teleports happen much slower in singleplayer
					//Boss fading away also happens slower
					if(Main.netMode != NetmodeID.SinglePlayer)
						npc.ai[3] += 15f;
					else
						npc.ai[3] += 25f;

					if(npc.ai[3] >= 255f){
						npc.ai[3] = 255f;
						npc.position.X = npc.ai[1] * 16f - npc.width / 2;
						npc.position.Y = npc.ai[2] * 16f - npc.height / 2;
						Main.PlaySound(SoundID.Item8, npc.Center);
						npc.ai[0] = -3f;
						npc.netUpdate = true;
						npc.netSpam = 0;
					}

					npc.alpha = (int)npc.ai[3];
				}else if(npc.ai[0] == -3f){
					//After teleporting

					//Boss fading in happens much slower in singleplayer 
					if(Main.netMode != NetmodeID.SinglePlayer)
						npc.ai[3] -= 15f;
					else
						npc.ai[3] -= 25f;

					if(npc.ai[3] <= 0f){
						npc.ai[3] = 0f;
						npc.ai[0] = -1f;
						npc.netUpdate = true;
						npc.netSpam = 0;
					}

					npc.alpha = (int)npc.ai[3];
				}
			}else{
				//Move towards the player

				npc.TargetClosest(true);

				Vector2 vector95 = new Vector2(npc.Center.X, npc.Center.Y);
				float num775 = npc.Target().Center.X - vector95.X;
				float num776 = npc.Target().Center.Y - vector95.Y;
				float num777 = (float)Math.Sqrt(num775 * num775 + num776 * num776);
				float num778 = 1f;

				if(num777 < num778){
					//Boss's velocity vector length is always at least 1
					npc.velocity.X = num775;
					npc.velocity.Y = num776;
				}else{
					num777 = num778 / num777;
					npc.velocity.X = num775 * num777;
					npc.velocity.Y = num776 * num777;
				}

				if(npc.ai[0] == 0f){
					if(Main.netMode != NetmodeID.MultiplayerClient){
						int num779 = 0;

						//Count how many Creepers are alive
						for(int num780 = 0; num780 < 200; num780++)
							if(Main.npc[num780].active && Main.npc[num780].type == NPCID.Creeper)
								num779++;

						//If no Creepers are alive, start the transition to phase 2
						if(num779 == 0){
							npc.ai[0] = -1f;
							npc.localAI[1] = 0f;
							npc.alpha = 0;
							npc.netUpdate = true;
						}

						//Teleports happen every 2-5 seconds
						npc.localAI[1] += 1f;
						if(npc.localAI[1] >= 120 + Main.rand.Next(300)){
							//Try to teleport near the player

							npc.localAI[1] = 0f;
							npc.TargetClosest(true);
							int num781 = 0;
							int num782;
							int num783;

							while(true){
								num781++;
								num782 = (int)npc.Target().Center.X / 16;
								num783 = (int)npc.Target().Center.Y / 16;
								int x, y;
								do{
									x = Main.rand.Next(-50, 51);
									y = Main.rand.Next(-50, 51);
								}while(Math.Abs(x) <= 8 || Math.Abs(y) <= 8);

								num782 += x;
								num783 += y;

								//Only teleport if the target tile is within line-of-sight of the player
								if(!WorldGen.SolidTile(num782, num783) && Collision.CanHit(new Vector2(num782, num783) * 16, 1, 1, npc.Target().position, npc.Target().width, npc.Target().height)){
									break;
								}

								//Teleport after 100 checks regardless of whether the target tile is solid or not
								if(num781 > 100)
								{
									break;
								}
							}

							npc.ai[0] = 1f;
							npc.ai[1] = num782;
							npc.ai[2] = num783;
							npc.netUpdate = true;
						}
					}
				}else if(npc.ai[0] == 1f){
					//Before teleporting - boss slowly fades away
					//Teleporting - boss is completely invisible

					npc.alpha += 5;
					if(npc.alpha >= 255){
						Main.PlaySound(SoundID.Item8, npc.Center);
						npc.alpha = 255;
						npc.position.X = npc.ai[1] * 16f - npc.width / 2;
						npc.position.Y = npc.ai[2] * 16f - npc.height / 2;
						npc.ai[0] = 2f;
					}
				}else if(npc.ai[0] == 2f){
					//After teleporting - boss slowly fades in

					npc.alpha -= 5;
					if(npc.alpha <= 0){
						npc.alpha = 0;
						npc.ai[0] = 0f;
					}
				}
			}

			//If the target player is dead or not in the Crimson biome
			if(npc.Target().dead || !npc.Target().ZoneCrimson){
				//Increment the timer until it reaches 2 seconds
				if(npc.localAI[3] < 120f)
					npc.localAI[3] += 1f;

				//If the timer has reached more than 1 second, increase the NPC's velocity faster and faster
				if(npc.localAI[3] > 60f)
					npc.velocity.Y += (npc.localAI[3] - 60f) * 0.25f;

				//And make the NPC semi-transparent for good measure
				npc.ai[0] = 2f;
				npc.alpha = 10;
				return;
			}

			//This member should always tend towards being <= 0
			if(npc.localAI[3] > 0f)
				npc.localAI[3] -= 1f;
		}

		private static void BoC_AttackCheck(NPC npc){
			//First psychic attack
			npc.Helper().Timer++;

			int offset = BrainPsychicMine.Attack_Timer_Max - BrainPsychicMine.Attack_Death_Delay;
			int timerMax;
			if(npc.ai[0] < 0)
				timerMax = offset - 60;
			else
				timerMax = (int)(2.5f * 60 + offset);

			if(npc.Helper().Timer > timerMax){
				npc.Helper().Timer = 0;

				int proj = CosmivengeonUtils.SpawnProjectileSynced(npc.Target().Center, Vector2.Zero,
					ModContent.ProjectileType<BrainPsychicMine>(),
					80,
					4f,
					ai1: npc.target,
					owner: Main.myPlayer);

				if(npc.ai[0] < 0)
					(Main.projectile[proj].modProjectile as BrainPsychicMine).fastAttack = true;
			}

			//Second psychic attack
			npc.Helper().Timer2++;

			int target = npc.ai[0] < 0f ? 180 : 300;

			if(npc.Helper().Timer2 == target - 60){
				var sound = CosmivengeonMod.Instance.PlayCustomSound(npc.Target().Center - new Vector2(0, 50 * 16), "Zap");
				sound.Volume *= 0.75f;
			}

			if(npc.Helper().Timer2 > target){
				npc.Helper().Timer2 = 0;

				int dir = npc.Target().velocity.X > 0 ? 1 : -1;

				for(int i = -8; i < 8; i++){
					int positionOffset = i;

					positionOffset *= 16 * 16;

					Vector2 position = npc.Target().Center + new Vector2(positionOffset, -BrainPsychicLightning.FinalHeight / 2);

					int proj = CosmivengeonUtils.SpawnProjectileSynced(position, Vector2.Zero,
						ModContent.ProjectileType<BrainPsychicLightning>(),
						65,
						3.5f);

					BrainPsychicLightning lightning = Main.projectile[proj].modProjectile as BrainPsychicLightning;
					lightning.AttackDelay = (int)(-dir * 180f * i / 15f);

					if(npc.ai[0] < 0)
						lightning.fastAttack = true;
				}
			}
		}
	}
}
