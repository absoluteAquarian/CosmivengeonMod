using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Desomode{
	public class ModifiedHornet : ModNPC{
		public override string Texture => $"Terraria/NPC_{NPCID.Hornet}";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Angry Hornet");
			Main.npcFrameCount[NPC.type] = Main.npcFrameCount[NPCID.Hornet];
		}

		public override void SetDefaults(){
			NPC.CloneDefaults(NPCID.Hornet);
			NPC.aiStyle = -1;
			AnimationType = NPCID.Hornet;
			NPC.noTileCollide = true;
		}

		public override void AI(){
			if(NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead)
				NPC.TargetClosest();

			float num = 3.5f;
			float num2 = 0.021f;

			num *= 1f - NPC.scale;
			num2 *= 1f - NPC.scale;

			Vector2 vector = NPC.Center;
			float num4 = Main.player[NPC.target].position.X + Main.player[NPC.target].width / 2;
			float num5 = Main.player[NPC.target].position.Y + Main.player[NPC.target].height / 2;

			num4 = (int)(num4 / 8f) * 8;
			num5 = (int)(num5 / 8f) * 8;
			vector.X = (int)(vector.X / 8f) * 8;
			vector.Y = (int)(vector.Y / 8f) * 8;
			num4 -= vector.X;
			num5 -= vector.Y;

			float num6 = (float)Math.Sqrt(num4 * num4 + num5 * num5);

			if(num6 == 0f){
				num4 = NPC.velocity.X;
				num5 = NPC.velocity.Y;
			}else{
				num6 = num / num6;
				num4 *= num6;
				num5 *= num6;
			}

			NPC.ai[0] += 1f;

			//Ignoring the usual velocity-setting code
			NPC.velocity += NPC.DirectionTo(Main.player[NPC.target].Center) * 0.023f;

			if(NPC.ai[0] > 200f)
				NPC.ai[0] = -200f;

			if(Main.player[NPC.target].dead){
				num4 = NPC.direction * num / 2f;
				num5 = (0f - num) / 2f;
			}

			if(NPC.velocity.X < num4)
				NPC.velocity.X += num2;
			else if(NPC.velocity.X > num4)
				NPC.velocity.X -= num2;

			if(NPC.velocity.Y < num5)
				NPC.velocity.Y += num2;
			else if(NPC.velocity.Y > num5)
				NPC.velocity.Y -= num2;

			if(NPC.velocity.X > 0f)
				NPC.spriteDirection = 1;

			if(NPC.velocity.X < 0f)
				NPC.spriteDirection = -1;

			NPC.rotation = NPC.velocity.X * 0.1f;

			//No bouncing code: NPC phases through all tiles

			if(NPC.wet){
				if(NPC.velocity.Y > 0f)
					NPC.velocity.Y *= 0.95f;

				NPC.velocity.Y -= 0.5f;
				if(NPC.velocity.Y < -4f)
					NPC.velocity.Y = -4f;

				NPC.TargetClosest();
			}

			if(NPC.ai[1] == 101f){
				SoundEngine.PlaySound(SoundID.Item17, NPC.position);
				NPC.ai[1] = 0f;
			}

			if(Main.netMode != NetmodeID.MultiplayerClient){
				NPC.ai[1] += Main.rand.Next(5, 20) * 0.1f * NPC.scale;

				if(Main.player[NPC.target].stealth == 0f && Main.player[NPC.target].itemAnimation == 0)
					NPC.ai[1] = 0f;

				if(NPC.ai[1] >= 130f){
					if(Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height)){
						float num17 = 8f;
						Vector2 vector2 = NPC.Center;
						float num18 = Main.player[NPC.target].position.X + Main.player[NPC.target].width * 0.5f - vector2.X + Main.rand.Next(-20, 21);
						float num19 = Main.player[NPC.target].position.Y + Main.player[NPC.target].height * 0.5f - vector2.Y + Main.rand.Next(-20, 21);
						if((num18 < 0f && NPC.velocity.X < 0f) || (num18 > 0f && NPC.velocity.X > 0f)){
							float num20 = (float)Math.Sqrt(num18 * num18 + num19 * num19);
							num20 = num17 / num20;
							num18 *= num20;
							num19 *= num20;
							int num21 = (int)(10f * NPC.scale);

							int num22 = 55;
							int num23 = Projectile.NewProjectile(vector2.X, vector2.Y, num18, num19, num22, num21, 0f, Main.myPlayer);
							Main.projectile[num23].timeLeft = 300;
							NPC.ai[1] = 101f;
							NPC.netUpdate = true;
						}else
							NPC.ai[1] = 0f;
					}else
						NPC.ai[1] = 0f;
				}
			}

			if(Main.player[NPC.target].dead){
				NPC.velocity.Y -= num2 * 2f;
				if(NPC.timeLeft > 10)
					NPC.timeLeft = 10;
			}

			if(!NPC.justHit)
				NPC.netUpdate = true;
		}
	}
}
