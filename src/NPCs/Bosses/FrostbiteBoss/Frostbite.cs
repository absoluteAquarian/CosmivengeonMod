﻿using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Bags;
using CosmivengeonMod.Items.Equippable.Accessories.Frostbite;
using CosmivengeonMod.Items.Equippable.Vanity.BossMasks;
using CosmivengeonMod.Items.Lore;
using CosmivengeonMod.Items.Weapons.Frostbite;
using CosmivengeonMod.NPCs.Bosses.FrostbiteBoss.Summons;
using CosmivengeonMod.Projectiles.NPCSpawned.FrostbiteBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Items.Placeable.Trophies;
using Terraria.GameContent.Bestiary;
using CosmivengeonMod.DataStructures.Bestiary;
using System.Collections.Generic;

namespace CosmivengeonMod.NPCs.Bosses.FrostbiteBoss {
	[AutoloadBossHead]
	public class Frostbite : ModNPC {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Frostbite");
			Main.npcFrameCount[NPC.type] = 15;

			// Add this in for bosses that have a summon item, requires corresponding code in the item
			NPCID.Sets.MPAllowedEnemies[Type] = true;
			// Automatically group with other bosses
			NPCID.Sets.BossBestiaryPriority.Add(Type);

			// Influences how the NPC looks in the Bestiary
			NPCID.Sets.NPCBestiaryDrawModifiers drawModifiers = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Direction = -1,
				SpriteDirection = -1,
				PortraitScale = 0.6f,
				PortraitPositionYOverride = 0,
				Velocity = 1
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(Type, drawModifiers);
		}

		public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry) {
			bestiaryEntry.Info.AddRange(new List<IBestiaryInfoElement>() {
				new SnowPortraitBackgroundProviderBestiaryInfoElement(),
				new FlavorTextBestiaryInfoElement("Mods.CosmivengeonMod.Bestiary.Bosses.Frostbite")
			});
		}

		public override void SetDefaults() {
			NPC.height = 80;
			NPC.width = 180;
			NPC.aiStyle = -1;
			NPC.damage = 26;
			NPC.defense = 8;
			NPC.lifeMax = 2000;
			NPC.HitSound = SoundID.NPCHit11;    //Snow NPC hit sound
			NPC.DeathSound = SoundID.NPCDeath27;
			NPC.knockBackResist = 0f;   //100% kb resist
			NPC.npcSlots = 30f;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noTileCollide = false;

			NPC.value = Item.buyPrice(gold: 2, silver: 75);

			NPC.buffImmune[BuffID.Frostburn] = true;
			NPC.buffImmune[BuffID.OnFire] = true;
			NPC.buffImmune[BuffID.Poisoned] = true;

			MiscUtils.PlayMusic(this, CosmivengeonBoss.Frostbite);

			if (!Main.expertMode && !WorldEvents.desoMode)
				Subphases = new int[] {
					AI_Attack_Walk,
					AI_Attack_Charge,
					AI_Attack_Walk,
					AI_Attack_Flick,
					AI_Attack_Walk,
					AI_Attack_Stomp
				};
			else if (Main.expertMode && !WorldEvents.desoMode)
				Subphases = new int[] {
					AI_Attack_Walk,
					AI_Attack_Charge,
					AI_Attack_Walk,
					AI_Attack_Flick,
					AI_Attack_Walk,
					AI_Attack_Stomp,
					AI_Attack_Walk,
					AI_Expert_Snowball
				};
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			NPC.lifeMax /= 2;   //Negate vanilla health buff
			if (!WorldEvents.desoMode) {
				NPC.ScaleHealthBy(0.8f);
				NPC.damage = 40;
				NPC.defense = 11;
			} else {
				NPC.ScaleHealthBy(0.875f);
				NPC.damage = 55;
				NPC.defense = 14;
			}
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor) {
			Texture2D texture = TextureAssets.Npc[NPC.type].Value;

			spriteBatch.Draw(texture, NPC.Center - Main.screenPosition, NPC.frame, drawColor, 0f, NPC.frame.Size() / 2f, NPC.scale, NPC.spriteDirection == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);

			return false;
		}

		private const int Walk_Frames_Max = 6;
		private const int Flick_Frames_Start = 6;
		private const int Smash_Frames_Start = 9;
		private const int Bite_Frames_Start = 12;

		private const int AI_Attack_Walk = 0;
		private const int AI_Attack_Charge = 1;
		private const int AI_Attack_Flick = 2;
		private const int AI_Attack_Stomp = 3;
		private const int AI_Enrage_Smash = 4;
		private const int AI_Expert_Snowball = 5;
		private const int AI_Expert_SnowCloud = 6;

		private float GetWaitBetweenSubphases()
			=> Main.rand.NextFloat(MiscUtils.GetModeChoice(5, 3, 2), MiscUtils.GetModeChoice(6, 4, 3));

		public override void OnKill() {
			Debug.CheckWorldFlagUpdate(ref WorldEvents.downedFrostbiteBoss);
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<FrostbiteBag>()));

			AddDrops(npcLoot, restrictNormalDrops: true);

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<FrostbiteTrophy>(), 10));

			npcLoot.AddInstancedDrop(static () => !WorldEvents.downedFrostbiteBoss, ModContent.ItemType<IceforgedRelic>(), "Mods.CosmivengeonMod.LootText.FirstKill", literal: false);
		}

		public static void AddDrops(ILoot loot, bool restrictNormalDrops) {
			var weaponOrEquipDrop = ItemDropRule.OneFromOptions(1,
				ModContent.ItemType<FrostbiteFlamethrower>(),
				ModContent.ItemType<SnowballFlail>(),
				ModContent.ItemType<IceDisk>(),
				ModContent.ItemType<BlizzardRod>(),
				ModContent.ItemType<SubZero>(),
				ModContent.ItemType<IceScepter>(),
				ModContent.ItemType<FrostDemonHorn>(),
				ModContent.ItemType<SnowscaleCoat>());

			var materialDrop = ItemDropRule.Common(ModContent.ItemType<FrostCrystal>(), 1, 10, 20);

			if (restrictNormalDrops) {
				var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
				normalOnly.OnSuccess(weaponOrEquipDrop);
				normalOnly.OnSuccess(materialDrop);
				loot.Add(normalOnly);
			} else {
				loot.Add(weaponOrEquipDrop);
				loot.Add(materialDrop);
			}

			loot.Add(ItemDropRule.Common(ModContent.ItemType<FrostbiteMask>(), 10, 1, 1));
		}

		public override bool CheckActive() {
			return Vector2.Distance(NPC.Center, Target?.Center ?? NPC.Center) > 200 * 16;
		}

		public override void FindFrame(int frameHeight) {
			int frame = 0;
			if (CurrentSubphase == AI_Attack_Walk) {
				//He walkin
				frame = AI_AnimationTimer % (Walk_Frames_Max * 12) / 12;
			} else if ((CurrentSubphase == AI_Attack_Charge && AI_AttackProgress < 3) || CurrentSubphase == AI_Attack_Stomp)
				frame = AI_AnimationTimer % (Walk_Frames_Max * 8) / 8;
			else if (CurrentSubphase == AI_Attack_Charge && AI_AttackProgress >= 3)
				frame = Bite_Frames_Start;
			else if ((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress == 1 && AI_Timer > 0) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 0))
				frame = Flick_Frames_Start;
			else if ((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress < normal_icicle_count + 1 && AI_Timer > 0) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 1))
				frame = Flick_Frames_Start + 1;
			else if ((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress == normal_icicle_count + 1) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 2))
				frame = Flick_Frames_Start + 2;
			else if (CurrentSubphase == AI_Enrage_Smash)
				frame = Smash_Frames_Start + AI_AttackProgress;
			else if (CurrentSubphase == AI_Expert_Snowball)
				frame = Bite_Frames_Start + Utils.Clamp(AI_AttackProgress, 0, 2);

			NPC.frame.Y = frame * frameHeight;
		}

		private Player Target;

		private const int Phase_1 = 0;
		private const int Phase_2 = 1;
		private int Phase = Phase_1;
		private int CurrentSubphase = 0;

		private int AI_Timer = -1;
		private int AI_AnimationTimer = 0;
		private int AI_AttackProgress = 0;
		private int AI_WaitTimer = -1;
		private bool switchSubPhase = false;

		private int spriteDir = 1;
		private int curMaxSpeed = 0;

		private const int normal_icicle_count = 7;
		private const int normal_icicle_wait = 45;
		private const int enraged_icicle_count = 6;
		private const int enraged_icicle_wait = 30;
		private const int expert_icicle_count = 11;
		private const int expert_enraged_icicle_count = 8;

		private int subphaseIndex = 0;
		private int[] Subphases;

		private bool noTargetsAlive = false;
		private bool startDespawn = false;

		private bool forceSpriteTurn = false;

		private int timePhasing = 0;

		public override void SendExtraAI(BinaryWriter writer) {
			BitsByte flag = new BitsByte(switchSubPhase, noTargetsAlive, startDespawn, forceSpriteTurn);

			writer.Write(flag);
			writer.Write((byte)Target.whoAmI);
			writer.Write((byte)Phase);
			writer.Write((byte)CurrentSubphase);
			writer.Write(AI_Timer);
			writer.Write(AI_AnimationTimer);
			writer.Write((byte)AI_AttackProgress);
			writer.Write(AI_WaitTimer);
			writer.Write((byte)spriteDir);
			writer.Write((byte)curMaxSpeed);
			writer.Write((byte)subphaseIndex);
			writer.Write(timePhasing);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref switchSubPhase, ref noTargetsAlive, ref startDespawn, ref forceSpriteTurn);
			Target = Main.player[reader.ReadByte()];
			Phase = reader.ReadByte();
			CurrentSubphase = reader.ReadByte();
			AI_Timer = reader.ReadInt32();
			AI_AnimationTimer = reader.ReadInt32();
			AI_AttackProgress = reader.ReadByte();
			AI_WaitTimer = reader.ReadInt32();
			spriteDir = reader.ReadByte();
			curMaxSpeed = reader.ReadByte();
			subphaseIndex = reader.ReadByte();
			timePhasing = reader.ReadInt32();
		}

		public bool CanSeeTarget() => Target != null && Collision.CanHit(NPC.position, NPC.width, NPC.height, Target.position, Target.width, Target.height);

		public bool AnyActiveTilesInHitbox(bool doAhead) {
			Vector2 ahead = doAhead ? NPC.DirectionTo(Target.Center) * 24 : Vector2.Zero;

			Point tileTL = (NPC.position + ahead).ToTileCoordinates();
			Point tileBR = (NPC.BottomRight + ahead).ToTileCoordinates();

			for (int x = tileTL.X; x < tileBR.X; x++) {
				for (int y = tileTL.Y; y < tileBR.Y; y++) {
					if (MiscUtils.TileIsSolidNotPlatform(x, y))
						return true;
				}
			}

			return false;
		}

		public override void DrawEffects(ref Color drawColor) {
			//Cool effect while phasing through tiles
			if (timePhasing > 0) {
				drawColor = Color.White * 0.65f;

				for (int i = 0; i < 10; i++) {
					Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Snow);
					dust.noGravity = true;
					dust.velocity = Vector2.Zero;
				}
			}
		}

		public override void AI() {
			//AI:  https://docs.google.com/document/d/1B7liHxU-65k_f8eXlC6-M4HLMx9Xu5d5LQWFIznylv8

			int prevTarget = NPC.target;
			forceSpriteTurn = false;

			NPC.TargetClosest(true);
			Target = Main.player[NPC.target];

			if (NPC.target != prevTarget)
				NPC.netUpdate = true;

			noTargetsAlive = Target.dead || !Target.active;
			CheckTargetIsDead();

			if (noTargetsAlive)
				return;

			CheckSubPhaseChange();
			CheckPhaseChange();

			if (switchSubPhase)
				return;

			CheckFallThroughPlatforms(out _);

			if ((AnyActiveTilesInHitbox(doAhead: true) || AnyActiveTilesInHitbox(doAhead: false)) && CurrentSubphase == AI_Attack_Walk) {
				timePhasing++;

				PhaseFloat();

				goto skipAI;
			} else
				timePhasing = 0;

			NPC.noGravity = false;

			float breathAngle = NPC.DirectionTo(Target.Center).ToRotation();

			if (Phase == Phase_1) {
				if (CurrentSubphase == AI_Attack_Walk && AI_WaitTimer < 0) {
					AI_WaitTimer = (int)(GetWaitBetweenSubphases() * 60);

					NPC.netUpdate = true;
				} else if (CurrentSubphase == AI_Attack_Walk && AI_WaitTimer >= 0) {
					if (AI_WaitTimer == 0)
						switchSubPhase = true;
					int speed = MiscUtils.GetModeChoice(3, 5, 0);
					float acceleration = MiscUtils.GetModeChoice(2f, 4f, 0);
					AI_Walk(speed, acceleration / 60f);
				} else if (CurrentSubphase == AI_Attack_Charge) {
					int chargeSpeed = MiscUtils.GetModeChoice(5, 7, 0);
					int chargeFrames = MiscUtils.GetModeChoice(120, 90, 0);
					float breathSpeed = MiscUtils.GetModeChoice(6f, 9f, 0);
					float breathTime = MiscUtils.GetModeChoice(1.5f, 1f, 0);
					AI_Charge_BreatheFrost(
						chargeSpeed,
						chargeFrames,
						breathSpeed,
						breathAngle: breathAngle,
						maxFlamesShot: 40,
						breathTime
					);
				} else if (CurrentSubphase == AI_Attack_Flick) {
					int count = Main.expertMode ? expert_icicle_count : normal_icicle_count;
					AI_Flick(20f * 16f / 60f, count, MathHelper.ToRadians(20f));
				} else if (CurrentSubphase == AI_Attack_Stomp) {
					float initialYVel = MiscUtils.GetModeChoice(10f, 13f, 0);
					int xVel = MiscUtils.GetModeChoice(4, 6, 0);
					float acceleration = MiscUtils.GetModeChoice(3f, 5f, 0);
					AI_Stomp(60, initialYVel, xVel, acceleration / 60f);
				} else if (CurrentSubphase == AI_Expert_Snowball)
					AI_Snowball(20, 10f);
			} else if (Phase == Phase_2) {
				if (CurrentSubphase % 2 == 0 && AI_WaitTimer < 0)
					AI_WaitTimer = (int)(GetWaitBetweenSubphases() * 60);
				else if (CurrentSubphase == AI_Attack_Walk && AI_WaitTimer >= 0) {
					if (AI_WaitTimer == 0)
						switchSubPhase = true;
					int speed = MiscUtils.GetModeChoice(5, 7, 8);
					float acceleration = MiscUtils.GetModeChoice(4f, 7f, 9f);
					AI_Walk(speed, acceleration / 60f);
				} else if (CurrentSubphase == AI_Attack_Charge) {
					int chargeSpeed = MiscUtils.GetModeChoice(8, 10, 12);
					int chargeFrames = MiscUtils.GetModeChoice(90, 75, 60);
					float breathSpeed = MiscUtils.GetModeChoice(9f, 11f, 13f);
					int maxFlamesShot = MiscUtils.GetModeChoice(60, 25, 45);
					float breathTime = MiscUtils.GetModeChoice(1f, 0.75f, 0.5f);
					AI_Charge_BreatheFrost(
						chargeSpeed,
						chargeFrames,
						breathSpeed,
						breathAngle: breathAngle,
						maxFlamesShot,
						breathTime
					);
				} else if (CurrentSubphase == AI_Attack_Flick) {
					int count = MiscUtils.GetModeChoice(enraged_icicle_count, expert_enraged_icicle_count, expert_enraged_icicle_count + 4);
					AI_Flick(25f * 16f / 60f, count, MathHelper.ToRadians(15f));
				} else if (CurrentSubphase == AI_Attack_Stomp) {
					float initialYVel = MiscUtils.GetModeChoice(13f, 14f, 15f);
					int xVel = MiscUtils.GetModeChoice(6, 9, 10);
					float acceleration = MiscUtils.GetModeChoice(5f, 8f, 10f);
					AI_Stomp(45, initialYVel, xVel, acceleration / 60f);
				} else if (CurrentSubphase == AI_Enrage_Smash)
					AI_Smash(30, 5 * 60, Main.expertMode);
				else if (CurrentSubphase == AI_Expert_Snowball)
					AI_Snowball(15, 15f);
				else if (CurrentSubphase == AI_Expert_SnowCloud)
					AI_SummonCloud();
			}

			if (AI_Timer >= 0)
				AI_Timer--;

			if (AI_WaitTimer >= 0)
				AI_WaitTimer--;

			//Extra gravity when not stomping
			if (CurrentSubphase != AI_Attack_Stomp)
				NPC.velocity.Y += 16f / 60f;

			CheckTileStep();

skipAI:
			//If Frostbite is charging/breathing frost, don't update the direction
			if (AI_WaitTimer < 0 && CurrentSubphase == AI_Attack_Charge && AI_AttackProgress < 3)
				NPC.spriteDirection = spriteDir;
			else if (!forceSpriteTurn) {
				int sign = Math.Sign(NPC.velocity.X);
				NPC.spriteDirection = sign == 0 ? NPC.spriteDirection : sign;
			}

			AI_AnimationTimer++;

			//Increased friction when turning around
			if (Math.Sign(NPC.Center.X - Target.Center.X) == Math.Sign(NPC.velocity.X))
				NPC.velocity.X *= 0.9742f;

			if (timePhasing == 0)
				NPC.velocity.X.Clamp(-curMaxSpeed, curMaxSpeed);
		}

		private void PhaseFloat() {
			NPC.noTileCollide = true;
			NPC.noGravity = true;

			int speed = MiscUtils.GetModeChoice(5, 7, 8);
			float acceleration = MiscUtils.GetModeChoice(2.25f, 5.635f, 8.15f);
			AI_Walk(speed, acceleration / 60f);

			float targetY = Target.Bottom.Y - NPC.height / 2f - 4;
			float epsilon = 2;
			float diffY = NPC.Center.Y - targetY;

			if (Math.Sign(NPC.velocity.Y) == Math.Sign(diffY))
				NPC.velocity.Y *= 1f - 8.35f / 60f;

			if (Math.Abs(diffY) < epsilon) {
				Vector2 v = NPC.Center;
				v.Y = targetY;
				NPC.Center = v;
				NPC.velocity.Y = 0;
			} else
				NPC.velocity.Y += -Math.Sign(diffY) * 9.75f / 60f;

			float cap = 6 + (1.125f / 60f) * timePhasing;

			NPC.velocity.X.Clamp(-cap, cap);
			NPC.velocity.Y.Clamp(-cap, cap);
		}

		private void CheckTargetIsDead() {
			//If the target is dead or not active, slow down the NPC
			//Then, plummet to hell and despawn naturally
			if (noTargetsAlive) {
				if (Math.Abs(NPC.velocity.X) > 0)
					NPC.velocity.X *= 0.5f;

				if (NPC.velocity.Length() < 1 && NPC.velocity != Vector2.Zero) {
					NPC.velocity = Vector2.Zero;
				}

				if (NPC.velocity.X == 0) {
					NPC.velocity.Y += 15f;
				}

				if (!startDespawn) {
					startDespawn = true;
					NPC.noTileCollide = true;
					NPC.timeLeft = (int)(0.5f * 60);

					NPC.netUpdate = true;
				}

				if (NPC.timeLeft == 0)
					NPC.active = false;

				NPC.timeLeft--;
			}
		}

		private void CheckPhaseChange() {
			if (!Main.expertMode && !WorldEvents.desoMode) {
				if (NPC.life / (float)NPC.lifeMax < 0.5f && Phase == Phase_1) {
					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[] {
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash
					};

					NPC.netUpdate = true;
				}
			} else if (Main.expertMode && !WorldEvents.desoMode) {
				if (NPC.life / (float)NPC.lifeMax < 0.7f && Phase == Phase_1) {
					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[] {
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash,
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Expert_Snowball,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Expert_SnowCloud
					};

					NPC.netUpdate = true;
				}
			} else {
				if (Phase == Phase_1) {
					Main.raining = true;
					Main.rainTime = 3 * 60 * 60;

					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[] {
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Stomp,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash,
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Expert_Snowball,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Expert_SnowCloud
					};

					NPC.netUpdate = true;
				}
			}
		}

		private void CheckSubPhaseChange() {
			if (!switchSubPhase)
				return;

			CurrentSubphase = Subphases[++subphaseIndex % Subphases.Length];

			AI_AnimationTimer = 0;
			AI_AttackProgress = 0;
			AI_Timer = -1;
			AI_WaitTimer = -1;
			switchSubPhase = false;

			NPC.netUpdate = true;
		}

		private void CheckFallThroughPlatforms(out bool checkFailed) {
			checkFailed = true;

			if (CurrentSubphase != AI_Attack_Walk || CurrentSubphase == AI_Attack_Stomp)
				return;

			//If the player is too far away, don't do the platform checks
			float dist = 40 * 16;
			if (NPC.DistanceSQ(Target.Center) > dist * dist)
				return;

			//If we can't see the player, don't do the platform checks
			if (!CanSeeTarget()) {
				NPC.noTileCollide = false;
				return;
			}

			checkFailed = false;

			//First, check if the tiles directly beneath Frostbite
			// are platforms/air.  If they are, disable tile collision
			int tileStartX = (int)(NPC.position.X / 16f);
			int tileEndX = (int)(NPC.BottomRight.X / 16f);
			int tileY = (int)((NPC.Bottom.Y + 8f) / 16f);

			for (int x = tileStartX; x <= tileEndX; x++) {
				//If this tile isn't a platform and is solid, re-enable tile collision
				if (MiscUtils.TileIsSolidNotPlatform(x, tileY)) {
					NPC.noTileCollide = false;
					NPC.netUpdate = true;
					return;
				}
			}

			//Next, if we're in line with the player target or they're above us,
			// don't do anything
			//Enable tile collision just in case though
			if (Target.Top.Y <= NPC.Bottom.Y) {
				NPC.noTileCollide = false;
				return;
			}

			//Finally, we just have platforms under us.  Disable tile collision
			NPC.noTileCollide = true;
			NPC.netUpdate = true;
		}

		private void CheckTileStep() {
			if (CurrentSubphase == AI_Attack_Stomp)
				return;

			Collision.StepUp(ref NPC.position, ref NPC.velocity, NPC.width, NPC.height, ref NPC.stepSpeed, ref NPC.gfxOffY, specialChecksMode: 1);
		}

		private void AI_Walk(int speed, float acceleration) {
			curMaxSpeed = speed;
			NPC.velocity.X += acceleration * (NPC.Center.X < Target.Center.X ? 1 : -1);

			int nextSubPhase = Subphases[(subphaseIndex + 1) % Subphases.Length];
			if (Math.Sign(NPC.velocity.X) != Math.Sign(Target.Center.X - NPC.Center.X) && (nextSubPhase == AI_Attack_Flick || nextSubPhase == AI_Attack_Stomp))
				AI_WaitTimer++;
		}

		private void AI_Charge_BreatheFrost(int chargeSpeed, int chargeFrames, float breathSpeed, float breathAngle, float maxFlamesShot, float breathTime) {
			if (AI_AttackProgress < 3)
				AI_Charge(chargeSpeed, chargeFrames);
			else if (AI_AttackProgress >= 3)
				AI_BreatheFrost(breathSpeed, breathAngle, maxFlamesShot, breathTime);
		}

		private void AI_Charge(int speed, int maxFrames) {
			curMaxSpeed = speed;
			if (AI_AttackProgress == 0) {
				//Set the NPC's direction and spriteDirection so that he doesn't turn around while charging
				spriteDir = NPC.spriteDirection;
				AI_AttackProgress++;
				AI_Timer = -1;
			} else if (AI_AttackProgress == 1) {
				//Slow down and prepare to charge
				if (AI_Timer < 0)
					AI_Timer = 60;

				if (AI_Timer > 0)
					NPC.velocity.X *= 0.873f;
				else {
					AI_AttackProgress++;
					AI_Timer = -1;
					SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
					NPC.netUpdate = true;
				}
			} else if (AI_AttackProgress == 2) {
				//The direction has been set.  Ram towards where the player was
				NPC.velocity.X = speed * spriteDir;
				if (AI_Timer < 0)
					AI_Timer = maxFrames;

				if (AI_Timer == 0) {
					AI_AttackProgress++;
					AI_Timer = -1;
					NPC.netUpdate = true;
				}
			}
		}

		private void AI_BreatheFrost(float speed, float angle, float flamesPerSecond, float breatheTime) {
			if (Main.netMode == NetmodeID.MultiplayerClient)
				return;

			NPC.spriteDirection = NPC.Center.X < Target.Center.X ? 1 : -1;
			forceSpriteTurn = true;

			//Get the point and direction the frost breath should go towards
			Vector2 start = (NPC.spriteDirection == 1) ? NPC.Right - new Vector2(45, 0) : NPC.Left + new Vector2(45, 0);
			//Then, get the maximum amount of flames Frostbite will shoot
			//This is determined by the "breatheTime" parameter (seconds)
			//Frostbite will shoot "flamesPerSecond" flames per second
			int maxFlames = (int)(flamesPerSecond * breatheTime + 1);

			if (AI_Timer < 0)
				AI_Timer = (int)(60f / flamesPerSecond) + 1;

			if (AI_AttackProgress - 3 < maxFlames && AI_Timer == 0) {
				//Get a random angle to add to "angle"; variation is [-3, 3] degrees
				//Get the speed vector based on this angle
				Vector2 flameSpeed = angle.ToRotationVector2() * speed;
				//Finally, spawn the projectile
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					start,
					flameSpeed.RotatedByRandom(MathHelper.ToRadians(3)),
					ModContent.ProjectileType<FrostbiteBreath>(),
					30,
					2f
				);

				AI_AttackProgress++;
				AI_Timer = -1;

				SoundEngine.PlaySound(SoundID.Item34, start);
			}

			if (AI_AttackProgress == maxFlames + 3)
				switchSubPhase = true;

			NPC.velocity.X *= 0.8159f;
		}

		private int curIcicle = 0;

		private void AI_Flick(float speedX, int numProjectiles, float angle) {
			speedX *= NPC.spriteDirection;

			if (AI_Timer < 0 && AI_AttackProgress == 0) {
				curIcicle = -(int)(numProjectiles / 2f);
				AI_Timer = Phase == Phase_1 ? normal_icicle_wait : enraged_icicle_wait;
				AI_AttackProgress++;
			} else if (AI_Timer >= 0 && AI_AttackProgress == 1) {
				NPC.velocity.X *= 0.9185f;
			} else if (AI_Timer < 0 && AI_AttackProgress < 1 + numProjectiles) {
				angle.MirrorAngle(mirrorY: NPC.spriteDirection == 1);

				//Calculate the Y-velocity the middle projectile needs to move at,
				// then just add/subtract some from the X-velocity to make the spread
				//Yes fuck you I'm stealing Cyrogen's attack.  Bite me
				//Why did i make that comment ^
				float speedY = 1.5f * -Math.Abs(speedX);

				float speedXFactor = 0.7f;

				Vector2 spawn = NPC.spriteDirection == -1 ? NPC.TopRight : NPC.position;

				//Spawn the icicles
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					spawn,
					new Vector2(speedX + speedXFactor * curIcicle, speedY),
					ModContent.ProjectileType<FrostbiteIcicle>(),
					MiscUtils.GetModeChoice(16, 28, 35),
					3f
				);

				AI_AttackProgress++;

				curIcicle++;
			} else if (AI_AttackProgress >= 1 + numProjectiles)
				switchSubPhase = true;
		}

		private void AI_Stomp(int jumpDelay, float initialYVel, int xVel, float walkAccel) {
			if (AI_Timer < 0 && AI_AttackProgress == 0) {
				AI_Timer = jumpDelay;
				AI_AttackProgress++;
				AI_Walk(xVel, walkAccel);
			} else if (AI_Timer >= 0 && AI_AttackProgress == 1) {
				AI_Walk(xVel, walkAccel);

				if (NPC.velocity.X != 0 && Math.Sign(NPC.velocity.X) != Math.Sign(Target.Center.X - NPC.Center.X))
					AI_Timer++;
			} else if (AI_Timer < 0 && AI_AttackProgress == 1) {
				NPC.velocity.X += MiscUtils.GetModeChoice(4f, 6f, 9f) * NPC.spriteDirection;
				NPC.velocity.Y = -initialYVel;
				AI_AttackProgress++;
			} else if (AI_AttackProgress == 2) {
				NPC.velocity.X += 8f / 60f * NPC.spriteDirection;
				NPC.velocity.Y += initialYVel / 20f / 60f;

				if (NPC.position.Y == NPC.oldPosition.Y && NPC.oldVelocity.Y != 0)
					AI_AttackProgress++;
			} else if (AI_AttackProgress == 3) {
				//Spawn a shitton of the flame particles and play an explosion sound
				SoundEngine.PlaySound(SoundID.Item14 with { Volume = 0.75f }, NPC.Bottom);

				for (int i = 0; i < 30; i++) {
					MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
						NPC.Bottom,
						new Vector2(0, -9).RotatedByRandom(MathHelper.ToRadians(60)),
						ModContent.ProjectileType<FrostbiteBreath>(),
						30,
						2f,
						1f
					);
				}

				switchSubPhase = true;
			}
		}

		private void AI_Smash(int delay, int wallTimeLeft, bool wallsShootBolts) {
			if (AI_AttackProgress == 0 && AI_Timer < 0)
				AI_Timer = delay;
			else if (AI_AttackProgress == 0 && AI_Timer >= 0) {
				if (AI_Timer == 0)
					AI_AttackProgress++;

				NPC.velocity.X *= 0.925f;
			} else if (AI_AttackProgress == 1 && AI_Timer < 0) {
				AI_Timer = 20;

				for (int i = 0; i < 2; i++) {
					Vector2 offset = Target.Top + new Vector2(i == 0 ? 10 * 16f : -10 * 16f, -2 * 16);
					MiscUtils.SpawnNPCSynced(NPC.GetSource_FromAI(), offset, ModContent.NPCType<FrostbiteWall>(), wallTimeLeft, 80, wallsShootBolts ? 1 : 0);
				}
			} else if (AI_AttackProgress == 1 && AI_Timer == 0) {
				AI_AttackProgress++;
				AI_Timer = 20;
			} else if (AI_AttackProgress == 2 && AI_Timer == 0)
				switchSubPhase = true;
		}

		private void AI_Snowball(int delay, float speed) {
			NPC.velocity.X *= 0.8732f;

			if (AI_AttackProgress == 0 && AI_Timer < 0)
				AI_Timer = delay;
			else if (AI_AttackProgress == 0 && AI_Timer == 0) {
				AI_AttackProgress++;
				AI_Timer = 10;
			} else if (AI_AttackProgress == 1 && AI_Timer < 0) {
				AI_Timer = 20;
				AI_AttackProgress++;
			} else if (AI_AttackProgress == 2 && AI_Timer < 0) {
				//Get the starting position of the snowball (it's the same as the breath attack, just further fowards in the mouth)
				Vector2 start = (NPC.spriteDirection == 1) ? NPC.Right - new Vector2(40, 0) : NPC.Left + new Vector2(40, 0);

				Vector2 speedToTarget = Vector2.Normalize(Target.Center - start) * speed;

				//Spawn the snowball
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					start,
					speedToTarget,
					ModContent.ProjectileType<FrostbiteRock>(),
					70,
					6f,
					NPC.target,
					speed
				);

				AI_Timer = 20;
				AI_AttackProgress++;
			} else if (AI_AttackProgress == 3 && AI_Timer < 0)
				switchSubPhase = true;
		}

		private void AI_SummonCloud() {
			if (Main.npc.Any(n => n.active && n.type == ModContent.NPCType<FrostCloud>() && n.ai[0] == NPC.whoAmI)) {
				switchSubPhase = true;
				return;
			}

			if (AI_Timer < 0 && AI_AttackProgress == 0) {
				AI_Timer = Phase == Phase_1 ? normal_icicle_wait : enraged_icicle_wait;
			} else if (AI_Timer >= 0 && AI_AttackProgress == 0) {
				NPC.velocity.X *= 0.9185f;

				if (AI_Timer == 0)
					AI_AttackProgress++;
			} else if (AI_Timer < 0 && AI_AttackProgress == 1) {
				//Spawn the cloud
				Vector2 spawn = NPC.spriteDirection == -1 ? NPC.TopRight : NPC.position;
				spawn -= new Vector2(0, 3 * 16);

				float angle = MathHelper.ToRadians(45f);
				angle.MirrorAngle(mirrorY: NPC.spriteDirection == -1);

				Vector2 initialSpeed = angle.ToRotationVector2() * (FrostCloud.TargetSpeed + 8);
				initialSpeed = initialSpeed.RotatedByRandom(MathHelper.ToRadians(5f));

				MiscUtils.SpawnNPCSynced(NPC.GetSource_FromAI(), spawn, ModContent.NPCType<FrostCloud>(), NPC.whoAmI, initialSpeed.X, initialSpeed.Y);

				AI_AttackProgress++;
				AI_Timer = 20;
			} else if (AI_AttackProgress == 2 && AI_Timer < 0)
				switchSubPhase = true;
		}
	}
}
