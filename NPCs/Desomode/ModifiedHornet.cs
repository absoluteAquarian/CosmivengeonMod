using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class ModifiedHornet : ModNPC{
		public override string Texture => $"Terraria/NPC_{NPCID.Hornet}";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Angry Hornet");
			Main.npcFrameCount[npc.type] = Main.npcFrameCount[NPCID.Hornet];
		}

		public override void SetDefaults(){
			npc.CloneDefaults(NPCID.Hornet);
			npc.aiStyle = -1;
			animationType = NPCID.Hornet;
			npc.noTileCollide = true;
		}

		public override void AI(){
			if(npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead)
				npc.TargetClosest();

			float num = 3.5f;
			float num2 = 0.021f;

			num *= 1f - npc.scale;
			num2 *= 1f - npc.scale;

			Vector2 vector = npc.Center;
			float num4 = Main.player[npc.target].position.X + Main.player[npc.target].width / 2;
			float num5 = Main.player[npc.target].position.Y + Main.player[npc.target].height / 2;

			num4 = (int)(num4 / 8f) * 8;
			num5 = (int)(num5 / 8f) * 8;
			vector.X = (int)(vector.X / 8f) * 8;
			vector.Y = (int)(vector.Y / 8f) * 8;
			num4 -= vector.X;
			num5 -= vector.Y;

			float num6 = (float)Math.Sqrt(num4 * num4 + num5 * num5);

			if(num6 == 0f){
				num4 = npc.velocity.X;
				num5 = npc.velocity.Y;
			}else{
				num6 = num / num6;
				num4 *= num6;
				num5 *= num6;
			}

			npc.ai[0] += 1f;

			//Ignoring the usual velocity-setting code
			npc.velocity += npc.DirectionTo(Main.player[npc.target].Center) * 0.023f;

			if(npc.ai[0] > 200f)
				npc.ai[0] = -200f;

			if(Main.player[npc.target].dead){
				num4 = npc.direction * num / 2f;
				num5 = (0f - num) / 2f;
			}

			if(npc.velocity.X < num4)
				npc.velocity.X += num2;
			else if(npc.velocity.X > num4)
				npc.velocity.X -= num2;

			if(npc.velocity.Y < num5)
				npc.velocity.Y += num2;
			else if(npc.velocity.Y > num5)
				npc.velocity.Y -= num2;

			if(npc.velocity.X > 0f)
				npc.spriteDirection = 1;

			if(npc.velocity.X < 0f)
				npc.spriteDirection = -1;

			npc.rotation = npc.velocity.X * 0.1f;

			//No bouncing code: NPC phases through all tiles

			if(npc.wet){
				if(npc.velocity.Y > 0f)
					npc.velocity.Y *= 0.95f;

				npc.velocity.Y -= 0.5f;
				if(npc.velocity.Y < -4f)
					npc.velocity.Y = -4f;

				npc.TargetClosest();
			}

			if(npc.ai[1] == 101f){
				Main.PlaySound(SoundID.Item17, npc.position);
				npc.ai[1] = 0f;
			}

			if(Main.netMode != NetmodeID.MultiplayerClient){
				npc.ai[1] += Main.rand.Next(5, 20) * 0.1f * npc.scale;

				if(Main.player[npc.target].stealth == 0f && Main.player[npc.target].itemAnimation == 0)
					npc.ai[1] = 0f;

				if(npc.ai[1] >= 130f){
					if(Collision.CanHit(npc.position, npc.width, npc.height, Main.player[npc.target].position, Main.player[npc.target].width, Main.player[npc.target].height)){
						float num17 = 8f;
						Vector2 vector2 = npc.Center;
						float num18 = Main.player[npc.target].position.X + Main.player[npc.target].width * 0.5f - vector2.X + Main.rand.Next(-20, 21);
						float num19 = Main.player[npc.target].position.Y + Main.player[npc.target].height * 0.5f - vector2.Y + Main.rand.Next(-20, 21);
						if((num18 < 0f && npc.velocity.X < 0f) || (num18 > 0f && npc.velocity.X > 0f)){
							float num20 = (float)Math.Sqrt(num18 * num18 + num19 * num19);
							num20 = num17 / num20;
							num18 *= num20;
							num19 *= num20;
							int num21 = (int)(10f * npc.scale);

							int num22 = 55;
							int num23 = Projectile.NewProjectile(vector2.X, vector2.Y, num18, num19, num22, num21, 0f, Main.myPlayer);
							Main.projectile[num23].timeLeft = 300;
							npc.ai[1] = 101f;
							npc.netUpdate = true;
						}else
							npc.ai[1] = 0f;
					}else
						npc.ai[1] = 0f;
				}
			}

			if(Main.player[npc.target].dead){
				npc.velocity.Y -= num2 * 2f;
				if(npc.timeLeft > 10)
					npc.timeLeft = 10;
			}

			if(!npc.justHit)
				npc.netUpdate = true;
		}
	}
}
