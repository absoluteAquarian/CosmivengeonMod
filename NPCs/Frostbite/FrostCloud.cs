using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Frostbite{
	public class FrostCloud : ModNPC{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Cloud");
			Main.npcFrameCount[npc.type] = 4;
		}

		public override void SetDefaults(){
			npc.dontTakeDamage = true;
			npc.lifeMax = 1;
			npc.width = 46;
			npc.height = 32;
			npc.damage = 30;
			npc.noTileCollide = true;
			npc.noGravity = true;
		}

		private bool spawned = false;
		private int AI_State = 0;
		private int AI_Timer = 0;

		private const int AI_JustSpawned = 0;
		private const int AI_FollowPlayer = 1;

		private Vector2 TargetPosition;
		private Player PlayerTarget;
		private NPC Parent;

		public const float TargetSpeed = 8f;

		private int frameCount = 0;

		private int activeTime = 15 * 60;

		public override void FindFrame(int frameHeight){
			if(AI_Timer % 15 == 0)
				npc.frame.Y = ++frameCount % Main.npcFrameCount[npc.type] * frameHeight;
		}

		public override void AI(){
			if(!spawned){
				spawned = true;
				npc.TargetClosest(true);
				PlayerTarget = Main.player[npc.target];
				Parent = Main.npc[(int)npc.ai[0]];
				npc.velocity = new Vector2(npc.ai[1], npc.ai[2]);
			}

			if(activeTime < 0 || !Parent.active || Parent.townNPC || !Parent.boss || Parent.friendly){
				npc.life = 0;
				npc.active = false;
			}

			TargetPosition = PlayerTarget.Center - new Vector2(0, 15 * 16);

			if(AI_State == AI_JustSpawned){
				if(npc.velocity.Length() > TargetSpeed)
					npc.velocity *= 0.9435f;
				else{
					npc.velocity = Vector2.Normalize(npc.velocity) * TargetSpeed;
					AI_State++;
				}
			}else if(AI_State == AI_FollowPlayer){
				npc.velocity *= 0.9427f;

				npc.velocity += Vector2.Normalize(TargetPosition - npc.Center) * 0.735f;

				if(npc.velocity.Length() > TargetSpeed)
					npc.velocity = Vector2.Normalize(npc.velocity) * TargetSpeed;

				if(Math.Abs(npc.Center.X - TargetPosition.X) < 2 * 16f && AI_Timer % 20 == 0){
					npc.SpawnProjectile(
						npc.Center.X + Main.rand.NextFloat(-24, 24),
						npc.Center.Y,
						0f,
						0f,
						ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteIcicle>(),
						20,
						4f
					);
				}
			}

			AI_Timer++;

			if(!CosmivengeonWorld.desoMode)
				activeTime--;
		}
	}
}
