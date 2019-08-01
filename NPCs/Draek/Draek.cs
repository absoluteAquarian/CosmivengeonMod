using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

using DraekSword = CosmivengeonMod.Projectiles.Draek.DraekSword;
using DraekProjectile = CosmivengeonMod.Projectiles.Draek.DraekProjectile;
using Summon = CosmivengeonMod.NPCs.Draek.DraekWyrmSummon_Head;

namespace CosmivengeonMod.NPCs.Draek{
	[AutoloadBossHead]
	public class Draek : ModNPC{
		private bool dashing = false;
		private bool dashWait = true;

//		private PID pid = PID.Uninitialized;
		
#region Defaults
		public override void SetStaticDefaults(){
			DisplayName.SetDefault("Draek");
			Main.npcFrameCount[npc.type] = 16;
			NPCID.Sets.TrailCacheLength[npc.type] = 5;
			NPCID.Sets.TrailingMode[npc.type] = 0;
		}

		public override void SetDefaults(){
			npc.scale = 1f;
			//Draek frame dimentions:  186x290px
			npc.width = 150;
			npc.height = 240;
			npc.aiStyle = -1;	//-1 means that this enemy has a unique AI; don't copy an existing style
			npc.damage = 45;
			npc.defense = 20;
			npc.lifeMax = 3000;
			npc.HitSound = new Terraria.Audio.LegacySoundStyle(SoundID.Tink, 0);	//Stone tile hit sound
			npc.noGravity = true;
			npc.knockBackResist = 0f;	//100% kb resist
			npc.npcSlots = 30f;
			npc.boss = true;
			npc.lavaImmune = true;
			npc.noTileCollide = true;

			npc.buffImmune[BuffID.Poisoned] = true;
			npc.buffImmune[BuffID.Confused] = true;
			npc.buffImmune[BuffID.Burning] = true;

			music = mod.GetSoundSlot(SoundType.Music, "Sounds/Music/SuccessorOfTheJewel");
			musicPriority = MusicPriority.BossLow;
		}
#endregion

		public override bool PreNPCLoot(){
			return false;	//First phase shouldn't drop anything
		}

		public override void ScaleExpertStats(int numPlayers, float bossLifeScale){
			npc.lifeMax = 3000 + 1500 * (numPlayers + 1);
			npc.damage = 55;
			npc.defense = 25;
		}

#region AI Properties
		//consts for showing what ai[] slot does what
		private const int AI_Timer_Slot = 0;
		private const int AI_Attack_Slot = 1;
		private const int AI_Attack_Progress_Slot = 2;
		private const int AI_Animation_Counter_Slot = 3;

		//states for ai[1] = AI_Attack
		private const int Attack_Idle = 0;
		private const int Attack_Shoot = 1;
		private const int Attack_Throw_Sword = 2;
		private const int Attack_Shoot_No_Sword = 3;
		private const int Attack_Dash = 4;
		private const int Attack_Retrieve_Sword = 5;

		//properties for ai[]
		public float AI_Timer{
			get{
				return npc.ai[AI_Timer_Slot];
			}
			set{
				npc.ai[AI_Timer_Slot] = value;
			}
		}
		public float AI_Attack{
			get{
				return npc.ai[AI_Attack_Slot];
			}
			set{
				npc.ai[AI_Attack_Slot] = value;
			}
		}
		public float AI_Attack_Progress{
			get{
				return npc.ai[AI_Attack_Progress_Slot];
			}
			set{
				npc.ai[AI_Attack_Progress_Slot] = value;
			}
		}
		public float AI_Animation_Counter{
			get{
				return npc.ai[AI_Animation_Counter_Slot];
			}
			set{
				npc.ai[AI_Animation_Counter_Slot] = value;
			}
		}
#endregion

#region Animation Constants
		//consts for animation frames
		private const int Idle_Sword_0 = 0;
		private const int Idle_Sword_1 = 1;
		private const int Idle_Sword_2 = 2;
		private const int Idle_Sword_3 = 3;
		private const int Idle_No_Sword_0 = 4;
		private const int Idle_No_Sword_1 = 5;
		private const int Idle_No_Sword_2 = 6;
		private const int Idle_No_Sword_3 = 7;
		private const int Throw_Sword_0 = 8;
		private const int Throw_Sword_1 = 9;
		private const int Throw_Sword_2 = 10;
		private const int Throw_Sword_3 = 11;
		private const int Retrieve_Sword_0 = 12;
		private const int Retrieve_Sword_1 = 13;
		private const int Retrieve_Sword_2 = 14;
		private const int Retrieve_Sword_3 = 15;
		#endregion

#region Other Variables
		private int CurrentPhase = Phase_1;
		private int afterImageLength = 0;
		private bool hasSpawned = false;
		private bool switchPhases = false;
		private bool switchSubPhases = false;
		private bool throwingSword = false;
		private bool delayPhaseChange = false;
		private const int P1_subphase0_Attacks = 8;
		private const int P1_subphase2_Attacks = 10;
		private const int P1_Enrage_subphase0_Attacks = 8;
		private const int P1_Enrage_subphase2_Attacks = 16;
		private const int Phase_1 = 0;
		private const int Phase_1_Enrage = 1;
		
		public static Color TextColour{
			get{
				return new Color(55, 148, 107);
			}
		}

		private bool noTargetsAlive;
		
		private Player playerTarget;

		private int SummonedWyrms = 0;
#endregion
		
		public override void FindFrame(int frameHeight){
			int CounterMod30 = (int)(AI_Animation_Counter % 30);
			int offset = 0;
			bool useSwordThrowAnimation = (CurrentPhase == Phase_1 || CurrentPhase == Phase_1_Enrage) && throwingSword;
			if(CurrentPhase == Phase_1 || CurrentPhase == Phase_1_Enrage){
				if(AI_Attack == Attack_Idle || AI_Attack == Attack_Shoot || (AI_Attack == Attack_Throw_Sword && !useSwordThrowAnimation)){
					switch(CounterMod30){
						case 0:
							offset = Idle_Sword_0;
							break;
						case 7:
							offset = Idle_Sword_1;
							break;
						case 15:
							offset = Idle_Sword_2;
							break;
						case 22:
							offset = Idle_Sword_3;
							break;
						default:
							return;
					}
				}else if(AI_Attack == Attack_Throw_Sword && useSwordThrowAnimation){
					switch(CounterMod30){
						case 0:
							offset = Throw_Sword_0;
							break;
						case 7:
							offset = Throw_Sword_1;
							break;
						case 15:
							offset = Throw_Sword_2;
							break;
						case 22:
							offset = Throw_Sword_3;
							break;
						default:
							return;
					}
				}else if(AI_Attack == Attack_Shoot_No_Sword || AI_Attack == Attack_Dash){
					switch(CounterMod30){
						case 0:
							offset = Idle_No_Sword_0;
							break;
						case 7:
							offset = Idle_No_Sword_1;
							break;
						case 15:
							offset = Idle_No_Sword_2;
							break;
						case 22:
							offset = Idle_No_Sword_3;
							break;
						default:
							return;
					}
				}else if(AI_Attack == Attack_Retrieve_Sword){
					switch(CounterMod30){
						case 0:
							offset = Retrieve_Sword_0;
							break;
						case 7:
							offset = Retrieve_Sword_1;
							break;
						case 15:
							offset = Retrieve_Sword_2;
							break;
						case 22:
							offset = Retrieve_Sword_3;
							break;
						default:
							return;
					}
				}
			}
			npc.frame.Y = offset * frameHeight;
		}

		public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor){
			//Only apply afterimage effects while dashing
			if(afterImageLength > 1){
				//Afterimage effect
				Vector2 drawOrigin = new Vector2(Main.npcTexture[npc.type].Width * 0.5f, 0.5f * Main.npcTexture[npc.type].Height / Main.npcFrameCount[npc.type]);   
				SpriteEffects effect = (npc.spriteDirection == 1) ? SpriteEffects.FlipHorizontally : SpriteEffects.None;
				for (int k = 0; k < afterImageLength / 2; k++){
					Vector2 drawPos = npc.oldPos[k] - Main.screenPosition + drawOrigin + new Vector2(0f, npc.gfxOffY);

					drawPos.Y -= 32;
					
					Color color = npc.GetAlpha(lightColor) * ((float)(npc.oldPos.Length - k) / (float)npc.oldPos.Length);
					color.A = (byte)(0.75f * 255f * (npc.oldPos.Length - k) / (float)npc.oldPos.Length);	//Apply transparency

					spriteBatch.Draw(Main.npcTexture[npc.type], drawPos, npc.frame, color, npc.rotation, drawOrigin, npc.scale, effect, 0f);
				}
			}
			return true;
		}

		public override bool CheckDead(){
			NPC.NewNPC((int)(npc.Center.X), (int)(npc.Center.Y), mod.NPCType<DraekP2Head>());
			
			//Spawn 8 gores, 4 per arm
			Vector2 goreTop = npc.Center;
			goreTop.X += (npc.TopLeft.X - npc.Center.X) / 2f;
			goreTop.Y += 20f;
			for(int i = 0; i < 4; i++){
				int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
			}
			goreTop.X = npc.Center.X;
			goreTop.X += (npc.TopRight.X - npc.Center.X) / 2f;
			for(int i = 0; i < 4; i++){
				int gore = Gore.NewGore(goreTop + new Vector2(0, 16 * i), new Vector2(Main.rand.NextFloat(-2, 2), Main.rand.NextFloat(-3, 5)), mod.GetGoreSlot("Gores/DraekArm"));
				Main.gore[gore].numFrames = 3;
				Main.gore[gore].frame = (byte)Main.rand.Next(3);
			}

			return true;
		}

		/*	AI (Normal mode):
		 *	- Phase 1, 50-100% HP
		 *		- Shoot
		 *			- Hovers near player and shoots projectiles
		 *		- Sword throw
		 *			- Hovers near player, then throw after being in a certain range close to target offset for some time
		 *		- Shoot again, no sword
		 *			- Follows player and shoots same projectiles, but more often
		 *		- Dash
		 *			- Attempts to ram the player twice
		 *			- Afterimage effects
		 *		- Retrive Sword
		 *			- Slows down, plays "retrieve sword" animation
		 *			- Becomes intangible for a short period of time
		 *		- Repeat
		 *	- Phase 1 (Enraged), 0-49% HP
		 *		- Shoot
		 *			- Faster than P1 SP0
		 *		- Sword throw
		 *			- Throws three swords this time
		 *		- Shoot again
		 *			- Faster P1 SP2
		 *		- Dash
		 *			- Three times, much faster but still avoidable
		 *		- Retrieve Sword
		 *		- Repeat
		 *	- At 0% HP, a new DraekP2 NPC spawns at this NPC's position and then this NPC dies
		 *	
		 *	AI (Expert mode):
		 *	- same as Normal mode AI, except:
		 *		- Shoot attacks
		 *			- sometimes fires a "shotgun" spread of lasers
		 *			- each shotgun attack reduces laser count by 3
		 *		- Sword throw
		 *			- spawns two Young Wyrms after final sword is thrown
		 *			- swords are slightly homing
		 *		- Dash
		 *			- one extra dash for each phase
		 *			- dashes are also slightly faster
		 */
		public override void AI(){
			if(!dashing){
				npc.spriteDirection = (npc.Center.X > Main.player[npc.target].Center.X) ? -1 : 1;
				afterImageLength--;
			}

			if (npc.target < 0 || npc.target == 255 || Main.player[npc.target].dead) {
				npc.TargetClosest(true);
			}

			AI_Animation_Counter++;

			AI_Check_Phase_Switch();

			playerTarget = Main.player[npc.target];
			noTargetsAlive = playerTarget.dead || !playerTarget.active;
			CheckTargetIsDead();
			
			if(noTargetsAlive)
				return;
			
			if(!hasSpawned){
				npc.TargetClosest(true);
				
				hasSpawned = true;
				AI_Attack = Attack_Shoot;
				CurrentPhase = Phase_1;
				Main.NewText("So, a new challenger has arisen to take my domain, hm?", TextColour);

				//Force the boss to appear within a 100-tile radius of the player
				Vector2 targetPointOnCircle = playerTarget.Center + Vector2.Normalize(playerTarget.Center - npc.Center) * 100f * 16f;
				if(Vector2.Distance(playerTarget.Center, npc.Center) > 100f * 16f)
					npc.position = targetPointOnCircle - new Vector2(Main.npcTexture[npc.type].Width * 0.5f, Main.npcTexture[npc.type].Height * 0.5f);
			}

			if(CurrentPhase == Phase_1){
				if(AI_Attack == Attack_Shoot){
					AI_Hover_Shoot(60, P1_subphase0_Attacks);
				}else if(AI_Attack == Attack_Throw_Sword){
					AI_Hover_Throw_Sword(1, 60);
				}else if(AI_Attack == Attack_Shoot_No_Sword){
					AI_Charge_Shoot(40, P1_subphase2_Attacks, 4, 4);
				}else if(AI_Attack == Attack_Dash){
					int dashCount = Main.expertMode ? 3 : 2;
					float dashVelocity = Main.expertMode ? 10f : 7f;
					AI_Dash(dashVelocity, dashCount, 75, 30);
				}else if(AI_Attack == Attack_Retrieve_Sword){
					AI_Retrieve_Sword();
				}
			}else if(CurrentPhase == Phase_1_Enrage){
				if(AI_Attack == Attack_Shoot){
					AI_Hover_Shoot(40, P1_Enrage_subphase0_Attacks);
				}else if(AI_Attack == Attack_Throw_Sword){
					AI_Hover_Throw_Sword(3, 45);
				}else if(AI_Attack == Attack_Shoot_No_Sword){
					AI_Charge_Shoot(20, P1_Enrage_subphase2_Attacks, 6, 6);
				}else if(AI_Attack == Attack_Dash){
					int dashCount = Main.expertMode ? 4 : 3;
					float dashVelocity = Main.expertMode ? 12f : 9f;
					AI_Dash(dashVelocity, dashCount, 60, 20);
				}else if(AI_Attack == Attack_Retrieve_Sword){
					AI_Retrieve_Sword();
				}
			}
			
			afterImageLength = Utils.Clamp(afterImageLength, 0, npc.oldPos.Length * 2);

			if(npc.life < npc.lifeMax / 2f && !switchPhases && CurrentPhase == Phase_1){
				switchPhases = true;

				Main.NewText("You're stronger than I expected, aren't you?  No matter.", TextColour);

				AI_Timer = 120;
				delayPhaseChange = true;

				npc.dontTakeDamage = true;
			}
		}

		private void AI_Check_Phase_Switch(){
			if(delayPhaseChange){
				if(AI_Timer == 0){
					delayPhaseChange = false;
					npc.dontTakeDamage = false;
					npc.velocity = Vector2.Zero;
				}else{
					AI_Attack = Attack_Idle;
					npc.dontTakeDamage = true;
					npc.velocity *= 0.91f;
					dashing = false;
					dashWait = false;
					AI_Timer--;
					return;
				}
			}
			
			if(switchPhases){
				switchPhases = false;
				dashing = false;
				dashWait = true;
				npc.dontTakeDamage = false;
				throwingSword = false;
				AI_Animation_Counter = 0;
				AI_Timer = 0;
				AI_Attack_Progress = 0;
				AI_Attack = Attack_Shoot;
				CurrentPhase++;
			}

			if(switchSubPhases){
				switchSubPhases = false;
				dashing = false;
				dashWait = true;
				npc.dontTakeDamage = false;
				AI_Animation_Counter = 0;
				AI_Timer = 0;
				AI_Attack_Progress = 0;

				//Move to the next subphase
				AI_Attack++;
				if((CurrentPhase == Phase_1 || CurrentPhase == Phase_1_Enrage) && AI_Attack > Attack_Retrieve_Sword)	//Repeat P1 subphases
					AI_Attack = Attack_Shoot;
			}
		}

		private void CheckTargetIsDead(){
			//If the target is dead or not active, slow down the NPC
			//Then, plummet to hell and despawn naturally
			if(noTargetsAlive){
				if(Math.Abs(npc.velocity.X) > 0)
					npc.velocity.X *= 0.5f;
				if(Math.Abs(npc.velocity.Y) > 0)
					npc.velocity.Y *= 0.5f;
				
				if(npc.velocity.Length() < 1 && npc.velocity != Vector2.Zero){
					npc.velocity = Vector2.Zero;
				}

				if(npc.velocity.X == 0){
					npc.velocity.Y += 8f;
				}
			}
		}

		private void AI_Hover(Player target, int facingDirection, out Vector2 npcTarget){
			//Generate a Vector2 point about 12 blocks up, 20 blocks left/right from the player
			//X will be negative if the player is facing left
				
			npcTarget = target.Center;		//Get the player's coordinates
			npcTarget.X += 20 * 16 * facingDirection;		//Add the offset
			npcTarget.Y += -12 * 16;

			if(Vector2.Distance(npcTarget, npc.Center) > 16)	//If the boss isn't near the target point
				npc.velocity += Vector2.Normalize(npcTarget - npc.Center) * new Vector2(0.8f, 0.8f);
			else
				npc.velocity = Vector2.Zero;
			
//			pid.Control(npc, npcTarget);
			PID.PlaceholderControl(npc, npcTarget, 0.86f);
			
			npc.velocity.X = Utils.Clamp(npc.velocity.X, -8, 8);
			npc.velocity.Y = Utils.Clamp(npc.velocity.Y, -10, 10);
		}

		private void AI_Shoot_Laser(int delay, int times){
			AI_Timer++;

			Vector2 positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;

			int npcDamage = npc.damage;
			npc.damage = 0;

			if(AI_Timer % delay == 0){
				//20% chance for attack to be a "shotgun" variant
				if(Main.expertMode && Main.rand.Next(5) == 0){
					for(int i = 0; i < 5; i++){
						Projectile.NewProjectile(npc.Center.X,
							npc.Bottom.Y - (0.667f * npc.height),
							0f,
							0f,
							mod.ProjectileType<DraekProjectile>(),
							CosmivengeonMod.TrueDamage(20 + (Main.expertMode ? 20 : 0)),
							6f,
							Main.myPlayer,
							playerTarget.Center.X + positionOffset.X,
							playerTarget.Center.Y + positionOffset.Y
						);
						positionOffset = new Vector2(Main.rand.NextFloat(-1, 1), Main.rand.NextFloat(-1, 1)) * 48f;
					}
					AI_Attack_Progress += 2;
				}else{
					Projectile.NewProjectile(npc.Center.X,
						npc.Bottom.Y - (0.667f * npc.height),
						0f,
						0f,
						mod.ProjectileType<DraekProjectile>(),
						CosmivengeonMod.TrueDamage(20 + (Main.expertMode ? 20 : 0)),
						6f,
						Main.myPlayer,
						playerTarget.Center.X + positionOffset.X,
						playerTarget.Center.Y + positionOffset.Y
					);
					AI_Attack_Progress++;
				}
				//Play "boss laser" sound effect
				Main.PlaySound(SoundID.Item33, npc.position);
			}

			npc.damage = npcDamage;

			if(AI_Attack_Progress >= times)
				switchSubPhases = true;
		}

		private void AI_Hover_Shoot(int delay, int times){
			AI_Hover(playerTarget, playerTarget.direction, out _);
					
			AI_Shoot_Laser(delay, times);
		}

		private void AI_Hover_Throw_Sword(int times, int waitDuration){
			AI_Hover(playerTarget, playerTarget.direction, out Vector2 npcTarget);
					
			//If the sword was thrown, wait until the animation has played
			if(throwingSword){
				if(AI_Timer == 30)
					AI_Animation_Counter = 0;
						
				AI_Timer--;
				if(AI_Timer == 30 - 22){
					//Throw the sword on the next-to-last frame
					Vector2 dir = (npc.spriteDirection == 1) ? npc.BottomRight : npc.BottomLeft;

					int npcDamage = npc.damage;
					npc.damage = 0;

					//Base 45 damage projectile
					Projectile.NewProjectile(dir,
						Vector2.Zero,
						mod.ProjectileType<DraekSword>(),
						CosmivengeonMod.TrueDamage(45 + (Main.expertMode ? 45 : 0)),
						12f,
						Main.myPlayer,
						npc.target
					);

					npc.damage = npcDamage;

					//Play sword swing sound effect
					Main.PlaySound(SoundID.Item1, npc.position);
				}

				if(AI_Timer == 0){
					throwingSword = false;

					if(AI_Attack_Progress == times){
						switchSubPhases = true;

						//If we're in expert mode, then summon two Young Wyrms after all swords have been thrown
						if(Main.expertMode)
							AI_Summon(2, true, 0f);
					}
				}
			}
			if(!throwingSword){
				if(Vector2.Distance(npcTarget, npc.Center) < 100 * 16)
					AI_Timer++;
				else
					AI_Timer = 0;
						
				//If Draek has been within the 100-tile radius for sword throw for 2 seconds
				if(AI_Timer == waitDuration){
					//Set the timer for sword throw
					AI_Timer = 30;
					AI_Attack_Progress++;
					throwingSword = true;
				}
			}
		}

		private void AI_Charge_Shoot(int delay, int times, float maxXvel, float maxYvel){
			if(Vector2.Distance(playerTarget.Center, npc.Center) > 16)		//If the boss isn't near the target
				npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * new Vector2(0.7f, 0.7f);
			else
				npc.velocity = Vector2.Zero;
					
			AI_Shoot_Laser(delay, times);
			
			PID.PlaceholderControl(npc, playerTarget.Center, 0.86f);

			npc.velocity.X = Utils.Clamp(npc.velocity.X, -maxXvel, maxXvel);
			npc.velocity.Y = Utils.Clamp(npc.velocity.Y, -maxYvel, maxYvel);
		}

		private void AI_Dash(float velocity, int times, int duration, int wait){
			if(dashWait){
				//Delay dash for 30 ticks
				AI_Timer = wait;
				dashWait = false;
			}else if(!dashWait && (AI_Attack_Progress % 2 == 0)){
				//Keep following the player until the wait duration has passed
				if(AI_Attack_Progress == 0){
					npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * 0.7f;
					npc.velocity *= 0.52f;
				}else if(AI_Attack_Progress / 2f < times){
					if(Vector2.Distance(playerTarget.Center, npc.Center) > 16)		//If the boss isn't near the target
						npc.velocity += Vector2.Normalize(playerTarget.Center - npc.Center) * 0.7f;
					else
						npc.velocity = Vector2.Zero;
				}

				AI_Timer--;
				if(AI_Timer == 0)
					AI_Attack_Progress++;
			}else if(AI_Attack_Progress % 2 == 1){
				if(dashing){
					AI_Timer--;

					afterImageLength++;
							
					if(AI_Timer == 0){
						dashing = false;
						dashWait = true;
						AI_Attack_Progress++;
					}
				}else if(!dashing){
					if(AI_Timer == 0){
						//Start dashing if we haven't already
						dashing = true;
						AI_Timer = duration;

						npc.velocity = Vector2.Normalize(playerTarget.Center - npc.Center) * velocity;

						Main.PlaySound(new Terraria.Audio.LegacySoundStyle(SoundID.Roar, 0), npc.Center);

						afterImageLength++;
					}
				}
			}

			if(AI_Attack_Progress == 2 * times)
				switchSubPhases = true;
			
			if(!dashing){
				npc.velocity.X = Utils.Clamp(npc.velocity.X, -6, 6);
				npc.velocity.Y = Utils.Clamp(npc.velocity.Y, -8, 8);
			}
		}

		private void AI_Retrieve_Sword(){
			npc.velocity *= 0.7f;
			
			npc.dontTakeDamage = true;

			if(AI_Animation_Counter == 30)
				switchSubPhases = true;
		}

		private void AI_Summon(int times, bool randomAngle, params float[] angles){
			SummonedWyrms = UpdateWyrmCount();

			while(SummonedWyrms < times){
				float rotation;
				Vector2 range = npc.Center + new Vector2(Main.rand.NextFloat(-8 * 16, 8 * 16), Main.rand.NextFloat(-8 * 16, 8 * 16));
			
				if(randomAngle)
					rotation = Main.rand.NextFloat(0, 2 * MathHelper.Pi);
				else
					rotation = angles[SummonedWyrms];
			
				NPC.NewNPC((int)range.X, (int)range.Y, mod.NPCType<Summon>(), ai1: npc.whoAmI, ai2: rotation);
			
				SummonedWyrms++;
			}
		}

		private int UpdateWyrmCount(){
			int wyrms = 0;
			
			for(int i = 0; i < Main.npc.Length; i++){
				if(Main.npc[i].type == mod.NPCType<Summon>() && (int)Main.npc[i].ai[1] == npc.whoAmI && Main.npc[i].active)
					wyrms++;
			}
			
			return wyrms;
		}
	}
}