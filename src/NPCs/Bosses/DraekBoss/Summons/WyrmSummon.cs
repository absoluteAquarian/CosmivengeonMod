﻿using CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.Bestiary;
using CosmivengeonMod.DataStructures.Bestiary;
using System.Collections.Generic;

namespace CosmivengeonMod.NPCs.Bosses.DraekBoss.Summons {
	public class DraekWyrmSummon_Head : Worm {
		private bool hasSpawned = false;
		public int bossID = 0;
		private int AcidSpitTimer = -1;
		private int FastChargeTimer = -1;
		public bool fastCharge = false;
		private float prevSpeed;
		private float prevTurnSpeed;
		private int baseDefense;

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Young Wyrm");

			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				CustomTexturePath = "CosmivengeonMod/Assets/NPCs/Bosses/DraekBoss/Summons/LesserWyrm_Bestiary",
				SpriteDirection = 1,
				Rotation = MathHelper.ToRadians(-30)
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>() {
				new ForestPortraitBackgroundProviderBestiaryInfoElement(),
				new FlavorTextBestiaryInfoElement("Mods.CosmivengeonMod.Bestiary.BossMinions.Draek.LesserWyrm")
			});
		}

		public override void SetDefaults() {
			head = true;

			NPC.width = 25;
			NPC.height = 25;

			NPC.aiStyle = -1;
			NPC.lifeMax = 200;
			NPC.defense = 6;
			NPC.damage = 20;
			NPC.scale = 1f;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			minLength = maxLength = 4;

			headType = ModContent.NPCType<DraekWyrmSummon_Head>();
			//no body type b/c differing body segments
			tailType = ModContent.NPCType<DraekWyrmSummon_Tail>();

			speed = MiscUtils.GetModeChoice(7, 8, 10);
			turnSpeed = 0.1f;

			prevSpeed = speed;
			prevTurnSpeed = turnSpeed;

			fly = false;
			maxDigDistance = 16 * MiscUtils.GetModeChoice(15, 10, 7);
			customBodySegments = true;

			NPC.HitSound = SoundID.Tink;   //Stone tile hit sound

			bossID = (int)NPC.ai[1];

			baseDefense = NPC.defense;
		}

		public override int SetCustomBodySegments(int startDistance) {
			int latestNPC = NPC.whoAmI;
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body0>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekWyrmSummon_Body1>(), latestNPC);
			return latestNPC;
		}

		public override void SendExtraAI(BinaryWriter writer) {
			BitsByte flag = new BitsByte(hasSpawned, fastCharge);
			writer.Write(flag);
			writer.Write(AcidSpitTimer);
			writer.Write((byte)bossID);
			writer.Write(FastChargeTimer);
			writer.Write((byte)baseDefense);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref hasSpawned, ref fastCharge);
			AcidSpitTimer = reader.ReadInt32();
			bossID = reader.ReadByte();
			FastChargeTimer = reader.ReadInt32();
			baseDefense = reader.ReadByte();
		}

		public override void AI() {
			if (!hasSpawned) {
				hasSpawned = true;
				NPC.TargetClosest(false);
				FastChargeTimer = Main.rand.Next(6 * 60, 10 * 60);
				NPC.netUpdate = true;
			}

			if (NPC.target < 0 || NPC.target == 255 || Main.player[NPC.target].dead) {
				NPC.TargetClosest(true);
				NPC.netUpdate = true;
			}

			if (Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) > 100 * 16) {
				NPC.life = 0;
				NPC.active = false;
			}

			//Occasionally spit acid
			if (WorldEvents.desoMode) {
				if (AcidSpitTimer < 0) {
					MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
						NPC.position,
						Vector2.Zero,
						ModContent.ProjectileType<DraekAcidSpit>(),
						20,
						3f,
						Main.player[NPC.target].Center.X,
						Main.player[NPC.target].Center.Y
					);

					AcidSpitTimer = Main.rand.Next(60, 150);

					NPC.netUpdate = true;
				}
				AcidSpitTimer--;
			}

			//Check for Expert+ and the fast charge
			if (Main.expertMode) {
				FastChargeTimer--;

				if (!fastCharge && FastChargeTimer == 0) {
					//If the timer is 0, do the thing
					fastCharge = true;
					speed = prevSpeed + 4f;
					turnSpeed = 0.25f;
					FastChargeTimer = 120;
					NPC.defense = 0;

					SoundEngine.PlaySound(SoundID.Item27 with { Volume = 0.35f }, NPC.Center);

					NPC.netUpdate = true;
				} else if (fastCharge && FastChargeTimer == 0) {
					fastCharge = false;
					speed = prevSpeed;
					turnSpeed = prevTurnSpeed;
					FastChargeTimer = Main.rand.Next(6 * 60, 10 * 60);
					NPC.defense = baseDefense;

					NPC.netUpdate = true;
				}
			}

			if (fastCharge && Main.rand.NextFloat() < 0.85f) {
				Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GreenFairy);
				dust.velocity = Vector2.Zero;
				dust.noGravity = true;
			}
		}
	}

	internal class DraekWyrmSummon_Body0 : Worm {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Young Wyrm");

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public override void SetDefaults() {
			NPC.width = 30;
			NPC.height = 30;

			NPC.aiStyle = -1;
			NPC.lifeMax = 250;
			NPC.defense = 6;
			NPC.damage = 20;
			NPC.scale = 1f;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			NPC.dontCountMe = true;

			NPC.HitSound = SoundID.Tink;   //Stone tile hit sound
		}

		public override void AI() {
			if (Main.npc[(int)NPC.ai[3]].ModNPC is DraekWyrmSummon_Head head) {
				if (head.fastCharge && Main.rand.NextFloat() < 0.1667) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.GreenFairy);
					dust.velocity = Vector2.Zero;
					dust.noGravity = true;
				}

				NPC.defense = head.NPC.defense;
			}
		}
	}
	internal class DraekWyrmSummon_Body1 : DraekWyrmSummon_Body0 {
		public override void SetDefaults() {
			NPC.CloneDefaults(ModContent.NPCType<DraekWyrmSummon_Body0>());

			NPC.width = 25;
			NPC.height = 25;
		}
	}

	internal class DraekWyrmSummon_Tail : DraekWyrmSummon_Body0 {
		public override void SetDefaults() {
			tail = true;

			NPC.width = 20;
			NPC.height = 20;

			NPC.aiStyle = -1;
			NPC.lifeMax = 250;
			NPC.defense = 6;
			NPC.damage = 20;
			NPC.scale = 1f;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			NPC.dontCountMe = true;

			NPC.HitSound = SoundID.Tink;   //Stone tile hit sound
		}
	}
}
