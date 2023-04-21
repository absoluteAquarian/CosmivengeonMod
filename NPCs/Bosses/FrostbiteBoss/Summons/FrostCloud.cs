using CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Worlds;
using Microsoft.Xna.Framework;
using System;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Bosses.FrostbiteBoss.Summons{
	public class FrostCloud : ModNPC{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frost Cloud");
			Main.npcFrameCount[NPC.type] = 4;
		}

		public override void SetDefaults(){
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 1;
			NPC.width = 46;
			NPC.height = 32;
			NPC.damage = 30;
			NPC.noTileCollide = true;
			NPC.noGravity = true;
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

		public override void SendExtraAI(BinaryWriter writer){
			writer.Write(spawned);
			writer.Write((byte)AI_State);
			writer.Write(AI_Timer);
			writer.Write((byte)PlayerTarget.whoAmI);
			writer.Write((byte)Parent.whoAmI);
			writer.Write(frameCount);
			writer.Write(activeTime);
		}

		public override void ReceiveExtraAI(BinaryReader reader){
			spawned = reader.ReadByte() == 1;
			AI_State = reader.ReadByte();
			AI_Timer = reader.ReadInt32();
			PlayerTarget = Main.player[reader.ReadByte()];
			Parent = Main.npc[reader.ReadByte()];
			frameCount = reader.ReadByte();
			activeTime = reader.ReadInt32();
		}

		public override void FindFrame(int frameHeight){
			if(AI_Timer % 15 == 0)
				NPC.frame.Y = ++frameCount % Main.npcFrameCount[NPC.type] * frameHeight;
		}

		public override void AI(){
			if(!spawned){
				spawned = true;
				NPC.TargetClosest(true);
				PlayerTarget = Main.player[NPC.target];
				Parent = Main.npc[(int)NPC.ai[0]];
				NPC.velocity = new Vector2(NPC.ai[1], NPC.ai[2]);

				NPC.netUpdate = true;
			}

			if(activeTime < 0 || !Parent.active || Parent.townNPC || !Parent.boss || Parent.friendly){
				NPC.life = 0;
				NPC.active = false;
			}

			TargetPosition = PlayerTarget.Center - new Vector2(0, 15 * 16);

			if(AI_State == AI_JustSpawned){
				if(NPC.velocity.Length() > TargetSpeed)
					NPC.velocity *= 0.9435f;
				else{
					NPC.velocity = Vector2.Normalize(NPC.velocity) * TargetSpeed;
					AI_State++;

					NPC.netUpdate = true;
				}
			}else if(AI_State == AI_FollowPlayer){
				NPC.velocity *= 0.9427f;

				NPC.velocity += Vector2.Normalize(TargetPosition - NPC.Center) * 0.735f;

				if(NPC.velocity.Length() > TargetSpeed)
					NPC.velocity = Vector2.Normalize(NPC.velocity) * TargetSpeed;

				if(Math.Abs(NPC.Center.X - TargetPosition.X) < 2 * 16f && AI_Timer % 20 == 0){
					MiscUtils.SpawnProjectileSynced(NPC.Center + new Vector2(Main.rand.NextFloat(-24, 24), 0),
						Vector2.Zero,
						ModContent.ProjectileType<FrostbiteIcicle>(),
						20,
						4f
					);

					NPC.netUpdate = true;
				}
			}

			AI_Timer++;

			if(!WorldEvents.desoMode)
				activeTime--;
		}
	}
}
