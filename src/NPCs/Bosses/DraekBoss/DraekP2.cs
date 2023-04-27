using CosmivengeonMod.Buffs.Harmful;
using CosmivengeonMod.Enums;
using CosmivengeonMod.Items.Bags;
using CosmivengeonMod.Items.Equippable.Vanity.BossMasks;
using CosmivengeonMod.Items.Lore;
using CosmivengeonMod.Items.Materials;
using CosmivengeonMod.Items.Weapons.Draek;
using CosmivengeonMod.NPCs.Bosses.DraekBoss.Summons;
using CosmivengeonMod.NPCs.Global;
using CosmivengeonMod.Projectiles.NPCSpawned.DraekBoss;
using CosmivengeonMod.Utility;
using CosmivengeonMod.Systems;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent.ItemDropRules;
using CosmivengeonMod.Items.Placeable.Trophies;

namespace CosmivengeonMod.NPCs.Bosses.DraekBoss {
	//Head NPC will contain any custom behaviour, as the other segments just follow it
	[AutoloadBossHead]
	public class DraekP2Head : Worm {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/DraekP2_Head";

		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Draek");

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public int ActualMaxHealth;

		public static readonly int BaseHealth = 2000;

		// Expert:   +37.5% base max per player
		// Desomode: +10% base max per player
		public static float GetHealthAugmentation() => MiscUtils.GetModeChoice(0, 0.6875f, 0.55f);

		public override void SetDefaults() {
			head = true;

			NPC.width = 40;
			NPC.height = 40;

			NPC.aiStyle = -1;
			NPC.lifeMax = BaseHealth;
			NPC.defense = 16;
			NPC.damage = 40;
			NPC.boss = true;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			NPC.HitSound = SoundID.Tink;    //Stone tile hit sound
			NPC.DeathSound = SoundID.NPCDeath60;    //Phantasm Dragon death sound

			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Burning] = true;
			NPC.buffImmune[BuffID.Frostburn] = true;

			minLength = maxLength = 6;

			headType = ModContent.NPCType<DraekP2Head>();
			//no bodyType since we have differing body segment textures
			tailType = ModContent.NPCType<DraekP2Tail>();

			speed = speed_subphase0_normal;
			turnSpeed = turnSpeed_subphase0_normal;

			maxDigDistance = 16 * 40;
			customBodySegments = true;

			NPC.value = Draek.Value;

			MiscUtils.PlayMusic(this, CosmivengeonBoss.Draek);
		}

		//Worm speeds
		private const float speed_subphase0_normal = 5f;
		private const float speed_subphase0_enraged_normal = 8f;
		private const float speed_subphase0_expert = 7f;
		private const float speed_subphase0_enraged_expert = 9f;

		private const float turnSpeed_subphase0_normal = 0.147f;
		private const float turnSpeed_subphase0_enraged_normal = 0.221f;
		private const float turnSpeed_subphase0_expert = 0.185f;
		private const float turnSpeed_subphase0_enraged_expert = 0.263f;

		private const int summonCount_normal = 3;
		private const int summonCount_enraged_normal = 5;
		private const int summonCount_expert = 4;
		private const int summonCount_enraged_expert = 6;

		private const int rockCount_normal = 6;
		private const int rockCount_enraged_normal = 10;
		private const int rockCount_expert = 8;
		private const int rockCount_enraged_expert = 14;

		private const int Worm_subphase0 = 0;
		private const int Summon = 1;
		private const int Worm_subphase2 = 2;
		private const int Mega_Charge = 3;

		private const int Enraged_Worm_subphase0 = 4;
		private const int Enraged_Summon = 5;
		private const int Enraged_Worm_subphase2 = 6;
		private const int Enraged_Mega_Charge = 7;

		private const int DesoMode_subphase0 = 0;
		private const int DesoMode_Mega_Charge = 1;
		private const int DesoMode_Try_Land_Explosion = 2;
		private const int DesoMode_Berserker = 3;
		private const int DesoMode_Berserker_Lasers = 4;
		private const int DesoMode_Berserker_Constant = 5;

		private const int Phase_2 = 0;
		private const int Phase_2_Enraged = 1;
		private const int Phase_2_DesoMode = 2;
		private int CurrentPhase = Phase_2;

		private int desomode_subphase = 0;
		private int[] desoModeSubphases = new int[] { };
		private float curLifeRatio = 1f;
		private bool switchDesoModeSubphaseSet = false;
		private int desoModeSubphaseSet = 0;

		public const int Laser_Delay = 90;
		private const int DesoMode_RockDelay = 20;

		private int attackPhase = Worm_subphase0;
		private int attackProgress = 0;
		private int attackTimer = 0;
		private int spitTimer = -1;
		private int laserTimer = -1;
		private const int MaxExplosionTimer = 150;
		private int explosionTimer = MaxExplosionTimer;

		private int SummonedWyrms = 0;

		private bool hasSpawned = false;
		private bool switchPhases = false;
		private bool switchSubPhases = false;
		private bool desoMode_enrageTextPrinted = false;

		private float prevSpeed;
		private float prevTurnSpeed;

		public bool SpawnChargeDusts = false;

		public override void SendExtraAI(BinaryWriter writer) {
			BitsByte flag = new BitsByte(switchDesoModeSubphaseSet, hasSpawned, switchPhases, switchSubPhases, desoMode_enrageTextPrinted);

			writer.Write(flag);
			writer.Write((byte)CurrentPhase);
			writer.Write((byte)desomode_subphase);
			writer.Write(curLifeRatio);
			writer.Write((byte)desoModeSubphaseSet);
			writer.Write((byte)attackPhase);
			writer.Write((byte)attackProgress);
			writer.Write(attackTimer);
			writer.Write(spitTimer);
			writer.Write(laserTimer);
			writer.Write(explosionTimer);
			writer.Write((byte)SummonedWyrms);
			writer.Write(prevSpeed);
			writer.Write(prevTurnSpeed);
			writer.Write(CustomTarget.X);
			writer.Write(CustomTarget.Y);
		}

		public override void ReceiveExtraAI(BinaryReader reader) {
			BitsByte flag = reader.ReadByte();
			flag.Retrieve(ref switchDesoModeSubphaseSet, ref hasSpawned, ref switchPhases, ref switchSubPhases, ref desoMode_enrageTextPrinted);
			CurrentPhase = reader.ReadByte();
			desomode_subphase = reader.ReadByte();
			curLifeRatio = reader.ReadSingle();
			desoModeSubphaseSet = reader.ReadByte();
			attackPhase = reader.ReadByte();
			attackProgress = reader.ReadByte();
			attackTimer = reader.ReadInt32();
			spitTimer = reader.ReadInt32();
			laserTimer = reader.ReadInt32();
			explosionTimer = reader.ReadInt32();
			SummonedWyrms = reader.ReadByte();
			prevSpeed = reader.ReadSingle();
			prevTurnSpeed = reader.ReadSingle();
			CustomTarget.X = reader.ReadSingle();
			CustomTarget.Y = reader.ReadSingle();
		}

		public override bool CheckActive() {
			return Vector2.Distance(NPC.Center, CustomTarget) > 200 * 16;
		}

		public override void OnKill() {
			Debug.CheckWorldFlagUpdate(ref WorldEvents.downedDraekBoss);
		}

		public override void ModifyNPCLoot(NPCLoot npcLoot) {
			npcLoot.Add(ItemDropRule.BossBag(ModContent.ItemType<DraekBag>()));

			AddDrops(npcLoot, restrictNormalDrops: true);

			npcLoot.Add(ItemDropRule.Common(ModContent.ItemType<DraekTrophy>(), 10));

			npcLoot.AddInstancedDrop(static () => !WorldEvents.downedDraekBoss, ModContent.ItemType<StoneTablet>(), "Mods.CosmivengeonMod.LootText.FirstKill", literal: false);
		}

		public static void AddDrops(ILoot loot, bool restrictNormalDrops) {
			var weaponDrop = new OneFromRulesRule(1,
				ItemDropRule.OneFromOptions(1,
					ModContent.ItemType<BasiliskStaff>(),
					ModContent.ItemType<EarthBolt>(),
					ModContent.ItemType<ForsakenOronoblade>(),
					ModContent.ItemType<Rockslide>(),
					ModContent.ItemType<Scalestorm>(),
					ModContent.ItemType<SlitherWand>(),
					ModContent.ItemType<Stoneskipper>()),
				ItemDropRule.Common(ModContent.ItemType<BoulderChunk>(), 1, 20, 40));

			var materialDrop = ItemDropRule.Common(ModContent.ItemType<DraekScales>(), 1, 20, 30);

			if (restrictNormalDrops) {
				var normalOnly = new LeadingConditionRule(new Conditions.NotExpert());
				normalOnly.OnSuccess(weaponDrop);
				normalOnly.OnSuccess(materialDrop);
				loot.Add(normalOnly);
			} else {
				loot.Add(weaponDrop);
				loot.Add(materialDrop);
			}

			loot.Add(ItemDropRule.Common(ModContent.ItemType<DraekMask>(), 10, 1, 1));
		}

		public static void ExpertStatsHelper(NPC npc) {
			//Health increase handled in DetourNPC

			if (!WorldEvents.desoMode) {
				npc.damage = 55;
				npc.defense = 20;
			} else {
				npc.damage = 70;
				npc.defense = 24;
			}
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			ExpertStatsHelper(NPC);

			speed = speed_subphase0_expert;
			turnSpeed = turnSpeed_subphase0_expert;
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (WorldEvents.desoMode)
				damage = (int)(Math.Min(projectile.maxPenetrate < 1 ? 0.4f : 2.3f / projectile.maxPenetrate, 1f) * damage);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			if (WorldEvents.desoMode)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 150);
		}

		public override void AI() {
			//AI:  https://docs.google.com/document/d/13IlpNUdO2X_elLPwsYcB1mzd_FMxQkwAcRKq1oSWgLg

			if (!hasSpawned) {
				NPC.TargetClosest(true);
				hasSpawned = true;

				MiscUtils.SendMessage("<Draek> MY JEWEL!", Draek.TextColour);
				MiscUtils.SendMessage("<Draek> AARG, YOU'LL PAY FOR THAT, YOU WRETCHED LITTLE WORM!", Draek.TextColour);

				if (WorldEvents.desoMode) {
					CurrentPhase = Phase_2_DesoMode;
					attackPhase = DesoMode_subphase0;
				}
			}

			if ((NPC.life < ActualMaxHealth * 0.3 && CurrentPhase == Phase_2) || (NPC.life < ActualMaxHealth * 0.4f && WorldEvents.desoMode && !desoMode_enrageTextPrinted)) {
				switchPhases = true;
				desoMode_enrageTextPrinted = true;

				MiscUtils.SendMessage("<Draek> YOU ACCURSED INSECT!  I'LL BURY YOU ALIVE!", Draek.TextColour);
			}

			//If we're in desolation mode, we want to force the changed Berserker subphase when possible
			TryForceDesoModeSubphaseChange();

			CheckSubphaseChange();
			CheckPhaseChange();

			if (Main.expertMode) {
				SummonedWyrms = UpdateWyrmCount();

				StatsNPC gnpc = NPC.GetGlobalNPC<StatsNPC>();

				if (SummonedWyrms > 0) {
					gnpc.endurance += WorldEvents.desoMode ? 0.2f : 0.4f;

					for (int i = 0; i < Segments.Count; i++)
						Segments[i].NPC.GetGlobalNPC<StatsNPC>().endurance = gnpc.endurance;
				}
			}

			if (CurrentPhase == Phase_2) {
				float wormSpeed = Main.expertMode ? speed_subphase0_expert : speed_subphase0_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_expert : turnSpeed_subphase0_normal;

				if (attackPhase == Worm_subphase0) {
					AI_Worm(wormSpeed, wormTurnSpeed, 6 * 60);
				} else if (attackPhase == Summon) {
					int count = Main.expertMode ? summonCount_expert : summonCount_normal;

					AI_Summon(count, 15);
				} else if (attackPhase == Worm_subphase2) {
					AI_Worm(wormSpeed, wormTurnSpeed, 4 * 60);
				} else if (attackPhase == Mega_Charge) {
					int delay = Main.expertMode ? 45 : 60;
					float duration = Main.rand.NextFloat(2, 4) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_expert : rockCount_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount, 9 * 16);
				}
			} else if (CurrentPhase == Phase_2_Enraged) {
				float wormSpeed = Main.expertMode ? speed_subphase0_enraged_expert : speed_subphase0_enraged_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_enraged_expert : turnSpeed_subphase0_enraged_normal;

				if (attackPhase == Enraged_Worm_subphase0) {
					AI_Worm(wormSpeed, wormTurnSpeed, 5 * 60);
				} else if (attackPhase == Enraged_Summon) {
					int count = Main.expertMode ? summonCount_enraged_expert : summonCount_enraged_normal;

					AI_Summon(count, 10);
				} else if (attackPhase == Enraged_Worm_subphase2) {
					AI_Worm(wormSpeed, wormTurnSpeed, 3 * 60);
				} else if (attackPhase == Enraged_Mega_Charge) {
					int delay = Main.expertMode ? 30 : 45;
					float duration = Main.rand.NextFloat(3, 5) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_enraged_expert : rockCount_enraged_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount, 9 * 16);
				}
			} else if (CurrentPhase == Phase_2_DesoMode) {
				if (attackPhase == DesoMode_subphase0) {
					AI_Worm(speed_subphase0_expert + 3f, turnSpeed_subphase0_expert + 0.08f, 5 * 60);
				} else if (attackPhase == DesoMode_Mega_Charge) {
					float duration = Main.rand.NextFloat(3, 5) * 1.5f;
					AI_MegaCharge(20, (int)(duration * 60), 12, 50 * 16);
				} else if (attackPhase == DesoMode_Try_Land_Explosion) {
					AI_TryLandExplosion();
					explosionTimer--;
				} else if (attackPhase == DesoMode_Berserker) {
					AI_Beserker();
				} else if (attackPhase == DesoMode_Berserker_Lasers) {
					AI_Beserker(true);
				} else if (attackPhase == DesoMode_Berserker_Constant) {
					AI_Beserker(true);
				}
				AI_Spit();

				spitTimer--;
				laserTimer--;
			}

			attackTimer++;

			bool normalModeChargeAttack = !Main.expertMode && !WorldEvents.desoMode && attackPhase == Mega_Charge;
			bool expertModeChargeAttack = Main.expertMode && !WorldEvents.desoMode && attackPhase == Enraged_Mega_Charge;
			bool desoModeAttackMatches = attackPhase != DesoMode_subphase0 && attackPhase != DesoMode_Try_Land_Explosion;
			bool desoModeChargeAttack = Main.expertMode && WorldEvents.desoMode && desoModeAttackMatches;

			SpawnChargeDusts = (normalModeChargeAttack || expertModeChargeAttack || desoModeChargeAttack) && speed > 1;

			if (SpawnChargeDusts)
				SpawnDust(NPC);
		}

		public static void SpawnDust(NPC npc) {
			Dust dust = Dust.NewDustDirect(npc.position, npc.width, npc.height, DustID.GreenFairy);
			dust.velocity = Vector2.Zero;
			dust.noGravity = true;
		}

		private void TryForceDesoModeSubphaseChange() {
			//Don't run this method if we're not in Desolation Mode or the "desoModeSubphases" array hasn't been set yet
			if (!WorldEvents.desoMode || desoModeSubphases.Length == 0)
				return;

			curLifeRatio = NPC.life / (float)ActualMaxHealth;

			if (desoModeSubphaseSet == 0 && curLifeRatio <= 0.6f && curLifeRatio > 0.4f) {
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 0;
				desoModeSubphaseSet = 1;
			} else if (desoModeSubphaseSet == 1 && curLifeRatio <= 0.4f && curLifeRatio > 0.2f) {
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 0;
				desoModeSubphaseSet = 2;
			} else if (desoModeSubphaseSet == 2 && curLifeRatio <= 0.2f) {
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 3;
			}
		}

		private void CheckSubphaseChange() {
			if (!switchSubPhases)
				return;

			if (attackPhase == Mega_Charge && CurrentPhase == Phase_2)
				attackPhase = Worm_subphase0;
			else if (attackPhase == Enraged_Mega_Charge && CurrentPhase == Phase_2_Enraged)
				attackPhase = Enraged_Worm_subphase0;
			else if (CurrentPhase == Phase_2_DesoMode || switchDesoModeSubphaseSet) {
				if (desoModeSubphaseSet == 0)
					desoModeSubphases = new int[] { DesoMode_subphase0, DesoMode_Mega_Charge, DesoMode_Try_Land_Explosion };
				else if (desoModeSubphaseSet == 1)
					desoModeSubphases = new int[] { DesoMode_Mega_Charge, DesoMode_Berserker, DesoMode_Try_Land_Explosion };
				else if (desoModeSubphaseSet == 2)
					desoModeSubphases = new int[] { DesoMode_Mega_Charge, DesoMode_Berserker_Lasers, DesoMode_Try_Land_Explosion };
				else
					desoModeSubphases = new int[] { DesoMode_Berserker_Constant };

				attackPhase = desoModeSubphases[++desomode_subphase % desoModeSubphases.Length];

				if (attackPhase == DesoMode_Mega_Charge)
					attackProgress = 2;

				explosionTimer = MaxExplosionTimer;
			} else
				attackPhase++;

			attackTimer = 0;
			attackProgress = 0;
			switchSubPhases = false;
			switchDesoModeSubphaseSet = false;

			NPC.netUpdate = true;
		}

		private void CheckPhaseChange() {
			if (!switchPhases || WorldEvents.desoMode)
				return;

			CurrentPhase = Phase_2_Enraged;
			attackPhase = Enraged_Worm_subphase0;

			attackTimer = 0;
			attackProgress = 0;
			switchPhases = false;
			switchSubPhases = false;

			NPC.netUpdate = true;
		}

		private int UpdateWyrmCount() {
			int wyrms = 0;

			for (int i = 0; i < Main.npc.Length; i++) {
				if (Main.npc[i].active && Main.npc[i].type == ModContent.NPCType<DraekWyrmSummon_Head>() && (int)Main.npc[i].ai[2] == NPC.whoAmI)
					wyrms++;
			}

			return wyrms;
		}

		private void AI_Spit() {
			if (spitTimer < 0) {
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					NPC.Center,
					Vector2.Zero,
					ModContent.ProjectileType<DraekAcidSpit>(),
					20,
					3f,
					Main.player[NPC.target].Center.X,
					Main.player[NPC.target].Center.Y
				);

				spitTimer = Main.rand.Next(120, 180);

				NPC.netUpdate = true;
			}
		}

		private void AI_Worm(float speed, float turnSpeed, int wait) {
			fly = false;
			this.speed = speed;
			this.turnSpeed = turnSpeed;
			prevSpeed = speed;
			prevTurnSpeed = turnSpeed;
			if (attackTimer >= wait)
				switchSubPhases = true;
		}

		private void AI_Summon(int times, int waitBetween) {
			if (attackProgress >= times || SummonedWyrms == times) {
				switchSubPhases = true;
				return;
			}

			if (attackTimer < waitBetween || Main.netMode == NetmodeID.MultiplayerClient)
				return;

			Vector2 range = NPC.Center + new Vector2(Main.rand.NextFloat(-5 * 16, 5 * 16), Main.rand.NextFloat(-5 * 16, 5 * 16));

			MiscUtils.SpawnNPCSynced(NPC.GetSource_FromAI(), range, ModContent.NPCType<DraekWyrmSummon_Head>(), ai2: NPC.whoAmI);

			SummonedWyrms++;

			attackProgress++;
			attackTimer = 0;

			NPC.netUpdate = true;
		}

		private void AI_MegaCharge(int delay, int flyTime, int rockCount, float targetTolerance) {
			/*			ATTACK PROGRESS
			 *		0: Slow down and target where player currently is
			 *		1: Move like normal until timer runs out
			 *		2: Charge towards old position and enable flying
			 *		3: Attempt to "hover" directly above player, adjusting speed
			 *			to be always faster than the player's current velocity.
			 *			Also drop "Draek Rock" projectiles
			 */
			if (attackProgress == 0) {
				speed = 1f;
				turnSpeed = 0.5f;
				CustomTarget = Main.player[NPC.target].Center;
				attackProgress++;
			} else if (attackProgress == 1) {
				if (attackTimer < delay)
					return;
				else {
					attackProgress++;
					attackTimer = 0;
				}
			} else if (attackProgress == 2 || attackProgress == 3) {
				speed = MiscUtils.GetModeChoice(8, 11, 14);
				turnSpeed = MiscUtils.GetModeChoice(0.62f, 0.7f, 0.85f);
				fly = true;
				JewelExplosion();
				if (Vector2.Distance(CustomTarget, NPC.Center) < 1 * 16)
					attackProgress++;
			} else if (attackProgress >= 4) {
				CustomTarget = Main.player[NPC.target].Center - new Vector2(0, 16 * 10);
				speed = Main.player[NPC.target].velocity.Length() + MiscUtils.GetModeChoice(6, 6, 9);

				float rockDelay = flyTime / (rockCount + 1);

				if (Vector2.Distance(NPC.Center, CustomTarget) > targetTolerance) {
					attackTimer--;
					return;
				}

				if (attackTimer > rockDelay) {
					float rockSpawnOffset = Main.rand.NextFloat(-3, 3);

					MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
						NPC.Center + new Vector2(rockSpawnOffset, 0),
						Vector2.Zero,
						ModContent.ProjectileType<DraekRock>(),
						30 + (Main.expertMode ? 30 : 0),
						16f,
						20f,
						0.35f
					);

					SoundEngine.PlaySound(SoundID.Item69, NPC.Center);

					attackTimer = 0;
					attackProgress++;

					if (attackProgress == 4 + rockCount) {
						switchSubPhases = true;
						CustomTarget = Vector2.Zero;
						speed = prevSpeed;
						turnSpeed = prevTurnSpeed;
						fly = false;
					}
				}
			}
		}

		private void AI_TryLandExplosion() {
			List<Point> tileCoords = GetCollidingTileCoords();
			bool triggerExplosion = false;

			//Loop over all tiles we're colliding with
			//If any are solid, trigger the explosion and go to the next subphase
			foreach (Point p in tileCoords) {
				if (MiscUtils.TileIsSolidOrPlatform(p.X, p.Y) && Main.tile[p.X, p.Y].TileType != TileID.Platforms) {
					triggerExplosion = true;
					switchSubPhases = true;
					break;
				}
			}

			//If we should trigger the explosion due to colliding with tiles, do so
			//OR if the timer has run out
			if (triggerExplosion || explosionTimer < 0)
				RockExplosion();
		}

		private void AI_Beserker(bool lasers = false) {
			if (attackProgress < 4)
				CustomTarget = Main.player[NPC.target].Center;
			else
				CustomTarget = Main.player[NPC.target].Center - new Vector2(0, 8 * 16);

			if (attackProgress == 0) {
				AI_Worm(speed, turnSpeed, 1);
				attackProgress = 2;
			}

			if (NPC.ai[1] == 0f && lasers && laserTimer < 0) {
				NPC.ai[1] = 2;  //Signal to the jewel segment to fire lasers
				laserTimer = Laser_Delay;
			}

			if (attackProgress == 2 || attackProgress == 3) {
				speed = 16f;
				turnSpeed = 0.525f;
				fly = true;
				JewelExplosion();
				if (Vector2.Distance(CustomTarget, NPC.Center) < 1 * 16)
					attackProgress++;
			}

			if (attackTimer > DesoMode_RockDelay) {
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					NPC.Center,
					new Vector2(0, -15),
					ModContent.ProjectileType<DraekRock>(),
					75,
					16f,
					25f,
					0.35f
				);

				SoundEngine.PlaySound(SoundID.Item69, NPC.Center);

				attackTimer = 0;
				attackProgress++;

				if (attackProgress == rockCount_enraged_expert) {
					switchSubPhases = true;
					CustomTarget = Vector2.Zero;
					speed = prevSpeed;
					turnSpeed = prevTurnSpeed;
					fly = false;
				}
			}
		}

		private void JewelExplosion() {
			if (NPC.ai[1] == 0f && attackProgress == 2) {   //Notify the second body segment to cause the explosion
				NPC.ai[1] = 1f;
				attackProgress++;

				if (CurrentPhase == Phase_2_DesoMode && attackPhase >= DesoMode_Berserker)
					SoundEngine.PlaySound(SoundID.ForceRoar, NPC.Center);
			}
		}

		private void RockExplosion() {
			//Generate 10-16 small rocks going in several different directions and velocity changes
			int amount = Main.rand.Next(10, 17);

			for (int i = 0; i < amount; i++) {
				MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
					NPC.Center,
					new Vector2(0, -7),
					ModContent.ProjectileType<DraekRockExplosion>(),
					20,
					4f,
					Main.rand.NextFloat(0.5f / 60f, 5f / 60f) * (Main.rand.NextBool() ? 1 : -1),
					Main.rand.NextFloat(6f / 60f, 12f / 60f)
				);
			}

			var sound = SoundID.Item14 with {
				PitchVariance = 0.6f,
				Volume = 0.8f
			};

			SoundEngine.PlaySound(sound, NPC.Center);

			switchSubPhases = true;
		}

		private List<Point> GetCollidingTileCoords() {
			List<Point> ret = new List<Point>();

			int curTileX = (int)(NPC.position.X / 16f);
			int curTileY = (int)(NPC.position.Y / 16f);

			int tileX = curTileX - 1;
			int tileY = curTileY - 1;
			int endTileX = curTileX + (int)(NPC.width / 16f) + 1;
			int endTileY = curTileY + (int)(NPC.height / 16f) + 1;

			for (; tileX < endTileX; tileX++)
				for (; tileY < endTileY; tileY++)
					ret.Add(new Point(tileX, tileY));

			return ret;
		}

		public override int SetCustomBodySegments(int startDistance) {
			int latestNPC = NPC.whoAmI;
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body0>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body1>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body2>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body3>(), latestNPC);
			return latestNPC;
		}
	}

	//Draek only has 4 body segments, so I can just hardcode the classes :chaelure:
	internal class DraekP2_Body0 : Worm {
		public override void SetStaticDefaults() {
			DisplayName.SetDefault("Draek");

			NPCID.Sets.NPCBestiaryDrawModifiers value = new NPCID.Sets.NPCBestiaryDrawModifiers(0) {
				Hide = true // Hides this NPC from the Bestiary, useful for multi-part NPCs whom you only want one entry.
			};
			NPCID.Sets.NPCBestiaryDrawOffset.Add(NPC.type, value);
		}

		public override void SetDefaults() {
			NPC.CloneDefaults(headType);

			NPC.boss = true;

			NPC.width = 30;
			NPC.height = 30;

			NPC.aiStyle = -1;
			NPC.lifeMax = 2500;
			NPC.defense = 25;
			NPC.damage = 40;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Burning] = true;
			NPC.buffImmune[BuffID.Frostburn] = true;

			NPC.dontCountMe = true;

			NPC.HitSound = SoundID.Tink;    //Stone tile hit sound
			NPC.DeathSound = SoundID.NPCDeath60;    //Phantasm Dragon death sound
		}

		public override bool PreKill() {
			return false;   //Don't drop anything
		}

		public override void AI() {
			NPC parent = Main.npc[(int)NPC.ai[3]];
			if ((parent.ModNPC as DraekP2Head).SpawnChargeDusts)
				DraekP2Head.SpawnDust(NPC);
		}

		public override void OnHitPlayer(Player target, int damage, bool crit) {
			if (WorldEvents.desoMode)
				target.AddBuff(ModContent.BuffType<PrimordialWrath>(), 150);
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale) {
			DraekP2Head.ExpertStatsHelper(NPC);
		}

		public override void ModifyHitByProjectile(Projectile projectile, ref int damage, ref float knockback, ref bool crit, ref int hitDirection) {
			if (WorldEvents.desoMode)
				damage = (int)((projectile.maxPenetrate < 1 ? 0.2 : 1f / projectile.maxPenetrate) * damage);
		}

		public override bool CheckActive() {
			return Vector2.Distance(NPC.Center, (Main.npc[(int)NPC.ai[3]].ModNPC as Worm)?.CustomTarget ?? NPC.Center) > 200 * 16;
		}
	}
	internal class DraekP2_Body1 : DraekP2_Body0 {
		public override void AI() {
			NPC parent = Main.npc[(int)NPC.ai[3]];

			if (parent.ai[1] == 1f) {   //Head segment has flagged for this one to cause the "Jewel Explosion"
										//Play the sounds
				SoundEngine.PlaySound(SoundID.Item27, NPC.Center);      //Crystal break sound effect
				SoundEngine.PlaySound(SoundID.Item70, NPC.Center);      //Staff of Earth alternative sound effect

				//Spawn the dust
				for (int i = 0; i < 60; i++) {
					Dust.NewDust(NPC.Center, 50, 50, DustID.GreenFairy, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
					Dust.NewDust(NPC.Center, 50, 50, DustID.TerraBlade, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
				}

				parent.ai[1] = 0f;
			} else if (parent.ai[1] == 2f) {    //Head segment has flagged for laser attack to happen
				Vector2 spawnOrigin = Main.player[NPC.target].Center;
				Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;
				for (int i = 0; i < 3; i++) {
					MiscUtils.SpawnProjectileSynced(NPC.GetSource_FromAI(),
						NPC.Center,
						Vector2.Zero,
						ModContent.ProjectileType<DraekLaser>(),
						50,
						6f,
						spawnOrigin.X + positionOffset.X,
						spawnOrigin.Y + positionOffset.Y
					);

					positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;
				}

				//Play "boss laser" sound effect
				SoundEngine.PlaySound(SoundID.Item33, NPC.position);

				parent.ai[1] = 0f;
			}

			base.AI();
		}
	}
	internal class DraekP2_Body2 : DraekP2_Body0 { }
	internal class DraekP2_Body3 : DraekP2_Body0 { }

	internal class DraekP2Tail : DraekP2_Body0 {
		public override string Texture => "CosmivengeonMod/NPCs/Bosses/DraekBoss/DraekP2_Tail";

		public override void SetDefaults() {
			NPC.CloneDefaults(headType);

			NPC.boss = true;

			NPC.aiStyle = -1;
			NPC.lifeMax = 500;
			NPC.defense = 25;
			NPC.damage = 40;
			NPC.lavaImmune = true;
			NPC.noGravity = true;
			NPC.noTileCollide = true;
			NPC.knockBackResist = 0f;

			NPC.buffImmune[BuffID.Poisoned] = true;
			NPC.buffImmune[BuffID.Confused] = true;
			NPC.buffImmune[BuffID.Burning] = true;
			NPC.buffImmune[BuffID.Frostburn] = true;

			tail = true;
			NPC.width = 30;
			NPC.height = 30;

			NPC.dontCountMe = true;

			NPC.HitSound = SoundID.Tink;    //Stone tile hit sound
			NPC.DeathSound = SoundID.NPCDeath60;    //Phantasm Dragon death sound
		}

		public override void AI() {
			NPC parent = Main.npc[(int)NPC.ai[3]];
			if ((parent.ModNPC as DraekP2Head).SpawnChargeDusts)
				DraekP2Head.SpawnDust(NPC);
		}
	}
}
