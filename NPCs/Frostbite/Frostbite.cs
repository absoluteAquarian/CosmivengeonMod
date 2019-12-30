using CosmivengeonMod.Items.Boss_Bags;
using CosmivengeonMod.Items.Frostbite;
using CosmivengeonMod.Items.Masks;
using Microsoft.Xna.Framework;
using System;
using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CosmivengeonMod.NPCs.Frostbite{
	[AutoloadBossHead]
	public class Frostbite : ModNPC{
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Frostbite");
			Main.npcFrameCount[npc.type] = 15;
		}

		public override void SetDefaults(){
			npc.scale = 1f;
			//frame dimensions:  82x246px
			npc.height = 80;
			npc.width = 250;
			npc.aiStyle = -1;
			npc.damage = 30;
			npc.defense = 8;
			npc.lifeMax = 2000;
			npc.HitSound = SoundID.NPCHit11;	//Snow NPC hit sound
			npc.noGravity = false;
			npc.knockBackResist = 0f;	//100% kb resist
			npc.npcSlots = 30f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noTileCollide = false;

			npc.value = Item.buyPrice(gold: 2, silver: 75);

			bossBag = ModContent.ItemType<FrostbiteBag>();

			npc.buffImmune[BuffID.Frostburn] = true;
			npc.buffImmune[BuffID.OnFire] = true;
			npc.buffImmune[BuffID.Poisoned] = true;

			CosmivengeonUtils.PlayMusic(this, CosmivengeonBoss.Frostbite);

			if(!Main.expertMode && !CosmivengeonWorld.desoMode)
				Subphases = new int[]{
					AI_Attack_Walk,
					AI_Attack_Charge,
					AI_Attack_Walk,
					AI_Attack_Flick,
					AI_Attack_Walk,
					AI_Attack_Stomp
				};
			else if(Main.expertMode && !CosmivengeonWorld.desoMode)
				Subphases = new int[]{
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

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			npc.lifeMax /= 2;	//Negate vanilla health buff
			if(!CosmivengeonWorld.desoMode){
				npc.ScaleHealthBy(3f / 5f, numPlayers);
				npc.damage = 48;
				npc.defense = 11;
			}else{
				npc.ScaleHealthBy(3f / 4f, numPlayers);
				npc.damage = 62;
				npc.defense = 14;
			}
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

		private float GetWaitBetweenSubphases
			=> Main.rand.NextFloat(CosmivengeonUtils.GetModeChoice(5, 3, 2), CosmivengeonUtils.GetModeChoice(6, 4, 3));

		public override void NPCLoot(){
			CosmivengeonWorld.downedFrostbiteBoss = true;

			if(Main.expertMode)
				npc.DropBossBags();
			else
				NormalModeDrops(npc: npc);

			CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.downedFrostbiteBoss));
		}

		public static void NormalModeDrops(Player player = null, NPC npc = null, bool quickSpawn = false){
			int[] drops = new int[]{
				ModContent.ItemType<FrostbiteFlamethrower>(),
				ModContent.ItemType<SnowballFlail>(),
				ModContent.ItemType<IceDisk>(),
				ModContent.ItemType<BlizzardRod>(),
				ModContent.ItemType<FrostRifle>(),
				ModContent.ItemType<SubZero>(),
				ModContent.ItemType<IceScepter>(),
				ModContent.ItemType<FrostDemonHorn>(),
				ModContent.ItemType<SnowscaleCoat>()
			};

			int dropType, dropAmount = 1;

			dropType = Main.rand.Next(drops);

			if(player != null && quickSpawn){
				player.QuickSpawnItem(dropType, dropAmount);

				if(Main.rand.NextFloat() < 0.1)
					player.QuickSpawnItem(ModContent.ItemType<FrostbiteMask>());

				if(!CosmivengeonWorld.obtainedLore_FrostbiteBoss){
					player.QuickSpawnItem(ModContent.ItemType<IceforgedRelic>());
					CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.obtainedLore_FrostbiteBoss));
				}
			}else if(npc != null){
				Item.NewItem(npc.getRect(), dropType, dropAmount);
				
				if(Main.rand.NextFloat() < 0.1)
					Item.NewItem(npc.getRect(), ModContent.ItemType<FrostbiteMask>());

				if(!CosmivengeonWorld.obtainedLore_FrostbiteBoss){
					Item.NewItem(npc.getRect(), ModContent.ItemType<IceforgedRelic>());
					CosmivengeonWorld.CheckWorldFlagUpdate(nameof(CosmivengeonWorld.obtainedLore_FrostbiteBoss));
				}
			}
		}

		public override bool CheckActive(){
			return Vector2.Distance(npc.Center, Target?.Center ?? npc.Center) > 200 * 16;
		}

		public override void FindFrame(int frameHeight){
			int frame = 0;
			if(CurrentSubphase == AI_Attack_Walk){
				//He walkin
				frame = AI_AnimationTimer % (Walk_Frames_Max * 12) / 12;
			}else if((CurrentSubphase == AI_Attack_Charge && AI_AttackProgress < 3) || CurrentSubphase == AI_Attack_Stomp)
				frame = AI_AnimationTimer % (Walk_Frames_Max * 8) / 8;
			else if(CurrentSubphase == AI_Attack_Charge && AI_AttackProgress >= 3)
				frame = Bite_Frames_Start;
			else if((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress == 1 && AI_Timer > 0) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 0))
				frame = Flick_Frames_Start;
			else if((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress < normal_icicle_count + 1 && AI_Timer > 0) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 1))
				frame = Flick_Frames_Start + 1;
			else if((CurrentSubphase == AI_Attack_Flick && AI_AttackProgress == normal_icicle_count + 1) || (CurrentSubphase == AI_Expert_SnowCloud && AI_AttackProgress == 2))
				frame = Flick_Frames_Start + 2;
			else if(CurrentSubphase == AI_Enrage_Smash)
				frame = Smash_Frames_Start + AI_AttackProgress;
			else if(CurrentSubphase == AI_Expert_Snowball)
				frame = Bite_Frames_Start + Utils.Clamp(AI_AttackProgress, 0, 2);

			npc.frame.Y = frame * frameHeight;
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
		private const int enraged_icicle_count = 10;
		private const int enraged_icicle_wait = 30;
		private const int expert_icicle_count = 11;
		private const int expert_enraged_icicle_count = 16;

		private int subphaseIndex = 0;
		private int[] Subphases;

		private bool noTargetsAlive = false;
		private bool startDespawn = false;

		public override void AI(){
			//AI:  https://docs.google.com/document/d/1B7liHxU-65k_f8eXlC6-M4HLMx9Xu5d5LQWFIznylv8

			npc.TargetClosest(true);
			Target = Main.player[npc.target];

			noTargetsAlive = Target.dead || !Target.active;
			CheckTargetIsDead();
			
			if(noTargetsAlive)
				return;
			
			CheckSubPhaseChange();
			CheckPhaseChange();
			CheckFallThroughPlatforms();
			CheckTileStep();

			if(switchSubPhase)
				return;

			if(Phase == Phase_1){
				if(CurrentSubphase == AI_Attack_Walk && AI_WaitTimer < 0)
					AI_WaitTimer = (int)(GetWaitBetweenSubphases * 60);
				else if(CurrentSubphase == AI_Attack_Walk && AI_WaitTimer >= 0){
					if(AI_WaitTimer == 0)
						switchSubPhase = true;
					int speed = CosmivengeonUtils.GetModeChoice(3, 5, 0);
					float acceleration = CosmivengeonUtils.GetModeChoice(2f, 4f, 0);
					AI_Walk(speed, acceleration / 60f);
				}else if(CurrentSubphase == AI_Attack_Charge){
					int chargeSpeed = CosmivengeonUtils.GetModeChoice(5, 7, 0);
					int chargeFrames = CosmivengeonUtils.GetModeChoice(120, 90, 0);
					float breathSpeed = CosmivengeonUtils.GetModeChoice(5f, 7f, 0);
					float breathTime = CosmivengeonUtils.GetModeChoice(1.5f, 1f, 0);
					AI_Charge_BreatheFrost(
						chargeSpeed,
						chargeFrames,
						breathSpeed,
						breathAngle: 0f,
						maxFlamesShot: 40,
						breathTime
					);
				}else if(CurrentSubphase == AI_Attack_Flick){
					int count = Main.expertMode ? expert_icicle_count : normal_icicle_count;
					AI_Flick(20f * 16f / 60f, count, MathHelper.ToRadians(20f));
				}else if(CurrentSubphase == AI_Attack_Stomp){
					float initialYVel = CosmivengeonUtils.GetModeChoice(10f, 13f, 0);
					int xVel = CosmivengeonUtils.GetModeChoice(4, 6, 0);
					float acceleration = CosmivengeonUtils.GetModeChoice(3f, 5f, 0);
					AI_Stomp(60, initialYVel, xVel, acceleration / 60f);
				}else if(CurrentSubphase == AI_Expert_Snowball)
					AI_Snowball(20, 10f);
			}else if(Phase == Phase_2){
				if(CurrentSubphase % 2 == 0 && AI_WaitTimer < 0)
					AI_WaitTimer = (int)(GetWaitBetweenSubphases * 60);
				else if(CurrentSubphase == AI_Attack_Walk && AI_WaitTimer >= 0){
					if(AI_WaitTimer == 0)
						switchSubPhase = true;
					int speed = CosmivengeonUtils.GetModeChoice(5, 7, 8);
					float acceleration = CosmivengeonUtils.GetModeChoice(4f, 7f, 9f);
					AI_Walk(speed, acceleration / 60f);
				}else if(CurrentSubphase == AI_Attack_Charge){
					int chargeSpeed = CosmivengeonUtils.GetModeChoice(8, 10, 12);
					int chargeFrames = CosmivengeonUtils.GetModeChoice(90, 75, 60);
					float breathSpeed = CosmivengeonUtils.GetModeChoice(8f, 10f, 15f);
					int maxFlamesShot = CosmivengeonUtils.GetModeChoice(60, 25, 45);
					float breathTime = CosmivengeonUtils.GetModeChoice(1f, 0.75f, 0.5f);
					AI_Charge_BreatheFrost(
						chargeSpeed,
						chargeFrames,
						breathSpeed,
						breathAngle: 0f,
						maxFlamesShot,
						breathTime
					);
				}else if(CurrentSubphase == AI_Attack_Flick){
					int count = CosmivengeonUtils.GetModeChoice(enraged_icicle_count, expert_enraged_icicle_count, expert_enraged_icicle_count + 5);
					AI_Flick(25f * 16f / 60f, count, MathHelper.ToRadians(15f));
				}else if(CurrentSubphase == AI_Attack_Stomp){
					float initialYVel = CosmivengeonUtils.GetModeChoice(13f, 14f, 15f);
					int xVel = CosmivengeonUtils.GetModeChoice(6, 9, 10);
					float acceleration = CosmivengeonUtils.GetModeChoice(5f, 8f, 10f);
					AI_Stomp(45, initialYVel, xVel, acceleration / 60f);
				}else if(CurrentSubphase == AI_Enrage_Smash)
					AI_Smash(30, 5 * 60, Main.expertMode);
				else if(CurrentSubphase == AI_Expert_Snowball)
					AI_Snowball(15, 15f);
				else if(CurrentSubphase == AI_Expert_SnowCloud)
					AI_SummonCloud();
			}

			//If Frostbite is charging/breathing frost, don't update the direction
			if(AI_WaitTimer < 0 && CurrentSubphase == AI_Attack_Charge && AI_AttackProgress < 3)
				npc.spriteDirection = spriteDir;
			else
				npc.spriteDirection = (Target.Center.X > npc.Center.X) ? 1 : -1;

			if(AI_Timer >= 0)
				AI_Timer--;
			
			AI_AnimationTimer++;
			
			if(AI_WaitTimer >= 0)
				AI_WaitTimer--;

			if(CurrentSubphase != AI_Attack_Stomp)
				npc.velocity.Y += 16f / 60f;

			npc.velocity.X.Clamp(-curMaxSpeed, curMaxSpeed);
		}

		private void CheckTargetIsDead(){
			//If the target is dead or not active, slow down the NPC
			//Then, plummet to hell and despawn naturally
			if(noTargetsAlive){
				if(Math.Abs(npc.velocity.X) > 0)
					npc.velocity.X *= 0.5f;
				
				if(npc.velocity.Length() < 1 && npc.velocity != Vector2.Zero){
					npc.velocity = Vector2.Zero;
				}

				if(npc.velocity.X == 0){
					npc.velocity.Y += 15f;
				}

				if(!startDespawn){
					startDespawn = true;
					npc.noTileCollide = true;
					npc.timeLeft = (int)(0.5f * 60);
				}

				if(npc.timeLeft == 0)
					npc.active = false;

				npc.timeLeft--;
			}
		}

		private void CheckPhaseChange(){
			if(Main.raining && CosmivengeonWorld.desoMode)
				Main.rainTime = 60;

			if(!Main.expertMode && !CosmivengeonWorld.desoMode){
				if(npc.life / (float)npc.lifeMax < 0.5f && Phase == Phase_1){
					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[]{
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash
					};
				}
			}else if(Main.expertMode && !CosmivengeonWorld.desoMode){
				if(npc.life / (float)npc.lifeMax < 0.7f && Phase == Phase_1){
					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[]{
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash,
						AI_Attack_Walk,
						AI_Expert_Snowball,
						AI_Attack_Walk,
						AI_Expert_SnowCloud
					};
				}
			}else{
				if(Phase == Phase_1){
					if(!Main.raining)
						Main.raining = true;

					Phase++;
					subphaseIndex = -1;
					switchSubPhase = true;

					Subphases = new int[]{
						AI_Attack_Walk,
						AI_Attack_Charge,
						AI_Attack_Walk,
						AI_Attack_Flick,
						AI_Attack_Walk,
						AI_Attack_Stomp,
						AI_Attack_Walk,
						AI_Enrage_Smash,
						AI_Attack_Walk,
						AI_Expert_Snowball,
						AI_Attack_Walk,
						AI_Expert_SnowCloud
					};
				}
			}
		}

		private void CheckSubPhaseChange(){
			if(!switchSubPhase)
				return;

			CurrentSubphase = Subphases[++subphaseIndex % Subphases.Length];

			AI_AnimationTimer = 0;
			AI_AttackProgress = 0;
			AI_Timer = -1;
			AI_WaitTimer = -1;
			switchSubPhase = false;
		}

		private void CheckFallThroughPlatforms(){
			if(CurrentSubphase == AI_Attack_Walk || CurrentSubphase == AI_Attack_Stomp)
				return;

			//First, check if the tiles directly beneath Frostbite
			// are platforms/air.  If they are, disable tile collision
			int tileStartX = (int)(npc.position.X / 16f);
			int tileEndX = (int)(npc.BottomRight.X / 16f);
			int tileY = (int)((npc.Bottom.Y + 8f) / 16f);
			
			for(int x = tileStartX; x <= tileEndX; x++){
				Tile tile = Main.tile[x, tileY];
				//If this tile isn't a platform, re-enable tile collision
				if(CosmivengeonUtils.TileIsSolidOrPlatform(x, tileY) && tile.type != TileID.Platforms){
					npc.noTileCollide = false;
					return;
				}
			}
			
			//Next, if we're in line with the player target or they're above us,
			// don't do anything
			//Set tile collision to true just in case though
			if(Target.Top.Y <= npc.Bottom.Y){
				npc.noTileCollide = false;
				return;
			}

			//Finally, we just have platforms under us.  Disable tile collision
			npc.noTileCollide = true;
		}

		private void CheckTileStep(){
			if(CurrentSubphase == AI_Attack_Stomp)
				return;

			if(npc.velocity.Y == 0f)
				Collision.StepDown(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY);
			if(npc.velocity.Y >= 0)
				Collision.StepUp(ref npc.position, ref npc.velocity, npc.width, npc.height, ref npc.stepSpeed, ref npc.gfxOffY, specialChecksMode: 1);
		}

		private void AI_Walk(int speed, float acceleration){
			curMaxSpeed = speed;
			npc.velocity.X += acceleration * npc.spriteDirection;
		}

		private void AI_Charge_BreatheFrost(int chargeSpeed, int chargeFrames, float breathSpeed, float breathAngle, float maxFlamesShot, float breathTime){
			if(AI_AttackProgress < 3)
				AI_Charge(chargeSpeed, chargeFrames);
			else if(AI_AttackProgress >= 3)
				AI_BreatheFrost(breathSpeed, breathAngle, maxFlamesShot, breathTime);
		}

		private void AI_Charge(int speed, int maxFrames){
			curMaxSpeed = speed;
			if(AI_AttackProgress == 0){
				//Set the NPC's direction and spriteDirection so that he doesn't turn around while charging
				spriteDir = npc.spriteDirection;
				AI_AttackProgress++;
				AI_Timer = -1;
			}else if(AI_AttackProgress == 1){
				//Slow down and prepare to charge
				if(AI_Timer < 0)
					AI_Timer = 60;

				if(AI_Timer > 0)
					npc.velocity.X *= 0.873f;
				else{
					AI_AttackProgress++;
					AI_Timer = -1;
					Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0), npc.Center);
				}
			}else if(AI_AttackProgress == 2){
				//The direction has been set.  Ram towards where the player was
				npc.velocity.X = speed * spriteDir;
				if(AI_Timer < 0)
					AI_Timer = maxFrames;

				if(AI_Timer == 0){
					AI_AttackProgress++;
					AI_Timer = -1;
				}
			}
		}

		private void AI_BreatheFrost(float speed, float angle, float flamesPerSecond, float breatheTime){
			//Get the point and direction the frost breath should go towards
			Vector2 start = (npc.spriteDirection == 1) ? npc.Right - new Vector2(45, 0) : npc.Left + new Vector2(45, 0);
			//Then, get the maximum amount of flames Frostbite will shoot
			//This is determined by the "breatheTime" parameter (seconds)
			//Frostbite will shoot "flamesPerSecond" flames per second
			int maxFlames = (int)(flamesPerSecond * breatheTime + 1);

			if(AI_Timer < 0)
				AI_Timer = (int)(60f / flamesPerSecond) + 1;

			if(AI_AttackProgress - 3 < maxFlames && AI_Timer == 0){
				//Get a random angle to add to "angle"; variation is [-3, 3] degrees
				angle.MirrorAngle(mirrorY: npc.spriteDirection == -1);
				//Get the speed vector based on this angle
				Vector2 flameSpeed = angle.ToRotationVector2() * speed;
				//Finally, spawn the projectile
				npc.SpawnProjectile(
					start,
					flameSpeed.RotatedByRandom(MathHelper.ToRadians(3)),
					ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteBreath>(),
					30,
					2f,
					Main.myPlayer,
					0f,
					0f
				);

				AI_AttackProgress++;
				AI_Timer = -1;

				Main.PlaySound(SoundID.Item34, start);
			}

			if(AI_AttackProgress == maxFlames + 3)
				switchSubPhase = true;

			npc.velocity.X *= 0.8159f;
		}

		private int curIcicle = 0;

		private void AI_Flick(float speedX, int numProjectiles, float angle){
			speedX *= npc.spriteDirection;

			if(AI_Timer < 0 && AI_AttackProgress == 0){
				curIcicle = -(int)(numProjectiles / 2f);
				AI_Timer = Phase == Phase_1 ? normal_icicle_wait : enraged_icicle_wait;
				AI_AttackProgress++;
			}else if(AI_Timer >= 0 && AI_AttackProgress == 1){
				npc.velocity.X *= 0.9185f;
			}else if(AI_Timer < 0 && AI_AttackProgress < 1 + numProjectiles){
				angle.MirrorAngle(mirrorY: npc.spriteDirection == 1);

				//Calculate the Y-velocity the middle projectile needs to move at,
				// then just add/subtract some from the X-velocity to make the spread
				//Yes fuck you I'm stealing Cyrogen's attack.  Bite me
				float speedY = 1.5f * -Math.Abs(speedX);

				float speedXFactor = 0.35f;

				Vector2 spawn = npc.spriteDirection == -1 ? npc.TopRight : npc.position;

				//Spawn the icicles
				npc.SpawnProjectile(
					spawn,
					new Vector2(speedX + speedXFactor * curIcicle, speedY),
					ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteIcicle>(),
					48,
					3f
				);

				AI_AttackProgress++;

				curIcicle++;
			}else if(AI_AttackProgress >= 1 + numProjectiles)
				switchSubPhase = true;
		}

		private void AI_Stomp(int jumpDelay, float initialYVel, int xVel, float walkAccel){
			if(AI_Timer < 0 && AI_AttackProgress == 0){
				AI_Timer = jumpDelay;
				AI_AttackProgress++;
				AI_Walk(xVel, walkAccel);
			}else if(AI_Timer >= 0 && AI_AttackProgress == 1){
				npc.velocity.X += 8f / 60f * npc.spriteDirection;
				AI_Walk(xVel, walkAccel);
			}else if(AI_Timer < 0 && AI_AttackProgress == 1){
				npc.velocity.X += 8f / 60f * npc.spriteDirection;
				npc.velocity.Y = -initialYVel;
				AI_AttackProgress++;
			}else if(AI_AttackProgress == 2){
				npc.velocity.X += 8f / 60f * npc.spriteDirection;
				npc.velocity.Y += initialYVel / 20f / 60f;

				if(npc.position.Y == npc.oldPosition.Y && npc.oldVelocity.Y != 0)
					AI_AttackProgress++;
			}else if(AI_AttackProgress == 3){
				//Spawn a shitton of the flame particles and play an explosion sound
				Main.PlaySound(SoundID.Item14.WithVolume(0.75f), npc.Bottom);

				for(int i = 0; i < 30; i++){
					npc.SpawnProjectile(
						npc.Bottom,
						new Vector2(0, -8).RotatedByRandom(MathHelper.ToRadians(45)),
						ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteBreath>(),
						30,
						2f,
						Main.myPlayer,
						1f,
						0f
					);
				}

				switchSubPhase = true;
			}
		}

		private void AI_Smash(int delay, int wallTimeLeft, bool wallsShootBolts){
			if(AI_AttackProgress == 0 && AI_Timer < 0)
				AI_Timer = delay;
			else if(AI_AttackProgress == 0 && AI_Timer >= 0){
				if(AI_Timer == 0)
					AI_AttackProgress++;

				npc.velocity.X *= 0.925f;
			}else if(AI_AttackProgress == 1 && AI_Timer < 0){
				AI_Timer = 20;

				for(int i = 0; i < 2; i++){
					Vector2 offset = Target.Top + new Vector2(i == 0 ? 10 * 16f : -10 * 16f, -2 * 16);
					NPC.NewNPC((int)offset.X, (int)offset.Y, ModContent.NPCType<FrostbiteWall>(), 0, wallTimeLeft, 80, wallsShootBolts ? 1 : 0);
				}
			}else if(AI_AttackProgress == 1 && AI_Timer == 0){
				AI_AttackProgress++;
				AI_Timer = 20;
			}else if(AI_AttackProgress == 2 && AI_Timer == 0)
				switchSubPhase = true;
		}

		private void AI_Snowball(int delay, float speed){
			npc.velocity.X *= 0.8732f;

			if(AI_AttackProgress == 0 && AI_Timer < 0)
				AI_Timer = delay;
			else if(AI_AttackProgress == 0 && AI_Timer == 0){
				AI_AttackProgress++;
				AI_Timer = 10;
			}else if(AI_AttackProgress == 1 && AI_Timer < 0){
				AI_Timer = 20;
				AI_AttackProgress++;
			}else if(AI_AttackProgress == 2 && AI_Timer < 0){
				//Get the starting position of the snowball (it's the same as the breath attack, just further fowards in the mouth)
				Vector2 start = (npc.spriteDirection == 1) ? npc.Right - new Vector2(40, 0) : npc.Left + new Vector2(40, 0);

				Vector2 speedToTarget = Vector2.Normalize(Target.Center - start) * speed;

				//Spawn the snowball
				npc.SpawnProjectile(
					start,
					speedToTarget,
					ModContent.ProjectileType<Projectiles.Frostbite.FrostbiteRock>(),
					70,
					6f,
					Main.myPlayer,
					npc.target,
					speed
				);

				AI_Timer = 20;
				AI_AttackProgress++;
			}else if(AI_AttackProgress == 3 && AI_Timer < 0)
				switchSubPhase = true;
		}

		private void AI_SummonCloud(){
			if(Main.npc.Any(n => n.active && n.type == ModContent.NPCType<FrostCloud>() && n.ai[0] == npc.whoAmI)){
				switchSubPhase = true;
				return;
			}

			if(AI_Timer < 0 && AI_AttackProgress == 0){
				AI_Timer = Phase == Phase_1 ? normal_icicle_wait : enraged_icicle_wait;
			}else if(AI_Timer >= 0 && AI_AttackProgress == 0){
				npc.velocity.X *= 0.9185f;

				if(AI_Timer == 0)
					AI_AttackProgress++;
			}else if(AI_Timer < 0 && AI_AttackProgress == 1){
				//Spawn the cloud
				Vector2 spawn = npc.spriteDirection == -1 ? npc.TopRight : npc.position;
				spawn -= new Vector2(0, 3 * 16);

				float angle = MathHelper.ToRadians(45f);
				angle.MirrorAngle(mirrorY: npc.spriteDirection == -1);

				Vector2 initialSpeed = angle.ToRotationVector2() * (FrostCloud.TargetSpeed + 8);
				initialSpeed = initialSpeed.RotatedByRandom(MathHelper.ToRadians(5f));

				NPC.NewNPC(
					(int)spawn.X,
					(int)spawn.Y,
					ModContent.NPCType<FrostCloud>(),
					0,
					npc.whoAmI,
					initialSpeed.X,
					initialSpeed.Y
				);

				AI_AttackProgress++;
				AI_Timer = 20;
			}else if(AI_AttackProgress == 2 && AI_Timer < 0)
				switchSubPhase = true;
		}
	}
}