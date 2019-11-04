using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using Summon = CosmivengeonMod.NPCs.Draek.DraekWyrmSummon_Head;

namespace CosmivengeonMod.NPCs.Draek{
	//Head NPC will contain any custom behaviour, as the other segments just follow it
	[AutoloadBossHead]
	public class DraekP2Head : Worm{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekP2_Head";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}
		
		public override void SetDefaults(){
			head = true;
			
			npc.width = 20;
			npc.height = 20;

			npc.aiStyle = -1;
			npc.lifeMax = 2500;
			npc.defense = 16;
			npc.damage = 40;
			npc.scale = 2f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound

			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/SuccessorOfTheJewel");
			musicPriority = MusicPriority.BossLow;
			
			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			minLength = maxLength = 6;

			headType = ModContent.NPCType<DraekP2Head>();
			//no bodyType since we have differing body segment textures
			tailType = ModContent.NPCType<DraekP2Tail>();
			
			speed = speed_subphase0_normal;
			turnSpeed = turnSpeed_subphase0_normal;
			
			maxDigDistance = 16 * 40;
			customBodySegments = true;

			bossBag = ModContent.ItemType<Items.Boss_Bags.DraekBag>();
		}

		private const float speed_subphase0_normal = 6f;
		private const float speed_subphase0_enraged_normal = 9f;
		private const float speed_subphase0_expert = 8f;
		private const float speed_subphase0_enraged_expert = 11f;
		
		private const float turnSpeed_subphase0_normal = 0.125f;
		private const float turnSpeed_subphase0_enraged_normal = 0.185f;
		private const float turnSpeed_subphase0_expert = 0.15f;
		private const float turnSpeed_subphase0_enraged_expert = 0.205f;

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
		private int[] desoModeSubphases = new int[]{ };
		private float curLifeRatio = 1f;
		private bool switchDesoModeSubphaseSet = false;

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

		public override void NPCLoot(){
			CosmivengeonWorld.downedDraekBoss = true;

			if(Main.expertMode)
				npc.DropBossBags();
			else
				NormalModeDrops(npc: npc);

			CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.downedDraekBoss));
		}

		public static void NormalModeDrops(Player player = null, NPC npc = null, bool quickSpawn = false){
			int[] drops = new int[]{
				ModContent.ItemType<Items.Draek.BasiliskStaff>(),
				ModContent.ItemType<Items.Draek.BoulderChunk>(),
				ModContent.ItemType<Items.Draek.EarthBolt>(),
				ModContent.ItemType<Items.Draek.ForsakenOronoblade>(),
				ModContent.ItemType<Items.Draek.RockslideYoyo>(),
				ModContent.ItemType<Items.Draek.Scalestorm>(),
				ModContent.ItemType<Items.Draek.SlitherWand>(),
				ModContent.ItemType<Items.Draek.Stoneskipper>()
			};

			bool dropChosen;
			int dropType, dropAmount = 1;

			do{
				dropType = Main.rand.Next(drops);
				dropChosen = Main.rand.NextFloat() < 0.2;
			}while(!dropChosen);

			if(dropType == ModContent.ItemType<Items.Draek.BoulderChunk>())
				dropAmount = Main.rand.Next(20, 41);

			if(player != null && quickSpawn){
				player.QuickSpawnItem(dropType, dropAmount);
				player.QuickSpawnItem(ModContent.ItemType<Items.Draek.DraekScales>(), Main.rand.Next(20, 31));

				if(Main.rand.NextFloat() < 0.1)
					player.QuickSpawnItem(ModContent.ItemType<Items.Masks.DraekMask>());

				if(!CosmivengeonWorld.obtainedLore_DraekBoss){
					player.QuickSpawnItem(ModContent.ItemType<Items.Draek.StoneTablet>());
					CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.obtainedLore_DraekBoss));
				}
			}else if(npc != null){
				Item.NewItem(npc.getRect(), dropType, dropAmount);
				Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Draek.DraekScales>(), Main.rand.Next(20, 31));
				
				if(Main.rand.NextFloat() < 0.1)
					Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Masks.DraekMask>());

				if(!CosmivengeonWorld.obtainedLore_DraekBoss){
					Item.NewItem(npc.getRect(), ModContent.ItemType<Items.Draek.StoneTablet>());
					CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.obtainedLore_DraekBoss));
				}
			}
		}

		[SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Might be used by other methods or NPCs.")]
		public static void ExpertStatsHelper(NPC npc, int numPlayers, float bossLifeScale){
			npc.lifeMax /= 2;	//Negate vanilla health bonus

			if(!CosmivengeonWorld.desoMode){
				npc.lifeMax += (int)(npc.lifeMax * 2f / 5f) * (numPlayers + 1);
				npc.damage = 55;
				npc.defense = 20;
			}else{
				npc.lifeMax += (int)(npc.lifeMax * 3f / 4f) * (numPlayers + 1);
				npc.damage = 70;
				npc.defense = 24;
			}
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			ExpertStatsHelper(npc, numPlayers, bossLifeScale);

			speed = speed_subphase0_expert;
			turnSpeed = turnSpeed_subphase0_expert;
		}

		public override void AI(){
			//AI:  https://docs.google.com/document/d/13IlpNUdO2X_elLPwsYcB1mzd_FMxQkwAcRKq1oSWgLg
			
			if(!hasSpawned){
				npc.TargetClosest(true);
				hasSpawned = true;

				Main.NewText("MY JEWEL!", Draek.TextColour);
				Main.NewText("AARG, YOU'LL PAY FOR THAT, YOU WRETCHED LITTLE WORM!", Draek.TextColour);

				if(CosmivengeonWorld.desoMode){
					CurrentPhase = Phase_2_DesoMode;
					attackPhase = DesoMode_subphase0;
				}
			}

			if((npc.life < npc.lifeMax * 0.3 && CurrentPhase == Phase_2) || (npc.life < npc.lifeMax * 0.4f && CosmivengeonWorld.desoMode && !desoMode_enrageTextPrinted)){
				switchPhases = true;
				desoMode_enrageTextPrinted = true;

				Main.NewText("YOU ACCURSED INSECT!  I'LL BURY YOU ALIVE!", Draek.TextColour);
			}

			//If we're in desolation mode, we want to force the changed Berserker subphase when possible
			TryForceDesoModeSubphaseChange();

			CheckSubphaseChange();
			CheckPhaseChange();

			if(CurrentPhase == Phase_2){
				float wormSpeed = Main.expertMode ? speed_subphase0_expert : speed_subphase0_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_expert : turnSpeed_subphase0_normal;

				if(attackPhase == Worm_subphase0){
					AI_Worm(wormSpeed, wormTurnSpeed, 6 * 60);
				}else if(attackPhase == Summon){
					int count = Main.expertMode ? summonCount_expert : summonCount_normal;

					AI_Summon(count, 15, true, 0f);
				}else if(attackPhase == Worm_subphase2){
					AI_Worm(wormSpeed, wormTurnSpeed, 4 * 60);
				}else if(attackPhase == Mega_Charge){
					int delay = Main.expertMode ? 45 : 60;
					float duration = Main.rand.NextFloat(2, 4) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_expert : rockCount_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount, 9 * 16);
				}
			}else if(CurrentPhase == Phase_2_Enraged){
				float wormSpeed = Main.expertMode ? speed_subphase0_enraged_expert : speed_subphase0_enraged_normal;
				float wormTurnSpeed = Main.expertMode ? turnSpeed_subphase0_enraged_expert : turnSpeed_subphase0_enraged_normal;

				if(attackPhase == Enraged_Worm_subphase0){
					AI_Worm(wormSpeed, wormTurnSpeed, 5 * 60);
				}else if(attackPhase == Enraged_Summon){
					int count = Main.expertMode ? summonCount_enraged_expert : summonCount_enraged_normal;

					AI_Summon(count, 10, true, 0f);
				}else if(attackPhase == Enraged_Worm_subphase2){
					AI_Worm(wormSpeed, wormTurnSpeed, 3 * 60);
				}else if(attackPhase == Enraged_Mega_Charge){
					int delay = Main.expertMode ? 30 : 45;
					float duration = Main.rand.NextFloat(3, 5) + (Main.expertMode ? 1 : 0);
					int rockCount = Main.expertMode ? rockCount_enraged_expert : rockCount_enraged_normal;

					AI_MegaCharge(delay, (int)(60 * duration), rockCount, 9 * 16);
				}
			}else if(CurrentPhase == Phase_2_DesoMode){
				if(attackPhase == DesoMode_subphase0){
					AI_Worm(speed_subphase0_expert, turnSpeed_subphase0_expert, 5 * 60);
				}else if(attackPhase == DesoMode_Mega_Charge){
					float duration = Main.rand.NextFloat(3, 5) * 1.5f;
					AI_MegaCharge(20, (int)(duration * 60), 12, 50 * 16);
				}else if(attackPhase == DesoMode_Try_Land_Explosion){
					AI_TryLandExplosion();
					explosionTimer--;
				}else if(attackPhase == DesoMode_Berserker){
					AI_Beserker();
				}else if(attackPhase == DesoMode_Berserker_Lasers){
					AI_Beserker(true);
				}else if(attackPhase == DesoMode_Berserker_Constant){
					AI_Beserker(true);
				}
				AI_Spit();

				spitTimer--;
				laserTimer--;
			}

			attackTimer++;
		}

		private void TryForceDesoModeSubphaseChange(){
			//Don't run this method if we're not in Desolation Mode and the "desoModeSubphases" array hasn't been set yet
			if(!CosmivengeonWorld.desoMode || desoModeSubphases.Length == 0)
				return;

			curLifeRatio = npc.life / (float)npc.lifeMax;
			if(desoModeSubphases[0] == DesoMode_subphase0 && curLifeRatio <= 0.6f && curLifeRatio > 0.4f){
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 0;
			}else if(desoModeSubphases[0] == DesoMode_Mega_Charge && curLifeRatio <= 0.4f && curLifeRatio > 0.2f){
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 0;
			}else if(desoModeSubphases[0] == DesoMode_Mega_Charge && curLifeRatio <= 0.2f){
				switchDesoModeSubphaseSet = true;
				desomode_subphase = 0;
			}
		}

		private void CheckSubphaseChange(){
			if(!switchSubPhases)
				return;
			
			if(attackPhase == Mega_Charge && CurrentPhase == Phase_2)
				attackPhase = Worm_subphase0;
			else if(attackPhase == Enraged_Mega_Charge && CurrentPhase == Phase_2_Enraged)
				attackPhase = Enraged_Worm_subphase0;
			else if(CurrentPhase == Phase_2_DesoMode || switchDesoModeSubphaseSet){
				if(curLifeRatio > 0.6f)
					desoModeSubphases = new int[]{ DesoMode_subphase0, DesoMode_Mega_Charge, DesoMode_Try_Land_Explosion };
				else if(curLifeRatio > 0.4f)
					desoModeSubphases = new int[]{ DesoMode_Mega_Charge, DesoMode_Berserker, DesoMode_Try_Land_Explosion };
				else if(curLifeRatio > 0.2f)
					desoModeSubphases = new int[]{ DesoMode_Mega_Charge, DesoMode_Berserker_Lasers, DesoMode_Try_Land_Explosion };
				else
					desoModeSubphases = new int[]{ DesoMode_Berserker_Constant };

				attackPhase = desoModeSubphases[++desomode_subphase % desoModeSubphases.Length];

				if(attackPhase == DesoMode_Mega_Charge)
					attackProgress = 2;

				explosionTimer = MaxExplosionTimer;
			}else
				attackPhase++;
			
			attackTimer = 0;
			attackProgress = 0;
			switchSubPhases = false;
			switchDesoModeSubphaseSet = false;
		}

		private void CheckPhaseChange(){
			if(!switchPhases || CosmivengeonWorld.desoMode)
				return;
			
			CurrentPhase = Phase_2_Enraged;
			attackPhase = Enraged_Worm_subphase0;

			attackTimer = 0;
			attackProgress = 0;
			switchPhases = false;
			switchSubPhases = false;
		}

		private int UpdateWyrmCount(){
			int wyrms = 0;
			
			for(int i = 0; i < Main.npc.Length; i++){
				if(Main.npc[i].type == ModContent.NPCType<Summon>() && (int)Main.npc[i].ai[1] == npc.whoAmI && Main.npc[i].active)
					wyrms++;
			}
			
			return wyrms;
		}

		private void AI_Spit(){
			if(spitTimer < 0){
				npc.SpawnProjectile(npc.Center,
					Vector2.Zero,
					ModContent.ProjectileType<Projectiles.Draek.DraekAcidSpit>(),
					20,
					3f,
					Main.myPlayer,
					Main.player[npc.target].Center.X,
					Main.player[npc.target].Center.Y
				);

				spitTimer = Main.rand.Next(120, 180);
			}
		}

		private void AI_Worm(float speed, float turnSpeed, int wait){
			fly = false;
			this.speed = speed;
			this.turnSpeed = turnSpeed;
			prevSpeed = speed;
			prevTurnSpeed = turnSpeed;
			if(attackTimer >= wait)
				switchSubPhases = true;
		}

		private void AI_Summon(int times, int waitBetween, bool randomAngle, params float[] angles){
			SummonedWyrms = UpdateWyrmCount();

			if(attackProgress >= times || SummonedWyrms == times){
				switchSubPhases = true;
				return;
			}

			if(attackTimer < waitBetween)
				return;
			
			float rotation;
			Vector2 range = npc.Center + new Vector2(Main.rand.NextFloat(-5 * 16, 5 * 16), Main.rand.NextFloat(-5 * 16, 5 * 16));
			
			if(randomAngle)
				rotation = Main.rand.NextFloat(0, 2 * MathHelper.Pi);
			else
				rotation = angles[attackProgress];
			
			NPC.NewNPC((int)range.X, (int)range.Y, ModContent.NPCType<Summon>(), ai1: npc.whoAmI, ai2: rotation);
			
			SummonedWyrms++;

			attackProgress++;
			attackTimer = 0;
		}

		private void AI_MegaCharge(int delay, int flyTime, int rockCount, float targetTolerance){
			/*			ATTACK PROGRESS
			 *		0: Slow down and target where player currently is
			 *		1: Move like normal until timer runs out
			 *		2: Charge towards old position and enable flying
			 *		3: Attempt to "hover" directly above player, adjusting speed
			 *			to be always faster than the player's current velocity.
			 *			Also drop "Draek Rock" projectiles
			 */
			if(attackProgress == 0){
				speed = 1f;
				turnSpeed = 0.3f;
				CustomTarget = Main.player[npc.target].Center;
				attackProgress++;
			}else if(attackProgress == 1){
				if(attackTimer < delay)
					return;
				else{
					attackProgress++;
					attackTimer = 0;
				}
			}else if(attackProgress == 2 || attackProgress == 3){
				speed = Main.expertMode ? 16f : 12f;
				fly = true;
				JewelExplosion();
				if(Vector2.Distance(CustomTarget, npc.Center) < 1 * 16)
					attackProgress++;
			}else if(attackProgress >= 4){
				CustomTarget = Main.player[npc.target].Center - new Vector2(0, 16 * 10);
				speed = Main.player[npc.target].velocity.Length() + 6f;

				float rockDelay = flyTime / (rockCount + 1);

				if(Vector2.Distance(npc.Center, CustomTarget) > targetTolerance){
					attackTimer--;
					return;
				}

				if(attackTimer > rockDelay){
					float rockSpawnOffset = Main.rand.NextFloat(-3, 3);

					npc.SpawnProjectile(npc.Center + new Vector2(rockSpawnOffset, 0),
						Vector2.Zero,
						ModContent.ProjectileType<Projectiles.Draek.DraekRock>(),
						30 + (Main.expertMode ? 30 : 0),
						16f,
						Main.myPlayer,
						20f,
						0.35f
					);

					Main.PlaySound(SoundID.Item69, npc.Center);

					attackTimer = 0;
					attackProgress++;

					if(attackProgress == 4 + rockCount){
						switchSubPhases = true;
						CustomTarget = Vector2.Zero;
						speed = prevSpeed;
						turnSpeed = prevTurnSpeed;
						fly = false;
					}
				}
			}
		}

		private void AI_TryLandExplosion(){
			List<Point> tileCoords = GetCollidingTileCoords();
			bool triggerExplosion = false;

			//Loop over all tiles we're colliding with
			//If any are solid, trigger the explosion and go to the next subphase
			foreach(Point p in tileCoords){
				if(CosmivengeonUtils.TileIsSolidOrPlatform(p.X, p.Y) && Main.tile[p.X, p.Y].type != TileID.Platforms){
					triggerExplosion = true;
					switchSubPhases = true;
					break;
				}
			}

			//If we should trigger the explosion due to colliding with tiles, do so
			//OR if the timer has run out
			if(triggerExplosion || explosionTimer < 0)
				RockExplosion();
		}

		private void AI_Beserker(bool lasers = false){
			if(attackProgress < 4)
				CustomTarget = Main.player[npc.target].Center;
			else
				CustomTarget = Main.player[npc.target].Center - new Vector2(0, 8 * 16);

			if(attackProgress == 0){
				AI_Worm(speed, turnSpeed, 1);
				attackProgress = 2;
			}

			if(npc.ai[1] == 0f && lasers && laserTimer < 0){
				npc.ai[1] = 2;	//Signal to the jewel segment to fire lasers
				laserTimer = Laser_Delay;
			}

			if(attackProgress == 2 || attackProgress == 3){
				speed = 20f;
				turnSpeed = 0.245f;
				fly = true;
				JewelExplosion();
				if(Vector2.Distance(CustomTarget, npc.Center) < 1 * 16)
					attackProgress++;
			}

			if(attackTimer > DesoMode_RockDelay){
				npc.SpawnProjectile(npc.Center,
					new Vector2(0, -15),
					ModContent.ProjectileType<Projectiles.Draek.DraekRock>(),
					75,
					16f,
					Main.myPlayer,
					25f,
					0.35f
				);

				Main.PlaySound(SoundID.Item69, npc.Center);

				attackTimer = 0;
				attackProgress++;

				if(attackProgress == rockCount_enraged_expert){
					switchSubPhases = true;
					CustomTarget = Vector2.Zero;
					speed = prevSpeed;
					turnSpeed = prevTurnSpeed;
					fly = false;
				}
			}
		}

		private void JewelExplosion(){
			if(npc.ai[1] == 0f && attackProgress == 2){	//Notify the second body segment to cause the explosion
				npc.ai[1] = 1f;
				attackProgress++;

				if(CurrentPhase == Phase_2_DesoMode && attackPhase >= DesoMode_Berserker)
					Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0), npc.Center);
			}
		}

		private void RockExplosion(){
			//Generate 10-16 small rocks going in several different directions and velocity changes
			int amount = Main.rand.Next(10, 17);

			for(int i = 0; i < amount; i++){
				npc.SpawnProjectile(npc.Center,
					new Vector2(0, -7),
					ModContent.ProjectileType<Projectiles.Draek.DraekRockExplosion>(),
					20,
					4f,
					Main.myPlayer,
					Main.rand.NextFloat(0.5f / 60f, 5f / 60f) * (Main.rand.NextBool() ? 1 : -1),
					Main.rand.NextFloat(6f / 60f, 12f / 60f)
				);
			}

			Main.PlaySound(SoundID.Item14.WithPitchVariance(0.6f).WithVolume(0.8f), npc.Center);

			switchSubPhases = true;
		}

		private List<Point> GetCollidingTileCoords(){
			List<Point> ret = new List<Point>();

			int curTileX = (int)(npc.position.X / 16f);
			int curTileY = (int)(npc.position.Y / 16f);

			int tileX = curTileX - 1;
			int tileY = curTileY - 1;
			int endTileX = curTileX + (int)(npc.width / 16f) + 1;
			int endTileY = curTileY + (int)(npc.height / 16f) + 1;

			for(; tileX < endTileX; tileX++)
				for(; tileY < endTileY; tileY++)
					ret.Add(new Point(tileX, tileY));

			return ret;
		}

		public override int SetCustomBodySegments(int startDistance){
			int latestNPC = npc.whoAmI;
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body0>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body1>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body2>(), latestNPC);
			latestNPC = NewBodySegment(ModContent.NPCType<DraekP2_Body3>(), latestNPC);
			return latestNPC;
		}
	}

	//Draek only has 4 body segments, so I can just hardcode the classes :chaelure:
	internal class DraekP2_Body0 : Worm{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}

		public override void SetDefaults(){
			npc.CloneDefaults(headType);

			npc.boss = true;
			
			npc.width = 15;
			npc.height = 15;
			
			npc.aiStyle = -1;
			npc.lifeMax = 2500;
			npc.defense = 25;
			npc.damage = 40;
			npc.scale = 2f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound
		}

		public override bool PreNPCLoot(){
			return false;	//Don't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			DraekP2Head.ExpertStatsHelper(npc, numPlayers, bossLifeScale);
		}
	}
	internal class DraekP2_Body1 : DraekP2_Body0{
		public override void AI(){
			if(Main.npc[(int)npc.ai[3]].ai[1] == 1f){	//Head segment has flagged for this one to cause the "Jewel Explosion"
				//Play the sounds
				Main.PlaySound(SoundID.Item27, npc.Center);		//Crystal break sound effect
				Main.PlaySound(SoundID.Item70, npc.Center);		//Staff of Earth alternative sound effect

				//Spawn the dust
				for(int i = 0; i < 60; i++){
					Dust.NewDust(npc.Center, 50, 50, 74, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
					Dust.NewDust(npc.Center, 50, 50, 107, Main.rand.NextFloat(-8, 8), Main.rand.NextFloat(-8, 8));
				}
				
				Main.npc[(int)npc.ai[3]].ai[1] = 0f;
			}else if(Main.npc[(int)npc.ai[3]].ai[1] == 2f){	//Head segment has flagged for laser attack to happen
				Vector2 spawnOrigin = Main.player[npc.target].Center;
				Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;
				for(int i = 0; i < 3; i++){
					npc.SpawnProjectile(npc.Center,
						Vector2.Zero,
						ModContent.ProjectileType<Projectiles.Draek.DraekLaser>(),
						50,
						6f,
						Main.myPlayer,
						spawnOrigin.X + positionOffset.X,
						spawnOrigin.Y + positionOffset.Y
					);

					positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;
				}

				//Play "boss laser" sound effect
				Main.PlaySound(SoundID.Item33, npc.position);

				Main.npc[(int)npc.ai[3]].ai[1] = 0f;
			}
		}
	}
	internal class DraekP2_Body2 : DraekP2_Body0{}
	internal class DraekP2_Body3 : DraekP2_Body0{}

	internal class DraekP2Tail : Worm{
		public override string Texture => "CosmivengeonMod/NPCs/Draek/DraekP2_Tail";

		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
		}

		public override void SetDefaults(){
			npc.CloneDefaults(headType);

			npc.boss = true;
			
			npc.aiStyle = -1;
			npc.lifeMax = 500;
			npc.defense = 25;
			npc.damage = 40;
			npc.scale = 2f;
			npc.lavaImmune = true;
			npc.noGravity = true;
			npc.noTileCollide = true;
			npc.knockBackResist = 0f;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			tail = true;
			npc.width = 15;
			npc.height = 15;

			npc.dontCountMe = true;

			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.DeathSound = SoundID.NPCDeath60;	//Phantasm Dragon death sound
		}

		public override bool PreNPCLoot(){
			return false;	//Don't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			DraekP2Head.ExpertStatsHelper(npc, numPlayers, bossLifeScale);
		}
	}
}