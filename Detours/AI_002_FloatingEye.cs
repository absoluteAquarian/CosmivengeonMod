using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;

namespace CosmivengeonMod.Detours{
	public static partial class DesolationModeMonsterAI{
		public static void AI_002_FloatingEye(NPC npc){
			if((npc.type == NPCID.PigronCorruption || npc.type == NPCID.PigronHallow || npc.type == NPCID.PigronCrimson) && Main.rand.Next(1000) == 0)
				Main.PlaySound(SoundID.Zombie, (int)npc.position.X, (int)npc.position.Y, 9);

			npc.noGravity = true;
			if(!npc.noTileCollide){
				if(npc.collideX){
					npc.velocity.X = npc.oldVelocity.X * -0.5f;
					if(npc.direction == -1 && npc.velocity.X > 0f && npc.velocity.X < 2f)
						npc.velocity.X = 2f;

					if(npc.direction == 1 && npc.velocity.X < 0f && npc.velocity.X > -2f)
						npc.velocity.X = -2f;
				}

				if(npc.collideY){
					npc.velocity.Y = npc.oldVelocity.Y * -0.5f;
					if(npc.velocity.Y > 0f && npc.velocity.Y < 1f)
						npc.velocity.Y = 1f;

					if(npc.velocity.Y < 0f && npc.velocity.Y > -1f)
						npc.velocity.Y = -1f;
				}
			}

			if(Main.dayTime && npc.position.Y <= Main.worldSurface * 16.0 && (npc.type == NPCID.DemonEye || npc.type == NPCID.WanderingEye || npc.type == NPCID.CataractEye || npc.type == NPCID.SleepyEye || npc.type == NPCID.DialatedEye || npc.type == NPCID.GreenEye || npc.type == NPCID.PurpleEye || npc.type == NPCID.DemonEyeOwl || npc.type == NPCID.DemonEyeSpaceship)){
				if(npc.timeLeft > 10)
					npc.timeLeft = 10;

				npc.directionY = -1;
				if(npc.velocity.Y > 0f)
					npc.direction = 1;

				npc.direction = -1;
				if(npc.velocity.X > 0f)
					npc.direction = 1;
			}else{
				npc.TargetClosest();
			}

			if(npc.type == NPCID.PigronCorruption || npc.type == NPCID.PigronHallow || npc.type == NPCID.PigronCrimson){
				if(Collision.CanHit(npc.position, npc.width, npc.height, npc.Target().position, npc.Target().width, npc.Target().height)){
					if(npc.ai[1] > 0f && !Collision.SolidCollision(npc.position, npc.width, npc.height)){
						npc.ai[1] = 0f;
						npc.ai[0] = 0f;
						npc.netUpdate = true;
					}
				}else if(npc.ai[1] == 0f){
					npc.ai[0] += 1f;
				}

				if(npc.ai[0] >= 300f){
					npc.ai[1] = 1f;
					npc.ai[0] = 0f;
					npc.netUpdate = true;
				}

				if(npc.ai[1] == 0f){
					npc.alpha = 0;
					npc.noTileCollide = false;
				}else{
					npc.wet = false;
					npc.alpha = 200;
					npc.noTileCollide = true;
				}

				npc.rotation = npc.velocity.Y * 0.1f * npc.direction;
				npc.TargetClosest();
				if(npc.direction == -1 && npc.velocity.X > -4f && npc.position.X > npc.Target().position.X + npc.Target().width) {
					npc.velocity.X -= 0.08f;
					if(npc.velocity.X > 4f)
						npc.velocity.X -= 0.04f;
					else if(npc.velocity.X > 0f)
						npc.velocity.X -= 0.2f;

					if(npc.velocity.X < -4f)
						npc.velocity.X = -4f;
				}else if(npc.direction == 1 && npc.velocity.X < 4f && npc.position.X + npc.width < npc.Target().position.X){
					npc.velocity.X += 0.08f;
					if(npc.velocity.X < -4f)
						npc.velocity.X += 0.04f;
					else if(npc.velocity.X < 0f)
						npc.velocity.X += 0.2f;

					if(npc.velocity.X > 4f)
						npc.velocity.X = 4f;
				}

				if(npc.directionY == -1 && npc.velocity.Y > -2.5 && npc.position.Y > npc.Target().position.Y + npc.Target().height) {
					npc.velocity.Y -= 0.1f;
					if(npc.velocity.Y > 2.5)
						npc.velocity.Y -= 0.05f;
					else if(npc.velocity.Y > 0f)
						npc.velocity.Y -= 0.15f;

					if(npc.velocity.Y < -2.5)
						npc.velocity.Y = -2.5f;
				}else if(npc.directionY == 1 && npc.velocity.Y < 2.5 && npc.position.Y + npc.height < npc.Target().position.Y){
					npc.velocity.Y += 0.1f;
					if(npc.velocity.Y < -2.5)
						npc.velocity.Y += 0.05f;
					else if(npc.velocity.Y < 0f)
						npc.velocity.Y += 0.15f;

					if(npc.velocity.Y > 2.5)
						npc.velocity.Y = 2.5f;
				}
			}else if(npc.type == NPCID.TheHungryII){
				npc.TargetClosest();
				Lighting.AddLight((int)(npc.position.X + npc.width / 2) / 16, (int)(npc.position.Y + npc.height / 2) / 16, 0.3f, 0.2f, 0.1f);
				if(npc.direction == -1 && npc.velocity.X > -6f){
					npc.velocity.X -= 0.1f;
					if(npc.velocity.X > 6f)
						npc.velocity.X -= 0.1f;
					else if(npc.velocity.X > 0f)
						npc.velocity.X -= 0.2f;

					if(npc.velocity.X < -6f)
						npc.velocity.X = -6f;
				}else if(npc.direction == 1 && npc.velocity.X < 6f){
					npc.velocity.X += 0.1f;
					if(npc.velocity.X < -6f)
						npc.velocity.X += 0.1f;
					else if(npc.velocity.X < 0f)
						npc.velocity.X += 0.2f;

					if(npc.velocity.X > 6f)
						npc.velocity.X = 6f;
				}

				if(npc.directionY == -1 && npc.velocity.Y > -2.5){
					npc.velocity.Y -= 0.04f;
					if(npc.velocity.Y > 2.5)
						npc.velocity.Y -= 0.05f;
					else if(npc.velocity.Y > 0f)
						npc.velocity.Y -= 0.15f;

					if(npc.velocity.Y < -2.5)
						npc.velocity.Y = -2.5f;
				}else if(npc.directionY == 1 && npc.velocity.Y < 1.5){
					npc.velocity.Y += 0.04f;
					if(npc.velocity.Y < -2.5)
						npc.velocity.Y += 0.05f;
					else if(npc.velocity.Y < 0f)
						npc.velocity.Y += 0.15f;

					if(npc.velocity.Y > 2.5)
						npc.velocity.Y = 2.5f;
				}

				if(Main.rand.Next(40) == 0){
					int num = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), 5, npc.velocity.X, 2f);
					Main.dust[num].velocity.X *= 0.5f;
					Main.dust[num].velocity.Y *= 0.1f;
				}
			}else if(npc.type == NPCID.WanderingEye){
				if(npc.life < npc.lifeMax * 0.5){
					if(npc.direction == -1 && npc.velocity.X > -6f){
						npc.velocity.X -= 0.1f;
						if(npc.velocity.X > 6f)
							npc.velocity.X -= 0.1f;
						else if(npc.velocity.X > 0f)
							npc.velocity.X += 0.05f;

						if(npc.velocity.X < -6f)
							npc.velocity.X = -6f;
					}else if(npc.direction == 1 && npc.velocity.X < 6f){
						npc.velocity.X += 0.1f;
						if(npc.velocity.X < -6f)
							npc.velocity.X += 0.1f;
						else if(npc.velocity.X < 0f)
							npc.velocity.X -= 0.05f;

						if(npc.velocity.X > 6f)
							npc.velocity.X = 6f;
					}

					if(npc.directionY == -1 && npc.velocity.Y > -4f){
						npc.velocity.Y -= 0.1f;
						if(npc.velocity.Y > 4f)
							npc.velocity.Y -= 0.1f;
						else if(npc.velocity.Y > 0f)
							npc.velocity.Y += 0.05f;

						if(npc.velocity.Y < -4f)
							npc.velocity.Y = -4f;
					}else if(npc.directionY == 1 && npc.velocity.Y < 4f){
						npc.velocity.Y += 0.1f;
						if(npc.velocity.Y < -4f)
							npc.velocity.Y += 0.1f;
						else if(npc.velocity.Y < 0f)
							npc.velocity.Y -= 0.05f;

						if(npc.velocity.Y > 4f)
							npc.velocity.Y = 4f;
					}
				}else{
					if(npc.direction == -1 && npc.velocity.X > -4f){
						npc.velocity.X -= 0.1f;
						if(npc.velocity.X > 4f)
							npc.velocity.X -= 0.1f;
						else if(npc.velocity.X > 0f)
							npc.velocity.X += 0.05f;

						if(npc.velocity.X < -4f)
							npc.velocity.X = -4f;
					}
					else if(npc.direction == 1 && npc.velocity.X < 4f){
						npc.velocity.X += 0.1f;
						if(npc.velocity.X < -4f)
							npc.velocity.X += 0.1f;
						else if(npc.velocity.X < 0f)
							npc.velocity.X -= 0.05f;

						if(npc.velocity.X > 4f)
							npc.velocity.X = 4f;
					}

					if(npc.directionY == -1 && npc.velocity.Y > -1.5){
						npc.velocity.Y -= 0.04f;
						if(npc.velocity.Y > 1.5)
							npc.velocity.Y -= 0.05f;
						else if(npc.velocity.Y > 0f)
							npc.velocity.Y += 0.03f;

						if(npc.velocity.Y < -1.5)
							npc.velocity.Y = -1.5f;
					}else if(npc.directionY == 1 && npc.velocity.Y < 1.5){
						npc.velocity.Y += 0.04f;
						if(npc.velocity.Y < -1.5)
							npc.velocity.Y += 0.05f;
						else if(npc.velocity.Y < 0f)
							npc.velocity.Y -= 0.03f;

						if(npc.velocity.Y > 1.5)
							npc.velocity.Y = 1.5f;
					}
				}
			}else{
				float num2 = 4f;
				float num3 = 1.5f;
				num2 *= 1f + (1f - npc.scale);
				num3 *= 1f + (1f - npc.scale);
				if(npc.direction == -1 && npc.velocity.X > 0f - num2){
					npc.velocity.X -= 0.1f;
					if(npc.velocity.X > num2)
						npc.velocity.X -= 0.1f;
					else if(npc.velocity.X > 0f)
						npc.velocity.X += 0.05f;

					if(npc.velocity.X < 0f - num2)
						npc.velocity.X = 0f - num2;
				}else if(npc.direction == 1 && npc.velocity.X < num2){
					npc.velocity.X += 0.1f;
					if(npc.velocity.X < 0f - num2)
						npc.velocity.X += 0.1f;
					else if(npc.velocity.X < 0f)
						npc.velocity.X -= 0.05f;

					if(npc.velocity.X > num2)
						npc.velocity.X = num2;
				}

				if(npc.directionY == -1 && npc.velocity.Y > 0f - num3){
					npc.velocity.Y -= 0.04f;
					if(npc.velocity.Y > num3)
						npc.velocity.Y -= 0.05f;
					else if(npc.velocity.Y > 0f)
						npc.velocity.Y += 0.03f;

					if(npc.velocity.Y < 0f - num3)
						npc.velocity.Y = 0f - num3;
				}else if(npc.directionY == 1 && npc.velocity.Y < num3){
					npc.velocity.Y += 0.04f;
					if(npc.velocity.Y < 0f - num3)
						npc.velocity.Y += 0.05f;
					else if(npc.velocity.Y < 0f)
						npc.velocity.Y -= 0.03f;

					if(npc.velocity.Y > num3)
						npc.velocity.Y = num3;
				}
			}

			if((npc.type == NPCID.DemonEye || npc.type == NPCID.WanderingEye || npc.type == NPCID.CataractEye || npc.type == NPCID.SleepyEye || npc.type == NPCID.DialatedEye || npc.type == NPCID.GreenEye || npc.type == NPCID.PurpleEye) && Main.rand.Next(40) == 0){
				int num4 = Dust.NewDust(new Vector2(npc.position.X, npc.position.Y + npc.height * 0.25f), npc.width, (int)(npc.height * 0.5f), 5, npc.velocity.X, 2f);
				Main.dust[num4].velocity.X *= 0.5f;
				Main.dust[num4].velocity.Y *= 0.1f;
			}

			if(npc.wet && npc.type != NPCID.PigronCorruption && npc.type != NPCID.PigronHallow && npc.type != NPCID.RuneWizard){
				if(npc.velocity.Y > 0f)
					npc.velocity.Y *= 0.95f;

				npc.velocity.Y -= 0.5f;
				if(npc.velocity.Y < -4f)
					npc.velocity.Y = -4f;

				npc.TargetClosest();
			}
		}
	}
}
