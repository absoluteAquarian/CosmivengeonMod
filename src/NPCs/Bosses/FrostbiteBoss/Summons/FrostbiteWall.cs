using CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss;
using CosmivengeonMod.Utility;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Bosses.FrostbiteBoss.Summons {
	public class FrostbiteWall : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frozen Totem");
		}

		private bool shouldShootBolts = false;
		private int activeTime = 1;

		public override void SetDefaults() {
			NPC.noTileCollide = false;
			NPC.noGravity = false;
			NPC.dontTakeDamage = true;
			NPC.lifeMax = 1;
			NPC.knockBackResist = 0f;
			NPC.width = 30;
			NPC.height = 116;
			NPC.alpha = 255;
			NPC.friendly = false;
		}

		private bool spawned = false;
		private int AI_Timer = -1;

		public override void AI() {
			if (!spawned) {
				spawned = true;
				NPC.TargetClosest(false);
				activeTime = (int)NPC.ai[0];
				NPC.defDamage = NPC.damage = (int)NPC.ai[1];
				shouldShootBolts = NPC.ai[2] == 1;

				NPC.netUpdate = true;
			}

			activeTime--;
			if (activeTime < 0) {
				NPC.life = 0;
				NPC.active = false;
			}

			if (NPC.alpha > 0)
				NPC.alpha -= 3;
			else if (NPC.alpha < 0)
				NPC.alpha = 0;

			if (shouldShootBolts) {
				if (AI_Timer < 0) {
					AI_Timer = Main.rand.Next(60, 120);

					NPC.netUpdate = true;
				} else if (AI_Timer == 0) {
					//Spawn some Frostbite ice projectiles (the breath ones)
					for (int i = 0; i < 6; i++) {
						MiscUtils.SpawnProjectileSynced(
							NPC.GetSource_FromAI(),
							NPC.Top + new Vector2(0, 16),
							new Vector2(0, -7).RotatedByRandom(MathHelper.ToRadians(15)),
							ModContent.ProjectileType<FrostbiteBreath>(),
							30,
							2f,
							1f
						);
					}
				}
			}

			NPC.velocity.Y += 8f / 60f;

			AI_Timer--;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			BitsByte flag = new BitsByte(shouldShootBolts, spawned);

			writer.Write(flag);
			writer.Write(AI_Timer);
			writer.Write(activeTime);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref shouldShootBolts, ref spawned);
			AI_Timer = reader.ReadInt32();
			activeTime = reader.ReadInt32();
		}

		public override void ModifyHitPlayer(Player target, ref int damage, ref bool crit) {
			if (NPC.alpha > 0)
				damage = 0;
		}
	}
}
